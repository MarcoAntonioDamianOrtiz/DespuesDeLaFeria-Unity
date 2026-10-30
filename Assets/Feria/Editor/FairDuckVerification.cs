using System;
using System.Reflection;
using Feria;
using UnityEditor;
using UnityEngine;

public static class FairDuckVerification
{
    public static string[] Run()
    {
        if(!EditorApplication.isPlaying)throw new Exception("Entra en Play.");var game=UnityEngine.Object.FindFirstObjectByType<FairGame>();if(!game.IsDucks||game.InPractice)throw new Exception("Abre los patitos, fuera de práctica.");
        var original=game.Snapshot();bool paused=game.Paused;int best=game.DuckBest;game.suppressSave=true;
        var late=typeof(FairPlayer).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
        try
        {
            game.TogglePractice();Check(!game.TryRestore(),"No activa el canal antes de limpiarlo");
            var ready=game.Snapshot();Array.Fill(ready.removed,true);Array.Fill(ready.dirt,1f);ready.signFixed=true;ready.restored=true;game.Apply(ready);game.SetPaused(false);
            game.player.Place(new Vector3(0,.05f,2.6f),0);game.player.view.transform.LookAt(game.activityAnchor.position);var camera=game.player.view.transform;var pos=camera.localPosition;var rotation=camera.localRotation;float fov=game.player.view.fieldOfView;
            game.Interact();Check(game.Ducks.State==DuckGame.Phase.Aiming,"E abre el canal");late.Invoke(game.player,null);
            var aim=game.Ducks.AimPoint;game.Ducks.Aim(new Vector2(10,10));Check(game.Ducks.AimPoint!=aim,"Ratón desplaza la guía");game.Ducks.Aim(Vector2.one*100000);Check(game.Ducks.AimPoint.x<=1.04f&&game.Ducks.AimPoint.z<=5.59f,"La guía queda dentro del agua");
            var target=game.duckTargets[0].position;game.SetPaused(true);game.Tick(1);Check(game.duckTargets[0].position==target,"Pausa detiene patitos");game.SetPaused(false);game.Tick(.05f);Check(game.duckTargets[0].position!=target,"Patitos se mueven al reanudar");
            game.Ducks.SetAim(game.duckTargets[0].position);game.Interact();Check(game.Ducks.State==DuckGame.Phase.Throwing,"Lanzamiento comienza");game.Interact();for(int i=0;i<50;i++)game.Tick(.02f);Check(game.Ducks.Throws==1&&game.Ducks.Score==100,"Aro acierta al patito del gorro; pulsar dos veces no consume otro");
            game.Ducks.SetAim(new Vector3(0,1.54f,5.59f));game.Interact();for(int i=0;i<50;i++)game.Tick(.02f);Check(game.Ducks.Throws==2&&game.Ducks.LastScore==0,"Un fallo consume un aro sin bloquear la ronda");
            game.Ducks.SetAim(game.duckTargets[1].position);game.Interact();for(int i=0;i<50;i++)game.Tick(.02f);Check(game.Ducks.State==DuckGame.Phase.Result&&game.Ducks.Throws==3&&game.Ducks.Score==150,"Tres aros finalizan con puntuación acumulada");Check(game.DuckBest==best,"Práctica conserva el récord");
            game.Interact();Check(game.Ducks.State==DuckGame.Phase.Aiming&&game.Ducks.Throws==0,"Revancha reinicia la ronda");game.TogglePause();late.Invoke(game.player,null);Check(!game.MinigameActive&&!game.Paused&&Vector3.Distance(pos,camera.localPosition)<.001f&&Quaternion.Angle(rotation,camera.localRotation)<.001f&&Mathf.Abs(fov-game.player.view.fieldOfView)<.001f,"Esc devuelve cámara y herramientas");
            return new[]{"PASS · Activación exige las tareas y E abre los patitos","PASS · Apuntado limitado al agua, movimiento y pausa","PASS · Acierto 100, fallo 0 y acierto 50; tres aros y antirrepetición","PASS · Récord protegido en práctica, revancha y regreso de cámara"};
        }
        finally{game.suppressSave=true;if(game.InPractice)game.TogglePractice();game.Apply(original);game.SetPaused(paused);}
    }
    static void Check(bool condition,string message){if(!condition)throw new Exception("FAIL · "+message);}
}
