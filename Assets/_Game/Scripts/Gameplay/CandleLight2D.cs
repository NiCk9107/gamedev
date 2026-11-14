using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Привязка света к пламени свечи: чем меньше пламя, тем тусклее и уже свет.
/// Вешается на объект с Light2D (CandleLight, дочерний к Player).
/// Flame берётся из родителя автоматически, можно назначить вручную.
/// </summary>
[RequireComponent(typeof(Light2D))]
public sealed class CandleLight2D : MonoBehaviour
{
    [SerializeField] private CandleFlame flame;
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 1f;
    [SerializeField, Tooltip("Радиус света при нулевом пламени")]
    private float minRadius = 0f;
    [SerializeField, Tooltip("Радиус света при полном пламени (рекомендация ТЗ: 1.2–5)")]
    private float maxRadius = 5f;

    private Light2D light2d;

    private void Awake()
    {
        light2d = GetComponent<Light2D>();
        if (flame == null) flame = GetComponentInParent<CandleFlame>();
    }

    private void Update()
    {
        if (light2d == null) return;

        float n = flame != null ? flame.Normalized : 0f;
        light2d.intensity = Mathf.Lerp(minIntensity, maxIntensity, n);
        light2d.pointLightOuterRadius = Mathf.Lerp(minRadius, maxRadius, n);
        light2d.enabled = n > 0f;   // при нуле свет гаснет полностью
    }
}
