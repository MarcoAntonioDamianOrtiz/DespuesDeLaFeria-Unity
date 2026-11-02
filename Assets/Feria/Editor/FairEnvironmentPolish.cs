using System;
using System.Collections.Generic;
using System.Linq;
using Feria;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Original modular park furniture, authored in metres. Existing gameplay objects retain their identities.</summary>
public static class FairEnvironmentPolish
{
    const string Art="Assets/Feria/Art/Original/ParqueArtesanal",Prefabs="Assets/Feria/Prefabs/ParqueArtesanal";
    static Material wood,metal,brass,cream,leaf,leafLight,bark,glass,flower;
    static Material ColorMat(string name,string hex,float smooth=.2f)
    {
        string path=Art+"/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
        ColorUtility.TryParseHtmlString("#"+hex,out var color);mat.color=color;mat.SetFloat("_Smoothness",smooth);mat.enableInstancing=true;return mat;
    }
    [MenuItem("Feria/Pulido integral del parque v0.11")]
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new Exception("Sal de Play antes de editar.");var park=UnityEngine.Object.FindFirstObjectByType<ParkSession>();if(!park)throw new Exception("Abre ParquePrincipal.");
        System.IO.Directory.CreateDirectory(Art);System.IO.Directory.CreateDirectory(Prefabs);AssetDatabase.Refresh();
        wood=ColorMat("Roble miel","AA7954");metal=ColorMat("Hierro petróleo","315959",.32f);brass=ColorMat("Latón cálido","CBA55C",.38f);cream=ColorMat("Esmalte marfil","EADAB5");bark=ColorMat("Corteza tallada","806246");leaf=ColorMat("Salvia profunda","648B72");leafLight=ColorMat("Olivo dorado","A9AD76");flower=ColorMat("Flores coral","CB9179");glass=ColorMat("Papel luminoso","F4DDA3");glass.EnableKeyword("_EMISSION");glass.SetColor("_EmissionColor",new Color(.28f,.19f,.07f));
        for(int v=0;v<3;v++){SaveBench(v);SaveTree(v);SaveBush(v);}for(int v=0;v<2;v++)SaveLamp(v);SaveBin();SaveCloth();
        if(park.player.cloth&&!park.player.cloth.Find("Paño artesanal"))
        {
            foreach(Transform child in park.player.cloth)child.gameObject.SetActive(false);
            Instance("Pano_Artesanal",park.player.cloth,Vector3.zero,Quaternion.identity,Vector3.one,"Paño artesanal");
        }
        int benches=0,lamps=0,trees=0,bushes=0;
        var all=park.GetComponentsInChildren<Transform>(true);
        foreach(var root in all.Where(t=>t.name=="Banco del paseo"||t.name=="Banco"))
        {
            if(!root.gameObject.activeSelf)continue;
            if(root.name=="Banco")
            {
                var foot=root.localPosition-Vector3.up*.5f;float yaw=foot.x<0?-90:90;
                Instance("Banco_"+(benches++%3),root.parent,foot,Quaternion.Euler(0,yaw,0),Vector3.one,"Banco artesanal del puesto");
                foreach(var old in all)if(old.parent==root.parent&&old.name=="Pata banco"&&Vector3.Distance(old.position,root.position)<1.2f)old.gameObject.SetActive(false);
                root.gameObject.SetActive(false);continue;
            }
            if(root.Find("Diseño artesanal"))continue;foreach(Transform child in root)child.gameObject.SetActive(false);
            Instance("Banco_"+(benches++%3),root,Vector3.zero,Quaternion.identity,Vector3.one,"Diseño artesanal");
        }
        foreach(var post in all.Where(t=>t.name=="Farola del paseo"&&t.gameObject.activeSelf))
        {
            var foot=post.position-Vector3.up*1.5f;
            foreach(var old in all)if(old.parent==post.parent&&(old.name=="Brazo de farola"||old.name=="Farol de papel")&&Vector3.Distance(old.position,foot+new Vector3(.4f,2.65f,0))<.8f)old.gameObject.SetActive(false);
            Instance("Farol_"+(lamps++%2),post.parent,post.parent.InverseTransformPoint(foot),Quaternion.identity,Vector3.one,"Farola artesanal");post.gameObject.SetActive(false);
        }
        foreach(var post in all.Where(t=>t.name=="Farola de madera"&&t.gameObject.activeSelf))
        {
            var foot=post.localPosition-Vector3.up*1.4f;float side=foot.x<0?1:-1;var center=post.parent.TransformPoint(foot+new Vector3(side*.4f,2.6f,0));
            foreach(var old in all)if(old.parent==post.parent&&(old.name=="Brazo de farola"||old.name=="Farol de papel"||old.name=="Tapa de farol")&&Vector3.Distance(old.position,center)<.9f)old.gameObject.SetActive(false);
            Instance("Farol_"+(lamps++%2),post.parent,foot,Quaternion.Euler(0,side<0?180:0,0),Vector3.one,"Farol artesanal del puesto");post.gameObject.SetActive(false);
        }
        foreach(var crown in all.Where(t=>(t.name=="Copa de salvia"||t.name=="Árbol del paseo")&&t.gameObject.activeSelf))
        {
            string trunkName=crown.name=="Copa de salvia"?"Tronco del jardín":"Tronco de sombra";
            var trunk=all.FirstOrDefault(t=>t.name==trunkName&&t.parent==crown.parent&&Mathf.Abs(t.position.x-crown.position.x)<.05f&&Mathf.Abs(t.position.z-crown.position.z)<.05f);
            if(!trunk)continue;float height=trunk.lossyScale.y;var foot=new Vector3(trunk.position.x,trunk.position.y-height*.5f,trunk.position.z);
            float scale=Mathf.Clamp((crown.position.y-foot.y+1.5f)/5.4f,.75f,1.4f);
            Instance("Arbol_"+(trees%3),crown.parent,crown.parent.InverseTransformPoint(foot),Quaternion.Euler(0,trees*137.5f,0),Vector3.one*scale,"Árbol artesanal "+trees);trees++;crown.gameObject.SetActive(false);trunk.gameObject.SetActive(false);
        }
        foreach(var bush in all.Where(t=>t.name=="Arbusto tallado"&&t.gameObject.activeSelf))
        {
            var foot=bush.position-Vector3.up*.5f;Instance("Arbusto_"+(bushes%3),bush.parent,bush.parent.InverseTransformPoint(foot),Quaternion.Euler(0,bushes*71,0),Vector3.one,"Jardín artesanal "+bushes);bushes++;bush.gameObject.SetActive(false);
        }
        var landscape=park.transform.Find("Paseos, plaza y jardines");
        if(!landscape.Find("Detalles artesanales"))
        {
            var details=new GameObject("Detalles artesanales").transform;details.SetParent(landscape,false);
            foreach(var point in new[]{new Vector3(-9,0,18),new Vector3(9,0,18),new Vector3(-4,0,82),new Vector3(4,0,82)})Instance("Papelera",details,point,Quaternion.identity,Vector3.one,"Papelera del paseo");
        }
        var bevel=new Builder(new[]{wood});bevel.Plank(Vector3.zero,Vector3.one,0);var softBox=bevel.Store("Tablón biselado");
        foreach(var prop in all.Where(t=>t.name=="Mesa de descanso"||t.name=="Jardinera del parque"||t.name=="Poste del parque"))
        {var filter=prop.GetComponent<MeshFilter>();if(filter)filter.sharedMesh=softBox;}
        foreach(var path in all.Where(t=>t.GetComponent<MeshFilter>()&&(t.name.StartsWith("Camino a")||t.name=="Paseo de entrada"||t.name=="Paseo circular"||t.name=="Plaza octogonal"||t.name=="Descanso del paseo"||t.name=="Explanada")))
        {
            if(!path.GetComponent<Collider>()){var col=path.gameObject.AddComponent<MeshCollider>();col.sharedMesh=path.GetComponent<MeshFilter>().sharedMesh;}
            var marker=path.GetComponent<FairGroundSurface>();if(!marker)marker=path.gameObject.AddComponent<FairGroundSurface>();marker.surface=path.name=="Explanada"?WalkSurface.Wood:path.name=="Plaza octogonal"||path.name=="Descanso del paseo"?WalkSurface.Stone:WalkSurface.Earth;
        }
        var sun=GameObject.Find("Sol de amanecer");if(sun){var light=sun.GetComponent<Light>();light.color=new Color(1,.88f,.72f);light.intensity=1.45f;}
        PlayerSettings.bundleVersion="0.11.0";EditorSceneManager.MarkSceneDirty(park.gameObject.scene);EditorSceneManager.SaveScene(park.gameObject.scene);AssetDatabase.SaveAssets();
        return $"Diseños incorporados: {benches} bancas, {lamps} farolas, {trees} árboles, {bushes} arbustos. Originales conservados e inactivos.";
    }
    static void Instance(string asset,Transform parent,Vector3 p,Quaternion r,Vector3 s,string name)
    {var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+"/"+asset+".prefab");var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);instance.name=name;instance.transform.SetLocalPositionAndRotation(p,r);instance.transform.localScale=s;}
    static GameObject Visual(string name,Builder geometry)
    {var root=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));root.GetComponent<MeshFilter>().sharedMesh=geometry.Store(name);var renderer=root.GetComponent<MeshRenderer>();renderer.sharedMaterials=geometry.materials;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;return root;}
    static void Save(GameObject root,string name){PrefabUtility.SaveAsPrefabAsset(root,Prefabs+"/"+name+".prefab");UnityEngine.Object.DestroyImmediate(root);}
    static void SaveBench(int variant)
    {
        var b=new Builder(new[]{variant==2?cream:wood,metal,brass});
        for(int i=0;i<5;i++)b.Plank(new Vector3(0,.48f,-.25f+i*.12f),new Vector3(2.24f,.075f,.102f),0);
        if(variant==1)for(int i=0;i<8;i++)b.Plank(new Vector3(-.97f+i*.277f,.85f,.29f),new Vector3(.23f,.5f+.10f*Mathf.Sin(i*Mathf.PI/7),.075f),0);
        else for(int i=0;i<3;i++)b.Plank(new Vector3(0,.72f+i*.15f,.28f+i*.025f),new Vector3(2.24f,.125f,.07f),0);
        foreach(float side in new[]{-.84f,.84f})
        {
            foreach(float z in new[]{-.22f,.22f}){b.Tube(new Vector3(side,.04f,z*1.18f),new Vector3(side,.46f,z),.042f,.033f,1);b.Plank(new Vector3(side,.035f,z*1.18f),new Vector3(.17f,.04f,.14f),1);}
            b.Tube(new Vector3(side,.43f,.24f),new Vector3(side,1.1f,.37f),.032f,.028f,1);
            Vector3[] arm={new Vector3(side,.48f,-.22f),new Vector3(side,.69f,-.20f),new Vector3(side,.73f,.03f),new Vector3(side,.72f,.30f)};
            for(int i=0;i<arm.Length-1;i++)b.Tube(arm[i],arm[i+1],.035f,.035f,1);
            b.Plank(new Vector3(side,.737f,.01f),new Vector3(.115f,.045f,.38f),0);
            for(int i=0;i<3;i++)b.Tube(new Vector3(side,.72f+i*.15f,.23f),new Vector3(side,.72f+i*.15f,.215f),.025f,.025f,2);
        }
        var root=Visual("Banco_"+variant,b);var seat=root.AddComponent<BoxCollider>();seat.center=new Vector3(0,.31f,0);seat.size=new Vector3(2.24f,.63f,.6f);var back=root.AddComponent<BoxCollider>();back.center=new Vector3(0,.86f,.3f);back.size=new Vector3(2.24f,.55f,.13f);Save(root,"Banco_"+variant);
    }
    static void SaveLamp(int variant)
    {
        var b=new Builder(new[]{metal,brass,glass});b.Tube(Vector3.zero,Vector3.up*.12f,.23f,.22f,0);b.Tube(Vector3.up*.12f,Vector3.up*.30f,.15f,.10f,0);b.Tube(Vector3.up*.3f,Vector3.up*3.05f,.065f,.045f,0);
        for(int i=0;i<3;i++)b.Tube(Vector3.up*(.32f+i*1.15f),Vector3.up*(.36f+i*1.15f),.086f,.086f,1);
        Vector3[] neck={new(0,2.95f,0),new(.08f,3.16f,0),new(.32f,3.25f,0),new(.58f,3.18f,0),new(.62f,3.02f,0)};for(int i=0;i<neck.Length-1;i++)b.Tube(neck[i],neck[i+1],.034f,.034f,0);
        var c=new Vector3(.62f,2.64f,0);float radius=variant==0?.19f:.23f;
        b.Tube(c-Vector3.up*.23f,c+Vector3.up*.23f,radius*.82f,radius,2);
        b.Tube(c+Vector3.up*.23f,c+Vector3.up*.39f,radius*1.3f,.045f,0);b.Tube(c-Vector3.up*.29f,c-Vector3.up*.23f,radius*.6f,radius*1.1f,1);
        for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var edge=new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);b.Tube(c+edge-Vector3.up*.22f,c+edge+Vector3.up*.23f,.012f,.012f,0);}
        var root=Visual("Farol_"+variant,b);var col=root.AddComponent<CapsuleCollider>();col.center=Vector3.up*1.5f;col.height=3;col.radius=.10f;Save(root,"Farol_"+variant);
    }
    static void SaveTree(int variant)
    {
        var root=new GameObject("Arbol_"+variant);var stem=new Builder(new[]{bark});stem.Tube(Vector3.zero,new Vector3(.09f,2.65f,0),.24f,.10f,0);
        for(int i=0;i<4;i++){float a=i*2.3f+variant;stem.Tube(new Vector3(0,1.55f+i*.19f,0),new Vector3(Mathf.Cos(a)*.85f,3.2f+i*.20f,Mathf.Sin(a)*.65f),.095f,.035f,0);}
        var trunk=Visual("Tronco y ramas "+variant,stem);trunk.transform.SetParent(root.transform,false);
        var crown=new Builder(new[]{leaf,leafLight});int lobes=variant==1?4:6;
        for(int i=0;i<lobes;i++){float a=i*2.399f;float x=Mathf.Cos(a)*(variant==1?.55f:.95f),z=Mathf.Sin(a)*.8f;float y=variant==1?2.9f+i*.42f:3.1f+(i%3)*.62f;var size=variant==1?new Vector3(1.15f,1.25f,1):new Vector3(1.35f,1.15f,1.22f);crown.Crown(new Vector3(x,y,z),size,i%2,100+variant*31+i);}
        var canopy=Visual("Copa artesanal "+variant,crown);canopy.transform.SetParent(root.transform,false);
        var distant=new Builder(new[]{leaf});distant.Crown(new Vector3(0,3.7f,0),new Vector3(variant==1?1.2f:1.85f,variant==1?2.15f:1.7f,1.65f),0,480+variant);
        var low=Visual("Copa lejana "+variant,distant);low.transform.SetParent(root.transform,false);
        var lod=root.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.13f,new[]{trunk.GetComponent<Renderer>(),canopy.GetComponent<Renderer>()}),new LOD(.008f,new[]{trunk.GetComponent<Renderer>(),low.GetComponent<Renderer>()})});lod.RecalculateBounds();
        var col=root.AddComponent<CapsuleCollider>();col.center=new Vector3(0,1.2f,0);col.height=2.4f;col.radius=.22f;Save(root,"Arbol_"+variant);
    }
    static void SaveBush(int variant)
    {
        var b=new Builder(new[]{leaf,leafLight,flower});for(int i=0;i<3;i++)b.Crown(new Vector3((i-1)*.42f,.42f+(i%2)*.17f,Mathf.Sin(i)*.15f),new Vector3(.65f,.55f,.56f),i%2,variant*30+i);
        if(variant!=1)for(int i=0;i<5;i++)b.Crown(new Vector3(Mathf.Cos(i*2.4f)*.6f,.75f+Mathf.Sin(i)*.1f,Mathf.Sin(i*2.4f)*.4f),Vector3.one*.07f,2,300+i);
        Save(Visual("Arbusto_"+variant,b),"Arbusto_"+variant);
    }
    static void SaveBin()
    {
        var b=new Builder(new[]{metal,wood,brass});b.Tube(Vector3.up*.06f,Vector3.up*.85f,.27f,.30f,0);b.Tube(Vector3.up*.85f,Vector3.up*.89f,.33f,.33f,2);
        for(int i=0;i<12;i++){float a=i*Mathf.PI/6;b.Plank(new Vector3(Mathf.Cos(a)*.285f,.44f,Mathf.Sin(a)*.285f),new Vector3(.055f,.65f,.055f),1);}
        b.Tube(Vector3.up*.89f,Vector3.up*.895f,.20f,.20f,0);var root=Visual("Papelera",b);var col=root.AddComponent<CapsuleCollider>();col.center=Vector3.up*.45f;col.height=.9f;col.radius=.33f;Save(root,"Papelera");
    }
    static void SaveCloth()
    {
        var fabric=ColorMat("Lino natural","9FBEB4",.03f);var b=new Builder(new[]{fabric,metal});b.Fabric();Save(Visual("Pano_Artesanal",b),"Pano_Artesanal");
    }
    sealed class Builder
    {
        public Material[] materials;readonly List<Vector3> vertices=new();readonly List<int>[] indices;
        public Builder(Material[] materials){this.materials=materials;indices=materials.Select(_=>new List<int>()).ToArray();}
        void Tri(Vector3 a,Vector3 b,Vector3 c,int mat){int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);indices[mat].AddRange(new[]{n,n+1,n+2});}
        public void Plank(Vector3 center,Vector3 size,int mat)
        {
            float y=size.y*.5f,z=size.z*.5f,r=Mathf.Min(y,z)*.38f;Vector2[] p={new(-y+r,-z),new(y-r,-z),new(y,-z+r),new(y,z-r),new(y-r,z),new(-y+r,z),new(-y,z-r),new(-y,-z+r)};
            for(int i=0;i<8;i++){var a=p[i];var b=p[(i+1)%8];var aa=center+new Vector3(-size.x*.5f,a.x,a.y);var bb=center+new Vector3(-size.x*.5f,b.x,b.y);var cc=center+new Vector3(size.x*.5f,a.x,a.y);var dd=center+new Vector3(size.x*.5f,b.x,b.y);Tri(aa,bb,cc,mat);Tri(bb,dd,cc,mat);Tri(center-Vector3.right*size.x*.5f,bb,aa,mat);Tri(center+Vector3.right*size.x*.5f,cc,dd,mat);}
        }
        public void Tube(Vector3 from,Vector3 to,float radiusA,float radiusB,int mat)
        {
            var q=Quaternion.FromToRotation(Vector3.up,(to-from).normalized);for(int i=0;i<8;i++){float a=i*Mathf.PI/4,b=(i+1)*Mathf.PI/4;var va=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var vb=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b));var aa=from+q*(va*radiusA);var bb=from+q*(vb*radiusA);var cc=to+q*(va*radiusB);var dd=to+q*(vb*radiusB);Tri(aa,cc,bb,mat);Tri(bb,cc,dd,mat);Tri(from,aa,bb,mat);Tri(to,dd,cc,mat);}
        }
        public void Crown(Vector3 center,Vector3 size,int mat,int seed)
        {
            var random=new System.Random(seed);var grid=new Vector3[5,9];for(int r=0;r<=4;r++)for(int i=0;i<8;i++){float latitude=-Mathf.PI*.5f+r*Mathf.PI/4,angle=(i%8)*Mathf.PI/4;float noise=r==0||r==4?1:.9f+(float)random.NextDouble()*.17f;grid[r,i]=center+Vector3.Scale(new Vector3(Mathf.Cos(angle)*Mathf.Cos(latitude),Mathf.Sin(latitude),Mathf.Sin(angle)*Mathf.Cos(latitude)),size)*noise;}
            for(int r=0;r<=4;r++)grid[r,8]=grid[r,0];
            for(int r=0;r<4;r++)for(int i=0;i<8;i++){Tri(grid[r,i],grid[r+1,i],grid[r,i+1],mat);Tri(grid[r,i+1],grid[r+1,i],grid[r+1,i+1],mat);}
        }
        public void Fabric()
        {
            var grid=new Vector3[9,7];for(int x=0;x<9;x++)for(int z=0;z<7;z++){float xx=(x/8f-.5f)*.28f,zz=(z/6f-.5f)*.23f;grid[x,z]=new Vector3(xx,.007f*Mathf.Sin(x*.9f)+.004f*Mathf.Cos(z*1.2f),zz);}
            for(int x=0;x<8;x++)for(int z=0;z<6;z++)
            {var a=grid[x,z];var b=grid[x,z+1];var c=grid[x+1,z];var d=grid[x+1,z+1];Tri(a,b,c,0);Tri(b,d,c,0);var down=Vector3.down*.003f;Tri(a+down,c+down,b+down,0);Tri(b+down,c+down,d+down,0);}
            for(int x=0;x<8;x++){Tube(grid[x,0],grid[x+1,0],.002f,.002f,1);Tube(grid[x,6],grid[x+1,6],.002f,.002f,1);}
            for(int z=0;z<6;z++){Tube(grid[0,z],grid[0,z+1],.002f,.002f,1);Tube(grid[8,z],grid[8,z+1],.002f,.002f,1);}
        }
        public Mesh Store(string name)
        {
            var path=Art+"/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh){mesh=new Mesh{name=name};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();mesh.SetVertices(vertices);mesh.subMeshCount=indices.Length;for(int i=0;i<indices.Length;i++)mesh.SetTriangles(indices[i],i);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);return mesh;
        }
    }
}
