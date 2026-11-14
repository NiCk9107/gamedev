using UnityEngine;

/// <summary>Player's flame; shared by pickups, hazards, lighting and HUD.</summary>
[DisallowMultipleComponent]
public sealed class CandleFlame : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxFlame = 100f;
    [SerializeField, Min(0f)] private float lossPerSecond = 1f;

    private float currentFlame;

    public float Value => currentFlame;
    public float MaxFlame => maxFlame;
    public float Normalized => Mathf.Clamp01(currentFlame / maxFlame);

    private void Awake()
    {
        ValidateSettings();
        currentFlame = maxFlame;
    }

    private void OnValidate() => ValidateSettings();

    private void ValidateSettings()
    {
        if (!IsFinite(maxFlame) || maxFlame < 1f) maxFlame = 100f;
        if (!IsFinite(lossPerSecond) || lossPerSecond < 0f) lossPerSecond = 1f;
    }

    private void Update()
    {
        if (!IsPlaying()) return;
        Change(-lossPerSecond * Time.deltaTime);
    }

    public void Restore(float amount)
    {
        if (IsFinite(amount) && amount > 0f) Change(amount);
    }

    public void Hurt(float amount)
    {
        if (IsFinite(amount) && amount > 0f) Change(-amount);
    }

    private void Change(float delta)
    {
        if (!IsPlaying()) return;
        currentFlame = Mathf.Clamp(currentFlame + delta, 0f, maxFlame);
        if (currentFlame <= 0f) GameManager.Instance.Lose();
    }

    private static bool IsPlaying() => GameManager.Instance != null
        && GameManager.Instance.State == GameState.Playing;

    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
