using UnityEngine;

/// <summary>
/// Тень-летун: противник, который преследует игрока, когда тот в радиусе
/// обнаружения, и отнимает пламя при касании. Пока игрока нет рядом — стоит.
///
/// Установка:
/// 1. Объект: 2D Object → Sprites → Circle. Назовите ShadowMoth.
/// 2. Масштаб ~0.5–0.7, тёмный цвет (почти чёрный) — это тень.
/// 3. Add Component → Circle Collider 2D → Is Trigger = ON + этот скрипт.
/// 4. Rigidbody НЕ нужен — двигаем напрямую.
/// 5. Damage = 20, Damage Cooldown = 2 (не бьёт чаще, чем раз в 2 сек),
///    Chase Speed = 1.5, Detection Radius = 6.
/// </summary>
public class ShadowMoth : MonoBehaviour
{
    [Header("Преследование")]
    [SerializeField] private float chaseSpeed = 1.5f;
    [SerializeField] private float detectionRadius = 6f;

    [Header("Урон свече")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float damageCooldown = 2f;

    private Transform player;
    private float nextDamageTime;

    private void Update()
    {
        // Находим игрока (тот, у кого CandleFlame), если ещё не нашли
        if (player == null)
        {
            CandleFlame flame = FindFirstObjectByType<CandleFlame>();
            if (flame != null) player = flame.transform;
            return;
        }

        // Двигаемся только пока идёт игра
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing) return;

        // Преследуем, только если игрок в радиусе
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > detectionRadius) return;

        transform.position = Vector2.MoveTowards(
            transform.position, player.position,
            chaseSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing) return;

        CandleFlame flame = other.GetComponentInParent<CandleFlame>();
        if (flame == null) return;

        if (Time.time >= nextDamageTime)
        {
            flame.Hurt(damage);
            nextDamageTime = Time.time + damageCooldown;
        }
    }

    // Радиус обнаружения в сцене
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
