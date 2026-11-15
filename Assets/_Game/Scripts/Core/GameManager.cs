using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Ready, Playing, Won, Lost }

/// <summary>Owns the current run. Add one instance to the gameplay scene.</summary>
[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState State { get; private set; } = GameState.Ready;
    public event Action<GameState> StateChanged;

    private bool restartRequested;

    /// <summary>
    /// Переживает загрузку сцен: true после первого старта игры.
    /// Первый запуск — со стартовым меню; переходы между уровнями — сразу в игру.
    /// </summary>
    private static bool runStarted;

    /// <summary>Сбросить флаг прогона — вызывать перед возвратом в первую сцену после победы.</summary>
    public static void ResetRun() => runStarted = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance() => Instance = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Keep unrelated components on the duplicate object intact.
            enabled = false;
            Destroy(this);
            return;
        }

        Instance = this;
        if (runStarted)
        {
            // Переход с другого уровня: меню не показываем, играем сразу.
            State = GameState.Playing;
            Time.timeScale = 1f;
        }
        else
        {
            State = GameState.Ready;
            Time.timeScale = 0f;
        }
    }

    private void Start()
    {
        if (Instance == this) StateChanged?.Invoke(State);
    }

    public void StartGame()
    {
        if (Instance != this || restartRequested || State != GameState.Ready) return;
        runStarted = true;
        SetState(GameState.Playing);
    }

    public void Win() => Finish(GameState.Won);
    public void Lose() => Finish(GameState.Lost);

    private void Finish(GameState result)
    {
        if (Instance != this || restartRequested || State != GameState.Playing) return;
        SetState(result);
    }

    private void SetState(GameState state)
    {
        State = state;
        Time.timeScale = state == GameState.Playing ? 1f : 0f;
        StateChanged?.Invoke(state);
    }

    public void Restart()
    {
        if (Instance != this || restartRequested) return;

        Scene scene = SceneManager.GetActiveScene();
        // A path also supports a scene whose build index is unavailable in Editor.
        if (string.IsNullOrEmpty(scene.path) || !Application.CanStreamedLevelBeLoaded(scene.path))
        {
            Debug.LogError("Restart: add the gameplay scene to Build Profiles > Scene List.", this);
            return;
        }

        restartRequested = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene.path);
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        Instance = null;
        Time.timeScale = 1f;
    }
}
