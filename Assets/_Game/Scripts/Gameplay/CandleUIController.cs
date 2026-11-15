using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Контроллер UI «Последней свечи»: стартовое меню, HUD со шкалой пламени,
/// панели победы/поражения. Вешается на GameCanvas.
/// Все ссылки находит по именам дочерних объектов, кнопки подключает
/// к GameManager автоматически — вручную настраивать onClick не нужно.
/// </summary>
public sealed class CandleUIController : MonoBehaviour
{
    [Header("Ссылки (если пусто — ищется по имени дочернего объекта)")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Slider flameSlider;
    [SerializeField] private TMP_Text flameLabel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button winRestartButton;
    [SerializeField] private Button loseRestartButton;
    [SerializeField] private CandleFlame flame;

    [Header("После победы")]
    [Tooltip("Сцена, которая грузится по кнопке на экране победы. Game = начало игры.")]
    [SerializeField] private string firstLevelScene = "Game";

    private void Awake()
    {
        if (startPanel == null) startPanel = FindChild("StartPanel");
        if (hudPanel == null) hudPanel = FindChild("HUD");
        if (flameLabel == null) flameLabel = FindChild<TMP_Text>("FlameLabel");
        if (winPanel == null) winPanel = FindChild("WinPanel");
        if (losePanel == null) losePanel = FindChild("LosePanel");
        if (flameSlider == null) flameSlider = GetComponentInChildren<Slider>(true);
        if (playButton == null) playButton = FindChild<Button>("PlayButton");
        if (winRestartButton == null) winRestartButton = FindChild<Button>("WinRestartButton");
        if (loseRestartButton == null) loseRestartButton = FindChild<Button>("LoseRestartButton");
    }

    private void Start()
    {
        if (flame == null)
            flame = FindFirstObjectByType<CandleFlame>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StateChanged += OnStateChanged;
            OnStateChanged(GameManager.Instance.State);
        }

        if (playButton != null)
            playButton.onClick.AddListener(() => GameManager.Instance?.StartGame());
        if (winRestartButton != null)
            winRestartButton.onClick.AddListener(() =>
            {
                // После победы — в начало игры (с первого уровня),
                // независимо от того, на каком уровне победили.
                Time.timeScale = 1f;
                GameManager.ResetRun(); // возврат в начало игры — снова показать стартовое меню
                UnityEngine.SceneManagement.SceneManager.LoadScene(firstLevelScene);
            });
        if (loseRestartButton != null)
            loseRestartButton.onClick.AddListener(() => GameManager.Instance?.Restart());
    }

    private void Update()
    {
        if (flameSlider != null && flame != null)
            flameSlider.value = flame.Normalized;
        if (flameLabel != null && flame != null)
            flameLabel.text = Mathf.CeilToInt(flame.Normalized * 100f) + "%";
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        if (startPanel != null) startPanel.SetActive(state == GameState.Ready);
        if (hudPanel != null)   hudPanel.SetActive(state == GameState.Playing);
        if (winPanel != null)   winPanel.SetActive(state == GameState.Won);
        if (losePanel != null)  losePanel.SetActive(state == GameState.Lost);
    }

    private GameObject FindChild(string name)
    {
        var t = FindDescendant(name);
        return t != null ? t.gameObject : null;
    }

    private Transform FindDescendant(string name)
    {
        foreach (var child in GetComponentsInChildren<Transform>(true))
            if (child.name == name) return child;
        return null;
    }

    private T FindChild<T>(string name) where T : Component
    {
        var t = FindDescendant(name);
        return t != null ? t.GetComponent<T>() : null;
    }
}

