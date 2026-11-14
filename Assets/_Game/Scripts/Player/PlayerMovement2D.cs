using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D-управление: ходьба + прыжок через Rigidbody2D.
/// Фичи: coyote time, jump buffer, управляемая высота прыжка,
/// усиленное падение, поворот спрайта по направлению движения.
/// Требует: New Input System (включён в Project Settings → Player).
///
/// Установка в Unity:
/// 1. На объекте игрока: Rigidbody2D + CapsuleCollider2D/BoxCollider2D + этот скрипт.
/// 2. groundCheck — пустой дочерний объект у ног игрока.
/// 3. GroundLayer — слой платформ (в проекте уже создан слой Ground).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Прыжок")]
    [SerializeField] private float jumpForce = 10f;
    [Tooltip("Доля вертикальной скорости, которая сохраняется при отпускании пробела (< 1 — прыжок ниже)")]
    [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.4f;
    [Tooltip("Слой земли для проверки 'стою ли я на полу'")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Точка проверки земли (пустой дочерний объект у ног)")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Coyote time — можно прыгнуть чуть после схода с края")]
    [SerializeField] private float coyoteTime = 0.1f;

    [Header("Jump buffer — нажатие чуть ДО приземления всё равно прыгнет")]
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Гравитация")]
    [Tooltip("Множитель скорости падения (> 1 — падаем быстрее, чем поднимаемся)")]
    [SerializeField] private float fallMultiplier = 2.2f;
    [Tooltip("Множитель на коротком подъёме после отпускания прыжка")]
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Опционально: спрайт для поворота по направлению")]
    [SerializeField] private SpriteRenderer spriteToFlip;

    private Rigidbody2D rb;
    private float moveInput;            // -1..1 с клавиатуры
    private float coyoteCounter;        // сколько ещё можно прыгнуть
    private float jumpBufferCounter;    // сколько ещё «живёт» нажатый прыжок

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteToFlip == null) spriteToFlip = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        // Ввод читаем в Update — физику меняем в FixedUpdate.
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        moveInput = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput += 1f;

        if (keyboard.spaceKey.wasPressedThisFrame)
            jumpBufferCounter = jumpBufferTime;

        // Coyote time: после схода с платформы прыжок ещё немного доступен.
        coyoteCounter = IsGrounded() ? coyoteTime : coyoteCounter - Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;

        // Поворот спрайта по направлению движения.
        if (spriteToFlip != null && moveInput != 0f)
            spriteToFlip.flipX = moveInput < 0f;
    }

    private void FixedUpdate()
    {
        // Горизонтальная скорость — вертикальную задаём ниже с учётом гравитации.
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Прыжок: сработает, если живы И буфер, И coyote.
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }

        // Управляемая высота прыжка: отпустили пробел на подъёме — обрезаем скорость.
        var kb = Keyboard.current;
        bool jumpHeld = kb != null && kb.spaceKey.isPressed;
        if (rb.linearVelocity.y > 0f && !jumpHeld)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        // Разная гравитация на подъёме/падении: падение быстрее, прыжок «злее».
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // Кружок проверки земли рисуем в редакторе — удобно настраивать.
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
