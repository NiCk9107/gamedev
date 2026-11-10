using UnityEngine;

/// <summary>
/// Плавное следование камеры за целью (2D, вид сбоку/сверху).
/// Лёгкая альтернатива Cinemachine — для хакатона достаточно.
///
/// Установка в Unity:
/// 1. Скопируйте файл в Assets/_Game/Scripts/Player/ (или в Scripts/Core).
/// 2. На Main Camera: Add Component → CameraFollow2D.
/// 3. В поле Target перетащите объект игрока из иерархии.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [Tooltip("Кого сопровождаем — перетащить игрока")]
    [SerializeField] private Transform target;

    [Tooltip("Смещение камеры относительно цели (обычно z = -10)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Tooltip("Плавность: больше = резче, меньше = мягче. 5–10 — норм")]
    [SerializeField] private float smoothSpeed = 7f;

    [Tooltip("Жёсткие границы уровня, если нужны. Поставьте галочки.")]
    [SerializeField] private bool useBounds;
    [SerializeField] private float minX, maxX, minY, maxY;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        Vector3 smoothed = Vector3.Lerp(
            transform.position, desired, smoothSpeed * Time.deltaTime);

        if (useBounds)
        {
            smoothed.x = Mathf.Clamp(smoothed.x, minX, maxX);
            smoothed.y = Mathf.Clamp(smoothed.y, minY, maxY);
        }

        transform.position = smoothed;
    }
}
