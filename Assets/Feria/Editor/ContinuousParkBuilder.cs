using System;
using System.Collections.Generic;
using System.IO;
using Feria;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

/// <summary>Builds one walkable park. Existing attraction scenes stay as editable source kits.</summary>
public static class ContinuousParkBuilder
{
    const string Art="Assets/Feria/Art/Original",Output="Assets/Feria/Scenes/ParquePrincipal.unity";
    static Transform decor;
    static Material cream,teal,coral,gold,wood,ink,grass,leaves,path;
    static TMP_FontAsset font;
    [MenuItem("Feria/Construir parque continuo v0.6")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Sal de Play antes de construir el parque.");
        var source=EditorSceneManager.OpenScene("Assets/Feria/Scenes/PrimeraManana.unity");var first=Game(source);
        var sky=RenderSettings.skybox;
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
        var session=new GameObject("Parque · Sesión compartida").AddComponent<ParkSession>();
        var layout=AssetDatabase.LoadAssetAtPath<ParkDefinition>("Assets/Feria/Definitions/ParquePrincipal.asset");
        if(!layout){layout=ScriptableObject.CreateInstance<ParkDefinition>();AssetDatabase.CreateAsset(layout,"Assets/Feria/Definitions/ParquePrincipal.asset");}
        layout.catalog=first.catalog;layout.worldMin=new Vector2(-100,-20);layout.worldMax=new Vector2(100,180);layout.entrance=new Vector3(0,.05f,-3);layout.cartStart=new Vector3(2,0,9);
        layout.futurePlots=new[]{Plot("oeste","Jardín del oeste",new Vector3(-76,0,45),new Vector3(-52,0,45)),Plot("este","Jardín del este",new Vector3(76,0,45),new Vector3(52,0,45)),Plot("noroeste","Paseo norte",new Vector3(-28,0,118),new Vector3(-28,0,88)),Plot("noreste","Paseo norte",new Vector3(28,0,118),new Vector3(28,0,88))};
        session.definition=layout;session.player=first.player;session.cart=first.cart;session.hud=first.hud;
        Move(first.player.gameObject,scene,session.transform);Move(first.cart.gameObject,scene,session.transform);Move(first.hud.gameObject,scene,session.transform);
        foreach(var go in source.GetRootGameObjects())if(go.name=="Sol de amanecer"||go.name=="Ambiente de amanecer")Move(go,scene,session.transform);
        Transfer(source,scene,session,first,"Rincón de las canicas",new Vector3(-28,0,45),-12);EditorSceneManager.CloseScene(source,true);
        source=EditorSceneManager.OpenScene("Assets/Feria/Scenes/GaleriaPatitos.unity",OpenSceneMode.Additive);var ducks=Game(source);
        UnityEngine.Object.DestroyImmediate(ducks.player.gameObject);UnityEngine.Object.DestroyImmediate(ducks.cart.gameObject);UnityEngine.Object.DestroyImmediate(ducks.hud.gameObject);
        foreach(var go in source.GetRootGameObjects())if(go.name=="Sol de amanecer"||go.name=="Ambiente de amanecer")UnityEngine.Object.DestroyImmediate(go);
        Transfer(source,scene,session,ducks,"Poza de los patitos",new Vector3(28,0,45),12);EditorSceneManager.CloseScene(source,true);SceneManager.SetActiveScene(scene);
        session.zones=new[]{first,ducks};session.player.game=first;session.hud.game=first;session.hud.park=session;
        session.player.Place(layout.entrance,0,8);session.cart.SetPositionAndRotation(layout.cartStart,Quaternion.Euler(0,180,0));session.player.view.farClipPlane=300;
        decor=new GameObject("Paseos, plaza y jardines").transform;decor.SetParent(session.transform,false);font=session.hud.font;Materials();
        Ground();Roads(first,ducks);Plaza();Entrance();Merendero();Perimeter();Garden();Trees();Signs();
        RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=C("BBCED4");RenderSettings.ambientEquatorColor=C("DCCCB0");RenderSettings.ambientGroundColor=C("8F8B7C");RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=C("D6C1AA");RenderSettings.fogStartDistance=75;RenderSettings.fogEndDistance=230;
        PlayerSettings.bundleVersion="0.6.0";PlayerSettings.runInBackground=true;EditorUtility.SetDirty(layout);EditorUtility.SetDirty(session);
        EditorSceneManager.SaveScene(scene,Output);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Output,true)};AssetDatabase.SaveAssets();
        Debug.Log("FERIA_PARK_READY: parque continuo, sesión única y cuatro parcelas de ampliación.");
    }
    static ParkPlot Plot(string id,string label,Vector3 center,Vector3 connection)=>new ParkPlot{id=id,label=label,center=center,connection=connection,size=new Vector2(36,32)};
    static FairGame Game(Scene scene){foreach(var go in scene.GetRootGameObjects()){var game=go.GetComponentInChildren<FairGame>(true);if(game)return game;}throw new Exception("No hay puesto en "+scene.path);}
    static void Move(GameObject go,Scene scene,Transform parent){SceneManager.MoveGameObjectToScene(go,scene);go.transform.SetParent(parent,true);}
    static void Transfer(Scene source,Scene scene,ParkSession session,FairGame game,string name,Vector3 position,float yaw)
    {
        var root=new GameObject(name).transform;SceneManager.MoveGameObjectToScene(root.gameObject,scene);root.SetParent(session.transform,false);
        foreach(var go in source.GetRootGameObjects())Move(go,scene,root);
        var remove=new HashSet<string>{"Valla","Pasamanos","Valla trasera","Travesaño","Pradera","Tronco lejano","Copa tallada","Caseta lejana","Tejado de feria","Borde de piedra","Umbral de entrada","Paseo al siguiente puesto","Lateral"};
        foreach(var t in root.GetComponentsInChildren<Transform>(true))
        {
            if(!t)continue;if(remove.Contains(t.name)||(t.name=="Puesto"&&Mathf.Abs(t.position.x)>.5f))UnityEngine.Object.DestroyImmediate(t.gameObject);
        }
        root.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));game.park=session;game.origin=root;game.player=session.player;game.cart=session.cart;game.hud=session.hud;game.travelSign=null;game.name=name+" · tareas";
    }
    static Color C(string h){ColorUtility.TryParseHtmlString("#"+h,out var c);return c;}
    static Material Mat(string name,string color){string p=Art+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.color=C(color);m.SetFloat("_Smoothness",.14f);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
    static void Materials(){cream=Mat("Piedra marfil del parque","E9D9B8");teal=Mat("Petróleo del parque","326D72");coral=Mat("Coral del parque","CB806B");gold=Mat("Latón del parque","DFC171");wood=Mat("Madera de paseo","A07B58");ink=Mat("Tinta del parque","304C50");grass=Mat("Pradera del parque","A1B29B");leaves=Mat("Jardín de salvia","729985");path=Mat("Arena del paseo","C9BCA4");}
    static GameObject Box(string name,Vector3 p,Vector3 size,Material material,bool collider=false,Transform parent=null)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;if(!collider)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.transform.SetParent(parent?parent:decor,false);go.transform.localPosition=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;return go;
    }
    static GameObject Shape(string name,Mesh mesh,Material material,Vector3 p,Vector3 scale,Transform parent=null){var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent?parent:decor,false);go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=material;return go;}
    static void Ground(){Box("Suelo continuo del parque",new Vector3(0,-.18f,75),new Vector3(205,.30f,210),grass,true);}
    static void Roads(FairGame a,FairGame b)
    {
        Road("Paseo de entrada",new[]{new Vector3(0,0,-7),new Vector3(0,0,12),new Vector3(0,0,20)},5);
        Road("Camino a canicas",new[]{new Vector3(0,0,18),new Vector3(-10,0,24),new Vector3(-22,0,29),a.Entrance},4.8f);
        Road("Camino a patitos",new[]{new Vector3(0,0,18),new Vector3(10,0,24),new Vector3(22,0,29),b.Entrance},4.8f);
        Road("Paseo circular",new[]{a.Entrance,new Vector3(-40,0,42),new Vector3(-40,0,60),new Vector3(-21,0,76),new Vector3(0,0,79),new Vector3(21,0,76),new Vector3(40,0,60),new Vector3(40,0,42),b.Entrance},4.8f);
        Road("Jardín norte izquierdo",new[]{new Vector3(-21,0,76),new Vector3(-28,0,88),new Vector3(-28,0,97)},4.8f);
        Road("Jardín norte derecho",new[]{new Vector3(21,0,76),new Vector3(28,0,88),new Vector3(28,0,97)},4.8f);
        Road("Conexión oeste",new[]{new Vector3(-40,0,45),new Vector3(-61,0,45)},4.8f);Road("Conexión este",new[]{new Vector3(40,0,45),new Vector3(61,0,45)},4.8f);
    }
    static void Road(string name,Vector3[] points,float width)
    {
        var left=new Vector3[points.Length];var right=new Vector3[points.Length];var outerLeft=new Vector3[points.Length];var outerRight=new Vector3[points.Length];
        for(int i=0;i<points.Length;i++)
        {
            var incoming=(points[i]-points[Mathf.Max(0,i-1)]).normalized;var outgoing=(points[Mathf.Min(points.Length-1,i+1)]-points[i]).normalized;if(i==0)incoming=outgoing;if(i==points.Length-1)outgoing=incoming;
            var nextRight=Vector3.Cross(Vector3.up,outgoing).normalized;var normal=(Vector3.Cross(Vector3.up,incoming)+nextRight).normalized;float miter=1/Mathf.Max(.4f,Vector3.Dot(normal,nextRight));var center=points[i];center.y=.001f;
            left[i]=center-normal*(width*.5f)*miter;right[i]=center+normal*(width*.5f)*miter;outerLeft[i]=center-normal*(width*.5f+.18f)*miter;outerRight[i]=center+normal*(width*.5f+.18f)*miter;
        }
        var vertices=new List<Vector3>();var triangles=new List<int>();var borderVertices=new List<Vector3>();var borderTriangles=new List<int>();
        for(int i=0;i<points.Length-1;i++)
        {
            Tri(vertices,triangles,left[i],right[i+1],right[i]);Tri(vertices,triangles,left[i],left[i+1],right[i+1]);
            int samples=Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(points[i],points[i+1])));
            for(int sample=0;sample<samples;sample++)
            {
                float a=(float)sample/samples,b=(float)(sample+1)/samples;var middle=Vector3.Lerp(points[i],points[i+1],(a+b)*.5f);if(new Vector2(middle.x,middle.z-20).magnitude<9.8f)continue;
                bool inCourt=false;var session=decor.GetComponentInParent<ParkSession>();foreach(var zone in session.zones){var local=zone.Local(middle);if(Mathf.Abs(local.x)<7.2f&&local.z> -9&&local.z<9)inCourt=true;}if(inCourt)continue;
                var up=Vector3.up*.004f;var la=Vector3.Lerp(left[i],left[i+1],a)+up;var lb=Vector3.Lerp(left[i],left[i+1],b)+up;var oa=Vector3.Lerp(outerLeft[i],outerLeft[i+1],a)+up;var ob=Vector3.Lerp(outerLeft[i],outerLeft[i+1],b)+up;Tri(borderVertices,borderTriangles,oa,lb,la);Tri(borderVertices,borderTriangles,oa,ob,lb);
                la=Vector3.Lerp(right[i],right[i+1],a)+up;lb=Vector3.Lerp(right[i],right[i+1],b)+up;oa=Vector3.Lerp(outerRight[i],outerRight[i+1],a)+up;ob=Vector3.Lerp(outerRight[i],outerRight[i+1],b)+up;Tri(borderVertices,borderTriangles,la,ob,oa);Tri(borderVertices,borderTriangles,la,lb,ob);
            }
        }
        Shape(name,Store("Paseo · "+name,vertices,new[]{triangles}),path,Vector3.zero,Vector3.one);
        if(borderVertices.Count>0)Shape("Borde del paseo",Store("Borde · "+name,borderVertices,new[]{borderTriangles}),cream,Vector3.zero,Vector3.one);
    }
    [MenuItem("Feria/Pulir caminos del parque v0.6")]
    public static void PolishPaths()
    {
        if(EditorApplication.isPlaying)throw new Exception("Sal de Play.");var park=UnityEngine.Object.FindFirstObjectByType<ParkSession>();if(!park)throw new Exception("Abre ParquePrincipal.");decor=park.transform.Find("Paseos, plaza y jardines");font=park.hud.font;Materials();
        var names=new HashSet<string>{"Paseo de entrada","Camino a canicas","Camino a patitos","Paseo circular","Jardín norte izquierdo","Jardín norte derecho","Conexión oeste","Conexión este","Borde del paseo","Arboleda del paseo"};
        var remove=new List<GameObject>();foreach(Transform child in decor)if(names.Contains(child.name))remove.Add(child.gameObject);foreach(var go in remove)UnityEngine.Object.DestroyImmediate(go);
        Roads(park.zones[0],park.zones[1]);Trees();park.player.Place(park.definition.entrance,0,8);EditorSceneManager.MarkSceneDirty(park.gameObject.scene);EditorSceneManager.SaveScene(park.gameObject.scene);AssetDatabase.SaveAssets();
    }
    static void Trees()
    {
        var group=new GameObject("Arboleda del paseo").transform;group.SetParent(decor,false);var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Meshes/Follaje tallado.asset");
        foreach(var p in new[]{new Vector3(-18,0,36),new Vector3(18,0,36),new Vector3(-15,0,63),new Vector3(15,0,63),new Vector3(-44,0,26),new Vector3(44,0,26),new Vector3(-8,0,84),new Vector3(8,0,84)})
        {
            Box("Tronco de sombra",p+Vector3.up*1.8f,new Vector3(.32f,3.6f,.32f),wood,true,group);Shape("Árbol del paseo",mesh,leaves,p+Vector3.up*4.3f,new Vector3(2.9f,2.7f,2.7f),group);
        }
    }
    static void Plaza()
    {
        Shape("Plaza octogonal",Disc(16),path,new Vector3(0,-.005f,20),new Vector3(20,.05f,20));
        var kiosk=new GameObject("Kiosco de los faroles").transform;kiosk.SetParent(decor,false);kiosk.position=new Vector3(0,0,24);
        Shape("Tarima del kiosco",Disc(12),teal,new Vector3(0,.12f,0),new Vector3(4.2f,.24f,4.2f),kiosk);
        var roof=Shape("Cubierta de franjas",Roof(),cream,new Vector3(0,3.05f,0),Vector3.one,kiosk);roof.GetComponent<MeshRenderer>().sharedMaterials=new[]{cream,coral};
        for(int i=0;i<6;i++){float angle=i*Mathf.PI/3;Vector3 p=new Vector3(Mathf.Sin(angle)*1.7f,1.55f,Mathf.Cos(angle)*1.7f);Box("Columna del kiosco",p,new Vector3(.11f,3.0f,.11f),wood,true,kiosk);Lantern(kiosk.TransformPoint(new Vector3(p.x*.72f,2.55f,p.z*.72f)),kiosk);}
        Shape("Sol del kiosco",Disc(16),gold,new Vector3(0,4.48f,0),new Vector3(.7f,.14f,.7f),kiosk);
        Bench(new Vector3(-10,0,17),-30);Bench(new Vector3(10,0,17),30);
        Planter(new Vector3(-8,0,26));Planter(new Vector3(8,0,26));
    }
    static void Entrance()
    {
        for(int side=-1;side<=1;side+=2){Box("Columna de entrada",new Vector3(side*3,1.8f,-6),new Vector3(.22f,3.6f,.22f),wood,true);Box("Zapata de entrada",new Vector3(side*3,.35f,-6),new Vector3(.5f,.7f,.5f),teal,true);}
        Box("Rótulo de entrada",new Vector3(0,3.4f,-6),new Vector3(6.9f,.85f,.15f),teal);Text("Nombre del parque","DESPUÉS DE LA FERIA",new Vector3(0,3.42f,-6.09f),new Vector2(6.35f,.60f),2.8f,cream);
        var board=new GameObject("Plano de bienvenida").transform;board.SetParent(decor,false);board.position=new Vector3(5,0,1);board.rotation=Quaternion.Euler(0,-28,0);
        Box("Tablero del plano",new Vector3(0,1.52f,0),new Vector3(3.5f,2.5f,.12f),cream,false,board);for(int s=-1;s<=1;s+=2)Box("Pata del plano",new Vector3(s*1.25f,.65f,.08f),new Vector3(.1f,1.3f,.1f),wood,true,board);
        Text("Bienvenida","UNA MAÑANA PARA\nDEVOLVER EL COLOR",new Vector3(0,2.25f,-.08f),new Vector2(3.10f,.48f),1.4f,teal,board);
        MapLine(board,new Vector3(0,.65f,-.08f),new Vector3(0,1.10f,-.08f));MapLine(board,new Vector3(0,1.10f,-.08f),new Vector3(-1,1.50f,-.08f));MapLine(board,new Vector3(0,1.10f,-.08f),new Vector3(1,1.50f,-.08f));MapLine(board,new Vector3(-1,1.50f,-.08f),new Vector3(0,1.88f,-.08f));MapLine(board,new Vector3(1,1.50f,-.08f),new Vector3(0,1.88f,-.08f));
        Text("Mapa canicas","CANICAS",new Vector3(-1,1.66f,-.10f),new Vector2(1.3f,.25f),.8f,teal,board);Text("Mapa patitos","PATITOS",new Vector3(1,1.66f,-.10f),new Vector2(1.3f,.25f),.8f,teal,board);Text("Mapa plaza","PLAZA",new Vector3(0,.90f,-.10f),new Vector2(1.3f,.25f),.8f,teal,board);
        Text("Invitación","Sigue los caminos. Recupera los puestos.",new Vector3(0,.45f,-.10f),new Vector2(3.1f,.25f),.7f,ink,board);
    }
    static void MapLine(Transform parent,Vector3 a,Vector3 b){var line=new GameObject("Camino ilustrado").AddComponent<LineRenderer>();line.transform.SetParent(parent,false);line.useWorldSpace=false;line.positionCount=2;line.SetPositions(new[]{a,b});line.widthMultiplier=.04f;line.sharedMaterial=teal;}
    static void Merendero()
    {
        Shape("Descanso del paseo",Disc(16),path,new Vector3(0,-.005f,79),new Vector3(16,.05f,12));Bench(new Vector3(-5.5f,0,81),90);Bench(new Vector3(5.5f,0,81),-90);
        Box("Mesa de descanso",new Vector3(0,.85f,82),new Vector3(2.5f,.12f,1.2f),wood,true);for(int side=-1;side<=1;side+=2)Box("Pata de mesa",new Vector3(side*.9f,.42f,82),new Vector3(.12f,.84f,.8f),teal,true);
        Planter(new Vector3(-3,0,85));Planter(new Vector3(3,0,85));
    }
    static void Perimeter()
    {
        Fence(new Vector3(-52,0,-10),new Vector3(-4,0,-10));Fence(new Vector3(4,0,-10),new Vector3(52,0,-10));Gate(new Vector3(0,0,-10),0,"LA FERIA DESPIERTA");
        Fence(new Vector3(-52,0,-10),new Vector3(-52,0,41));Fence(new Vector3(-52,0,49),new Vector3(-52,0,88));Gate(new Vector3(-52,0,45),90,"JARDÍN DEL OESTE");
        Fence(new Vector3(52,0,-10),new Vector3(52,0,41));Fence(new Vector3(52,0,49),new Vector3(52,0,88));Gate(new Vector3(52,0,45),-90,"JARDÍN DEL ESTE");
        Fence(new Vector3(-52,0,88),new Vector3(-32,0,88));Fence(new Vector3(-24,0,88),new Vector3(24,0,88));Fence(new Vector3(32,0,88),new Vector3(52,0,88));Gate(new Vector3(-28,0,88),180,"PASEO NORTE");Gate(new Vector3(28,0,88),180,"PASEO NORTE");
    }
    static void Gate(Vector3 p,float yaw,string title){var gate=new GameObject("Acceso físico · "+title).transform;gate.SetParent(decor,false);gate.position=p;gate.rotation=Quaternion.Euler(0,yaw,0);Box("Puerta del jardín",new Vector3(0,.8f,0),new Vector3(8,1.6f,.12f),teal,true,gate);Text("Nombre del paseo",title,new Vector3(0,1.1f,-.08f),new Vector2(6.7f,.45f),2.1f,cream,gate);Text("Estado del acceso","En restauración",new Vector3(0,.55f,-.08f),new Vector2(5,.3f),1.2f,cream,gate);}
    static void Fence(Vector3 a,Vector3 b)
    {
        var direction=b-a;var mid=(a+b)*.5f;float length=direction.magnitude;
        for(int r=0;r<2;r++){var rail=Box("Cerca de paseo",mid+Vector3.up*(.4f+r*.45f),new Vector3(.09f,.10f,length),wood,true);rail.transform.rotation=Quaternion.LookRotation(direction);}
        int count=Mathf.CeilToInt(length/3);for(int i=0;i<=count;i++)Box("Poste del parque",Vector3.Lerp(a,b,(float)i/count)+Vector3.up*.55f,new Vector3(.14f,1.1f,.14f),teal,true);
    }
    static void Garden()
    {
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Meshes/Follaje tallado.asset");var random=new System.Random(32061);
        for(int i=0;i<90;i++)
        {
            bool side=i<50;float x=side?(i%2==0?-1:1)*(56+(float)random.NextDouble()*28):-86+(float)random.NextDouble()*172;float z=side?-6+(float)random.NextDouble()*116:96+(float)random.NextDouble()*64;float h=2.7f+(float)random.NextDouble()*2.5f;
            Box("Tronco del jardín",new Vector3(x,h*.4f,z),new Vector3(.25f,h*.8f,.25f),wood);Shape("Copa de salvia",mesh,leaves,new Vector3(x,h,z),new Vector3(2.4f,h*.7f,2.3f));
        }
        Vector3[] shrubs={new Vector3(-12,0,8),new Vector3(12,0,8),new Vector3(-17,0,19),new Vector3(17,0,19),new Vector3(-12,0,50),new Vector3(12,0,50),new Vector3(-17,0,61),new Vector3(17,0,61)};
        foreach(var p in shrubs){Shape("Isla de jardín",Disc(12),leaves,p+Vector3.down*.01f,new Vector3(8,.06f,6));for(int j=0;j<4;j++)Shape("Arbusto tallado",mesh,leaves,p+new Vector3(-2+j*1.3f,.65f,Mathf.Sin(j)*1.3f),new Vector3(1.2f,.85f,1));}
        for(int i=0;i<9;i++){float z=6+i*9;for(int side=-1;side<=1;side+=2)if(i<3||i>6)Lamp(new Vector3(side*5.8f,0,z));}
        foreach(var p in new[]{new Vector3(-42,0,52),new Vector3(42,0,52),new Vector3(-26,0,76),new Vector3(26,0,76)})Lamp(p);
    }
    static void Signs(){Signpost(new Vector3(0,0,13));Signpost(new Vector3(0,0,73));}
    static void Signpost(Vector3 p){var post=new GameObject("Señal de caminos").transform;post.SetParent(decor,false);post.position=p;Box("Poste de dirección",new Vector3(0,1.2f,0),new Vector3(.10f,2.4f,.10f),wood,true,post);foreach(int side in new[]{-1,1}){var arrow=Shape("Flecha física",Arrow(side),teal,new Vector3(side*.45f,1.65f+(side>0?.48f:0),0),new Vector3(2.8f,.40f,1),post);Text("Destino",side<0?"CANICAS":"PATITOS",new Vector3(side*.45f,arrow.transform.localPosition.y,-.02f),new Vector2(2.0f,.32f),1.4f,cream,post);}}
    static void Bench(Vector3 p,float yaw){var root=new GameObject("Banco del paseo").transform;root.SetParent(decor,false);root.position=p;root.rotation=Quaternion.Euler(0,yaw,0);Box("Asiento",new Vector3(0,.48f,0),new Vector3(2.2f,.10f,.6f),wood,true,root);for(int s=-1;s<=1;s+=2)Box("Pata",new Vector3(s*.8f,.22f,0),new Vector3(.12f,.44f,.55f),teal,true,root);for(int j=0;j<3;j++)Box("Listón",new Vector3(0,.70f+j*.13f,.26f),new Vector3(2.2f,.09f,.08f),wood,false,root);}
    static void Planter(Vector3 p){Box("Jardinera del parque",p+Vector3.up*.27f,new Vector3(1.7f,.54f,.9f),teal,true);var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Meshes/Follaje tallado.asset");for(int i=0;i<4;i++)Shape("Hojas de jardinera",mesh,leaves,p+new Vector3(-.6f+i*.4f,.85f,0),new Vector3(.4f,.55f,.4f));}
    static void Lamp(Vector3 p){Box("Farola del paseo",p+Vector3.up*1.5f,new Vector3(.12f,3,.12f),wood,true);Box("Brazo de farola",p+new Vector3(.25f,2.85f,0),new Vector3(.65f,.08f,.08f),wood);Lantern(p+new Vector3(.5f,2.55f,0),decor);}
    static void Lantern(Vector3 p,Transform parent){var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Meshes/Farol octogonal.asset");var lantern=Shape("Farol de papel",mesh,cream,Vector3.zero,new Vector3(.32f,.43f,.32f),parent);lantern.transform.position=p;}
    static void Text(string name,string text,Vector3 p,Vector2 size,float fontSize,Material color,Transform parent=null){var label=new GameObject(name).AddComponent<TextMeshPro>();label.transform.SetParent(parent?parent:decor,false);label.transform.localPosition=p;label.font=font;label.text=text;label.fontSize=fontSize;label.color=color.color;label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=size;}
    static void Tri(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c){int n=v.Count;v.AddRange(new[]{a,b,c});t.AddRange(new[]{n,n+1,n+2});}
    static Mesh Store(string name,List<Vector3> v,List<int>[] sub){string p=Art+"/Meshes/"+name+".asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(!m){m=new Mesh{name=name};AssetDatabase.CreateAsset(m,p);}else m.Clear();m.SetVertices(v);m.subMeshCount=sub.Length;for(int i=0;i<sub.Length;i++)m.SetTriangles(sub[i],i);m.RecalculateNormals();m.RecalculateBounds();EditorUtility.SetDirty(m);return m;}
    static Mesh Disc(int sides){string name="Tarima de "+sides+" lados";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Meshes/"+name+".asset");if(existing)return existing;var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;var aa=new Vector3(Mathf.Cos(a)*.5f,.5f,Mathf.Sin(a)*.5f);var bb=new Vector3(Mathf.Cos(b)*.5f,.5f,Mathf.Sin(b)*.5f);Tri(v,t,new Vector3(0,.5f,0),bb,aa);Tri(v,t,aa,bb,bb-Vector3.up);Tri(v,t,aa,bb-Vector3.up,aa-Vector3.up);Tri(v,t,new Vector3(0,-.5f,0),aa-Vector3.up,bb-Vector3.up);}return Store(name,v,new[]{t});}
    static Mesh Roof(){var v=new List<Vector3>();var sub=new[]{new List<int>(),new List<int>()};for(int i=0;i<12;i++){float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;Tri(v,sub[i%2],new Vector3(0,1.2f,0),new Vector3(Mathf.Cos(b)*2.6f,0,Mathf.Sin(b)*2.6f),new Vector3(Mathf.Cos(a)*2.6f,0,Mathf.Sin(a)*2.6f));Tri(v,sub[i%2],new Vector3(0,1.2f,0),new Vector3(Mathf.Cos(a)*2.6f,0,Mathf.Sin(a)*2.6f),new Vector3(Mathf.Cos(b)*2.6f,0,Mathf.Sin(b)*2.6f));}return Store("Cubierta del kiosco",v,sub);}
    static Mesh Arrow(int side){var v=new List<Vector3>();var t=new List<int>();var p=new[]{new Vector3(-.5f,-.35f,0),new Vector3(.2f,-.35f,0),new Vector3(.2f,-.5f,0),new Vector3(.5f,0,0),new Vector3(.2f,.5f,0),new Vector3(.2f,.35f,0),new Vector3(-.5f,.35f,0)};for(int i=0;i<p.Length;i++){var a=p[i];var b=p[(i+1)%p.Length];a.x*=side;b.x*=side;if(side>0)Tri(v,t,Vector3.zero,b,a);else Tri(v,t,Vector3.zero,a,b);}return Store("Señal "+(side>0?"derecha":"izquierda"),v,new[]{t});}
}
