using System;
using Feria;
using UnityEngine;

public static class FairMovementVerification
{
    public static string[] Run()
    {
        var park=UnityEngine.Object.FindFirstObjectByType<ParkSession>();var original=park.Snapshot();bool paused=park.Paused,suppressed=park.suppressSave,enabled=park.player.enabled;park.suppressSave=true;park.player.enabled=false;
        try
        {
            park.player.Place(park.definition.entrance,0,8);park.RefreshArea();park.SetPaused(false);int steps=park.CurrentGame.FootstepCount;
            for(int i=0;i<100;i++)park.player.Move(Vector2.up,.02f);
            Check(park.CurrentGame.FootstepCount>steps,"Caminar produce pasos por distancia");
            Check(park.player.transform.position.z>park.definition.entrance.z+5,"Desplazamiento fluido con aceleración");
            for(int i=0;i<15;i++)park.player.Move(Vector2.zero,.02f);steps=park.CurrentGame.FootstepCount;var position=park.player.transform.position;
            for(int i=0;i<80;i++)park.player.Move(Vector2.zero,.02f);
            Check(park.CurrentGame.FootstepCount==steps&&Vector2.Distance(new Vector2(position.x,position.z),new Vector2(park.player.transform.position.x,park.player.transform.position.z))<.001f,"Quieto no avanza ni reproduce pasos");
            park.SetPaused(true);position=park.player.transform.position;for(int i=0;i<30;i++)park.player.Move(Vector2.up,.02f);
            Check(park.player.transform.position==position&&park.CurrentGame.FootstepCount==steps,"Pausa bloquea movimiento y pasos");
            park.SetPaused(false);park.player.Place(park.definition.entrance+Vector3.up*3,0,8);steps=park.CurrentGame.FootstepCount;for(int i=0;i<8;i++)park.player.Move(Vector2.up,.02f);
            Check(park.CurrentGame.FootstepCount==steps,"En el aire no reproduce pasos");
            return new[]{"PASS · Caminata real con aceleración, frenado y pasos por distancia","PASS · Sin pasos en reposo, en el aire o en pausa"};
        }
        finally{park.Apply(original);park.SetPaused(paused);park.suppressSave=suppressed;park.player.enabled=enabled;}
    }
    static void Check(bool condition,string text){if(!condition)throw new Exception("FAIL · "+text);}
}
