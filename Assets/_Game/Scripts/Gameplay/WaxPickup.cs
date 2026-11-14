using UnityEngine;

/// <summary>Collect once, and only during an active run.</summary>
[DisallowMultipleComponent]
public sealed class WaxPickup : MonoBehaviour
{
    [SerializeField, Min(0f)] private float restoreAmount = 25f;
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || GameManager.Instance == null
            || GameManager.Instance.State != GameState.Playing) return;

        CandleFlame flame = other.GetComponentInParent<CandleFlame>();
        if (flame == null || flame.Value <= 0f) return;

        // Destroy is deferred until the end of the frame; additional player
        // colliders must not collect the same drop again during that frame.
        collected = true;
        flame.Restore(restoreAmount);
        Destroy(gameObject);
    }
}
