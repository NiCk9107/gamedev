using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PresentationTests
{
    Component manager,presentation,flame;
    static Type Kind(string n)=>Type.GetType(n+", Assembly-CSharp",true);
    static Component Find(string n)=>(Component)UnityEngine.Object.FindFirstObjectByType(Kind(n));
    static void Call(Component c,string method,params object[] args)=>c.GetType().GetMethod(method).Invoke(c,args);
    [UnitySetUp] public IEnumerator Setup()
    {
        yield return SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Game.unity");yield return null;
        manager=Find("GameManager");presentation=Find("CandlePresentation");flame=Find("CandleFlame");
        flame.GetComponent<Rigidbody2D>().simulated=false;
    }
    [UnityTest] public IEnumerator PauseFreezesFlameAndResumeRestoresTime()
    {
        Call(manager,"StartGame");Call(presentation,"Open","pause");
        float before=(float)flame.GetType().GetProperty("Value").GetValue(flame);
        Assert.That(Time.timeScale,Is.Zero);yield return new WaitForSecondsRealtime(.15f);
        Assert.That((float)flame.GetType().GetProperty("Value").GetValue(flame),Is.EqualTo(before));
        Call(presentation,"Close");Assert.That(Time.timeScale,Is.EqualTo(1));
    }
    [Test] public void HelpAndSettingsDoNotStartGame()
    {
        foreach(string page in new[]{"help","settings"})
        {Call(presentation,"Open",page);Call(presentation,"Close");Assert.That(Time.timeScale,Is.Zero);Assert.That(manager.GetType().GetProperty("State").GetValue(manager).ToString(),Is.EqualTo("Ready"));}
    }
    [Test] public void CosmeticGeometryDoesNotAddColliders()
    {
        var art=GameObject.Find("CandleWorldArt");Assert.That(art,Is.Not.Null);
        Assert.That(art.GetComponentsInChildren<Collider2D>().Length,Is.Zero);
        Assert.That(art.GetComponentsInChildren<SpriteRenderer>().Length,Is.GreaterThan(50));
    }
    [Test] public void StonePlatformsHaveVisibleGeometryBeforeFirstPhysicsTick()
    {
        var art=GameObject.Find("CandleWorldArt");
        var stones=art.GetComponentsInChildren<SpriteRenderer>().Where(x=>x.name=="CarvedStone").ToArray();
        Assert.That(stones.Length,Is.GreaterThan(60));
        foreach(var stone in stones){Assert.That(stone.bounds.size.x,Is.GreaterThan(.2f));Assert.That(stone.bounds.size.y,Is.GreaterThan(.2f));}
    }
    [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;yield return null;}
}
