using Feria;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class FairV012ToolsBuilder
{
    public static string Run()
    {
        var player=Object.FindFirstObjectByType<FairPlayer>();
        const string folder="Assets/Feria/Art/Original/ToolsV012";
        foreach(string name in new[]{"Escoba","Pano"})
        {
            var holder=new GameObject(name+" Blender v012");var model=AssetDatabase.LoadAssetAtPath<GameObject>(folder+"/"+name+".fbx");
            var visual=(GameObject)PrefabUtility.InstantiatePrefab(model,holder.transform);
            foreach(var r in visual.GetComponentsInChildren<Renderer>())
            {
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    var source=mats[i];string path=folder+"/"+source.name+".mat";
                    var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=source.name};m.color=source.HasProperty("_BaseColor")?source.GetColor("_BaseColor"):source.color;m.SetFloat("_Smoothness",source.name.Contains("Tela")||source.name.Contains("Cerdas")?.08f:.28f);AssetDatabase.CreateAsset(m,path);}mats[i]=m;
                }
                r.sharedMaterials=mats;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var root=name=="Escoba"?player.broom:player.cloth;
            if(!root){Object.DestroyImmediate(holder);continue;}
            if(root.Find(name+" Blender v012")){Object.DestroyImmediate(holder);continue;}
            foreach(Transform old in root)old.gameObject.SetActive(false);
            var renderer=root.GetComponent<Renderer>();if(renderer)renderer.enabled=false;
            var prefab=PrefabUtility.SaveAsPrefabAsset(holder,folder+"/"+name+".prefab");Object.DestroyImmediate(holder);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);instance.name=name+" Blender v012";
            if(name=="Escoba"){instance.transform.localPosition=new Vector3(0,.05f,.20f);instance.transform.localRotation=Quaternion.Euler(-15,0,-28);instance.transform.localScale=Vector3.one*.7f;}
            EditorUtility.SetDirty(root);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();return "Escoba y paño propios integrados; modelos anteriores conservados inactivos";
    }
}
