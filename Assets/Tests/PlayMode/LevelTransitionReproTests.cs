using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Репродюсер бага «прошёл уровень 1, но не перешёл на уровень 2».
/// Телепортирует игрока к алтарю Level_01 и проверяет, что грузится сцена Trials.
/// </summary>
public class LevelTransitionReproTests
{
    [UnityTest]
    public IEnumerator Level1AltarLoadsTrialsScene()
    {
        yield return SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Game.unity");
        yield return null;
        yield return null;

        var manager = Object.FindFirstObjectByType<GameManager>();
        var flame = Object.FindFirstObjectByType<CandleFlame>();
        var altar = Object.FindObjectsByType<ExitAltar>(FindObjectsSortMode.None)
            .OrderByDescending(a => a.transform.position.x).FirstOrDefault();

        Assert.That(manager, Is.Not.Null, "В сцене Game нет GameManager");
        Assert.That(flame, Is.Not.Null, "В сцене Game нет CandleFlame");
        Assert.That(altar, Is.Not.Null, "В сцене Game нет ExitAltar");

        var nextField = typeof(ExitAltar).GetField("nextSceneName",
            BindingFlags.Instance | BindingFlags.NonPublic);
        string nextAtRuntime = nextField != null ? (string)nextField.GetValue(altar) : "<field missing>";
        Debug.Log($"[REPRO] altar '{altar.name}' at x={altar.transform.position.x:F1}, " +
                  $"nextSceneName='{nextAtRuntime}'");

        manager.StartGame();
        Assert.That(manager.State, Is.EqualTo(GameState.Playing), "Игра не стартовала");

        var body = flame.GetComponent<Rigidbody2D>();
        Assert.That(body, Is.Not.Null, "У игрока нет Rigidbody2D");
        var movement = flame.GetComponent<PlayerMovement2D>();
        if (movement != null) movement.enabled = false;

        // Держим игрока внутри триггера несколько кадров — триггер должен сработать.
        for (int i = 0; i < 10; i++)
        {
            body.position = (Vector2)altar.transform.position;
            body.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
            yield return null;
        }

        string activeScene = SceneManager.GetActiveScene().name;
        string state = manager != null ? manager.State.ToString() : "<manager destroyed>";
        Debug.Log($"[REPRO] after altar: activeScene='{activeScene}', state={state}, " +
                  $"flame={flame.Value:F1}");

        Assert.That(nextAtRuntime, Is.Not.Empty,
            "У алтаря Level_01 пустое nextSceneName — вместо перехода на уровень 2 будет просто Win()");
        Assert.That(activeScene, Is.EqualTo("Trials"),
            $"Дотронулись до алтаря 1-го уровня, но активная сцена '{activeScene}' " +
            $"(состояние {state}) — перехода на уровень 2 не произошло");
    }
}
