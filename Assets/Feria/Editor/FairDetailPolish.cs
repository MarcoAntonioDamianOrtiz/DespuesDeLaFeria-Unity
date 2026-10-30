using System;
using System.Collections.Generic;
using Feria;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Reproducible original meshes; leaves persistent litter indices and tasks intact.</summary>
public static class FairDetailPolish
{
    const string Art="Assets/Feria/Art/Original";
    static Transform root;
    static Material teal,gold,cream,wood,coral,dark;
    static Color C(string h){ColorUtility.TryParseHtmlString("#"+h,out var c);return c;}
    [MenuItem("Feria/Pulir herramientas y canicas v0.4")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Sal de Play antes de aplicar el arte.");
        var game=UnityEngine.Object.FindFirstObjectByType<FairGame>();if(!game)throw new Exception("Abre PrimeraManana.");
        var old=GameObject.Find("Detalles artesanales v0.4");if(old)UnityEngine.Object.DestroyImmediate(old);
        root=new GameObject("Detalles artesanales v0.4").transform;
        teal=Mat("Esmalte petróleo","2C777D");gold=Mat("Paja dorada","E6B94D");cream=Mat("Papel crema","F6E7C6");wood=Mat("Mango de fresno","B88654");coral=Mat("Papel bermellón","E78770");dark=Mat("Fibra oscura","8D673D");
        Confetti(game);Broom(game);Marbles(game);
        PlayerSettings.bundleVersion="0.4.0";EditorUtility.SetDirty(game);EditorUtility.SetDirty(game.player);EditorUtility.SetDirty(game.definition);
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();
        Debug.Log("FERIA_DETAILS_READY: papel curvado, escoba artesanal y tablero de puntería.");
    }
    static void Confetti(FairGame game)
    {
        var meshes=new[]{Paper(0),Paper(1),Paper(2),Paper(3),Paper(4)};
        var colors=new[]{coral,teal,gold,cream,Mat("Papel frambuesa","CF7795"),Mat("Papel celeste","78B5C1")};
        for(int i=0;i<game.litter.Length;i++)
        {
            var piece=game.litter[i];if(piece.name.StartsWith("Vaso"))continue;
            var old=piece.Find("Papel artesanal");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            piece.GetComponent<Renderer>().enabled=false;
            // Parent keeps canonical floor coordinates. A child carries only visual flutter.
            piece.localScale=Vector3.one;var paper=Shape("Papel artesanal",meshes[i%5],colors[i%colors.Length],piece);
            float size=.065f+(i%7)*.006f;paper.transform.localScale=new Vector3(size,1,size*(i%5==0?2.2f:1));
            paper.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
        }
    }
    static void Broom(FairGame game)
    {
        foreach(Transform child in game.player.broom)child.gameObject.SetActive(false);
        var previous=game.player.broom.Find("Escoba artesanal");if(previous)UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var broom=new GameObject("Escoba artesanal").transform;broom.SetParent(game.player.broom,false);
        var head=Shape("Cabezal biselado",Bevel(),teal,broom);head.transform.localPosition=new Vector3(0,-.07f,.55f);head.transform.localScale=new Vector3(.43f,.075f,.15f);
        Box("Filete de latón",new Vector3(0,-.031f,.55f),new Vector3(.34f,.009f,.10f),gold,broom);
        var shaft=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Meshes/Mango torneado.asset");
        var handle=Shape("Mango de fresno",shaft,wood,broom);handle.transform.localPosition=new Vector3(0,.15f,.21f);handle.transform.localRotation=Quaternion.Euler(-57,0,0);handle.transform.localScale=new Vector3(.028f,.40f,.028f);
        var collar=Shape("Casquillo de latón",shaft,gold,broom);collar.transform.localPosition=new Vector3(0,-.005f,.49f);collar.transform.localRotation=handle.transform.localRotation;collar.transform.localScale=new Vector3(.047f,.06f,.047f);
        var grip=Shape("Empuñadura de hilo",shaft,dark,broom);grip.transform.localPosition=new Vector3(0,.31f,-.04f);grip.transform.localRotation=handle.transform.localRotation;grip.transform.localScale=new Vector3(.035f,.065f,.035f);
        var fibres=Shape("Fibras flexibles",Bristles(),gold,broom);fibres.transform.localPosition=new Vector3(0,-.105f,.55f);fibres.GetComponent<MeshRenderer>().sharedMaterials=new[]{gold,cream,dark};
        for(int i=0;i<3;i++)Box("Remache",new Vector3(-.13f+i*.13f,-.064f,.472f),new Vector3(.018f,.018f,.012f),gold,broom);
        game.player.broom.localPosition=new Vector3(.32f,-.34f,.53f);game.player.broom.localRotation=Quaternion.Euler(0,-6,-5);
        PrefabUtility.SaveAsPrefabAsset(broom.gameObject,"Assets/Feria/Prefabs/EscobaArtesanal.prefab");
    }
    static void Marbles(FairGame game)
    {
        foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))if(t.name=="Rampa de canicas")t.gameObject.SetActive(false);
        var oldBoard=GameObject.Find("Tablero canicas");if(oldBoard)oldBoard.SetActive(false);
        var board=new GameObject("Tablero artesanal de canicas").transform;board.SetParent(root,false);
        var surface=Shape("Base biselada",Bevel(),teal,board);surface.transform.localPosition=new Vector3(0,1.37f,5.13f);surface.transform.localScale=new Vector3(2.4f,.15f,2.1f);
        Box("Fieltro marfil",new Vector3(0,1.453f,5.13f),new Vector3(2.23f,.018f,1.94f),cream,board);
        var points=new Vector3[12];
        for(int row=0;row<6;row++)
        {
            float y=1.81f-row*.075f,z=5.85f-row*.22f;
            Box("Canal de madera",new Vector3(0,y,z),new Vector3(1.98f,.035f,.16f),wood,board);
            Box("Carril de color",new Vector3(0,y+.022f,z),new Vector3(1.94f,.015f,.10f),row%2==0?coral:teal,board);
            Box("Guía posterior",new Vector3(0,y+.044f,z+.079f),new Vector3(2.04f,.042f,.025f),gold,board);
            for(int end=-1;end<=1;end+=2)Box("Patín de apoyo",new Vector3(end*.83f,(y+1.46f)*.5f,z),new Vector3(.06f,Mathf.Max(.035f,y-1.46f),.10f),teal,board);
            float a=row%2==0?.88f:-.88f;points[row*2]=new Vector3(a,y+.105f,z);points[row*2+1]=new Vector3(-a,y+.105f,z);
        }
        game.definition.marbleWaypoints=points;game.definition.marbleSeconds=5.8f;
        Box("Bandeja de llegada",new Vector3(0,1.455f,4.24f),new Vector3(2.08f,.04f,.39f),teal,board);
        for(int i=-2;i<=2;i++)
        {
            float x=i*.39f;Box("Diana de llegada",new Vector3(x,1.479f,4.24f),new Vector3(.35f,.006f,.30f),i==0?gold:Mathf.Abs(i)==1?coral:cream,board);
            Label("Puntos",i==0?"100":Mathf.Abs(i)==1?"50":"10",new Vector3(x,1.486f,4.22f),new Vector2(.30f,.15f),.095f,dark,game.hud.font,board,Quaternion.Euler(90,0,0));
        }
        for(int side=-1;side<=1;side+=2)Box("Borde de llegada",new Vector3(side*1.06f,1.53f,4.28f),new Vector3(.04f,.14f,.42f),gold,board);
        Box("Tope frontal",new Vector3(0,1.53f,4.035f),new Vector3(2.15f,.14f,.04f),wood,board);
        Label("Regla de canicas","APUNTA AL CENTRO",new Vector3(0,1.65f,5.99f),new Vector2(1.95f,.22f),.16f,teal,game.hud.font,board,Quaternion.identity);
        game.marble.position=points[0];game.marble.localScale=Vector3.one*.14f;
        var stripe=Shape("Veta de cristal",MarbleBand(),cream,game.marble);stripe.transform.localPosition=Vector3.zero;stripe.transform.localScale=Vector3.one;stripe.transform.localRotation=Quaternion.Euler(25,0,25);
        var previous=game.marble.Find("Veta de cristal");for(int i=game.marble.childCount-1;i>=0;i--){var child=game.marble.GetChild(i);if(child!=stripe.transform&&child.name=="Veta de cristal")UnityEngine.Object.DestroyImmediate(child.gameObject);}
        PrefabUtility.SaveAsPrefabAsset(board.gameObject,"Assets/Feria/Prefabs/TableroCanicas.prefab");
    }
    static void Label(string name,string text,Vector3 p,Vector2 size,float fontSize,Material mat,TMP_FontAsset font,Transform parent,Quaternion rotation)
    {
        var label=new GameObject(name).AddComponent<TextMeshPro>();label.transform.SetParent(parent,false);label.transform.position=p;label.transform.rotation=rotation;label.font=font;label.text=text;label.fontSize=fontSize*10;label.color=mat.color;label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=size;
    }
    static Material Mat(string name,string hex){string path=Art+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=C(hex);m.SetFloat("_Smoothness",.22f);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
    static GameObject Shape(string name,Mesh mesh,Material material,Transform parent){var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=material;return go;}
    static GameObject Box(string name,Vector3 p,Vector3 scale,Material mat,Transform parent){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;return go;}
    static void Tri(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,bool both=false){int n=v.Count;v.AddRange(new[]{a,b,c});t.AddRange(new[]{n,n+1,n+2});if(both){n=v.Count;v.AddRange(new[]{c,b,a});t.AddRange(new[]{n,n+1,n+2});}}
    static void Quad(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d,bool both=false){Tri(v,t,a,b,c,both);Tri(v,t,a,c,d,both);}
    static Mesh Store(string name,List<Vector3> v,List<int>[] triangles){string path=Art+"/Meshes/"+name+".asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!m){m=new Mesh{name=name};AssetDatabase.CreateAsset(m,path);}else m.Clear();m.SetVertices(v);m.subMeshCount=triangles.Length;for(int i=0;i<triangles.Length;i++)m.SetTriangles(triangles[i],i);m.RecalculateNormals();m.RecalculateBounds();EditorUtility.SetDirty(m);return m;}
    static Mesh Paper(int kind)
    {
        var v=new List<Vector3>();var t=new List<int>();
        if(kind==0||kind==4)
        {
            for(int i=0;i<5;i++){float z=-.5f+i*.2f,zz=z+.2f;float y=Mathf.Sin((z+.5f)*Mathf.PI)*.018f,yy=Mathf.Sin((zz+.5f)*Mathf.PI)*.018f;float x=kind==4?Mathf.Sin(i*.9f)*.12f:0;Quad(v,t,new Vector3(-.5f+x,y,z),new Vector3(.5f+x,y,z),new Vector3(.5f+x,yy,zz),new Vector3(-.5f+x,yy,zz),true);}
        }
        else
        {
            int n=kind==1?4:kind==2?12:10;
            for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n,b=(i+1)*Mathf.PI*2/n;float ra=kind==3&&i%2==1?.23f:.5f,rb=kind==3&&(i+1)%2==1?.23f:.5f;Tri(v,t,new Vector3(0,.014f,0),new Vector3(Mathf.Cos(b)*rb,.002f,Mathf.Sin(b)*rb),new Vector3(Mathf.Cos(a)*ra,.002f,Mathf.Sin(a)*ra),true);}
        }
        return Store(new[]{"Confeti curvado","Confeti rombo","Confeti redondo","Confeti estrella","Cinta ondulada"}[kind],v,new[]{t});
    }
    static Mesh Bevel()
    {
        var v=new List<Vector3>();var t=new List<int>();var polygon=new[]{new Vector2(-.43f,-.5f),new Vector2(.43f,-.5f),new Vector2(.5f,-.32f),new Vector2(.5f,.32f),new Vector2(.43f,.5f),new Vector2(-.43f,.5f),new Vector2(-.5f,.32f),new Vector2(-.5f,-.32f)};
        for(int i=0;i<8;i++){var a=polygon[i];var b=polygon[(i+1)%8];Tri(v,t,new Vector3(0,.5f,0),new Vector3(b.x*.94f,.5f,b.y*.94f),new Vector3(a.x*.94f,.5f,a.y*.94f));Quad(v,t,new Vector3(a.x,-.5f,a.y),new Vector3(b.x,-.5f,b.y),new Vector3(b.x*.94f,.5f,b.y*.94f),new Vector3(a.x*.94f,.5f,a.y*.94f));Tri(v,t,new Vector3(0,-.5f,0),new Vector3(a.x,-.5f,a.y),new Vector3(b.x,-.5f,b.y));}
        return Store("Madera biselada",v,new[]{t});
    }
    static Mesh MarbleBand()
    {
        var v=new List<Vector3>();var t=new List<int>();
        Func<float,float,Vector3> p=(a,b)=>new Vector3(Mathf.Cos(a)*(.49f+Mathf.Cos(b)*.034f),Mathf.Sin(b)*.034f,Mathf.Sin(a)*(.49f+Mathf.Cos(b)*.034f));
        for(int i=0;i<24;i++)for(int j=0;j<6;j++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12,c=j*Mathf.PI/3,d=(j+1)*Mathf.PI/3;Quad(v,t,p(a,c),p(a,d),p(b,d),p(b,c));}
        return Store("Veta de canica",v,new[]{t});
    }
    static Mesh Bristles()
    {
        var v=new List<Vector3>();var sub=new[]{new List<int>(),new List<int>(),new List<int>()};
        for(int row=0;row<3;row++)for(int i=0;i<27;i++)
        {
            float x=-.19f+i*.0146f,z=-.055f+row*.052f;float end=x*1.13f,y=-.118f-(i%4)*.003f;var t=sub[(i+row)%7==0?2:(i+row)%3==0?1:0];
            var a=new Vector3(x-.0035f,0,z);var b=new Vector3(x+.0035f,0,z);var c=new Vector3(end+.0025f,y,z-.016f);var d=new Vector3(end-.0025f,y,z-.016f);Quad(v,t,a,b,c,d,true);
        }
        return Store("Fibras de escoba",v,sub);
    }
}
