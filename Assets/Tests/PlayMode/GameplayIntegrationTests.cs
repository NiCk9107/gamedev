using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameplayIntegrationTests
{
    private const string GameScene = "Assets/_Game/Scenes/Game.unity";
    private const string Prefabs = "Assets/_Game/Prefabs/GamePlay/";
    private Component flame;
    private Component manager;
    private Collider2D playerCollider;
    private Rigidbody2D body;
    private GameObject extra;

    private static Type Kind(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    private static Component Find(string name) => (Component)UnityEngine.Object.FindFirstObjectByType(Kind(name));
    private static void Call(Component component, string name, params object[] args)
        => component.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(component, args);
    private static float Value(Component component) => (float)component.GetType().GetProperty("Value").GetValue(component);
    private string State => manager.GetType().GetProperty("State").GetValue(manager).ToString();

    [UnitySetUp]
    public IEnumerator LoadGame()
    {
        yield return SceneManager.LoadSceneAsync(GameScene);
        yield return null;
        manager = Find("GameManager");
        flame = Find("CandleFlame");
        Assert.That(manager, Is.Not.Null, "Game.unity needs GameManager");
        Assert.That(flame, Is.Not.Null, "Player needs CandleFlame");
        flame.GetType().GetField("lossPerSecond", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(flame, 0f);
        var movement = flame.GetComponent(Kind("PlayerMovement2D")) as Behaviour;
        if (movement != null) movement.enabled = false;
        playerCollider = flame.GetComponent<Collider2D>();
        body = flame.GetComponent<Rigidbody2D>();
        body.simulated = false;
    }

    [UnityTearDown]
    public IEnumerator RemoveExtra()
    {
        if (extra != null) UnityEngine.Object.Destroy(extra);
        Time.timeScale = 1f;
        yield return null;
    }

    private Component Spawn(string prefab, string script)
    {
#if UNITY_EDITOR
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab");
        Assert.That(asset, Is.Not.Null);
        extra = UnityEngine.Object.Instantiate(asset, new Vector3(1000, 1000, 0), Quaternion.identity);
        var component = extra.GetComponent(Kind(script));
        Assert.That(component, Is.Not.Null, prefab + " prefab must contain " + script);
        return component;
#else
        throw new NotSupportedException("Asset verification runs in the Editor.");
#endif
    }

    [Test]
    public void MainSceneHasOneOwnerAndAllInteractions()
    {
        Assert.That(UnityEngine.Object.FindObjectsByType(Kind("GameManager"), FindObjectsSortMode.None).Length, Is.EqualTo(1));
        Assert.That(State, Is.EqualTo("Ready"));
        foreach (string type in new[] { "WaxPickup", "HazardZone", "ExitAltar", "CandleLight2D" })
            Assert.That(Find(type), Is.Not.Null, "Missing " + type);
    }

    [Test]
    public void WaxDoesNotDisappearBeforeStartOrAfterLoss()
    {
        Component wax = Spawn("WaxDrop", "WaxPickup");
        Call(wax, "OnTriggerEnter2D", playerCollider);
        Assert.That(Value(flame), Is.EqualTo(100f));
        Assert.That((bool)wax.GetType().GetField("collected", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(wax), Is.False);
        Call(manager, "StartGame");
        Call(flame, "Hurt", 100f);
        Call(wax, "OnTriggerEnter2D", playerCollider);
        Assert.That(Value(flame), Is.Zero);
        Assert.That((bool)wax.GetType().GetField("collected", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(wax), Is.False);
    }

    [Test]
    public void TwoPlayerCollidersCannotCollectWaxTwice()
    {
        Component wax = Spawn("WaxDrop", "WaxPickup");
        Call(manager, "StartGame");
        Call(flame, "Hurt", 80f);
        Call(wax, "OnTriggerEnter2D", playerCollider);
        Call(wax, "OnTriggerEnter2D", playerCollider);
        Assert.That(Value(flame), Is.EqualTo(45f));
    }

    [Test]
    public void HazardHitsOnceUntilAllPlayerCollidersExit()
    {
        Component hazard = Spawn("WindZone", "HazardZone");
        var second = flame.gameObject.AddComponent<CircleCollider2D>();
        Call(manager, "StartGame");
        Call(hazard, "OnTriggerEnter2D", playerCollider);
        Call(hazard, "OnTriggerEnter2D", second);
        Assert.That(Value(flame), Is.EqualTo(80f));
        Call(hazard, "OnTriggerExit2D", playerCollider);
        Call(hazard, "OnTriggerEnter2D", playerCollider);
        Assert.That(Value(flame), Is.EqualTo(80f));
        Call(hazard, "OnTriggerExit2D", playerCollider);
        Call(hazard, "OnTriggerExit2D", second);
        Call(hazard, "OnTriggerEnter2D", playerCollider);
        Assert.That(Value(flame), Is.EqualTo(60f));
    }

    [UnityTest]
    public IEnumerator RealPhysicsTriggerDealsOneWindHit()
    {
        Spawn("WindZone", "HazardZone");
        Call(manager, "StartGame");
        body.bodyType = RigidbodyType2D.Kinematic;
        body.position = new Vector2(995, 1000);
        body.simulated = true;
        Physics2D.SyncTransforms();
        yield return new WaitForFixedUpdate();
        body.position = new Vector2(1000, 1000);
        Physics2D.SyncTransforms();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Assert.That(Value(flame), Is.EqualTo(80f));
        yield return new WaitForSeconds(0.1f);
        Assert.That(Value(flame), Is.EqualTo(80f));
    }

    [Test]
    public void AltarAcceptsLastPositiveFlame()
    {
        Component altar = Spawn("ExitAltar", "ExitAltar");
        Call(manager, "StartGame");
        Call(flame, "Hurt", 99.99f);
        Call(altar, "OnTriggerEnter2D", playerCollider);
        Assert.That(State, Is.EqualTo("Won"));
    }

    [Test]
    public void AltarCannotOverrideLossAtZero()
    {
        Component altar = Spawn("ExitAltar", "ExitAltar");
        Call(manager, "StartGame");
        Call(flame, "Hurt", 100f);
        Call(altar, "OnTriggerEnter2D", playerCollider);
        Assert.That(State, Is.EqualTo("Lost"));
    }

    [UnityTest]
    public IEnumerator RealSceneSupportsWaxWinRestartAndLoss()
    {
        int waxBefore = UnityEngine.Object.FindObjectsByType(Kind("WaxPickup"), FindObjectsSortMode.None).Length;
        Component wax = Find("WaxPickup");
        Call(manager, "StartGame");
        Call(flame, "Hurt", 35f);
        Call(wax, "OnTriggerEnter2D", playerCollider);
        Assert.That(Value(flame), Is.EqualTo(90f));
        yield return null;
        Assert.That(UnityEngine.Object.FindObjectsByType(Kind("WaxPickup"), FindObjectsSortMode.None).Length,
            Is.EqualTo(waxBefore - 1));
        Call(Find("ExitAltar"), "OnTriggerEnter2D", playerCollider);
        Assert.That(State, Is.EqualTo("Won"));
        Call(manager, "Restart");
        yield return null;
        yield return null;
        manager = Find("GameManager");
        flame = Find("CandleFlame");
        Assert.That(State, Is.EqualTo("Ready"));
        Assert.That(Value(flame), Is.EqualTo(100f));
        Assert.That(Find("WaxPickup"), Is.Not.Null);
        Call(manager, "StartGame");
        Call(flame, "Hurt", 100f);
        Assert.That(State, Is.EqualTo("Lost"));
    }

    [Test]
    public void FullLevelUsesFunctionalWaxHazardsAndAltar()
    {
#if UNITY_EDITOR
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Levels/Level_01.prefab");
        extra = UnityEngine.Object.Instantiate(asset);
        foreach (var tr in extra.GetComponentsInChildren<Transform>(true))
        {
            string expected = tr.name.StartsWith("WaxDrop") ? "WaxPickup"
                : tr.name.StartsWith("WindZone") || tr.name.StartsWith("ShadowZone") ? "HazardZone"
                : tr.name.StartsWith("ExitAltar") ? "ExitAltar" : null;
            if (expected != null)
                Assert.That(tr.GetComponents(Kind(expected)).Length, Is.EqualTo(1), tr.name);
        }
        Assert.That(extra.GetComponentsInChildren(Kind("WaxPickup"), true).Length, Is.GreaterThan(0));
        Assert.That(extra.GetComponentsInChildren(Kind("ExitAltar"), true).Length, Is.EqualTo(1));
#endif
    }

    private static object Field(Component component, string name)
        => component.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(component);

    private static void Click(object button)
    {
        Assert.That(button, Is.Not.Null);
        var signal = button.GetType().GetProperty("onClick").GetValue(button);
        signal.GetType().GetMethod("Invoke").Invoke(signal, null);
    }

    [UnityTest]
    public IEnumerator MenuButtonStartsGameAndHudMatchesFlame()
    {
        Component ui = Find("CandleUIController");
        Assert.That(ui, Is.Not.Null);
        Assert.That(((GameObject)Field(ui, "startPanel")).activeSelf, Is.True);
        Assert.That(((GameObject)Field(ui, "hudPanel")).activeSelf, Is.False);
        Click(Field(ui, "playButton"));
        Assert.That(State, Is.EqualTo("Playing"));
        Assert.That(((GameObject)Field(ui, "startPanel")).activeSelf, Is.False);
        Assert.That(((GameObject)Field(ui, "hudPanel")).activeSelf, Is.True);
        Call(flame, "Hurt", 35f);
        yield return null;
        object slider = Field(ui, "flameSlider");
        object label = Field(ui, "flameLabel");
        Assert.That((float)slider.GetType().GetProperty("value").GetValue(slider), Is.EqualTo(0.65f).Within(0.001f));
        Assert.That((string)label.GetType().GetProperty("text").GetValue(label), Is.EqualTo("65%"));
    }

    [UnityTest]
    public IEnumerator BothResultButtonsRestartTheFullScene()
    {
        Component ui = Find("CandleUIController");
        Click(Field(ui, "playButton"));
        Call(manager, "Win");
        Assert.That(((GameObject)Field(ui, "winPanel")).activeSelf, Is.True);
        Assert.That(((GameObject)Field(ui, "hudPanel")).activeSelf, Is.False);
        Click(Field(ui, "winRestartButton"));
        yield return null;
        yield return null;
        manager = Find("GameManager");
        flame = Find("CandleFlame");
        ui = Find("CandleUIController");
        Assert.That(State, Is.EqualTo("Ready"));
        Assert.That(UnityEngine.Object.FindObjectsByType(Kind("WaxPickup"), FindObjectsSortMode.None).Length, Is.EqualTo(7));
        Click(Field(ui, "playButton"));
        Call(flame, "Hurt", 100f);
        Assert.That(((GameObject)Field(ui, "losePanel")).activeSelf, Is.True);
        Click(Field(ui, "loseRestartButton"));
        yield return null;
        yield return null;
        manager = Find("GameManager");
        flame = Find("CandleFlame");
        Assert.That(State, Is.EqualTo("Ready"));
        Assert.That(Value(flame), Is.EqualTo(100f));
    }

    [UnityTest]
    public IEnumerator LightShrinksWithFlameAndSwitchesOffAtZero()
    {
        Component controller = Find("CandleLight2D");
        object light = Field(controller, "light2d");
        var radius = light.GetType().GetProperty("pointLightOuterRadius");
        float full = (float)radius.GetValue(light);
        Call(manager, "StartGame");
        Call(flame, "Hurt", 80f);
        yield return null;
        Assert.That((float)radius.GetValue(light), Is.LessThan(full));
        Call(flame, "Hurt", 20f);
        yield return null;
        Assert.That(((Behaviour)light).enabled, Is.False);
    }

    [UnityTest]
    public IEnumerator FallingBelowLevelEndsRunWithoutWaitingForTimer()
    {
        Call(manager, "StartGame");
        flame.transform.position = new Vector3(0, -20, 0);
        yield return null;
        Assert.That(State, Is.EqualTo("Lost"));
        Assert.That(Value(flame), Is.Zero);
        var ui = Find("CandleUIController");
        Assert.That(((GameObject)Field(ui, "losePanel")).activeSelf, Is.True);
    }
}
