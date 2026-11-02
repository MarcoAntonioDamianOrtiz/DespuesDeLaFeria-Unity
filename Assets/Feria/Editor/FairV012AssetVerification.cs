using System;
using System.IO;
using UnityEditor;
using UnityEngine;
public static class FairV012AssetVerification
{
    public static string[] Run()
    {
        foreach(var name in new[]{"flower_bed","trash_can","park_sign","picnic_table","hedge","hedge_round"})
        {
            var path="Assets/_Game/Art/ExternalAssets/PlaygroundPark/"+name+".fbx";
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(!model)throw new Exception("FAIL · Modelo no importado: "+path);
            foreach(var f in model.GetComponentsInChildren<MeshFilter>())if(!f.sharedMesh||f.sharedMesh.vertexCount==0)throw new Exception("FAIL · Malla vacía: "+path);
        }
        if(!File.Exists("docs/receipts/v012-assets.json"))throw new Exception("FAIL · Falta inventario con procedencia y licencia");
        return new[]{"PASS · Seis modelos importados con mallas válidas e inventario presente"};
    }
}
