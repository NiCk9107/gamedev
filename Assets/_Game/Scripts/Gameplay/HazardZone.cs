using System.Collections.Generic;
using UnityEngine;

/// <summary>One hit per visit, even when the player has multiple colliders.</summary>
[DisallowMultipleComponent]
public sealed class HazardZone : MonoBehaviour
{
    [SerializeField, Min(0f)] private float damage = 20f;
    private readonly Dictionary<CandleFlame, HashSet<Collider2D>> occupants
        = new Dictionary<CandleFlame, HashSet<Collider2D>>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        CandleFlame flame = other.GetComponentInParent<CandleFlame>();
        if (flame == null || flame.Value <= 0f) return;

        if (!occupants.TryGetValue(flame, out HashSet<Collider2D> colliders))
        {
            colliders = new HashSet<Collider2D>();
            occupants.Add(flame, colliders);
        }

        // A collider can be disabled/destroyed without a useful exit callback.
        colliders.RemoveWhere(collider => collider == null || !collider.enabled
            || !collider.gameObject.activeInHierarchy);
        bool firstContact = colliders.Count == 0;
        if (colliders.Add(other) && firstContact) flame.Hurt(damage);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        CandleFlame flame = other.GetComponentInParent<CandleFlame>();
        if (flame == null || !occupants.TryGetValue(flame, out HashSet<Collider2D> colliders)) return;
        colliders.Remove(other);
        if (colliders.Count == 0) occupants.Remove(flame);
    }

    private void OnDisable() => occupants.Clear();
}
