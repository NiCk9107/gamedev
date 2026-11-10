using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Базовое 2D-управление: ходьба + прыжок через Rigidbody2D.
/// Требует: New Input System (включён в Project Settings → Player).
///
/// Установка в Unity:
/// 1. Скопируйте файл в Assets/_Game/Scripts/Player/
/// 2. На объекте игрока: Add Component → Rigidbody2D (Freeze Rotation Z = 0
///    не нужно, но CapsuleCollider2D/BoxCollider2D обязателен) + этот скрипт.
/// 3. Параметры подберите в Inspector под свой проект.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Прыжок")]
    [SerializeField] private float jumpForce = 10f;
    [Tooltip("Слой земли для проверки 'стою ли я на полу'")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Точка проверки земли (пустой дочерний объект у ног)")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Опционально: coyote time — можно прыгнуть чуть после края")]
    [SerializeField] private float coyoteTime = 0.1f;

    private Rigidbody2D rb;
    private float moveInput;          // -1..1 с клавиатуры
    private float coyoteCounter;      // сколько ещё можно прыгнуть
    private bool jumpBuffered;        // прыжок нажат чуть раньше приземления

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Ввод читаем в Update — физику меняем в FixedUpdate.
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        moveInput = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput += 1f;

        if (keyboard.spaceKey.wasPressedThisFrame) jumpBuffered = true;

        // Coyote time: после схода с платформы прыжок ещё немного доступен.
        coyoteCounter = IsGrounded() ? coyoteTime : coyoteCounter - Time.deltaTime;
    }

    private void FixedUpdate()
    {
        // Горизонтальная скорость — вертикальную не трогаем (гравитация сама).
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (jumpBuffered && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBuffered = false;
            coyoteCounter = 0f;
        }
        else if (!IsGrounded() && rb.linearVelocity.y < 0f)
        {
            // Отпускаем буфер в воздухе, чтобы не прыгал потом сам.
            jumpBuffered = false;
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
