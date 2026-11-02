using Feria;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class FairV012StainBuilder
{
    public static string Run()
    {
        const string folder="Assets/Feria/Art/Original/SpillsV012";
        System.IO.Directory.CreateDirectory(folder);AssetDatabase.Refresh();
        var catalog=AssetDatabase.LoadAssetAtPath<FairStainCatalog>("Assets/Feria/Definitions/ManchasV012.asset");
        if(!catalog){catalog=ScriptableObject.CreateInstance<FairStainCatalog>();AssetDatabase.CreateAsset(catalog,"Assets/Feria/Definitions/ManchasV012.asset");}
        var materials=new Material[7];
        for(int j=0;j<7;j++)
        {
            string path=folder+"/Bebida_"+j+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Feria/Spill"));AssetDatabase.CreateAsset(m,path);}
            var p=catalog.Get(j);var c=p.color;c.a=p.opacity;m.SetColor("_BaseColor",c);m.SetFloat("_Roughness",p.roughness);m.SetFloat("_Seed",p.seed);materials[j]=m;EditorUtility.SetDirty(m);
        }
        int count=0;
        foreach(var zone in Object.FindObjectsByType<FairGame>(FindObjectsSortMode.None))
        {
            zone.stainCatalog=catalog;EditorUtility.SetDirty(zone);
            for(int i=0;i<zone.dirtPatches.Length;i++)
            {
                var r=zone.dirtPatches[i];var mf=r.GetComponent<MeshFilter>();string path=folder+"/Derrame_"+zone.definition.zoneId+"_"+i+".asset";
                var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(!mesh)
                {
                    var bounds=mf.sharedMesh.bounds;var vertices=new Vector3[19];var uv=new Vector2[19];var triangles=new int[54];vertices[0]=bounds.center;uv[0]=Vector2.one*.5f;
                    for(int k=0;k<18;k++){float a=k*Mathf.PI*2/18;float radius=.83f+.11f*Mathf.Sin(a*(i%3+3)+i*1.9f)+.06f*Mathf.Cos(a*7+i);var v=new Vector3(Mathf.Cos(a)*radius*bounds.extents.x,0,Mathf.Sin(a)*radius*bounds.extents.z);vertices[k+1]=bounds.center+v;uv[k+1]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.5f+Vector2.one*.5f;triangles[k*3]=0;triangles[k*3+1]=(k+1)%18+1;triangles[k*3+2]=k+1;}
                    mesh=new Mesh{name="Derrame irregular "+i,vertices=vertices,triangles=triangles,uv=uv};mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);
                }
                mf.sharedMesh=mesh;r.sharedMaterial=materials[i%7];EditorUtility.SetDirty(mf);EditorUtility.SetDirty(r);count++;
            }
        }
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();return count+" manchas; IDs conservados";
    }
}
