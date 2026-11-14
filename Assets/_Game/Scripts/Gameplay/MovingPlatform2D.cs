using UnityEngine;

/// <summary>
/// Платформа (или тень), плавно ездящая туда-сюда между двумя точками.
/// Позиции точек запоминаются ОДИН РАЗ на старте — после этого не важно,
/// дочерние они или нет: платформа не разгонится.
///
/// Установка:
/// 1. На платформе: Box Collider 2D (НЕ trigger).
/// 2. Дочерние пустые объекты PointA и PointB в краях маршрута.
/// 3. В Inspector перетащить PointA/PointB в поля скрипта.
/// </summary>
public class MovingPlatform2D : MonoBehaviour
{
    [Header("Точки маршрута (дочерние пустые объекты)")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Настройки")]
    [Tooltip("Секунды на полный путь от точки до точки")]
    [SerializeField] private float travelTime = 3f;

    private Vector3 posA;
    private Vector3 posB;
    private bool initialized;

    private void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogWarning($"MovingPlatform2D на {name}: не заданы PointA/PointB");
            enabled = false;
            return;
        }

        // Фиксируем мировые позиции один раз. Дальше точки можешь хоть удалить.
        posA = pointA.position;
        posB = pointB.position;
        transform.position = posA;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        // 0 → 1 → 0 → 1... всегда, без остановок, заеданий и разгона
        float t = Mathf.PingPong(Time.time / Mathf.Max(0.1f, travelTime), 1f);
        transform.position = Vector3.Lerp(posA, posB, t);
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA.position, pointB.position);
        Gizmos.DrawWireSphere(pointA.position, 0.2f);
        Gizmos.DrawWireSphere(pointB.position, 0.2f);
    }
}
