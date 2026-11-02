using System;
using System.Linq;
using Feria;
using UnityEngine;
using UnityEngine.UI;

public static class FairPresentationVerification
{
    public static string[] Run()
    {
        var park=UnityEngine.Object.FindFirstObjectByType<ParkSession>();var hud=park.hud;
        Check(UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length==1,"Un único Canvas");
        Check(UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length==1,"Un único EventSystem");
        Check(hud.GetComponent<CanvasScaler>().uiScaleMode==CanvasScaler.ScaleMode.ScaleWithScreenSize,"HUD escalable");Canvas.ForceUpdateCanvases();
        foreach(string path in new[]{"Zona segura/Objetivos","Zona segura/Herramientas","Zona segura/Pausa/Tarjeta"})
        {
            var rect=(RectTransform)hud.transform.Find(path);var corners=new Vector3[4];rect.GetWorldCorners(corners);
            foreach(var point in corners)Check(point.x>=-1&&point.y>=-1&&point.x<=Screen.width+1&&point.y<=Screen.height+1,"Panel dentro de pantalla: "+path);
        }
        foreach(var image in hud.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            if(!image.GetComponent<UnityEngine.UI.Button>()&&!image.GetComponent<UnityEngine.UI.Slider>()&&!image.transform.parent.GetComponent<UnityEngine.UI.Slider>())Check(!image.raycastTarget,"Decoración del HUD no intercepta clics");
        Check(UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).All(s=>s.outputAudioMixerGroup),"Fuentes de audio con grupos asignados");
        Check(park.GetComponentsInChildren<LODGroup>().Length>=90,"Árboles con niveles de detalle");
        foreach(var prefab in new[]{"Banco_0","Banco_1","Banco_2","Farol_0","Farol_1","Arbol_0","Arbol_1","Arbol_2","Arbusto_0","Arbusto_1","Arbusto_2","Papelera","Pano_Artesanal"})
        {
            var asset=UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Feria/Prefabs/ParqueArtesanal/"+prefab+".prefab");Check(asset&&asset.GetComponentsInChildren<MeshFilter>().All(f=>f.sharedMesh),"Prefab con mallas: "+prefab);
            if(prefab.StartsWith("Banco")||prefab.StartsWith("Farol"))Check(asset.GetComponent<Collider>(),"Mobiliario con colisión: "+prefab);
        }
        foreach(var bench in park.GetComponentsInChildren<Transform>().Where(t=>t.name=="Banco artesanal del puesto"))Check(Vector3.Distance(bench.lossyScale,Vector3.one)<.001f,"Banca del puesto sin heredar escala de la antigua tabla");
        Check(!park.GetComponentsInChildren<Transform>().Any(t=>t.name=="Banco"||t.name=="Farola de madera"),"Modelos básicos de puesto sustituidos");
        return new[]{"PASS · HUD dentro de "+Screen.width+" × "+Screen.height+", Canvas/EventSystem únicos y clics libres","PASS · Audio en grupos, prefabs con mallas/colliders y LOD de árboles"};
    }
    static void Check(bool valid,string message){if(!valid)throw new Exception("FAIL · "+message);}
}
