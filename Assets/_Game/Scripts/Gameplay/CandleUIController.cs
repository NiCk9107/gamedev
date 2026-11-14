using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Slider flameSlider;
    [SerializeField] private Button playButton;
    [SerializeField] private Button winRestartButton;
    [SerializeField] private Button loseRestartButton;
    [SerializeField] private CandleFlame flame;

    private void Awake()
    {
        startPanel        ??= FindChild("StartPanel");
        winPanel          ??= FindChild("WinPanel");
        losePanel         ??= FindChild("LosePanel");
        flameSlider       ??= GetComponentInChildren<Slider>(true);
        playButton        ??= FindChild<Button>("PlayButton");
        winRestartButton  ??= FindChild<Button>("WinRestartButton");
        loseRestartButton ??= FindChild<Button>("LoseRestartButton");
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
            winRestartButton.onClick.AddListener(() => GameManager.Instance?.Restart());
        if (loseRestartButton != null)
            loseRestartButton.onClick.AddListener(() => GameManager.Instance?.Restart());
    }

    private void Update()
    {
        if (flameSlider != null && flame != null)
            flameSlider.value = flame.Normalized;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        if (startPanel != null) startPanel.SetActive(state == GameState.Ready);
        if (winPanel != null)   winPanel.SetActive(state == GameState.Won);
        if (losePanel != null)  losePanel.SetActive(state == GameState.Lost);
    }

    private GameObject FindChild(string name)
    {
        var t = transform.Find(name);
        return t != null ? t.gameObject : null;
    }

    private T FindChild<T>(string name) where T : Component
    {
        var t = transform.Find(name);
        return t != null ? t.GetComponent<T>() : null;
    }
}
