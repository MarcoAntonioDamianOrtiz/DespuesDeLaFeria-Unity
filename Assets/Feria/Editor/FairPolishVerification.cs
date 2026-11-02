using System;
using System.Collections.Generic;
using System.Reflection;
using Feria;
using UnityEditor;
using UnityEngine;

/// <summary>Regression checks for the shared park's original art and generated foley.</summary>
public static class FairPolishVerification
{
    public static string[] Run()
    {
        if(!EditorApplication.isPlaying)throw new InvalidOperationException("Entra en Play en ParquePrincipal.");
        var park=UnityEngine.Object.FindFirstObjectByType<ParkSession>();
        Check(park,"Existe la sesión del parque");
        foreach(var zone in park.zones)
        {
            Check(zone.dirtPatches.Length==16,"Se conservan las 16 identidades de manchas");
            for(int i=0;i<zone.dirtPatches.Length;i++)
            {
                var patch=zone.dirtPatches[i];
                Check(patch.GetComponent<MeshFilter>().sharedMesh.vertexCount>8,"Mancha con contorno orgánico");
                for(int j=i+1;j<zone.dirtPatches.Length;j++)
                    Check(Vector3.Distance(patch.transform.position,zone.dirtPatches[j].transform.position)>.42f,"Manchas distribuidas por el mostrador");
            }
        }
        Check(GameObject.Find("Relieve suave del borde"),"Relieve presente en la escena jugable");
        Check(GameObject.Find("Brisa suave").GetComponent<AudioLowPassFilter>(),"La brisa conserva su filtro");
        Check(!GameObject.Find("Trinos del parque").GetComponent<AudioLowPassFilter>(),"El filtro del viento no apaga los pájaros");

        var host=new GameObject("Verificación aislada de pulido");host.SetActive(false);
        FairAudio audio=null;
        try
        {
            var patch=host.AddComponent<MeshRenderer>();var scale=new Vector3(.37f,.008f,.21f);host.transform.localScale=scale;
            var surface=new SurfaceCleaning(new Renderer[]{patch});surface.Clean(host.transform.position,.5f);
            Check(Vector3.Distance(host.transform.localScale,scale*.55f)<.00001f,"Limpiar respeta las proporciones originales");
            surface.Restore(new[]{1f});Check(surface.Complete&&!host.activeSelf,"Una mancha terminada desaparece");
            surface.Restore(new[]{0f});host.SetActive(false);
            Check(host.transform.localScale==scale,"Recuperar el progreso restaura el tamaño original");
            host.SetActive(true);audio=new FairAudio(host);
            foreach(string field in new[]{"broomBrush","clothBrush","footstep","cartStep"})
            {
                var clip=(AudioClip)typeof(FairAudio).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(audio);
                var data=new float[clip.samples];Check(clip.GetData(data,0),"Audio generado legible");float peak=0;
                foreach(float value in data){Check(!float.IsNaN(value)&&!float.IsInfinity(value),"Audio finito");peak=Mathf.Max(peak,Mathf.Abs(value));}
                Check(peak>.01f&&peak<1,"Sonido presente y sin saturación");
            }
            audio.Brush(.13f);audio.Tick(.1f);audio.SilenceTools();
            foreach(var source in host.GetComponentsInChildren<AudioSource>(true))
                if(source.loop)Check(source.volume==0,"La pausa silencia las herramientas inmediatamente");
        }
        finally{audio?.Dispose();UnityEngine.Object.DestroyImmediate(host);}
        return new[]{"PASS · Dos mostradores con 16 manchas orgánicas separadas y escala recuperable","PASS · Relieve jugable, brisa y pájaros con filtros independientes","PASS · Foley generado sin saturación y herramientas silenciosas al pausar"};
    }
    static void Check(bool condition,string message){if(!condition)throw new Exception("FAIL · "+message);}
}
