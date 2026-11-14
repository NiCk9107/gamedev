using UnityEngine;

/// <summary>
/// Алтарь выхода: победа, если вошёл игрок с живым пламенем (Value > 0).
/// Вешается на ExitAltar (нужен Collider2D с Is Trigger).
/// </summary>
public sealed class ExitAltar : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var flame = other.GetComponentInParent<CandleFlame>();
        if (flame == null || flame.Value <= 0f) return;
        if (GameManager.Instance == null) return;

        GameManager.Instance.Win();
    }
}
