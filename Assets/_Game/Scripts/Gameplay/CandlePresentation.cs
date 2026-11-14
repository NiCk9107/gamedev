using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public sealed class CandlePresentation : MonoBehaviour
{
    CandleFlame flame; GameObject overlay; string mode; bool paused; float elapsed,previous,feedbackUntil;
    int initialWax; float startX,endX; TMP_Text feedback,progress; RawImage artwork; WaxPickup[] pickups; Image flameFill;
    public static bool ReducedMotion { get; private set; }
    T Child<T>(string name) where T:Component => GetComponentsInChildren<T>(true).First(x=>x.name==name);
    Button B(string name)=>Child<Button>(name);
    void Start()
    {
        flame=FindFirstObjectByType<CandleFlame>();if(flame==null)return;
        previous=flame.Value;startX=flame.transform.position.x;
        var altar=FindFirstObjectByType<ExitAltar>();endX=altar!=null?altar.transform.position.x:startX+380;
        pickups=FindObjectsByType<WaxPickup>(FindObjectsSortMode.None);initialWax=pickups.Length;
        flameFill=Child<Slider>("FlameSlider").fillRect.GetComponent<Image>();
        feedback=Child<TMP_Text>("PickupFeedback");progress=Child<TMP_Text>("JourneyProgress");artwork=Child<RawImage>("CryptArtwork");
        overlay=Child<RectTransform>("Overlay").gameObject;
        ReducedMotion=PlayerPrefs.GetInt("Candle.ReducedMotion",0)==1;
        AudioListener.volume=PlayerPrefs.GetFloat("Candle.Volume",.7f);
        B("HelpButton").onClick.AddListener(()=>Open("help"));
        B("SettingsButton").onClick.AddListener(()=>Open("settings"));
        B("ExitButton").onClick.AddListener(()=>Application.Quit());
        B("PauseButton").onClick.AddListener(()=>Open("pause"));
        B("CloseOverlayButton").onClick.AddListener(Close);
        B("MenuButton").onClick.AddListener(()=>{paused=false;GameManager.Instance.Restart();});
        B("VolumeButton").onClick.AddListener(()=>{AudioListener.volume=AudioListener.volume>=.99f?0:Mathf.Min(1,AudioListener.volume+.25f);PlayerPrefs.SetFloat("Candle.Volume",AudioListener.volume);RefreshSettings();});
        B("FullscreenButton").onClick.AddListener(()=>{Screen.fullScreen=!Screen.fullScreen;StartCoroutine(RefreshAfterFrame());});
        B("MotionButton").onClick.AddListener(()=>{ReducedMotion=!ReducedMotion;PlayerPrefs.SetInt("Candle.ReducedMotion",ReducedMotion?1:0);RefreshSettings();});
        GameManager.Instance.StateChanged+=StateChanged;
        CandleWorldArt.Create(flame);
        // Reproducible visual QA captures are explicitly opt-in command-line runs.
        var args=System.Environment.GetCommandLineArgs();int index=System.Array.IndexOf(args,"-candleCapture");
        if(index>=0 && index+1<args.Length) { Application.runInBackground=true; StartCoroutine(Capture(args[index+1])); }
    }
    IEnumerator RefreshAfterFrame(){yield return null;RefreshSettings();}
    public void Open(string page)
    {
        mode=page;paused=GameManager.Instance.State==GameState.Playing;if(paused)Time.timeScale=0;
        overlay.SetActive(true);Child<TMP_Text>("OverlayTitle").text=page=="help"?"КАК ИГРАТЬ":page=="pause"?"ОГОНЬ ПОДОЖДЁТ":"НАСТРОЙКИ";
        Child<TMP_Text>("OverlayBody").text=page=="help"?"A / D  или  ← / →   —   движение\nПРОБЕЛ   —   прыжок   •   удерживай для высокого прыжка\n\nСобирай золотой воск: он возвращает 25% пламени.\nИзбегай ветра и теней. Не падай в пропасть.\nДонеси живой огонь до алтаря в конце пути.":page=="pause"?"Переведи дыхание. Твоё пламя сейчас не убывает.":"";
        foreach(var key in new[]{"VolumeButton","FullscreenButton","MotionButton"})B(key).gameObject.SetActive(page=="settings");
        B("MenuButton").gameObject.SetActive(page=="pause");
        B("CloseOverlayButton").GetComponentInChildren<TMP_Text>().text=page=="pause"?"ПРОДОЛЖИТЬ":"НАЗАД";
        RefreshSettings();EventSystem.current?.SetSelectedGameObject(B("CloseOverlayButton").gameObject);
    }
    public void Close(){overlay.SetActive(false);if(paused && GameManager.Instance.State==GameState.Playing)Time.timeScale=1;paused=false;EventSystem.current?.SetSelectedGameObject(null);}
    void RefreshSettings()
    {
        B("VolumeButton").GetComponentInChildren<TMP_Text>().text="ЗВУК: "+Mathf.RoundToInt(AudioListener.volume*100)+"%";
        B("FullscreenButton").GetComponentInChildren<TMP_Text>().text=Screen.fullScreen?"ЭКРАН: ПОЛНЫЙ":"ЭКРАН: ОКНО";
        B("MotionButton").GetComponentInChildren<TMP_Text>().text=ReducedMotion?"АНИМАЦИЯ: МЕНЬШЕ":"АНИМАЦИЯ: ПОЛНАЯ";
    }
    void StateChanged(GameState state)
    {
        if(state==GameState.Won||state==GameState.Lost)
        {
            overlay.SetActive(false);paused=false;
            int wax=initialWax-pickups.Count(x=>x!=null);
            foreach(var text in GetComponentsInChildren<TMP_Text>(true).Where(x=>x.name=="RunStats"))
                text.text=$"Время: {Mathf.FloorToInt(elapsed/60):00}:{Mathf.FloorToInt(elapsed%60):00}    •    Воск: {wax} / {initialWax}";
        }
    }
    void Update()
    {
        if(flame==null||GameManager.Instance==null)return;
        if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {if(overlay.activeSelf)Close();else if(GameManager.Instance.State==GameState.Playing)Open("pause");}
        if(artwork!=null && artwork.texture!=null)
        {
            float target=(float)Screen.width/Screen.height,source=(float)artwork.texture.width/artwork.texture.height;
            artwork.uvRect=target>source?new Rect(0,(1-source/target)/2,1,source/target):new Rect((1-target/source)/2,0,target/source,1);
        }
        if(GameManager.Instance.State!=GameState.Playing)return;
        elapsed+=Time.deltaTime;
        int wax=initialWax-pickups.Count(x=>x!=null);
        float distance=Mathf.Clamp01((flame.transform.position.x-startX)/Mathf.Max(1,endX-startX));
        progress.text=$"Путь {Mathf.RoundToInt(distance*100)}%    •    Воск {wax} / {initialWax}";
        float change=flame.Value-previous;
        if(change>.5f || change < -5f)
        { feedback.text=change>0?"+ ВОСК  •  ПЛАМЯ ВОССТАНОВЛЕНО":"БЕРЕГИ ОГОНЬ";feedback.color=change>0?CandleMenuArt.Ivory:new Color(1,.45f,.3f);feedbackUntil=Time.unscaledTime+1.8f;}
        previous=flame.Value;
        if(Time.unscaledTime>feedbackUntil)feedback.text=flame.Normalized<.2f?"ПЛАМЯ УГАСАЕТ — ИЩИ ВОСК":"";
        flameFill.color=flame.Normalized<.25f?new Color(.92f,.3f,.16f):CandleMenuArt.Gold;
    }
    void OnDestroy(){if(GameManager.Instance!=null)GameManager.Instance.StateChanged-=StateChanged;if(paused)Time.timeScale=1;}
    IEnumerator Capture(string folder)
    {
        System.IO.Directory.CreateDirectory(folder);
        yield return new WaitForSecondsRealtime(2);ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"menu.png"));
        yield return new WaitForSecondsRealtime(1);Open("help");yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"help.png"));
        yield return new WaitForSecondsRealtime(.5f);Close();GameManager.Instance.StartGame();
        yield return new WaitForSecondsRealtime(.7f);Open("pause");overlay.SetActive(false);
        yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"game.png"));
        yield return new WaitForSecondsRealtime(.6f);overlay.SetActive(true);ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"pause.png"));
        yield return new WaitForSecondsRealtime(.6f);Close();flame.Hurt(100);
        yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"loss.png"));
        yield return new WaitForSecondsRealtime(1);Application.Quit();
    }
}



