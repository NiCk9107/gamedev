using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CandleVisualBuild
{
    public static void Prepare()
    {
        foreach(string name in new[]{"MenuCrypt","LevelCrypt","Objects"})
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/_Game/Resources/CandleArt/"+name+".png");
            importer.textureType=TextureImporterType.Default;importer.isReadable=name=="Objects";importer.alphaIsTransparency=true;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.SaveAndReimport();
        }
        var shader=Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if(shader==null)throw new Exception("Missing sprite shader");
        if(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Resources/CandleArt/Unlit.mat")==null)
            AssetDatabase.CreateAsset(new Material(shader),"Assets/_Game/Resources/CandleArt/Unlit.mat");
        var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/Game.unity");
        var canvas=GameObject.Find("GameCanvas");if(canvas!=null)UnityEngine.Object.DestroyImmediate(canvas);
        CandleMenuArt.Build();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Debug.Log("CANDLE_VISUAL: scene prepared");
    }
    public static void Build()
    {
        var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-candleBuild");if(i<0)throw new Exception("Missing destination");
        string old=PlayerSettings.productName;PlayerSettings.productName="Последняя свеча";
        PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        try
        {
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Game/Scenes/Game.unity"},locationPathName=args[i+1],target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4});
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed");
        }
        finally{PlayerSettings.productName=old;}
    }
}
