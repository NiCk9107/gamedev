using UnityEngine;

/// <summary>
/// Капля воска: восстанавливает +25 пламени игроку и уничтожается.
/// Вешается на префаб WaxDrop (нужен Collider2D с Is Trigger).
/// Работает только во время Playing (проверка внутри CandleFlame.Restore).
/// </summary>
public sealed class WaxPickup : MonoBehaviour
{
    [SerializeField, Min(0f)] private float restoreAmount = 25f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var flame = other.GetComponentInParent<CandleFlame>();
        if (flame == null) return;

        flame.Restore(restoreAmount);
        Destroy(gameObject);
    }
}
