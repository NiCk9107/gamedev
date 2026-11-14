using UnityEngine;

/// <summary>
/// Зона урона (ветер / тень): наносит урон один раз при входе игрока.
/// Вешается на WindZone (damage = 20) и ShadowZone (damage = 35).
/// Нужен Collider2D с Is Trigger.
/// </summary>
public sealed class HazardZone : MonoBehaviour
{
    [SerializeField, Min(0f)] private float damage = 20f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var flame = other.GetComponentInParent<CandleFlame>();
        if (flame == null) return;

        flame.Hurt(damage);
    }
}
