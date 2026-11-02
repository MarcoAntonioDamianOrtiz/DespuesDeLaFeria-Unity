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
    static Material cream,teal,coral,gold,wood,ink,grass,leaves,path,grassLight,soil,leafLitter;
    static TMP_FontAsset font;
    static readonly Vector2[] CounterStains={new Vector2(-2.5f,3.84f),new Vector2(-1.8f,3.86f),new Vector2(-1.1f,3.82f),new Vector2(-0.4f,3.85f),new Vector2(0.35f,3.83f),new Vector2(1.05f,3.86f),new Vector2(1.78f,3.84f),new Vector2(2.5f,3.82f),new Vector2(-2.46f,4.31f),new Vector2(-1.74f,4.33f),new Vector2(-1.04f,4.29f),new Vector2(-0.34f,4.32f),new Vector2(0.4f,4.3f),new Vector2(1.1f,4.33f),new Vector2(1.83f,4.31f),new Vector2(2.54f,4.29f)};
    [MenuItem("Feria/Construir parque continuo v0.10")]
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
        Ground();Roads(first,ducks);Plaza();Entrance();Merendero();Perimeter();Terrain();MeadowEdges(first,ducks);Garden();Trees();Signs();
        RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=C("B7D0DA");RenderSettings.ambientEquatorColor=C("E1C9A9");RenderSettings.ambientGroundColor=C("96947D");RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=C("DFC7B1");RenderSettings.fogStartDistance=68;RenderSettings.fogEndDistance=220;
        PlayerSettings.bundleVersion="0.10.0";PlayerSettings.runInBackground=true;EditorUtility.SetDirty(layout);EditorUtility.SetDirty(session);
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
        root.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));game.park=session;game.origin=root;ApplyCounterStains(game,root);game.player=session.player;game.cart=session.cart;game.hud=session.hud;game.travelSign=null;game.name=name+" · tareas";
    }
    [MenuItem("Feria/Pulir paisaje y mostradores v0.10")]
    public static void PolishExistingPark()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Sal de Play antes de pulir el parque.");
        EditorSceneManager.OpenScene(Output,OpenSceneMode.Single);var session=UnityEngine.Object.FindFirstObjectByType<ParkSession>();if(!session)throw new Exception("Abre ParquePrincipal.");
        decor=session.transform.Find("Paseos, plaza y jardines");if(!decor)throw new Exception("Falta la raíz del paisaje del parque.");font=session.hud.font;Materials();
        var ground=decor.Find("Suelo continuo del parque");if(ground){ground.localPosition=new Vector3(0,-.18f,75);ground.localScale=new Vector3(290,.30f,290);}
        RepositionGardenTrees();Terrain();MeadowEdges(session.zones[0],session.zones[1]);
        foreach(var zone in session.zones)if(zone&&zone.origin)ApplyCounterStains(zone,zone.origin);
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=C("B7D0DA");RenderSettings.ambientEquatorColor=C("E1C9A9");RenderSettings.ambientGroundColor=C("96947D");RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=C("DFC7B1");RenderSettings.fogStartDistance=68;RenderSettings.fogEndDistance=220;
        var sun=GameObject.Find("Sol de amanecer");if(sun){var light=sun.GetComponent<Light>();if(light){light.color=C("FFE3C1");light.intensity=1.5f;}}
        PlayerSettings.bundleVersion="0.10.0";EditorSceneManager.MarkSceneDirty(session.gameObject.scene);EditorSceneManager.SaveScene(session.gameObject.scene);AssetDatabase.SaveAssets();
        Debug.Log("FERIA_AMBIENCE_READY: lomas, horizonte, árboles despejados y manchas orgánicas en los dos puestos.");
    }
    static void RepositionGardenTrees()
    {
        var trunks=new List<Transform>();var crowns=new List<Transform>();foreach(Transform child in decor){if(child.name=="Tronco del jardín")trunks.Add(child);else if(child.name=="Copa de salvia")crowns.Add(child);}
        int count=Mathf.Min(trunks.Count,crowns.Count);var random=new System.Random(32061);
        for(int i=0;i<count;i++)
        {
            float side=i%2==0?-1:1;float x=side*(59+(float)random.NextDouble()*31);float z;
            if(i<50){int band=(i/2)%4;z=band==0?-14+(float)random.NextDouble()*34:band==1?66+(float)random.NextDouble()*30:band==2?96+(float)random.NextDouble()*37:137+(float)random.NextDouble()*38;}
            else z=82+(float)random.NextDouble()*94;
            float h=2.7f+(float)random.NextDouble()*2.5f;trunks[i].localPosition=new Vector3(x,h*.4f,z);crowns[i].localPosition=new Vector3(x,h,z);
        }
    }
    static void ApplyCounterStains(FairGame game,Transform root)
    {
        if(game.dirtPatches==null||game.dirtPatches.Length!=CounterStains.Length||(game.definition.zoneId!="rincon-canicas"&&game.definition.zoneId!="galeria-patitos"))return;
        for(int i=0;i<game.dirtPatches.Length;i++)
        {
            var patch=game.dirtPatches[i];patch.name="Mancha orgánica "+(i+1);patch.transform.SetParent(root,false);patch.transform.localPosition=new Vector3(CounterStains[i].x,1.135f,CounterStains[i].y);patch.transform.localRotation=Quaternion.Euler(0,(i*37)%29-14,0);patch.transform.localScale=new Vector3(.20f+(i%3)*.02f,.008f,.10f+(i%4)*.008f);
            patch.GetComponent<MeshFilter>().sharedMesh=StainMesh(i);
        }
    }
    static Mesh StainMesh(int index)
    {
        string pathName=Art+"/Meshes/Mancha orgánica "+index.ToString("00")+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(pathName);if(mesh)return mesh;
        const int sides=11;var vertices=new List<Vector3>{new(0,.5f,0)};var triangles=new List<int>();var random=new System.Random(931+index);
        for(int i=0;i<sides;i++){float angle=Mathf.PI*2*i/sides;float lobe=.78f+(float)random.NextDouble()*.30f;float ripple=.10f*Mathf.Sin(angle*3+index*.7f)+.06f*Mathf.Sin(angle*5-index*.31f);float radius=Mathf.Clamp(lobe+ripple,.62f,1.14f);vertices.Add(new Vector3(Mathf.Cos(angle)*radius,.5f,Mathf.Sin(angle)*radius));}
        for(int i=0;i<sides;i++){triangles.Add(0);triangles.Add(1+(i+1)%sides);triangles.Add(1+i);}
        mesh=new Mesh{name="Mancha irregular "+index.ToString("00")};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,pathName);return mesh;
    }
    static Color C(string h){ColorUtility.TryParseHtmlString("#"+h,out var c);return c;}
    static Material Mat(string name,string color){string p=Art+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.color=C(color);m.SetFloat("_Smoothness",.14f);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
    static void Materials(){cream=Mat("Piedra marfil del parque","E9D9B8");teal=Mat("Petróleo del parque","326D72");coral=Mat("Coral del parque","CB806B");gold=Mat("Latón del parque","DFC171");wood=Mat("Madera de paseo","A07B58");ink=Mat("Tinta del parque","304C50");grass=Mat("Pradera del parque","A1B29B");grassLight=Mat("Hierba dorada del parque","B2B18A");soil=Mat("Tierra oscura del parque","877960");leafLitter=Mat("Hojarasca del parque","A18F77");leaves=Mat("Jardín de salvia","729985");path=Mat("Arena del paseo","C9BCA4");}
    static GameObject Box(string name,Vector3 p,Vector3 size,Material material,bool collider=false,Transform parent=null)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;if(!collider)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.transform.SetParent(parent?parent:decor,false);go.transform.localPosition=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;return go;
    }
    static GameObject Shape(string name,Mesh mesh,Material material,Vector3 p,Vector3 scale,Transform parent=null){var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent?parent:decor,false);go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=material;return go;}
    static void Ground(){Box("Suelo continuo del parque",new Vector3(0,-.18f,75),new Vector3(290,.30f,290),grass,true);}
    static void Terrain()
    {
        // Soft, walkable berms give the park a natural edge while keeping every future attraction plot empty.
        var previous=decor.Find("Relieve suave del borde");if(previous)UnityEngine.Object.DestroyImmediate(previous.gameObject);var terrain=new GameObject("Relieve suave del borde").transform;terrain.SetParent(decor,false);
        Vector4[] mounds={new(-64,0,9,1.05f),new(64,0,9,1.15f),new(-66,0,78,1.20f),new(66,0,78,1.15f),new(-88,0,-5,1.45f),new(88,0,-5,1.35f),new(-91,0,80,1.45f),new(91,0,80,1.25f),new(-90,0,102,1.45f),new(90,0,102,1.35f),new(-86,0,144,1.8f),new(86,0,144,1.65f),new(-72,0,168,1.55f),new(72,0,168,1.55f),new(-35,0,190,2.0f),new(35,0,190,1.8f)};
        float[] radii={6.5f,6.5f,7f,7f,9f,9f,8f,8f,8f,8f,11f,11f,10f,10f,15f,15f};
        for(int i=0;i<mounds.Length;i++)
        {
            var mound=new GameObject("Loma suave del borde",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));mound.transform.SetParent(terrain,false);mound.transform.localPosition=new Vector3(mounds[i].x,0,mounds[i].z);mound.transform.localScale=new Vector3(radii[i],mounds[i].w,radii[i]);
            var mesh=MoundMesh(i%4);mound.GetComponent<MeshFilter>().sharedMesh=mesh;mound.GetComponent<MeshRenderer>().sharedMaterials=new[]{grass,grassLight,soil};mound.GetComponent<MeshCollider>().sharedMesh=mesh;
            if(i%2==0){var foliage=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Meshes/Follaje tallado.asset");var bush=Shape("Matorral de la loma",foliage,leaves,new Vector3(mounds[i].x, mounds[i].w*.63f,mounds[i].z),new Vector3(1.3f,.8f,1.2f),terrain);bush.transform.rotation=Quaternion.Euler(0,i*37,0);}
        }
    }
    static void MeadowEdges(FairGame first,FairGame second)
    {
        var terrain=decor.Find("Relieve suave del borde");if(!terrain)return;var group=terrain.Find("Islas de hierba y hojarasca");if(group)UnityEngine.Object.DestroyImmediate(group.gameObject);group=new GameObject("Islas de hierba y hojarasca").transform;group.SetParent(terrain,false);
        var roads=new (Vector3[] points,float width)[]
        {
            (new[]{new Vector3(0,0,-7),new Vector3(0,0,12),new Vector3(0,0,20)},5),
            (new[]{new Vector3(0,0,18),new Vector3(-10,0,24),new Vector3(-22,0,29),first.Entrance},4.8f),
            (new[]{new Vector3(0,0,18),new Vector3(10,0,24),new Vector3(22,0,29),second.Entrance},4.8f),
            (new[]{first.Entrance,new Vector3(-40,0,42),new Vector3(-40,0,60),new Vector3(-21,0,76),new Vector3(0,0,79),new Vector3(21,0,76),new Vector3(40,0,60),new Vector3(40,0,42),second.Entrance},4.8f),
            (new[]{new Vector3(-21,0,76),new Vector3(-28,0,88),new Vector3(-28,0,97)},4.8f),
            (new[]{new Vector3(21,0,76),new Vector3(28,0,88),new Vector3(28,0,97)},4.8f),
            (new[]{new Vector3(-40,0,45),new Vector3(-61,0,45)},4.8f),
            (new[]{new Vector3(40,0,45),new Vector3(61,0,45)},4.8f)
        };
        var session=decor.GetComponentInParent<ParkSession>();var placed=new List<Vector3>();var random=new System.Random(8412);int index=0;
        foreach(var road in roads)for(int segment=0;segment<road.points.Length-1;segment++)
        {
            Vector3 delta=road.points[segment+1]-road.points[segment];delta.y=0;float length=delta.magnitude;int steps=Mathf.Max(1,Mathf.CeilToInt(length/4.2f));Vector3 side=Vector3.Cross(Vector3.up,delta.normalized);
            for(int step=1;step<steps;step++)
            {
                Vector3 center=Vector3.Lerp(road.points[segment],road.points[segment+1],(float)step/steps);
                foreach(int sign in new[]{-1,1})
                {
                    Vector3 point=center+side*(road.width*.5f+1.25f)*sign;point.y=-.028f;if(ForbiddenMeadow(point,session)||placed.Exists(p=>(p-point).sqrMagnitude<7.5f))continue;placed.Add(point);
                    int variant=index%4;float patchWidth=.95f+(float)random.NextDouble()*.55f,patchDepth=.64f+(float)random.NextDouble()*.35f;if(variant==2){patchWidth*=.58f;patchDepth*=.68f;}var patch=Shape("Isla baja de hierba",MeadowMesh(variant),variant==0?grassLight:variant==1?leaves:variant==2?leafLitter:grass,point,new Vector3(patchWidth,.006f,patchDepth),group);patch.transform.rotation=Quaternion.Euler(0,(float)random.NextDouble()*180,0);var patchRenderer=patch.GetComponent<MeshRenderer>();patchRenderer.shadowCastingMode=ShadowCastingMode.Off;patchRenderer.receiveShadows=false;
                    if(index%4==0){var foliage=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Meshes/Follaje tallado.asset");var tuft=Shape("Hierba de otoño tallada",foliage,leaves,point+Vector3.up*.035f,new Vector3(.20f,.28f,.19f),group);tuft.transform.rotation=Quaternion.Euler(0,(float)random.NextDouble()*360,0);}
                    index++;
                }
            }
        }
    }
    static bool ForbiddenMeadow(Vector3 p,ParkSession session)
    {
        if(new Vector2(p.x,p.z-20).magnitude<13)return true;
        foreach(var zone in session.zones){var local=zone.Local(p);if(Mathf.Abs(local.x)<9&&local.z>-11&&local.z<13)return true;}
        foreach(var plot in session.definition.futurePlots)if(Mathf.Abs(p.x-plot.center.x)<plot.size.x*.5f+1&&Mathf.Abs(p.z-plot.center.z)<plot.size.y*.5f+1)return true;
        return false;
    }
    static Mesh MeadowMesh(int variant)
    {
        string pathName=Art+"/Meshes/Pradera irregular "+variant+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(pathName);if(existing)return existing;
        const int sides=9;var vertices=new List<Vector3>{new(0,.5f,0)};var triangles=new List<int>();var random=new System.Random(6370+variant);
        for(int i=0;i<sides;i++){float angle=Mathf.PI*2*i/sides;float radius=.42f+((float)random.NextDouble()*.16f)+.035f*Mathf.Sin(angle*3+variant);vertices.Add(new Vector3(Mathf.Cos(angle)*radius,.5f,Mathf.Sin(angle)*radius));}
        for(int i=0;i<sides;i++){triangles.Add(0);triangles.Add(1+(i+1)%sides);triangles.Add(1+i);}
        var mesh=new Mesh{name="Pradera irregular "+variant};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,pathName);return mesh;
    }
    static Mesh MoundMesh(int variant)
    {
        string pathName=Art+"/Meshes/Loma natural "+variant+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(pathName);if(existing)return existing;
        const int sides=10;float[] radii={.23f,.52f,.80f,1f},heights={.98f,.79f,.43f,.025f};var vertices=new List<Vector3>{new(0,1,0)};var sub=new[]{new List<int>(),new List<int>(),new List<int>()};var random=new System.Random(4200+variant);
        for(int ring=0;ring<radii.Length;ring++)for(int i=0;i<sides;i++)
        {
            float angle=Mathf.PI*2*i/sides;float jagged=.88f+(float)random.NextDouble()*.22f;float wave=1+.045f*Mathf.Sin(angle*3+variant);float r=radii[ring]*jagged*wave;
            vertices.Add(new Vector3(Mathf.Cos(angle)*r,heights[ring]*(.90f+(float)random.NextDouble()*.20f),Mathf.Sin(angle)*r));
        }
        for(int i=0;i<sides;i++)Tri(vertices,sub[0],vertices[0],vertices[1+(i+1)%sides],vertices[1+i]);
        for(int ring=1;ring<radii.Length;ring++)for(int i=0;i<sides;i++)
        {
            int inner=1+(ring-1)*sides+i,innerNext=1+(ring-1)*sides+(i+1)%sides,outer=1+ring*sides+i,outerNext=1+ring*sides+(i+1)%sides;int material=ring==radii.Length-1?2:ring==1?0:1;
            Tri(vertices,sub[material],vertices[inner],vertices[outerNext],vertices[outer]);Tri(vertices,sub[material],vertices[inner],vertices[innerNext],vertices[outerNext]);
        }
        var mesh=new Mesh{name="Loma natural "+variant};mesh.SetVertices(vertices);mesh.subMeshCount=sub.Length;for(int i=0;i<sub.Length;i++)mesh.SetTriangles(sub[i],i);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,pathName);return mesh;
    }
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
            float side=i%2==0?-1:1;float x=side*(59+(float)random.NextDouble()*31);float z;
            if(i<50){int band=(i/2)%4;z=band==0?-14+(float)random.NextDouble()*34:band==1?66+(float)random.NextDouble()*30:band==2?96+(float)random.NextDouble()*37:137+(float)random.NextDouble()*38;}
            else z=82+(float)random.NextDouble()*94;
            float h=2.7f+(float)random.NextDouble()*2.5f;Box("Tronco del jardín",new Vector3(x,h*.4f,z),new Vector3(.25f,h*.8f,.25f),wood);Shape("Copa de salvia",mesh,leaves,new Vector3(x,h,z),new Vector3(2.4f,h*.7f,2.3f));
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
