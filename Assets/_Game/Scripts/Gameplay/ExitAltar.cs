using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitAltar : MonoBehaviour
{
    [Tooltip("Если пусто — алтарь даёт победу. Если указано имя сцены — грузит её.")]
    [SerializeField] private string nextSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        CandleFlame flame = other.GetComponentInParent<CandleFlame>();
        if (flame == null) return; // коснулся не игрок

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[ExitAltar] Касание, но GameManager не найден", this);
            return;
        }
        if (GameManager.Instance.State != GameState.Playing)
        {
            Debug.LogWarning($"[ExitAltar] Касание, но состояние {GameManager.Instance.State} (нужно Playing)", this);
            return;
        }
        if (flame.Value <= 0f)
        {
            Debug.LogWarning("[ExitAltar] Касание, но пламя = 0", this);
            return;
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"[ExitAltar] Переход на сцену: {nextSceneName}", this);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("[ExitAltar] Финальный алтарь — победа!", this);
            GameManager.Instance.Win();
        }
    }
}