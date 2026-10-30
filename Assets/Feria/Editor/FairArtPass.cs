using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Feria;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Original, reproducible artwork. No downloaded prop or illustration packs.</summary>
public static class FairArtPass
{
    const string Root = "Assets/Feria/Art/Original";
    static readonly Color Cream = Hex("F3E2BC"), Teal = Hex("397F80"), Coral = Hex("CE715B"), Gold = Hex("F4C963"), Ink = Hex("304C50");
    static Transform decor;
    [MenuItem("Feria/Aplicar arte original v0.2")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying) throw new InvalidOperationException("Sal del modo de juego antes de editar el arte.");
        var game=UnityEngine.Object.FindFirstObjectByType<FairGame>();
        if(!game) throw new InvalidOperationException("Abre PrimeraManana antes de aplicar el arte.");
        Directory.CreateDirectory(Root+"/Sprites"); Directory.CreateDirectory(Root+"/Meshes"); Directory.CreateDirectory(Root+"/Materials"); Directory.CreateDirectory("ArtSources"); Directory.CreateDirectory("Assets/Feria/Prefabs"); AssetDatabase.Refresh();
        CreateIllustrations();
        var previous=GameObject.Find("Arte original v0.2"); if(previous) UnityEngine.Object.DestroyImmediate(previous);
        var oldCartArt=game.cart.Find("Carrito original"); if(oldCartArt) UnityEngine.Object.DestroyImmediate(oldCartArt.gameObject);
        var oldBroomArt=game.player.broom.Find("Escoba original"); if(oldBroomArt) UnityEngine.Object.DestroyImmediate(oldBroomArt.gameObject);
        if(game.player.cloth) UnityEngine.Object.DestroyImmediate(game.player.cloth.gameObject);
        decor=new GameObject("Arte original v0.2").transform;
        var teal=Material("Esmalte verde",Teal); var gold=Material("Latón pintado",Gold); var coral=Material("Lona coral",Coral); var cream=Material("Lona marfil",Cream); var wood=Material("Madera miel",Hex("AA7951")); var dark=Material("Hierro",Ink);
        string feedbackPath=Root+"/Materials/Indicador de barrido.mat";
        var feedback=AssetDatabase.LoadAssetAtPath<Material>(feedbackPath);
        if(!feedback) { feedback=new Material(Shader.Find("Universal Render Pipeline/Unlit")); feedback.name="Indicador de barrido"; AssetDatabase.CreateAsset(feedback,feedbackPath); }
        feedback.color=Gold; game.feedbackMaterial=feedback;
        // A curved twelve-panel canvas roof, authored as a shared mesh with two submeshes.
        Transform mainBooth=null;
        foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)) if(t.name=="Puesto" && Mathf.Abs(t.position.x)<.1f) { mainBooth=t; break; }
        if(mainBooth)
        {
            foreach(Transform t in mainBooth) if(t.name=="Toldo") t.gameObject.SetActive(false);
            var roof=MeshObject("Toldo curvo de franjas",Awning(),new[]{cream,coral}); roof.transform.position=new Vector3(0,2.65f,5.3f);
            Box("Moldura superior",new Vector3(0,3.43f,3.76f),new Vector3(6.05f,.07f,.12f),gold);
            Box("Moldura inferior",new Vector3(0,2.56f,3.76f),new Vector3(6.05f,.07f,.12f),gold);
            for(int side=-1;side<=1;side+=2)
            {
                Box("Marco del rótulo",new Vector3(side*2.99f,3,3.76f),new Vector3(.07f,.9f,.12f),gold);
                var medallion=SpritePlane("Roseta de canica", "emblema",new Vector3(side*2.5f,3,3.72f),new Vector2(.43f,.43f));
                for(int i=0;i<6;i++) Box("Panel frontal",new Vector3(side*(.1f+i*.46f),.5f,3.82f),new Vector3(.025f,.85f,.035f),cream);
            }
            SpritePlane("Cartel ilustrado · Canicas", "cartel-canicas",new Vector3(-1.9f,.56f,3.79f),new Vector2(.76f,.76f));
            SpritePlane("Cartel ilustrado · Amanecer", "cartel-amanecer",new Vector3(1.9f,.56f,3.79f),new Vector2(.76f,.76f));
            Box("Zócalo",new Vector3(0,.09f,3.76f),new Vector3(5.9f,.12f,.12f),wood);
        }
        // Curved, triangular bunting replaces the rectangular placeholder flags.
        foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if(t.name=="Banderín") { var filter=t.GetComponent<MeshFilter>(); filter.sharedMesh=Pennant(); t.localScale=new Vector3(.32f,.4f,.02f); }
        var cable=new GameObject("Cable de banderines").AddComponent<LineRenderer>(); cable.transform.SetParent(decor); cable.positionCount=33; cable.widthMultiplier=.025f; cable.sharedMaterial=dark;
        for(int i=0;i<33;i++) {float x=-6+i*12f/32; cable.SetPosition(i,new Vector3(x,3.88f-Mathf.Sin(i/32f*Mathf.PI)*.5f,-.8f));}
        // A usable-looking open cart, with a funnel-shaped pickup tray and four wheels.
        var oldBox=game.cart.Find("Caja"); if(oldBox) oldBox.gameObject.SetActive(false);
        var oldTray=game.cart.Find("Recogedor amarillo"); if(oldTray) oldTray.gameObject.SetActive(false);
        foreach(Transform t in game.cart) if(t.name=="Rueda") t.gameObject.SetActive(false);
        var cartArt=new GameObject("Carrito original").transform; cartArt.SetParent(decor);
        Box("Fondo del carro",new Vector3(0,.23f,0),new Vector3(1.1f,.1f,.72f),teal,cartArt);
        for(int side=-1;side<=1;side+=2) { Box("Lateral abierto",new Vector3(side*.53f,.56f,0),new Vector3(.08f,.62f,.7f),teal,cartArt); Box("Costilla de latón",new Vector3(side*.58f,.55f,0),new Vector3(.035f,.5f,.65f),gold,cartArt); }
        Box("Pared trasera",new Vector3(0,.55f,-.31f),new Vector3(1.1f,.6f,.08f),teal,cartArt);
        Box("Pared frontal",new Vector3(0,.55f,.31f),new Vector3(1.1f,.6f,.08f),teal,cartArt);
        var tray=MeshObject("Pala trapezoidal",Tray(),new[]{gold}); tray.transform.SetParent(cartArt,false);
        for(int side=-1;side<=1;side+=2) for(int end=-1;end<=1;end+=2)
        {
            var wheel=MeshObject("Rueda de doce radios",Wheel(),new[]{dark}); wheel.transform.SetParent(cartArt,false); wheel.transform.localPosition=new Vector3(side*.59f,.18f,end*.26f); wheel.transform.localScale=Vector3.one*.34f;
            var hub=Box("Eje dorado",new Vector3(side*.64f,.18f,end*.26f),Vector3.one*.1f,gold,cartArt);
        }
        PrefabUtility.SaveAsPrefabAsset(cartArt.gameObject,"Assets/Feria/Prefabs/CarritoVisual.prefab"); cartArt.SetParent(game.cart,false);
        // Rounded broom head, cylindrical handle, ferrule and a dense bristle fan.
        foreach(Transform t in game.player.broom) t.gameObject.SetActive(false);
        var broom=new GameObject("Escoba original").transform; broom.SetParent(decor);
        var handle=MeshObject("Mango torneado",Cylinder(),new[]{wood}); handle.transform.SetParent(broom,false); handle.transform.localScale=new Vector3(.04f,.61f,.04f); handle.transform.localPosition=new Vector3(0,-.08f,.26f); handle.transform.localRotation=Quaternion.Euler(58,0,0);
        Box("Cabezal de madera",new Vector3(0,-.33f,.74f),new Vector3(.52f,.10f,.16f),teal,broom);
        Box("Abrazadera",new Vector3(0,-.29f,.74f),new Vector3(.12f,.08f,.18f),gold,broom);
        for(int i=0;i<23;i++) { var bristle=Box("Fibra",new Vector3(-.235f+i*.021f,-.425f,.74f),new Vector3(.014f,.15f,.13f),i%3==0?cream:gold,broom); bristle.transform.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(10,-10,i/22f)); }
        PrefabUtility.SaveAsPrefabAsset(broom.gameObject,"Assets/Feria/Prefabs/EscobaVisual.prefab"); broom.SetParent(game.player.broom,false);
        var cloth=new GameObject("Paño original").transform; cloth.SetParent(decor);
        Box("Tela doblada",Vector3.zero,new Vector3(.32f,.06f,.28f),cream,cloth);
        for(int i=0;i<6;i++) Box("Costura coral",new Vector3(-.13f+i*.05f,.034f,0),new Vector3(.012f,.006f,.26f),coral,cloth);
        PrefabUtility.SaveAsPrefabAsset(cloth.gameObject,"Assets/Feria/Prefabs/PanoVisual.prefab"); cloth.SetParent(game.player.view.transform,false); cloth.localPosition=new Vector3(.42f,-.34f,.65f); game.player.cloth=cloth; cloth.gameObject.SetActive(false);
        // Dome-shaped paper cups are replaced by a hollow twelve-sided cup mesh.
        var cupMesh=Cup(); foreach(var t in game.litter) if(t.name.StartsWith("Vaso ")) { t.GetComponent<MeshFilter>().sharedMesh=cupMesh; t.GetComponent<MeshRenderer>().sharedMaterials=new[]{cream}; }
        game.hud.broomIcon=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Sprites/icono-escoba.png");
        game.hud.clothIcon=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Sprites/icono-pano.png");
        game.hud.emblem=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Sprites/emblema.png");
        // Additional depth without adding tasks: turned posts and slatted bench backs.
        for(int side=-1;side<=1;side+=2)
        {
            for(int j=0;j<3;j++) Box("Listón de respaldo",new Vector3(side*6.08f,.83f+j*.13f,.8f),new Vector3(.08f,.09f,2),wood);
            var cap=MeshObject("Remate de poste",Cylinder(),new[]{gold}); cap.transform.position=new Vector3(side*6,3.92f,-.8f); cap.transform.localScale=new Vector3(.23f,.04f,.23f);
        }
        PlayerSettings.bundleVersion="0.2.0";
        EditorUtility.SetDirty(game); EditorUtility.SetDirty(game.player); EditorUtility.SetDirty(game.hud);
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene); EditorSceneManager.SaveScene(game.gameObject.scene); AssetDatabase.SaveAssets();
        Debug.Log("FERIA_ART_READY: originales PNG/SVG, 6 mallas y 3 prefabs visuales.");
    }
    static Color Hex(string value) { ColorUtility.TryParseHtmlString("#"+value,out var c); return c; }
    static Material Material(string name,Color color)
    {
        string path=Root+"/Materials/"+name+".mat"; var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!mat) {mat=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat,path);}
        mat.color=color; mat.SetFloat("_Smoothness",.2f); mat.enableInstancing=true; return mat;
    }
    static GameObject Box(string name,Vector3 position,Vector3 size,Material material,Transform parent=null)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent?parent:decor,false); go.transform.localPosition=position; go.transform.localScale=size; go.GetComponent<Renderer>().sharedMaterial=material; return go;
    }
    static GameObject MeshObject(string name,Mesh mesh,Material[] materials)
    {
        var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer)); go.transform.SetParent(decor,false); go.GetComponent<MeshFilter>().sharedMesh=mesh; go.GetComponent<MeshRenderer>().sharedMaterials=materials; return go;
    }
    static GameObject SpritePlane(string name,string texture,Vector3 position,Vector2 size)
    {
        var mat=Material(texture,Color.white); mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Sprites/"+texture+".png");
        mat.SetFloat("_AlphaClip",1); mat.SetFloat("_Cutoff",.5f); mat.EnableKeyword("_ALPHATEST_ON");
        var go=GameObject.CreatePrimitive(PrimitiveType.Quad); go.name=name; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); go.transform.SetParent(decor,false); go.transform.position=position; go.transform.localScale=new Vector3(size.x,size.y,1); go.GetComponent<Renderer>().sharedMaterial=mat; return go;
    }
    static Mesh Store(string name,List<Vector3> vertices,List<int> triangles,List<int> alternate=null)
    {
        string path=Root+"/Meshes/"+name+".asset"; var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(!mesh) {mesh=new Mesh {name=name}; AssetDatabase.CreateAsset(mesh,path);} else mesh.Clear();
        mesh.SetVertices(vertices); mesh.subMeshCount=alternate==null?1:2; mesh.SetTriangles(triangles,0); if(alternate!=null) mesh.SetTriangles(alternate,1);
        mesh.RecalculateNormals(); mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh); return mesh;
    }
    static void Quad(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d,bool doubleSided=false)
    {
        int n=v.Count; v.AddRange(new[]{a,b,c,d}); t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
        if(doubleSided) { n=v.Count; v.AddRange(new[]{d,c,b,a}); t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3}); }
    }
    static Mesh Awning()
    {
        var v=new List<Vector3>(); var a=new List<int>(); var b=new List<int>();
        for(int stripe=0;stripe<12;stripe++) for(int step=0;step<12;step++)
        {
            float x=-3+stripe*.5f, z=.95f-step*2.55f/12, zz=.95f-(step+1)*2.55f/12;
            Func<float,float> y=q=>.28f*Mathf.Sin((q+1.6f)/2.55f*Mathf.PI*.7f);
            var t=stripe%2==0?a:b; Quad(v,t,new Vector3(x,y(z),z),new Vector3(x+.5f,y(z),z),new Vector3(x+.5f,y(zz),zz),new Vector3(x,y(zz),zz),true);
        }
        for(int stripe=0;stripe<12;stripe++) for(int step=0;step<8;step++)
        {
            float x=-3+stripe*.5f+step*.5f/8, xx=x+.5f/8;
            Func<float,float> y=q=>-.10f-.12f*Mathf.Sin((q+3-stripe*.5f)*Mathf.PI*2);
            Quad(v,stripe%2==0?a:b,new Vector3(x,0,-1.6f),new Vector3(xx,0,-1.6f),new Vector3(xx,y(xx),-1.6f),new Vector3(x,y(x),-1.6f),true);
        }
        return Store("Toldo festoneado",v,a,b);
    }
    static Mesh Pennant()
    {
        var a=new Vector3(-.5f,.5f,0); var b=new Vector3(.5f,.5f,0); var c=new Vector3(0,-.5f,.06f);
        return Store("Banderín triangular",new List<Vector3>{a,b,c,c,b,a},new List<int>{0,2,1,3,5,4});
    }
    static Mesh Tray()
    {
        var v=new List<Vector3>(); var t=new List<int>();
        Quad(v,t,new Vector3(-.45f,.12f,.38f),new Vector3(.45f,.12f,.38f),new Vector3(.7f,.025f,1.45f),new Vector3(-.7f,.025f,1.45f),true);
        for(int side=-1;side<=1;side+=2) Quad(v,t,new Vector3(side*.45f,.12f,.38f),new Vector3(side*.7f,.025f,1.45f),new Vector3(side*.7f,.10f,1.45f),new Vector3(side*.45f,.27f,.38f),true);
        return Store("Recogedor trapezoidal",v,t);
    }
    static Mesh Wheel()
    {
        var v=new List<Vector3>(); var t=new List<int>();
        for(int i=0;i<16;i++) {float a=i*Mathf.PI*2/16,b=(i+1)*Mathf.PI*2/16; Quad(v,t,new Vector3(-.12f,Mathf.Sin(a)*.5f,Mathf.Cos(a)*.5f),new Vector3(.12f,Mathf.Sin(a)*.5f,Mathf.Cos(a)*.5f),new Vector3(.12f,Mathf.Sin(b)*.5f,Mathf.Cos(b)*.5f),new Vector3(-.12f,Mathf.Sin(b)*.5f,Mathf.Cos(b)*.5f),true);
            Triangle(v,t,new Vector3(-.12f,0,0),new Vector3(-.12f,Mathf.Sin(a)*.5f,Mathf.Cos(a)*.5f),new Vector3(-.12f,Mathf.Sin(b)*.5f,Mathf.Cos(b)*.5f));
            Triangle(v,t,new Vector3(.12f,0,0),new Vector3(.12f,Mathf.Sin(b)*.5f,Mathf.Cos(b)*.5f),new Vector3(.12f,Mathf.Sin(a)*.5f,Mathf.Cos(a)*.5f)); }
        return Store("Rueda facetada",v,t);
    }
    static void Triangle(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c)
    {
        int n=v.Count; v.AddRange(new[]{a,b,c,c,b,a}); t.AddRange(new[]{n,n+1,n+2,n+3,n+4,n+5});
    }
    static Mesh Cylinder()
    {
        var v=new List<Vector3>(); var t=new List<int>();
        for(int i=0;i<12;i++) {float a=i*Mathf.PI*2/12,b=(i+1)*Mathf.PI*2/12; Quad(v,t,new Vector3(Mathf.Sin(a)*.5f,-1,Mathf.Cos(a)*.5f),new Vector3(Mathf.Sin(a)*.5f,1,Mathf.Cos(a)*.5f),new Vector3(Mathf.Sin(b)*.5f,1,Mathf.Cos(b)*.5f),new Vector3(Mathf.Sin(b)*.5f,-1,Mathf.Cos(b)*.5f),true);}
        return Store("Mango torneado",v,t);
    }
    static Mesh Cup()
    {
        var v=new List<Vector3>(); var t=new List<int>();
        for(int i=0;i<12;i++) {float a=i*Mathf.PI*2/12,b=(i+1)*Mathf.PI*2/12; Quad(v,t,new Vector3(Mathf.Sin(a)*.36f,-1,Mathf.Cos(a)*.36f),new Vector3(Mathf.Sin(a)*.5f,1,Mathf.Cos(a)*.5f),new Vector3(Mathf.Sin(b)*.5f,1,Mathf.Cos(b)*.5f),new Vector3(Mathf.Sin(b)*.36f,-1,Mathf.Cos(b)*.36f),true);}
        return Store("Vaso hueco",v,t);
    }
    static void CreateIllustrations()
    {
        var c=new Canvas();
        c.Circle(256,256,232,Teal); c.Circle(256,256,211,Gold); c.Circle(256,256,191,Teal);
        c.Circle(256,256,126,Cream); c.Circle(270,245,103,Coral); c.Circle(235,279,77,Gold); c.Circle(221,298,32,Cream);
        for(int i=0;i<8;i++) {float a=i*Mathf.PI/4; c.Circle(256+Mathf.Cos(a)*174,256+Mathf.Sin(a)*174,7,Cream);}
        c.Save("emblema");
        c=new Canvas(); c.Line(346,444,202,207,30,Gold); c.Line(346,444,202,207,16,Hex("AA7951"));
        c.Polygon(new[]{new Vector2(110,175),new Vector2(269,237),new Vector2(309,156),new Vector2(135,84)},Teal);
        c.Polygon(new[]{new Vector2(135,84),new Vector2(309,156),new Vector2(322,116),new Vector2(143,45)},Gold);
        for(int i=0;i<8;i++) c.Line(149+i*22,50+i*9,146+i*22,88+i*9,5,Coral);
        c.Save("icono-escoba");
        c=new Canvas(); c.Polygon(new[]{new Vector2(68,160),new Vector2(348,100),new Vector2(450,320),new Vector2(164,390)},Teal);
        c.Polygon(new[]{new Vector2(68,160),new Vector2(339,179),new Vector2(450,320),new Vector2(164,390)},Cream);
        c.Line(160,360,424,303,10,Coral); c.Line(145,332,405,278,5,Coral); c.Circle(361,408,15,Gold); c.Line(362,378,362,438,7,Gold); c.Line(332,408,392,408,7,Gold); c.Save("icono-pano");
        c=new Canvas(); c.Rect(0,0,512,512,Cream); c.Rect(24,24,464,464,Teal); c.Rect(39,39,434,434,Cream);
        for(int i=0;i<7;i++) c.Rect(46+i*60,415,28,51,i%2==0?Coral:Gold);
        c.Circle(256,277,133,Teal); c.Circle(256,277,117,Gold); c.Circle(270,265,92,Coral); c.Circle(232,310,40,Cream);
        c.Line(83,102,429,102,15,Teal); c.Line(110,76,402,76,7,Coral);
        for(int i=0;i<4;i++) c.Circle(125+i*87,148,12,Teal); c.Save("cartel-canicas");
        c=new Canvas(); c.Rect(0,0,512,512,Teal); c.Rect(20,20,472,472,Gold); c.Rect(32,32,448,448,Cream);
        for(int i=0;i<12;i++) {float a=i*Mathf.PI*2/12; c.Line(256+Mathf.Cos(a)*114,285+Mathf.Sin(a)*114,256+Mathf.Cos(a)*151,285+Mathf.Sin(a)*151,10,Coral);}
        c.Circle(256,285,94,Gold); c.Line(92,174,420,174,12,Teal); c.Line(112,137,400,137,8,Teal);
        c.Polygon(new[]{new Vector2(150,210),new Vector2(192,210),new Vector2(171,179)},Coral); c.Polygon(new[]{new Vector2(236,210),new Vector2(278,210),new Vector2(257,179)},Teal); c.Polygon(new[]{new Vector2(322,210),new Vector2(364,210),new Vector2(343,179)},Coral);
        c.Circle(256,81,18,Coral); c.Save("cartel-amanecer");
        AssetDatabase.Refresh();
    }
    // Each illustration writes both an editable SVG source and the game-ready PNG.
    sealed class Canvas
    {
        const int Size=512; readonly Color[] pixels=new Color[Size*Size]; readonly StringBuilder svg=new StringBuilder();
        static string F(float n)=>n.ToString("0.##",CultureInfo.InvariantCulture);
        static string ColorHex(Color c)=>"#"+ColorUtility.ToHtmlStringRGB(c);
        public void Rect(int x,int y,int w,int h,Color c) {for(int j=Mathf.Max(0,y);j<Mathf.Min(Size,y+h);j++) for(int i=Mathf.Max(0,x);i<Mathf.Min(Size,x+w);i++) pixels[j*Size+i]=c; svg.Append($"<rect x=\"{x}\" y=\"{y}\" width=\"{w}\" height=\"{h}\" fill=\"{ColorHex(c)}\"/>\n");}
        public void Circle(float x,float y,float r,Color c) {Paint(x-r-1,y-r-1,x+r+1,y+r+1,(i,j)=>Mathf.Clamp01(r+.5f-Vector2.Distance(new Vector2(i,j),new Vector2(x,y))),c); svg.Append($"<circle cx=\"{F(x)}\" cy=\"{F(y)}\" r=\"{F(r)}\" fill=\"{ColorHex(c)}\"/>\n");}
        public void Line(float x,float y,float xx,float yy,float width,Color c)
        {
            var a=new Vector2(x,y); var d=new Vector2(xx-x,yy-y); float length=d.sqrMagnitude;
            Paint(Mathf.Min(x,xx)-width,y<yy?y-width:yy-width,Mathf.Max(x,xx)+width,Mathf.Max(y,yy)+width,(i,j)=>{var p=new Vector2(i,j); float t=length>0?Mathf.Clamp01(Vector2.Dot(p-a,d)/length):0; return Mathf.Clamp01(width*.5f+.5f-Vector2.Distance(p,a+d*t));},c);
            svg.Append($"<path d=\"M {F(x)} {F(y)} L {F(xx)} {F(yy)}\" stroke=\"{ColorHex(c)}\" stroke-width=\"{F(width)}\" stroke-linecap=\"round\"/>\n");
        }
        public void Polygon(Vector2[] p,Color c)
        {
            float minX=Size,minY=Size,maxX=0,maxY=0; var points=new StringBuilder(); foreach(var v in p) {minX=Mathf.Min(minX,v.x); minY=Mathf.Min(minY,v.y); maxX=Mathf.Max(maxX,v.x); maxY=Mathf.Max(maxY,v.y); points.Append(F(v.x)+","+F(v.y)+" ");}
            Paint(minX,minY,maxX,maxY,(x,y)=>{bool inside=false; for(int i=0,j=p.Length-1;i<p.Length;j=i++) if((p[i].y>y)!=(p[j].y>y) && x<(p[j].x-p[i].x)*(y-p[i].y)/(p[j].y-p[i].y)+p[i].x) inside=!inside; return inside?1:0;},c);
            svg.Append($"<polygon points=\"{points}\" fill=\"{ColorHex(c)}\"/>\n");
        }
        void Paint(float x,float y,float xx,float yy,Func<float,float,float> coverage,Color c)
        {
            for(int j=Mathf.Max(0,Mathf.FloorToInt(y));j<Mathf.Min(Size,Mathf.CeilToInt(yy));j++) for(int i=Mathf.Max(0,Mathf.FloorToInt(x));i<Mathf.Min(Size,Mathf.CeilToInt(xx));i++)
            {float a=coverage(i+.5f,j+.5f); if(a<=0) continue; var before=pixels[j*Size+i]; float alpha=a+before.a*(1-a); var result=alpha>0?(c*a+before*before.a*(1-a))/alpha:Color.clear; result.a=alpha; pixels[j*Size+i]=result;}
        }
        public void Save(string name)
        {
            var texture=new Texture2D(Size,Size,TextureFormat.RGBA32,false); texture.SetPixels(pixels); texture.Apply(); string path=Root+"/Sprites/"+name+".png"; File.WriteAllBytes(path,texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            File.WriteAllText("ArtSources/"+name+".svg","<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"512\" height=\"512\" viewBox=\"0 0 512 512\"><g transform=\"translate(0 512) scale(1 -1)\">\n"+svg+"</g></svg>");
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport); var importer=(TextureImporter)AssetImporter.GetAtPath(path); importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single; importer.alphaIsTransparency=true; importer.mipmapEnabled=true; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
        }
    }
}
