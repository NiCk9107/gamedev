using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Reflection keeps production scripts in Assembly-CSharp, so teammates do not
// need to change their assembly definitions to use the public gameplay API.
public class GameSystemsTests : IPrebuildSetup, IPostBuildCleanup
{
    private const string Fixture = "Assets/Tests/PlayMode/Fixtures/GameSystemsTest.unity";
    private Type managerType;
    private Component flame;
    private Component Manager => (Component)managerType.GetProperty("Instance").GetValue(null);

    [OneTimeSetUp]
    public void SetUpFixture()
    {
        managerType = Type.GetType("GameManager, Assembly-CSharp", true);
    }

    // Runtime scene lists are captured when entering Play Mode, so register the
    // fixture before that transition rather than during NUnit SetUp.
    public void Setup()
    {
#if UNITY_EDITOR
        const string backupKey = "LastCandle.GameSystems.BuildScenes";
        if (string.IsNullOrEmpty(SessionState.GetString(backupKey, "")))
        {
            var backup = new SceneListBackup();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                backup.paths.Add(scene.path);
                backup.enabled.Add(scene.enabled);
            }
            SessionState.SetString(backupKey, JsonUtility.ToJson(backup));
        }
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        scenes.RemoveAll(scene => scene.path == Fixture);
        scenes.Add(new EditorBuildSettingsScene(Fixture, true));
        EditorBuildSettings.scenes = scenes.ToArray();
#endif
    }

    public void Cleanup()
    {
#if UNITY_EDITOR
        const string backupKey = "LastCandle.GameSystems.BuildScenes";
        string json = SessionState.GetString(backupKey, "");
        if (!string.IsNullOrEmpty(json))
        {
            var backup = JsonUtility.FromJson<SceneListBackup>(json);
            var scenes = new List<EditorBuildSettingsScene>();
            for (int i = 0; i < backup.paths.Count; i++)
                scenes.Add(new EditorBuildSettingsScene(backup.paths[i], backup.enabled[i]));
            EditorBuildSettings.scenes = scenes.ToArray();
            SessionState.EraseString(backupKey);
        }
#endif
        Time.timeScale = 1f;
    }

    [Serializable]
    private class SceneListBackup
    {
        public List<string> paths = new List<string>();
        public List<bool> enabled = new List<bool>();
    }

    [UnitySetUp]
    public IEnumerator LoadFreshRun()
    {
        yield return SceneManager.LoadSceneAsync(Fixture);
        yield return null;
        flame = GameObject.Find("TestPlayer").GetComponent(Type.GetType("CandleFlame, Assembly-CSharp", true));
    }

    private static void Call(Component component, string method, params object[] args)
        => component.GetType().GetMethod(method).Invoke(component, args);

    private static float Number(Component component, string property)
        => (float)component.GetType().GetProperty(property).GetValue(component);

    private string State => managerType.GetProperty("State").GetValue(Manager).ToString();

    [Test]
    public void MenuStartsPausedWithFullFlame()
    {
        Assert.That(State, Is.EqualTo("Ready"));
        Assert.That(Time.timeScale, Is.Zero);
        Assert.That(Number(flame, "Value"), Is.EqualTo(100f));
        Assert.That(Number(flame, "Normalized"), Is.EqualTo(1f));
        Call(flame, "Hurt", 35f);
        Call(Manager, "Win");
        Call(Manager, "Lose");
        Assert.That(State, Is.EqualTo("Ready"));
        Assert.That(Number(flame, "Value"), Is.EqualTo(100f));
    }

    [Test]
    public void WaxAndHazardsUseAgreedValuesAndClampToMaximum()
    {
        Call(Manager, "StartGame");
        Call(flame, "Hurt", 20f);
        Assert.That(Number(flame, "Value"), Is.EqualTo(80f));
        Call(flame, "Hurt", 35f);
        Assert.That(Number(flame, "Value"), Is.EqualTo(45f));
        Call(flame, "Restore", 25f);
        Assert.That(Number(flame, "Value"), Is.EqualTo(70f));
        Call(flame, "Restore", 1000f);
        Assert.That(Number(flame, "Value"), Is.EqualTo(100f));
        Assert.That(Number(flame, "Normalized"), Is.EqualTo(1f));
    }

    [Test]
    public void ZeroFlameLosesAndCannotBeRestoredAfterwards()
    {
        Call(Manager, "StartGame");
        Call(flame, "Hurt", 1000f);
        Assert.That(State, Is.EqualTo("Lost"));
        Assert.That(Number(flame, "Value"), Is.Zero);
        Assert.That(Number(flame, "Normalized"), Is.Zero);
        Assert.That(Time.timeScale, Is.Zero);
        Call(flame, "Restore", 25f);
        Call(Manager, "Win");
        Call(Manager, "StartGame");
        Assert.That(State, Is.EqualTo("Lost"));
        Assert.That(Number(flame, "Value"), Is.Zero);
    }

    [Test]
    public void WinningFreezesFlameAndIgnoresRepeatedFinish()
    {
        Call(Manager, "StartGame");
        Call(Manager, "Win");
        Call(flame, "Hurt", 35f);
        Call(flame, "Restore", 25f);
        Call(Manager, "Lose");
        Call(Manager, "Win");
        Assert.That(State, Is.EqualTo("Won"));
        Assert.That(Time.timeScale, Is.Zero);
        Assert.That(Number(flame, "Value"), Is.EqualTo(100f));
    }

    [Test]
    public void InvalidAmountsCannotCorruptFlame()
    {
        Call(Manager, "StartGame");
        foreach (float value in new[] { -25f, 0f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
        {
            Call(flame, "Restore", value);
            Call(flame, "Hurt", value);
        }
        Assert.That(Number(flame, "Value"), Is.EqualTo(100f));
        Assert.That(State, Is.EqualTo("Playing"));
    }

    [UnityTest]
    public IEnumerator FlameBurnsOnlyWhilePlaying()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        Assert.That(Number(flame, "Value"), Is.EqualTo(100f));
        Call(Manager, "StartGame");
        yield return new WaitForSecondsRealtime(0.3f);
        float remaining = Number(flame, "Value");
        Assert.That(100f - remaining, Is.InRange(0.15f, 1f));
        Call(Manager, "Win");
        yield return new WaitForSecondsRealtime(0.15f);
        Assert.That(Number(flame, "Value"), Is.EqualTo(remaining));
    }

    [UnityTest]
    public IEnumerator NaturalBurnoutEndsRun()
    {
        flame.GetType().GetField("lossPerSecond", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(flame, 10000f);
        Call(Manager, "StartGame");
        yield return new WaitForSecondsRealtime(0.15f);
        Assert.That(State, Is.EqualTo("Lost"));
        Assert.That(Number(flame, "Value"), Is.Zero);
    }

    [UnityTest]
    public IEnumerator DuplicateManagerCannotReplaceOwnerOrUnpauseGame()
    {
        Component original = Manager;
        var duplicateObject = new GameObject("DuplicateManager");
        Component duplicate = duplicateObject.AddComponent(managerType);
        Call(duplicate, "StartGame");
        yield return null;
        Assert.That(Manager, Is.SameAs(original));
        Assert.That(State, Is.EqualTo("Ready"));
        Assert.That(Time.timeScale, Is.Zero);
        Assert.That(duplicate == null, Is.True);
        Assert.That(duplicateObject != null, Is.True);
        UnityEngine.Object.Destroy(duplicateObject);
    }

    [UnityTest]
    public IEnumerator StartingBeforeStartCallbackDoesNotPauseAgain()
    {
        UnityEngine.Object.Destroy(Manager.gameObject);
        yield return null;
        Component earlyManager = new GameObject("EarlyManager").AddComponent(managerType);
        Call(earlyManager, "StartGame");
        yield return null;
        Assert.That(State, Is.EqualTo("Playing"));
        Assert.That(Time.timeScale, Is.EqualTo(1f));
    }

    [UnityTest]
    public IEnumerator RepeatedRestartRestoresFlameAndSceneObjects()
    {
        Call(Manager, "StartGame");
        Call(flame, "Hurt", 35f);
        UnityEngine.Object.Destroy(GameObject.Find("WaxResetMarker"));
        yield return null;
        Assert.That(GameObject.Find("WaxResetMarker"), Is.Null);
        Call(Manager, "Lose");
        Component previousManager = Manager;
        Call(previousManager, "Restart");
        Call(previousManager, "Restart");
        yield return null;
        yield return null;
        Assert.That(Manager, Is.Not.SameAs(previousManager));
        Assert.That(State, Is.EqualTo("Ready"));
        Assert.That(Time.timeScale, Is.Zero);
        flame = GameObject.Find("TestPlayer").GetComponent(Type.GetType("CandleFlame, Assembly-CSharp", true));
        Assert.That(Number(flame, "Value"), Is.EqualTo(100f));
        Assert.That(GameObject.Find("WaxResetMarker"), Is.Not.Null);
    }
}
