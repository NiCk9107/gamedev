using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitAltar : MonoBehaviour
{
    [Tooltip("Если пусто — алтарь даёт победу. Если указано имя сцены — грузит её.")]
    [SerializeField] private string nextSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing) return;

        CandleFlame flame = other.GetComponentInParent<CandleFlame>();
        if (flame == null || flame.Value <= 0f) return;

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            GameManager.Instance.Win();
    }
}