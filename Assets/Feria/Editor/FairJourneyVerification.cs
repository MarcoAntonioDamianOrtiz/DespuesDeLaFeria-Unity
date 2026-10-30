using System;
using System.IO;
using System.Reflection;
using Feria;
using UnityEditor;
using UnityEngine;

/// <summary>Scripted simulation of the whole work cycle, not a claim about human play time or enjoyment.</summary>
public static class FairJourneyVerification
{
    public static string[] Run()
    {
        if(!EditorApplication.isPlaying)throw new Exception("Entra en Play.");
        var game=UnityEngine.Object.FindFirstObjectByType<FairGame>();game.suppressSave=true;
        if(game.InPractice)throw new Exception("Vuelve a la partida antes de verificar.");
        var original=game.Snapshot();bool paused=game.Paused;
        var storeField=typeof(FairGame).GetField("campaignStore",BindingFlags.Instance|BindingFlags.NonPublic);
        var originalStore=storeField.GetValue(game);
        string testPath=Path.GetFullPath("../../work/verification-v03/practice-"+Guid.NewGuid().ToString("N")+".json");
        var isolated=new CampaignSaveStore(testPath,null,game.catalog);var campaign=new CampaignSave{activeZone=game.definition.zoneId};campaign.zones.Add(CampaignMapping.Capture(original,game.litterIds,game.surfaceIds));isolated.Write(campaign);storeField.SetValue(game,isolated);
        byte[] diskBefore=File.ReadAllBytes(testPath);
        int steps=0;
        try
        {
            game.TogglePractice();Check(game.InPractice&&!game.FloorDone,"Limpieza libre comienza con la zona pendiente");
            game.suppressSave=false;
            Check(!game.TryRestore(),"No se puede activar un puesto sin limpiar");
            const float dt=.02f;
            for(int piece=0;piece<game.litter.Length;piece++)
            {
                while(game.litter[piece].gameObject.activeSelf&&!game.FloorDone&&steps<40000)
                {
                    var point=game.litter[piece].position;var mouth=game.cart.position+game.cart.forward*.95f;
                    Vector3 direction=mouth-point;direction.y=0;
                    game.Sweep(point,direction.normalized*.04f,dt);game.Tick(dt);steps++;
                }
            }
            Check(game.FloorDone,"Las pasadas y el recogedor completan el suelo sin retirar piezas directamente");
            Check(!game.TryRestore(),"Todavía exige mostrador y cartel");
            foreach(var patch in game.dirtPatches)game.CleanAt(patch.transform.position,2);
            game.FixSign();Check(game.TryRestore(),"La secuencia completa reactiva el puesto");
            if(game.IsDucks)
            {
                game.Ducks.Begin();for(int shot=0;shot<3;shot++){game.Ducks.SetAim(game.duckTargets[0].position);game.Ducks.Confirm();for(int i=0;i<50;i++)game.Tick(dt);}
                Check(game.Ducks.State==DuckGame.Phase.Result&&game.Ducks.Score==300,"Tres aros alcanzan el patito del gorro");
            }
            else
            {
                game.ReleaseMarble();for(int i=0;i<450;i++)game.Tick(dt);
                Check(game.Marble.State==MarbleGame.Phase.Result&&game.Marble.Score==100&&Vector3.Distance(game.marble.position,game.Marble.Landing)<.01f,"Canica recorre rampas y alcanza la diana central");
            }
            game.Save();
            byte[] diskAfter=File.Exists(game.SavePath)?File.ReadAllBytes(game.SavePath):null;
            Check(Equal(diskBefore,diskAfter),"La sesión de prueba no modifica la partida en disco");
            game.suppressSave=true;game.TogglePractice();
            Check(game.SignFixed==original.signFixed&&game.Restored==original.restored&&Mathf.Abs(game.PlaySeconds-original.playSeconds)<.001f,"Volver de limpieza libre recupera el progreso anterior");
            var different=game.Snapshot();different.zoneId="otra-zona";bool rejected=false;try{game.Apply(different);}catch(InvalidDataException){rejected=true;}
            Check(rejected,"La validación rechaza partidas de otra zona");
            return new[]{$"PASS · Ciclo completo de {game.litter.Length} residuos mediante barrido y recogida",$"SIMULACIÓN · {steps} pasos de 20 ms; no representa duración de juego humana","PASS · Mostrador, cartel, reactivación y actividad del puesto","PASS · Limpieza libre y retorno conservan la partida","PASS · Identidad de zona validada"};
        }
        finally
        {
            game.suppressSave=true;if(game.InPractice)game.TogglePractice();game.Apply(original);storeField.SetValue(game,originalStore);game.SetPaused(paused);
        }
    }
    static bool Equal(byte[] a,byte[] b){if(a==null||b==null)return a==b;if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;return true;}
    static void Check(bool condition,string message){if(!condition)throw new Exception("FAIL · "+message);}
}
