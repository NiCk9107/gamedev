using UnityEngine;

/// <summary>
/// ВРЕМЕННЫЙ скрипт для интеграционной проверки без UI.
/// Любая клавиша — StartGame из Ready; R — Restart после финала.
/// УДАЛИТЬ после подключения канваса Вики (GameCanvas).
/// </summary>
public sealed class TempStartOnKey : MonoBehaviour
{
    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.State == GameState.Ready
            && KeyboardAnyKey())
        {
            GameManager.Instance.StartGame();
        }
        else if ((GameManager.Instance.State == GameState.Won
               || GameManager.Instance.State == GameState.Lost)
               && UnityEngine.InputSystem.Keyboard.current != null
               && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            GameManager.Instance.Restart();
        }
    }

    private static bool KeyboardAnyKey()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        return kb != null && kb.anyKey.wasPressedThisFrame;
    }
}
