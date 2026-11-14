using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Builds the authored, resolution-independent presentation hierarchy.</summary>
public static class CandleMenuArt
{
    public static readonly Color Gold = new Color(.77f,.56f,.30f);
    public static readonly Color Ivory = new Color(.96f,.89f,.73f);
    static TMP_FontAsset font;
    public static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        var o = new GameObject(name, typeof(RectTransform)); o.transform.SetParent(parent,false);
        var r=(RectTransform)o.transform; r.sizeDelta=size; r.anchoredPosition=pos; return r;
    }
    public static void Stretch(RectTransform r) { r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.offsetMin=r.offsetMax=Vector2.zero; }
    public static Image Box(string name,Transform p,Vector2 size,Vector2 pos,Color c)
    { var r=Rect(name,p,size,pos); var i=r.gameObject.AddComponent<Image>(); i.color=c; i.raycastTarget=false; return i; }
    public static TMP_Text Text(string name,Transform p,string value,int size,Vector2 area,Vector2 pos,Color color)
    {
        if(font==null) font=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        var t=Rect(name,p,area,pos).gameObject.AddComponent<TextMeshProUGUI>(); t.font=font;
        t.text=value; t.fontSize=size; t.color=color; t.alignment=TextAlignmentOptions.Center; t.raycastTarget=false;
        t.textWrappingMode=TextWrappingModes.Normal; return t;
    }
    public static Button Button(string name,Transform p,string label,Vector2 pos,bool primary=false)
    {
        var rim=Box(name,p,new Vector2(370,68),pos,Gold);
        rim.raycastTarget=true;
        var fill=Box("Surface",rim.transform,new Vector2(366,64),Vector2.zero,primary?new Color(.58f,.34f,.14f,.96f):new Color(.035f,.055f,.06f,.94f));
        Box("TopHighlight",fill.transform,new Vector2(350,1),new Vector2(0,29),new Color(1,.84f,.55f,.45f));
        var labelText=Text("Label",rim.transform,label,26,new Vector2(340,60),Vector2.zero,Ivory); labelText.characterSpacing=3;
        var b=rim.gameObject.AddComponent<Button>(); b.targetGraphic=fill;
        var colors=b.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color(1.4f,1.3f,1.15f); colors.pressedColor=new Color(.7f,.7f,.7f); colors.selectedColor=colors.highlightedColor; b.colors=colors;
        rim.gameObject.AddComponent<CandleButtonMotion>(); return b;
    }
    public static GameObject Panel(string name,Transform p,Color color)
    { var i=Box(name,p,Vector2.zero,Vector2.zero,color); Stretch(i.rectTransform); i.raycastTarget=true; return i.gameObject; }
    static void Rule(Transform p,float y,float width=480)
    {
        Box("GoldRule",p,new Vector2(width,1),new Vector2(0,y),new Color(.77f,.56f,.3f,.55f));
        var d=Box("Diamond",p,new Vector2(7,7),new Vector2(0,y),Gold);d.transform.localRotation=Quaternion.Euler(0,0,45);
    }
    public static GameObject Build()
    {
        var go=new GameObject("GameCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=go.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=.5f;
        var menu=Panel("StartPanel",go.transform,Color.black);
        var art=Rect("CryptArtwork",menu.transform,Vector2.zero,Vector2.zero);Stretch(art);
        var raw=art.gameObject.AddComponent<RawImage>();raw.texture=Resources.Load<Texture2D>("CandleArt/MenuCrypt");raw.raycastTarget=false;
        // Crop-to-fill is maintained by the runtime component at every aspect ratio.
        var shade=Box("Shade",menu.transform,new Vector2(650,1080),Vector2.zero,new Color(.008f,.023f,.027f,.52f));
        Text("Edition",menu.transform,"АТМОСФЕРНОЕ ПРИКЛЮЧЕНИЕ",18,new Vector2(850,40),new Vector2(0,415),Gold).characterSpacing=7;
        Rule(menu.transform,370,350);
        var title=Text("GameTitle",menu.transform,"ПОСЛЕДНЯЯ\nСВЕЧА",78,new Vector2(950,220),new Vector2(0,235),Ivory);title.characterSpacing=10;title.lineSpacing=-8;
        Text("Tagline",menu.transform,"Пока горит пламя — остаётся надежда.",23,new Vector2(850,50),new Vector2(0,83),new Color(.72f,.75f,.71f));
        Rule(menu.transform,35,350);
        Button("PlayButton",menu.transform,"НАЧАТЬ ПУТЬ",new Vector2(0,-60),true);
        Button("HelpButton",menu.transform,"КАК ИГРАТЬ",new Vector2(0,-150));
        Button("SettingsButton",menu.transform,"НАСТРОЙКИ",new Vector2(0,-240));
        Button("ExitButton",menu.transform,"ВЫЙТИ",new Vector2(0,-330));
        Text("Footer",menu.transform,"СОХРАНИ ОГОНЬ  /  НАЙДИ АЛТАРЬ",17,new Vector2(900,40),new Vector2(0,-465),Gold).characterSpacing=5;
        var hud=Panel("HUD",go.transform,Color.clear);hud.GetComponent<Image>().raycastTarget=false;
        var gauge=Box("FlameCard",hud.transform,new Vector2(420,116),Vector2.zero,new Color(.018f,.035f,.04f,.91f));
        gauge.rectTransform.anchorMin=gauge.rectTransform.anchorMax=new Vector2(0,1);gauge.rectTransform.pivot=new Vector2(0,1);gauge.rectTransform.anchoredPosition=new Vector2(38,-34);
        Text("FlameCaption",gauge.transform,"ПЛАМЯ ЖИЗНИ",18,new Vector2(280,30),new Vector2(-35,30),Gold).characterSpacing=3;
        Text("FlameLabel",gauge.transform,"100%",30,new Vector2(95,40),new Vector2(145,27),Ivory);
        var track=Box("FlameSlider",gauge.transform,new Vector2(360,12),new Vector2(0,-16),new Color(.1f,.14f,.15f));
        var fill=Box("Fill",track.transform,Vector2.zero,Vector2.zero,Gold);Stretch(fill.rectTransform);
        var slider=track.gameObject.AddComponent<Slider>();slider.fillRect=fill.rectTransform;slider.minValue=0;slider.maxValue=1;slider.value=1;slider.interactable=false;slider.transition=Selectable.Transition.None;
        Text("FlameHint",gauge.transform,"Воск возвращает 25% огня",16,new Vector2(360,25),new Vector2(0,-42),new Color(.63f,.7f,.69f));
        var objective=Text("Objective",hud.transform,"ДОЙДИ ДО АЛТАРЯ",22,new Vector2(640,65),Vector2.zero,Ivory);
        objective.rectTransform.anchorMin=objective.rectTransform.anchorMax=new Vector2(.5f,1);objective.rectTransform.anchoredPosition=new Vector2(0,-58);
        var progress=Text("JourneyProgress",hud.transform,"Путь 0%   •   Воск 0 / 7",18,new Vector2(640,40),Vector2.zero,Gold);
        progress.rectTransform.anchorMin=progress.rectTransform.anchorMax=new Vector2(.5f,1);progress.rectTransform.anchoredPosition=new Vector2(0,-105);
        var pause=Button("PauseButton",hud.transform,"ПАУЗА  /  ESC",Vector2.zero);
        var pr=(RectTransform)pause.transform;pr.anchorMin=pr.anchorMax=new Vector2(1,1);pr.anchoredPosition=new Vector2(-180,-65);pr.sizeDelta=new Vector2(280,60);
        pause.transform.Find("Surface").GetComponent<RectTransform>().sizeDelta=new Vector2(276,56);
        var hint=Text("ControlsHint",hud.transform,"A / D — движение     ПРОБЕЛ — прыжок     ESC — пауза",19,new Vector2(1100,44),Vector2.zero,new Color(.82f,.85f,.79f));
        hint.rectTransform.anchorMin=hint.rectTransform.anchorMax=new Vector2(.5f,0);hint.rectTransform.anchoredPosition=new Vector2(0,40);
        Text("PickupFeedback",hud.transform,"",32,new Vector2(900,70),new Vector2(0,220),Gold);
        Result(go.transform,"WinPanel","ОГОНЬ СОХРАНЁН","Ты принёс свет туда, где его больше не ждали.","WinRestartButton","ПРОЙТИ ЕЩЁ РАЗ");
        Result(go.transform,"LosePanel","ПЛАМЯ ПОГАСЛО","Тьма победила лишь на этот раз. Попробуй снова.","LoseRestartButton","ЕЩЁ ОДНА ПОПЫТКА");
        var modal=Panel("Overlay",go.transform,new Color(.006f,.016f,.02f,1f));
        Rule(modal.transform,330,700);
        Text("OverlayTitle",modal.transform,"КАК ИГРАТЬ",46,new Vector2(1000,90),new Vector2(0,250),Ivory).characterSpacing=5;
        Text("OverlayBody",modal.transform,"",26,new Vector2(1050,340),new Vector2(0,25),new Color(.78f,.82f,.77f));
        Button("VolumeButton",modal.transform,"",new Vector2(0,100));
        Button("FullscreenButton",modal.transform,"",new Vector2(0,0));
        Button("MotionButton",modal.transform,"",new Vector2(0,-100));
        Button("MenuButton",modal.transform,"В ГЛАВНОЕ МЕНЮ",new Vector2(0,-200));
        Button("CloseOverlayButton",modal.transform,"НАЗАД",new Vector2(0,-310),true);
        modal.SetActive(false);hud.SetActive(false);
        go.AddComponent<CandleUIController>();go.AddComponent<CandlePresentation>();
        return go;
    }
    static void Result(Transform root,string name,string title,string subtitle,string button,string label)
    {
        var p=Panel(name,root,new Color(.008f,.024f,.028f,1f));
        Rule(p.transform,210,650);
        Text("ResultTitle",p.transform,title,55,new Vector2(1300,100),new Vector2(0,110),Ivory).characterSpacing=5;
        Text("ResultSubtitle",p.transform,subtitle,25,new Vector2(1200,80),new Vector2(0,15),new Color(.68f,.75f,.71f));
        Text("RunStats",p.transform,"",22,new Vector2(1000,80),new Vector2(0,-80),Gold);
        Button(button,p.transform,label,new Vector2(0,-195),true);
        Rule(p.transform,-300,650);p.SetActive(false);
    }
}

