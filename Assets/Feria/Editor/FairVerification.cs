using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Feria;
using UnityEditor;
using UnityEngine;

public static class FairVerification
{
    [MenuItem("Feria/Verificar zona en Play")]
    public static void RunMenu() { foreach(string result in Run()) Debug.Log(result); }
    public static string[] Run()
    {
        if(!EditorApplication.isPlaying) throw new InvalidOperationException("La verificación requiere modo de juego.");
        var game=UnityEngine.Object.FindFirstObjectByType<FairGame>();
        Check(game!=null,"Existe FairGame");
        game.suppressSave=true;
        var original=game.Snapshot(); bool paused=game.Paused;
        var reports=new List<string>();
        float volume=AudioListener.volume, sensitivity=game.player.sensitivity;
        bool hadVolume=PlayerPrefs.HasKey("feria.volume"),hadSensitivity=PlayerPrefs.HasKey("feria.sensitivity");
        var update=typeof(FairGame).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic);
        try
        {
            var baseline=game.Snapshot(); Array.Clear(baseline.removed,0,baseline.removed.Length); Array.Clear(baseline.dirt,0,baseline.dirt.Length); baseline.signFixed=false; baseline.restored=false;
            for(int i=0;i<baseline.litter.Length;i++) baseline.litter[i]=new Vector3(-5+(i%30)*.34f,.03f,-6+(i/30)*.32f);
            baseline.cart=new Vector3(2.7f,0,-4.4f); baseline.cartYaw=0;
            game.Apply(baseline); game.SetPaused(false);
            game.litter[0].position=new Vector3(0,.03f,-3);
            Vector3 start=game.litter[0].position;
            Check(game.Sweep(start,Vector3.forward,.05f)>0,"El barrido toca residuos");
            update.Invoke(game,null); Check(game.litter[0].position!=start,"El barrido desplaza residuos"); reports.Add("PASS · Barrido y desplazamiento");
            var mouth=game.cart.position+game.cart.forward*.95f; game.litter[0].position=new Vector3(mouth.x,.03f,mouth.z);
            int collected=game.Collected; update.Invoke(game,null); Check(game.Collected>collected,"El recogedor retira residuos"); reports.Add("PASS · Recogida con carrito");
            var target=game.RequestHint(); bool targetFound=false;
            var state=game.Snapshot(); for(int i=0;i<game.litter.Length;i++) if(!state.removed[i] && Vector3.Distance(game.litter[i].position,target)<.01f) targetFound=true;
            Check(targetFound,"Tab señala un residuo pendiente"); reports.Add("PASS · Ayuda contextual sobre objetivo pendiente");
            Check(!game.TryRestore(),"El puesto exige sus tareas");
            for(int i=0;i<game.dirtPatches.Length;i++) game.CleanAt(game.dirtPatches[i].transform.position,2);
            Check(game.CounterDone,"Mostrador completo"); game.FixSign();
            state=game.Snapshot(); for(int i=0;i<state.removed.Length;i++) state.removed[i]=true; game.Apply(state);
            Check(game.TryRestore(),"Puesto reactivado"); game.ReleaseMarble(); var before=game.marble.position; update.Invoke(game,null); Check(game.marble.position!=before,"Canica animada");
            reports.Add("PASS · Tareas, luces y canica");
            game.Apply(baseline); Check(!game.SignFixed && !game.Restored,"Se restaura el estado inicial");
            foreach(var bulb in game.bulbs) Check(bulb.sharedMaterial!=game.litBulb,"Las bombillas vuelven al material apagado"); reports.Add("PASS · Restauración visual coherente");
            game.SetVolume(.34f); game.SetSensitivity(.11f); Check(Mathf.Abs(AudioListener.volume-.34f)<.001f && Mathf.Abs(game.player.sensitivity-.11f)<.001f,"Ajustes aplicados");
            Check(Mathf.Abs(PlayerPrefs.GetFloat("feria.volume")-.34f)<.001f && Mathf.Abs(PlayerPrefs.GetFloat("feria.sensitivity")-.11f)<.001f,"Preferencias guardadas"); reports.Add("PASS · Volumen y sensibilidad");
            Check(game.hud.broomIcon && game.hud.clothIcon && game.hud.emblem && game.player.cloth,"Referencias del arte original");
            Check(game.feedbackMaterial && game.feedbackMaterial.shader,"Material serializado del indicador de barrido");
            var meshes=AssetDatabase.FindAssets("t:Mesh",new[]{"Assets/Feria/Art/Original/Meshes"}); Check(meshes.Length==6,"Seis mallas originales");
            foreach(string guid in meshes)
            {
                var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(guid)); int nonzero=0; foreach(var normal in mesh.normals) if(normal.sqrMagnitude>.1f) nonzero++;
                Check(nonzero>mesh.vertexCount*.9f,"Normales válidas: "+mesh.name);
            }
            reports.Add("PASS · Iconos, paño y seis mallas con normales válidas");
            string directory=Path.GetFullPath("../../work/verification-v02"); Directory.CreateDirectory(directory);
            string path=Path.Combine(directory,"save-"+Guid.NewGuid().ToString("N")+".json");
            var store=new LocalSaveStore(path); store.Write(baseline); store.Write(baseline);
            Check(File.Exists(path+".bak"),"Existe respaldo"); File.WriteAllText(path,"{corrupto");
            Check(store.TryLoad(game.Apply,out bool recovered) && recovered,"Recupera el respaldo");
            Check(JsonUtility.FromJson<FairSave>(File.ReadAllText(path)).litter.Length==720,"Archivo principal reparado");
            var invalid=game.Snapshot(); invalid.version=99; bool rejected=false; try {game.Apply(invalid);} catch(InvalidDataException) {rejected=true;} Check(rejected,"Rechaza versión desconocida");
            reports.Add("PASS · Guardado, respaldo, corrupción y formato v1 en carpeta aislada");
        }
        finally
        {
            game.Apply(original); game.SetVolume(volume); game.SetSensitivity(sensitivity);
            if(!hadVolume) PlayerPrefs.DeleteKey("feria.volume"); if(!hadSensitivity) PlayerPrefs.DeleteKey("feria.sensitivity"); PlayerPrefs.Save();
            game.SetPaused(paused); game.suppressSave=true;
        }
        return reports.ToArray();
    }
    static void Check(bool condition,string message) { if(!condition) throw new Exception("FAIL · "+message); }
}
