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

public class LevelRouteTests
{
    private static Type Kind(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    private static Component Find(string name) => (Component)UnityEngine.Object.FindFirstObjectByType(Kind(name));

    [UnityTearDown]
    public IEnumerator RestoreClock()
    {
        Time.timeScale = 1f;
        yield return null;
    }

    [UnityTest]
    public IEnumerator FullLevelFlameBudgetAllowsThreeHazardHits()
    {
#if UNITY_EDITOR
        yield return SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Game.unity");
        yield return null;
        Component flame = Find("CandleFlame");
        Component manager = Find("GameManager");
        var movement = flame.GetComponent(Kind("PlayerMovement2D"));
        var body = flame.GetComponent<Rigidbody2D>();
        ((Behaviour)movement).enabled = false;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.linearVelocity = Vector2.zero;

        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root == flame.transform.root.gameObject || root == manager.transform.root.gameObject
                || root.GetComponent<Camera>() != null || root.name == "Global Light 2D") continue;
            UnityEngine.Object.Destroy(root);
        }
        var level = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/_Game/Prefabs/Levels/Level_01.prefab"));
        yield return null;
        var platforms = level.GetComponentsInChildren<BoxCollider2D>()
            .Where(c => !c.isTrigger && c.name.Contains("Platform_")).OrderBy(c => c.bounds.center.x).ToArray();
        Assert.That(platforms.Length, Is.EqualTo(24));

        // Replay a route with three mistakes (wind, shadow, wind). This checks
        // flame balance and actual triggers; jump feasibility is level QA.
        var hazards = level.GetComponentsInChildren(Kind("HazardZone"))
            .Cast<Component>().OrderBy(c => c.transform.position.x).ToArray();
        Assert.That(hazards.Length, Is.EqualTo(5));
        foreach (var hazard in hazards.Skip(3))
            foreach (var collider in hazard.GetComponents<Collider2D>()) collider.enabled = false;

        float speed = (float)movement.GetType().GetField("moveSpeed", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(movement);
        float halfHeight = flame.GetComponent<Collider2D>().bounds.extents.y;
        body.position = new Vector2(0, platforms[0].bounds.max.y + halfHeight + 0.01f);
        manager.GetType().GetMethod("StartGame").Invoke(manager, null);
        Time.timeScale = 4f;
        float began = Time.time;
        float deadline = Time.realtimeSinceStartup + 60f;
        while (manager.GetType().GetProperty("State").GetValue(manager).ToString() == "Playing"
               && Time.time - began < 120f && Time.realtimeSinceStartup < deadline)
        {
            float x = (Time.time - began) * speed;
            var platform = platforms.OrderBy(c => Mathf.Abs(c.bounds.center.x - x)).First();
            body.position = new Vector2(x, platform.bounds.max.y + halfHeight + 0.01f);
            Physics2D.SyncTransforms();
            yield return null;
        }
        string state = manager.GetType().GetProperty("State").GetValue(manager).ToString();
        float value = (float)flame.GetType().GetProperty("Value").GetValue(flame);
        float duration = Time.time - began;
        int waxCollected = 7 - level.GetComponentsInChildren(Kind("WaxPickup")).Length;
        Debug.Log($"YIMAN_BALANCE duration={duration:F2}; flame={value:F2}; waxCollected={waxCollected}; state={state}; x={body.position.x:F2}");
        Assert.That(state, Is.EqualTo("Won"));
        Assert.That(duration, Is.InRange(60f, 120f));
        Assert.That(value, Is.GreaterThan(0f));
        Assert.That(waxCollected, Is.EqualTo(7));
#else
        Assert.Ignore("The full prefab balance replay runs in the Editor.");
        yield break;
#endif
    }
}
