using System.Linq;
using Feria;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class FairV012EnvironmentBuilder
{
    public static string Run()
    {
        if(GameObject.Find("Decoracion v012"))return "Entorno v012 ya aplicado";
        var park=Object.FindFirstObjectByType<ParkSession>();var root=new GameObject("Decoracion v012");root.transform.SetParent(park.transform,false);
        var trees=AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/EmbersStorm -  Free Nature Pack/Prefabs/Trees/Oak"}).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).Take(6).ToArray();
        var originals=park.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Árbol artesanal")).Take(12).ToArray();
        int count=0;
        foreach(var old in originals)
        {
            var wrapper=new GameObject("Arbol externo "+count);wrapper.transform.SetParent(root.transform,false);wrapper.transform.position=old.position;
            var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(trees[count%trees.Length]),wrapper.transform);visual.name="Copa externa "+count%6;
            var rs=visual.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
            float scale=(4.0f+(count%3)*.4f)/Mathf.Max(.1f,b.size.y);visual.transform.localScale*=scale;
            var after=rs[0].bounds;foreach(var r in rs)after.Encapsulate(r.bounds);visual.transform.position+=Vector3.up*(old.position.y-after.min.y);
            foreach(var c in visual.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
            var collider=wrapper.AddComponent<CapsuleCollider>();collider.radius=.16f;collider.height=2;collider.center=Vector3.up;
            var distant=Object.Instantiate(old.gameObject,wrapper.transform);distant.name="Silueta lejana";distant.transform.localPosition=Vector3.zero;
            foreach(var c in distant.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
            foreach(var group in distant.GetComponentsInChildren<LODGroup>())Object.DestroyImmediate(group);
            var lod=wrapper.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.09f,rs),new LOD(.025f,distant.GetComponentsInChildren<Renderer>())});lod.RecalculateBounds();old.gameObject.SetActive(false);count++;
        }
        int props=0;
        foreach(var zone in park.zones)
        {
            foreach(float side in new[]{-1f,1f})
            {
                var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/ExternalAssets/PlaygroundPark/flower_bed.fbx");
                var plant=(GameObject)PrefabUtility.InstantiatePrefab(asset,root.transform);plant.name="Jardinera externa "+props++;plant.transform.position=zone.World(new Vector3(side*6.4f,0,2.6f));
                var rs=plant.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);plant.transform.localScale*=1.3f/Mathf.Max(b.size.x,b.size.z);
                var after=rs[0].bounds;foreach(var r in rs)after.Encapsulate(r.bounds);plant.transform.position+=Vector3.up*(zone.origin.position.y-after.min.y);
                var col=plant.AddComponent<BoxCollider>();col.center=plant.transform.InverseTransformPoint(after.center);col.size=new Vector3(b.size.x,b.size.y,b.size.z);
            }
        }
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();return count+" árboles variados con LOD y "+props+" jardineras; caminos conservados";
    }
}
