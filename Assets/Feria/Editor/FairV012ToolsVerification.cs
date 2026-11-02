using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class FairV012ToolsVerification
{
    public static string[] Run()
    {
        foreach(var name in new[]{"Escoba","Pano"})
        {
            var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Feria/Art/Original/ToolsV012/"+name+".fbx");
            if(!p)throw new Exception("FAIL · Falta modelo Blender: "+name);
            var meshes=p.GetComponentsInChildren<MeshFilter>().Select(x=>x.sharedMesh).ToArray();
            if(meshes.Length==0||meshes.Any(m=>!m||m.vertexCount==0)||meshes.Sum(m=>m.triangles.Length/3)>=3000)throw new Exception("FAIL · Presupuesto/mallas: "+name);
            if(p.GetComponentsInChildren<Collider>().Length!=0)throw new Exception("FAIL · Collider intercepta herramienta");
        }
        return new[]{"PASS · Escoba y paño Blender importados con menos de 3000 triángulos y sin colliders"};
    }
}
