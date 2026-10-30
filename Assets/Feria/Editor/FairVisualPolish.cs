using System;
using System.Collections.Generic;
using System.IO;
using Feria;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class FairVisualPolish
{
    const string Art="Assets/Feria/Art/Original";
    static Transform root;
    static Material teal,gold,cream,wood,green;
    static Color C(string h){ColorUtility.TryParseHtmlString("#"+h,out var c);return c;}
    [MenuItem("Feria/Pulir ambiente v0.3")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Sal de Play antes de editar la escena.");
        var game=UnityEngine.Object.FindFirstObjectByType<FairGame>();if(!game)throw new Exception("Abre PrimeraManana.");
        Directory.CreateDirectory("Assets/Feria/Definitions");AssetDatabase.Refresh();
        var definition=AssetDatabase.LoadAssetAtPath<FairZoneDefinition>("Assets/Feria/Definitions/RinconCanicas.asset");
        if(!definition){definition=ScriptableObject.CreateInstance<FairZoneDefinition>();AssetDatabase.CreateAsset(definition,"Assets/Feria/Definitions/RinconCanicas.asset");}game.definition=definition;
        definition.marbleWaypoints=new Vector3[12];for(int i=0;i<12;i++){int row=i/2;float x=i%2==0?.95f:-.95f;if(row%2==1)x=-x;definition.marbleWaypoints[i]=new Vector3(x,1.68f-row*.027f,5.8f-row*.24f);}
        var old=GameObject.Find("Ambiente original v0.3");if(old)UnityEngine.Object.DestroyImmediate(old);
        root=new GameObject("Ambiente original v0.3").transform;
        teal=Mat("Verde profundo",C("326D72"));gold=Mat("Latón suave",C("D7AA54"));cream=Mat("Marfil suave",C("EEE0B8"));wood=Mat("Madera cálida",C("95704F"));green=Mat("Hojas de salvia",C("6B9582"));
        var skyPath=Art+"/Materials/Cielo de amanecer.mat";var sky=AssetDatabase.LoadAssetAtPath<Material>(skyPath);
        if(!sky){sky=new Material(Shader.Find("Feria/Amanecer artesanal"));AssetDatabase.CreateAsset(sky,skyPath);}
        RenderSettings.skybox=sky;
        var camera=game.player.view;camera.clearFlags=CameraClearFlags.Skybox;camera.fieldOfView=68;camera.transform.localRotation=Quaternion.Euler(20,0,0);
        RenderSettings.fogColor=C("D6C1AA");RenderSettings.fogStartDistance=22;RenderSettings.fogEndDistance=65;
        RenderSettings.ambientSkyColor=C("BBCED4");RenderSettings.ambientEquatorColor=C("DCCCB0");RenderSettings.ambientGroundColor=C("8F8B7C");
        var sun=GameObject.Find("Sol de amanecer").GetComponent<Light>();sun.intensity=1.25f;sun.color=C("FFE2BD");sun.transform.rotation=Quaternion.Euler(22,-40,0);
        var volume=UnityEngine.Object.FindFirstObjectByType<Volume>();
        if(volume&&volume.sharedProfile.TryGet<Bloom>(out var bloom)){bloom.intensity.Override(.18f);bloom.threshold.Override(1.2f);EditorUtility.SetDirty(volume.sharedProfile);}
        // Rebuild only a named decoration root; gameplay objects and save indices stay intact.
        var oldDecor=GameObject.Find("Arte original v0.2");if(oldDecor)foreach(Transform child in oldDecor.transform)if(child.name=="Moldura superior"||child.name=="Marco del rótulo")child.gameObject.SetActive(false);
        foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if(t.name=="Puesto"&&Mathf.Abs(t.position.x)<.1f){var board=t.Find("Rótulo fondo");if(board)board.gameObject.SetActive(false);}
        var plaque=Shape("Rótulo arqueado",Arch(),teal);plaque.transform.position=new Vector3(0,2.64f,3.79f);
        var outline=new List<Vector3>{new Vector3(-3,0,-.03f),new Vector3(3,0,-.03f),new Vector3(3,.62f,-.03f)};
        for(int i=0;i<=24;i++){float x=3-i*.25f;outline.Add(new Vector3(x,.62f+.48f*Mathf.Sin((3-x)/6*Mathf.PI),-.03f));}
        outline.Add(new Vector3(-3,0,-.03f));Rail("Filete del arco",outline,gold,.045f,plaque.transform);
        var title=GameObject.Find("Rótulo").GetComponent<TextMeshPro>();title.transform.position=new Vector3(0,3.15f,3.69f);title.rectTransform.sizeDelta=new Vector2(4.65f,.68f);title.fontSize=3.2f;title.text="EL RINCÓN\nDE LAS CANICAS";
        // Lathed capitals and engraved side columns lend the hero stall a recognisable silhouette.
        for(int s=-1;s<=1;s+=2)
        {
            Box("Zapata de columna",new Vector3(s*2.85f,.16f,3.94f),new Vector3(.35f,.32f,.34f),teal);
            Box("Capitel",new Vector3(s*2.85f,2.54f,3.94f),new Vector3(.38f,.13f,.34f),gold);
            Box("Estría",new Vector3(s*2.85f,1.65f,3.835f),new Vector3(.035f,1.56f,.025f),cream);
            var rosette=Shape("Remate solar",Disc(),gold);rosette.transform.position=new Vector3(s*2.82f,3.19f,3.70f);rosette.transform.localScale=Vector3.one*.21f;
        }
        // Paper lanterns, framed planters and distant trees are static set dressing.
        var glow=Mat("Papel iluminado",C("FFDEA0"));glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",C("FFDEA0")*.8f);
        for(int s=-1;s<=1;s+=2)
        {
            for(int j=0;j<2;j++)
            {
                float z=-4+j*9;
                Box("Farola de madera",new Vector3(s*6.35f,1.4f,z),new Vector3(.10f,2.8f,.10f),wood);
                Box("Brazo de farola",new Vector3(s*6.12f,2.72f,z),new Vector3(.56f,.08f,.08f),wood);
                var lantern=Shape("Farol de papel",Lantern(),glow);lantern.transform.position=new Vector3(s*5.95f,2.4f,z);lantern.transform.localScale=new Vector3(.34f,.43f,.34f);
                Box("Tapa de farol",new Vector3(s*5.95f,2.64f,z),new Vector3(.28f,.045f,.28f),gold);
                var light=new GameObject("Luz de farol").AddComponent<Light>();light.transform.SetParent(root);light.transform.position=lantern.transform.position;light.type=LightType.Point;light.color=C("FFD195");light.range=3;light.intensity=.55f;light.shadows=LightShadows.None;
            }
            Planter(new Vector3(s*5,0,5.25f));
        }
        var grass=Mat("Pradera lejana",C("9BAE91"));Box("Pradera",new Vector3(0,-.34f,21),new Vector3(85,.2f,65),grass);
        var distant=Mat("Follaje lejano",C("92A99A"));
        for(int i=0;i<15;i++)
        {
            float x=-28+i*4;float z=16+(i%3)*5;float height=2.8f+(i%4)*.45f;
            Box("Tronco lejano",new Vector3(x,height*.4f,z),new Vector3(.25f,height*.8f,.25f),wood);
            var tree=Shape("Copa tallada",Foliage(),i%2==0?green:distant);tree.transform.position=new Vector3(x,height,z);tree.transform.localScale=new Vector3(2.1f,height*.65f,1.7f);
        }
        var mutedCoral=Mat("Lona lejana coral",C("B3897D"));
        for(int i=0;i<5;i++)
        {
            float x=-15+i*7;
            Box("Caseta lejana",new Vector3(x,.9f,21),new Vector3(3.5f,1.8f,2.5f),i%2==0?cream:teal);
            var roof=Shape("Tejado de feria",Roof(),i%2==0?mutedCoral:teal);roof.transform.position=new Vector3(x,1.8f,21);roof.transform.localScale=new Vector3(3.9f,1.2f,2.9f);
        }
        // A real depth cue at the entrance and a painted border frame the playable court.
        for(int s=-1;s<=1;s+=2)Box("Borde de piedra",new Vector3(s*6.65f,.04f,0),new Vector3(.25f,.14f,17.8f),cream);
        Box("Umbral de entrada",new Vector3(0,.015f,-8.65f),new Vector3(13,.045f,.25f),teal);
        PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Feria/Prefabs/AmbienteFeria.prefab");
        PlayerSettings.bundleVersion="0.3.0";EditorUtility.SetDirty(game);EditorUtility.SetDirty(definition);EditorUtility.SetDirty(sky);
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();
    }
    static void Planter(Vector3 p)
    {
        Box("Jardinera",p+Vector3.up*.26f,new Vector3(1.1f,.5f,.65f),teal);
        Box("Tierra",p+Vector3.up*.52f,new Vector3(.92f,.025f,.48f),wood);
        for(int i=0;i<5;i++){var leaf=Shape("Hojas talladas",Foliage(),green);leaf.transform.position=p+new Vector3(-.36f+i*.18f,.76f,Mathf.Sin(i)*.12f);leaf.transform.localScale=new Vector3(.22f,.35f,.20f);}
    }
    static GameObject Box(string name,Vector3 p,Vector3 size,Material m){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.transform.SetParent(root,false);go.transform.position=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=m;return go;}
    static Material Mat(string name,Color color){string path=Art+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.SetFloat("_Smoothness",.16f);m.enableInstancing=true;return m;}
    static GameObject Shape(string name,Mesh mesh,Material material){var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=material;return go;}
    static void Rail(string name,List<Vector3> points,Material material,float width,Transform parent){var line=new GameObject(name).AddComponent<LineRenderer>();line.transform.SetParent(parent,false);line.useWorldSpace=false;line.positionCount=points.Count;line.SetPositions(points.ToArray());line.widthMultiplier=width;line.sharedMaterial=material;}
    static Mesh Store(string name,List<Vector3> v,List<int> t){string path=Art+"/Meshes/"+name+".asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!m){m=new Mesh{name=name};AssetDatabase.CreateAsset(m,path);}else m.Clear();m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();EditorUtility.SetDirty(m);return m;}
    static void Tri(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c){int n=v.Count;v.AddRange(new[]{a,b,c});t.AddRange(new[]{n,n+1,n+2});}
    static Mesh Arch(){var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<24;i++){float x=-3+i*.25f,xx=x+.25f;float y=.62f+.48f*Mathf.Sin((x+3)/6*Mathf.PI),yy=.62f+.48f*Mathf.Sin((xx+3)/6*Mathf.PI);Tri(v,t,new Vector3(x,0,0),new Vector3(x,y,0),new Vector3(xx,yy,0));Tri(v,t,new Vector3(x,0,0),new Vector3(xx,yy,0),new Vector3(xx,0,0));}return Store("Rótulo arqueado",v,t);}
    static Mesh Disc(){var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<12;i++){float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;Tri(v,t,Vector3.zero,new Vector3(Mathf.Cos(b),Mathf.Sin(b),0),new Vector3(Mathf.Cos(a),Mathf.Sin(a),0));}return Store("Roseta solar",v,t);}
    static Mesh Lantern(){var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<8;i++){float a=i*Mathf.PI/4,b=(i+1)*Mathf.PI/4;var aa=new Vector3(Mathf.Sin(a)*.5f,-.5f,Mathf.Cos(a)*.5f);var bb=new Vector3(Mathf.Sin(b)*.5f,-.5f,Mathf.Cos(b)*.5f);Tri(v,t,aa,bb+Vector3.up,aa+Vector3.up);Tri(v,t,aa,bb,bb+Vector3.up);}return Store("Farol octogonal",v,t);}
    static Mesh Foliage()
    {
        var v=new List<Vector3>();var t=new List<int>();
        float[] ys={-.65f,-.3f,.2f,.67f,1f};float[] radii={.2f,.8f,1f,.76f,.2f};
        for(int ring=0;ring<4;ring++)for(int i=0;i<9;i++)
        {
            float a=i*Mathf.PI*2/9,b=(i+1)*Mathf.PI*2/9;
            var aa=new Vector3(Mathf.Sin(a)*radii[ring],ys[ring],Mathf.Cos(a)*radii[ring]);
            var bb=new Vector3(Mathf.Sin(b)*radii[ring],ys[ring],Mathf.Cos(b)*radii[ring]);
            var cc=new Vector3(Mathf.Sin(b)*radii[ring+1],ys[ring+1],Mathf.Cos(b)*radii[ring+1]);
            var dd=new Vector3(Mathf.Sin(a)*radii[ring+1],ys[ring+1],Mathf.Cos(a)*radii[ring+1]);
            Tri(v,t,aa,bb,cc);Tri(v,t,aa,cc,dd);
        }
        return Store("Follaje tallado",v,t);
    }
    static Mesh Roof(){var v=new List<Vector3>();var t=new List<int>();var a=new Vector3(-.5f,0,-.5f);var b=new Vector3(.5f,0,-.5f);var c=new Vector3(.5f,0,.5f);var d=new Vector3(-.5f,0,.5f);var peak=new Vector3(0,1,0);Tri(v,t,a,peak,b);Tri(v,t,b,peak,c);Tri(v,t,c,peak,d);Tri(v,t,d,peak,a);return Store("Tejado piramidal",v,t);}
}
