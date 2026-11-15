using System.Collections.Generic;
using UnityEngine;

/// <summary>Cosmetic layer only: no colliders, trigger or level geometry changes.</summary>
public sealed class CandleWorldArt:MonoBehaviour
{
    static Material material;
    readonly List<Transform> motes=new List<Transform>();
    readonly List<SpriteRenderer> glows=new List<SpriteRenderer>();
    readonly List<Object> owned=new List<Object>();
    Transform[] backgrounds;Camera cam;CandleFlame player;Sprite glow;float tileWidth;
    public static void Create(CandleFlame flame)
    {var root=new GameObject("CandleWorldArt");var a=root.AddComponent<CandleWorldArt>();a.player=flame;a.Assemble();}
    SpriteRenderer Draw(string name,Sprite sprite,Vector2 position,Vector2 size,int order,Color color,Transform parent=null)
    {
        var o=new GameObject(name);o.transform.SetParent(parent!=null?parent:transform,false);o.transform.position=position;
        var r=o.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.color=color;r.sortingOrder=order;
        o.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);return r;
    }
    // Sprite slicing happens on the original transparent atlas, preserving the source asset.
    Sprite Slice(Texture2D atlas,int column,int row)
    {
        int w=atlas.width/2,h=atlas.height/2,x0=column*w,y0=row*h;
        var pixels=atlas.GetPixels(x0,y0,w,h);int left=w,right=0,bottom=h,top=0;
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(pixels[y*w+x].a>.08f){left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}
        var sprite=Sprite.Create(atlas,new Rect(x0+left,y0+bottom,right-left+1,top-bottom+1),new Vector2(.5f,.5f),100);owned.Add(sprite);return sprite;
    }
    void Assemble()
    {
        Physics2D.SyncTransforms();
        material=Resources.Load<Material>("CandleArt/Unlit");cam=Camera.main;
        var atlas=Resources.Load<Texture2D>("CandleArt/Objects");if(atlas==null||cam==null)return;
        var candle=Slice(atlas,0,1);var wax=Slice(atlas,1,1);var stone=Slice(atlas,0,0);var altar=Slice(atlas,1,0);
        var glowTex=new Texture2D(64,64,TextureFormat.RGBA32,false);var pixels=new Color[4096];
        for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/32;pixels[y*64+x]=new Color(1,1,1,Mathf.Pow(Mathf.Max(0,1-d),2)*.65f);}
        glowTex.SetPixels(pixels);glowTex.Apply();glow=Sprite.Create(glowTex,new Rect(0,0,64,64),new Vector2(.5f,.5f),64);
        owned.Add(glowTex);owned.Add(glow);
        var bg=Resources.Load<Texture2D>("CandleArt/LevelCrypt");var backgroundSprite=Sprite.Create(bg,new Rect(0,0,bg.width,bg.height),new Vector2(.5f,.5f),100);
        owned.Add(backgroundSprite);
        tileWidth=cam.orthographicSize*2.45f*bg.width/bg.height;
        backgrounds=new Transform[3];
        for(int i=0;i<3;i++)backgrounds[i]=Draw("DistantCrypt",backgroundSprite,Vector2.zero,new Vector2(tileWidth,cam.orthographicSize*2.45f),-100,new Color(.65f,.75f,.76f)).transform;
        var level=GameObject.Find("Level_01") ?? GameObject.Find("Level_02");
        if(level!=null)
        {
            foreach(var old in level.GetComponentsInChildren<SpriteRenderer>())
            {
                if(old.name.Contains("BackgroundPillar")||old.name.Contains("LandingEdge")){old.enabled=false;continue;}
                var collider=old.GetComponent<BoxCollider2D>();
                if(collider==null||collider.isTrigger)continue;
                if(old.name.Contains("Wall")){old.color=new Color(.08f,.12f,.12f);continue;}
                var b=collider.bounds;old.enabled=false;
                int sections=Mathf.Max(1,Mathf.CeilToInt(b.size.x/3.5f));float width=b.size.x/sections;
                for(int i=0;i<sections;i++)
                {
                    float height=Mathf.Clamp(b.size.y,.65f,1.4f);
                    var tile = Draw("CarvedStone",stone,new Vector2(b.min.x+width*(i+.5f),b.max.y-height/2),new Vector2(width+.015f,height),1,new Color(.78f,.85f,.81f));
                    if (old.TryGetComponent<TrialPlatform>(out _)) tile.transform.SetParent(old.transform, true);
                }
            }
        }
        foreach(var original in player.GetComponentsInChildren<SpriteRenderer>())original.enabled=false;
        var bounds=player.GetComponent<Collider2D>().bounds;
        var body=Draw("IvoryCandle",candle,new Vector2(bounds.center.x,bounds.min.y+bounds.size.y*.6f),new Vector2(bounds.size.y*.47f,bounds.size.y*1.2f),10,Color.white);
        body.transform.SetParent(player.transform,true);
        var halo=Draw("LivingFlameGlow",glow,new Vector2(bounds.center.x,bounds.max.y),Vector2.one*3,8,new Color(1,.62f,.2f,.7f));halo.transform.SetParent(player.transform,true);glows.Add(halo);
        foreach(var pickup in FindObjectsByType<WaxPickup>(FindObjectsSortMode.None))
        {
            foreach(var r in pickup.GetComponentsInChildren<SpriteRenderer>())r.enabled=false;
            var s=Draw("AmberWax",wax,pickup.transform.position,new Vector2(.52f,.9f),9,Color.white);s.transform.SetParent(pickup.transform,true);
            var h=Draw("WaxAura",glow,pickup.transform.position,Vector2.one*2.4f,7,new Color(1,.65f,.18f,.7f));h.transform.SetParent(pickup.transform,true);glows.Add(h);
        }
        foreach(var exit in FindObjectsByType<ExitAltar>(FindObjectsSortMode.None))
        {
            foreach(var r in exit.GetComponentsInChildren<SpriteRenderer>())r.enabled=false;
            var b=exit.GetComponent<Collider2D>().bounds;
            var s=Draw("SanctuaryAltar",altar,b.center,new Vector2(2.5f,2.5f),6,Color.white);s.transform.SetParent(exit.transform,true);
            var h=Draw("SanctuaryGlow",glow,b.center,Vector2.one*5,5,new Color(1,.65f,.2f,.8f));glows.Add(h);
        }
        foreach(var hazard in FindObjectsByType<HazardZone>(FindObjectsSortMode.None))
        {
            bool wind=hazard.name.Contains("Wind");var b=hazard.GetComponent<Collider2D>().bounds;
            foreach(var r in hazard.GetComponentsInChildren<SpriteRenderer>())r.color=wind?new Color(.4f,.8f,.88f,.12f):new Color(.27f,.12f,.4f,.35f);
            for(int i=0;i<7;i++)
            {
                var line=new GameObject(wind?"WindRibbon":"ShadowWisp");line.transform.SetParent(transform,false);
                var lr=line.AddComponent<LineRenderer>();lr.sharedMaterial=material;lr.useWorldSpace=true;lr.positionCount=16;lr.sortingOrder=4;lr.widthMultiplier=wind?.025f:.055f;
                lr.startColor=lr.endColor=wind?new Color(.56f,.84f,.85f,.5f):new Color(.6f,.3f,.7f,.4f);
                for(int j=0;j<16;j++){float x=b.min.x+b.size.x*j/15;lr.SetPosition(j,new Vector3(x,b.min.y+b.size.y*(i+.5f)/7+Mathf.Sin(j*.6f+i)*.12f,0));}
            }
        }
        var random=new System.Random(42);
        for(int i=0;i<35;i++)
        {
            var m=Draw("Ember",glow,Vector2.zero,Vector2.one*(.025f+(float)random.NextDouble()*.06f),15,new Color(1,.7f,.3f,.65f));motes.Add(m.transform);
        }
    }
    void LateUpdate()
    {
        if(cam==null||backgrounds==null)return;
        float x=cam.transform.position.x;float phase=x*.12f;float center=Mathf.Floor((x-phase)/tileWidth)*tileWidth+phase;
        for(int i=0;i<3;i++)backgrounds[i].position=new Vector3(center+(i-1)*tileWidth,cam.transform.position.y+.9f,10);
        float t=CandlePresentation.ReducedMotion?0:Time.time;
        for(int i=0;i<motes.Count;i++)motes[i].position=new Vector3(x+Mathf.Repeat(i*3.17f+t*.13f,22)-11,cam.transform.position.y+Mathf.Repeat(i*1.71f+t*(.13f+i%4*.03f),12)-6,0);
        foreach(var h in glows)if(h!=null){var c=h.color;c.a=.55f+(CandlePresentation.ReducedMotion?0:Mathf.Sin(t*3+h.transform.position.x)*.12f);h.color=c;}
    }
    void OnDestroy(){foreach(var asset in owned)if(asset!=null)Destroy(asset);}
}

