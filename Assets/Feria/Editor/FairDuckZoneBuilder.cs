using System;
using System.Collections.Generic;
using System.IO;
using Feria;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Creates the second courtyard using the project's own fair kit and new duck artwork.</summary>
public static class FairDuckZoneBuilder
{
    const string Art="Assets/Feria/Art/Original",First="Assets/Feria/Scenes/PrimeraManana.unity",Second="Assets/Feria/Scenes/GaleriaPatitos.unity";
    static Transform root;
    static Material teal,cream,gold,coral,wood,ink;
    static TMP_FontAsset font;
    static Color C(string h){ColorUtility.TryParseHtmlString("#"+h,out var c);return c;}
    [MenuItem("Feria/Construir galería de patitos v0.5")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Sal de Play antes de editar los puestos.");
        EditorSceneManager.OpenScene(First);var first=UnityEngine.Object.FindFirstObjectByType<FairGame>();
        var catalog=AssetDatabase.LoadAssetAtPath<FairZoneCatalog>("Assets/Feria/Definitions/FeriaCatalog.asset");
        if(!catalog){catalog=ScriptableObject.CreateInstance<FairZoneCatalog>();AssetDatabase.CreateAsset(catalog,"Assets/Feria/Definitions/FeriaCatalog.asset");}
        var duck=AssetDatabase.LoadAssetAtPath<FairZoneDefinition>("Assets/Feria/Definitions/PozaPatitos.asset");
        if(!duck){duck=UnityEngine.Object.Instantiate(first.definition);AssetDatabase.CreateAsset(duck,"Assets/Feria/Definitions/PozaPatitos.asset");}
        first.definition.sceneName="PrimeraManana";first.definition.nextZoneId="galeria-patitos";first.definition.litterCount=720;first.definition.surfaceCount=16;
        duck.zoneId="galeria-patitos";duck.displayName="La poza de los patitos";duck.sceneName="GaleriaPatitos";duck.nextZoneId="rincon-canicas";duck.activity=FairActivity.Ducks;duck.litterCount=480;duck.surfaceCount=16;
        duck.activityViewPosition=new Vector3(0,2.4f,3);duck.activityViewTarget=new Vector3(0,1.55f,5.06f);
        catalog.zones=new[]{first.definition,duck};first.catalog=catalog;EnsureIds(first);Materials(first);TravelSign(first);
        EditorUtility.SetDirty(first);EditorUtility.SetDirty(first.definition);EditorSceneManager.MarkSceneDirty(first.gameObject.scene);EditorSceneManager.SaveScene(first.gameObject.scene);
        if(!File.Exists(Second))AssetDatabase.CopyAsset(First,Second);
        EditorSceneManager.OpenScene(Second);var game=UnityEngine.Object.FindFirstObjectByType<FairGame>();game.definition=duck;game.catalog=catalog;Materials(game);
        var old=GameObject.Find("La poza · arte original v0.5");if(old)UnityEngine.Object.DestroyImmediate(old);
        var oldBoard=GameObject.Find("Detalles artesanales v0.4");if(oldBoard)UnityEngine.Object.DestroyImmediate(oldBoard);
        var oldStripe=game.marble.Find("Veta de cristal");if(oldStripe)UnityEngine.Object.DestroyImmediate(oldStripe.gameObject);
        game.marble.GetComponent<Renderer>().enabled=false;
        root=new GameObject("La poza · arte original v0.5").transform;
        var litter=new List<Transform>(game.litter);for(int i=litter.Count-1;i>=480;i--){UnityEngine.Object.DestroyImmediate(litter[i].gameObject);litter.RemoveAt(i);}game.litter=litter.ToArray();EnsureIds(game);
        var random=new System.Random(6372);
        for(int i=0;i<game.litter.Length;i++)
        {
            float side=i%2==0?-1:1,x=side*(1.1f+(float)random.NextDouble()*3.4f),z=-6.3f+(float)random.NextDouble()*8.8f;
            var p=game.litter[i].position;p.x=x;p.z=z;game.litter[i].position=p;
        }
        var title=GameObject.Find("Rótulo").GetComponent<TextMeshPro>();title.text="LA POZA\nDE LOS PATITOS";title.fontSize=3.2f;
        var arch=GameObject.Find("Rótulo arqueado");if(arch)arch.GetComponent<Renderer>().sharedMaterial=coral;
        foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if(t.name=="Roseta de canica")t.gameObject.SetActive(false);
            if(t.name=="Sol del pavimento")t.gameObject.SetActive(false);
        }
        for(int side=-1;side<=1;side+=2){var ornament=Duck("Patito del rótulo",root,false);ornament.position=new Vector3(side*2.45f,2.95f,3.60f);ornament.localScale=Vector3.one*.4f;ornament.localRotation=Quaternion.Euler(0,155,0);}
        Basin(game);Posters();Mosaic();TravelSign(game);
        game.player.Place(new Vector3(0,.05f,-7.7f),0);game.cart.position=new Vector3(-2.7f,0,-4.3f);game.cart.rotation=Quaternion.identity;
        var left=GameObject.Find("Lateral");if(left)left.GetComponent<TextMeshPro>().text="AROS\nY PATITOS";
        PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Feria/Prefabs/PozaPatitosVisual.prefab");
        EditorUtility.SetDirty(game);EditorUtility.SetDirty(duck);EditorUtility.SetDirty(catalog);PlayerSettings.bundleVersion="0.5.0";
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(First,true),new EditorBuildSettingsScene(Second,true)};AssetDatabase.SaveAssets();
        Debug.Log("FERIA_DUCKS_READY: dos puestos, galería de aros, modelos y carteles originales.");
    }
    static void EnsureIds(FairGame game){game.litterIds=CampaignMapping.Ids("litter",game.litter.Length);game.surfaceIds=CampaignMapping.Ids("surface",game.dirtPatches.Length);}
    static void Materials(FairGame game){font=game.hud.font;teal=Mat("Esmalte de poza","397F80");cream=Mat("Marfil de patito","F6E7C6");gold=Mat("Amarillo de patito","ECC45A");coral=Mat("Coral de galería","CB7764");wood=Mat("Madera de canal","AC8259");ink=Mat("Tinta de patito","304C50");}
    static void TravelSign(FairGame game)
    {
        var old=GameObject.Find("Paseo al siguiente puesto");if(old)UnityEngine.Object.DestroyImmediate(old);
        var sign=new GameObject("Paseo al siguiente puesto").transform;sign.position=new Vector3(-4.35f,1.20f,-5.5f);
        Box("Poste de paseo",new Vector3(0,-.60f,.05f),new Vector3(.085f,1.2f,.085f),wood,sign);
        var arrow=Shape("Flecha de paseo",Arrow(),teal,sign);arrow.transform.localScale=new Vector3(1.50f,.62f,.06f);
        string text=game.definition.activity==FairActivity.Ducks?"VOLVER A\nLAS CANICAS":"VISITAR\nLOS PATITOS";
        Label("Destino",text,new Vector3(0,0,-.044f),new Vector2(1.20f,.48f),1.0f,cream,sign);
        game.travelSign=sign;
    }
    static void Basin(FairGame game)
    {
        var basin=new GameObject("Canal de patitos").transform;basin.SetParent(root,false);
        Box("Base del canal",new Vector3(0,1.36f,5.06f),new Vector3(2.65f,.20f,1.72f),teal,basin);
        for(int side=-1;side<=1;side+=2)Box("Borde lateral tallado",new Vector3(side*1.36f,1.54f,5.06f),new Vector3(.10f,.28f,1.85f),wood,basin);
        for(int end=-1;end<=1;end+=2){bool front=end<0;Box("Borde corto",new Vector3(0,front?1.49f:1.54f,5.06f+end*.92f),new Vector3(2.80f,front?.13f:.28f,.10f),wood,basin);Box("Filete crema",new Vector3(0,front?1.565f:1.69f,5.06f+end*.92f),new Vector3(2.78f,.025f,.12f),cream,basin);}
        var water=Mat("Agua turquesa","398E98");water.shader=Shader.Find("Feria/Agua pintada");EditorUtility.SetDirty(water);
        Box("Agua pintada",new Vector3(0,1.50f,5.06f),new Vector3(2.63f,.018f,1.73f),water,basin);
        var ducks=new List<Transform>();
        for(int row=0;row<3;row++)for(int col=0;col<2;col++)
        {
            int i=row*2+col;var duck=Duck(i==0?"Patito del gorro":"Patito pintado",basin,i==0);duck.position=new Vector3(col==0?-.55f:.55f,1.565f,4.56f+row*.46f);duck.localScale=Vector3.one*.28f;duck.rotation=Quaternion.Euler(0,180+(i%2==0?-12:12),0);ducks.Add(duck);
        }
        game.duckTargets=ducks.ToArray();
        game.aimRing=Shape("Guía del aro",Ring(),cream,root).transform;game.aimRing.localScale=Vector3.one*.44f;game.aimRing.position=new Vector3(0,1.54f,5);game.aimRing.gameObject.SetActive(false);
        game.flyingRing=Shape("Aro de cuerda",Ring(),gold,root).transform;game.flyingRing.localScale=Vector3.one*.44f;game.flyingRing.gameObject.SetActive(false);
        for(int i=0;i<3;i++){var spare=Shape("Aro en espera",Ring(),gold,basin);spare.transform.localPosition=new Vector3(1.60f+i*.055f,1.35f,4.8f);spare.transform.localRotation=Quaternion.Euler(70,0,0);spare.transform.localScale=Vector3.one*.35f;}
        game.activityAnchor=new GameObject("Mirar aquí para jugar").transform;game.activityAnchor.SetParent(root,false);game.activityAnchor.position=new Vector3(0,1.65f,4.55f);
        PrefabUtility.SaveAsPrefabAsset(ducks[0].gameObject,"Assets/Feria/Prefabs/PatitoArtesanal.prefab");
    }
    static Transform Duck(string name,Transform parent,bool hat)
    {
        var duck=new GameObject(name).transform;duck.SetParent(parent,false);var round=Round();
        var body=Shape("Cuerpo tallado",round,gold,duck);body.transform.localScale=new Vector3(.52f,.32f,.65f);
        var head=Shape("Cabeza redonda",round,gold,duck);head.transform.localPosition=new Vector3(0,.38f,.37f);head.transform.localScale=Vector3.one*.31f;
        var beak=Shape("Pico coral",Beak(),coral,duck);beak.transform.localPosition=new Vector3(0,.33f,.63f);
        for(int side=-1;side<=1;side+=2)
        {
            var eye=Shape("Ojo pintado",round,ink,duck);eye.transform.localPosition=new Vector3(side*.237f,.46f,.54f);eye.transform.localScale=Vector3.one*.043f;
            var shine=Shape("Luz del ojo",round,cream,duck);shine.transform.localPosition=new Vector3(side*.26f,.475f,.556f);shine.transform.localScale=Vector3.one*.014f;
            var wing=Shape("Ala marfil",round,cream,duck);wing.transform.localPosition=new Vector3(side*.43f,.04f,-.05f);wing.transform.localScale=new Vector3(.11f,.19f,.35f);wing.transform.localRotation=Quaternion.Euler(12,0,side*-12);
        }
        var tail=Shape("Colita tallada",Beak(),gold,duck);tail.transform.localPosition=new Vector3(0,.06f,-.59f);tail.transform.localRotation=Quaternion.Euler(-20,180,0);tail.transform.localScale=new Vector3(.7f,1.5f,.9f);
        if(hat){var cap=Shape("Gorro verde",round,teal,duck);cap.transform.localPosition=new Vector3(0,.66f,.37f);cap.transform.localScale=new Vector3(.34f,.085f,.32f);var pom=Shape("Botón de gorro",round,coral,duck);pom.transform.localPosition=new Vector3(0,.76f,.37f);pom.transform.localScale=Vector3.one*.06f;}
        return duck;
    }
    static void Mosaic()
    {
        for(int i=0;i<3;i++){var wave=Shape("Mosaico de ondas",Ring(),i%2==0?teal:cream,root);wave.transform.position=new Vector3(2.8f,.014f,-2);wave.transform.localScale=new Vector3(1.5f+i*.7f,.025f,1.5f+i*.7f);}
        Label("Letrero de aros","TRES AROS\nSIN PRISA",new Vector3(0,1.96f,5.99f),new Vector2(2.2f,.40f),1.7f,cream,root);
    }
    static void Posters()
    {
        PosterTexture();var mat=Mat("Cartel original de patitos","FFFFFF");mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/Sprites/cartel-patitos.png");
        foreach(var name in new[]{"Cartel ilustrado · Canicas","Cartel ilustrado · Amanecer"}){var poster=GameObject.Find(name);if(poster)poster.GetComponent<Renderer>().sharedMaterial=mat;}
    }
    static void PosterTexture()
    {
        const int n=512;var pixels=new Color[n*n];var paper=C("F6E7C6");var blue=C("397F80");var yellow=C("ECC45A");var red=C("CB7764");var black=C("304C50");
        for(int y=0;y<n;y++)for(int x=0;x<n;x++)
        {
            Color c=paper;if(x<24||y<24||x>487||y>487)c=blue;
            if(y>60&&y<100&&Mathf.Sin(x*.055f)>.0f)c=blue;
            if(((x-246f)*(x-246f)/(139*139f)+(y-228f)*(y-228f)/(83*83f))<1)c=yellow;
            if((new Vector2(x-305,y-319)).sqrMagnitude<67*67)c=yellow;
            if(x>345&&x<410&&Mathf.Abs(y-308)<(410-x)*.28f)c=red;
            if(new Vector2(x-325,y-333).sqrMagnitude<8*8)c=black;
            if(x>244&&x<347&&y>373&&y<393)c=blue;
            float ring=Vector2.Distance(new Vector2(x,y),new Vector2(173,374));if(ring>45&&ring<59)c=red;
            pixels[y*n+x]=c;
        }
        var texture=new Texture2D(n,n,TextureFormat.RGBA32,false);texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(Art+"/Sprites/cartel-patitos.png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        File.WriteAllText("ArtSources/cartel-patitos.svg","<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 512 512\"><rect width=\"512\" height=\"512\" fill=\"#F6E7C6\"/><rect x=\"12\" y=\"12\" width=\"488\" height=\"488\" fill=\"none\" stroke=\"#397F80\" stroke-width=\"24\"/><g transform=\"translate(0 512) scale(1 -1)\"><ellipse cx=\"246\" cy=\"228\" rx=\"139\" ry=\"83\" fill=\"#ECC45A\"/><circle cx=\"305\" cy=\"319\" r=\"67\" fill=\"#ECC45A\"/><path d=\"M345 326 L410 308 L345 290Z\" fill=\"#CB7764\"/><circle cx=\"325\" cy=\"333\" r=\"8\" fill=\"#304C50\"/><path d=\"M244 383H347\" stroke=\"#397F80\" stroke-width=\"20\"/><circle cx=\"173\" cy=\"374\" r=\"52\" fill=\"none\" stroke=\"#CB7764\" stroke-width=\"14\"/></g></svg>");AssetDatabase.Refresh();
    }
    static void Label(string name,string text,Vector3 p,Vector2 size,float sizeFont,Material mat,Transform parent){var label=new GameObject(name).AddComponent<TextMeshPro>();label.transform.SetParent(parent,false);label.transform.localPosition=p;label.font=font;label.text=text;label.fontSize=sizeFont;label.color=mat.color;label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=size;}
    static Material Mat(string name,string hex){string path=Art+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=C(hex);m.SetFloat("_Smoothness",.18f);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
    static GameObject Shape(string name,Mesh mesh,Material mat,Transform parent){var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;return go;}
    static GameObject Box(string name,Vector3 p,Vector3 scale,Material mat,Transform parent){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;return go;}
    static void Tri(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c){int n=v.Count;v.AddRange(new[]{a,b,c});t.AddRange(new[]{n,n+1,n+2});}
    static void Quad(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d){Tri(v,t,a,b,c);Tri(v,t,a,c,d);}
    static Mesh Store(string name,List<Vector3> v,List<int> t){string path=Art+"/Meshes/"+name+".asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!m){m=new Mesh{name=name};AssetDatabase.CreateAsset(m,path);}else m.Clear();m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();EditorUtility.SetDirty(m);return m;}
    static Mesh Round()
    {
        string path=Art+"/Meshes/Talla redondeada de patito.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing)return existing;
        var v=new List<Vector3>();var t=new List<int>();Func<float,float,Vector3> p=(a,b)=>new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b));
        for(int j=0;j<8;j++)for(int i=0;i<12;i++){float a=j*Mathf.PI/8,aa=(j+1)*Mathf.PI/8,b=i*Mathf.PI/6,bb=(i+1)*Mathf.PI/6;if(j!=0)Tri(v,t,p(a,b),p(a,bb),p(aa,bb));if(j!=7)Tri(v,t,p(a,b),p(aa,bb),p(aa,b));}
        return Store("Talla redondeada de patito",v,t);
    }
    static Mesh Ring()
    {
        string path=Art+"/Meshes/Aro de cuerda.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing)return existing;
        var v=new List<Vector3>();var t=new List<int>();Func<float,float,Vector3> p=(a,b)=>new Vector3(Mathf.Cos(a)*(.5f+Mathf.Cos(b)*.055f),Mathf.Sin(b)*.055f,Mathf.Sin(a)*(.5f+Mathf.Cos(b)*.055f));
        for(int i=0;i<32;i++)for(int j=0;j<6;j++){float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16,c=j*Mathf.PI/3,d=(j+1)*Mathf.PI/3;Quad(v,t,p(a,c),p(a,d),p(b,d),p(b,c));}return Store("Aro de cuerda",v,t);
    }
    static Mesh Beak()
    {
        string path=Art+"/Meshes/Pico tallado.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old)return old;
        var v=new List<Vector3>();var t=new List<int>();var a=new Vector3(-.20f,-.04f,0);var b=new Vector3(.20f,-.04f,0);var c=new Vector3(0,-.025f,.32f);var d=a+Vector3.up*.09f;var e=b+Vector3.up*.09f;var f=c+Vector3.up*.04f;Tri(v,t,d,e,f);Tri(v,t,c,b,a);Quad(v,t,a,b,e,d);Quad(v,t,b,c,f,e);Quad(v,t,c,a,d,f);return Store("Pico tallado",v,t);
    }
    static Mesh Arrow()
    {
        var v=new List<Vector3>();var t=new List<int>();var points=new[]{new Vector3(-.5f,-.35f,0),new Vector3(.20f,-.35f,0),new Vector3(.20f,-.5f,0),new Vector3(.5f,0,0),new Vector3(.20f,.5f,0),new Vector3(.20f,.35f,0),new Vector3(-.5f,.35f,0)};
        for(int i=0;i<points.Length;i++)Tri(v,t,Vector3.zero,points[(i+1)%points.Length],points[i]);return Store("Flecha de paseo",v,t);
    }
}
