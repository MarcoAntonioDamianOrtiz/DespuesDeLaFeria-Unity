using System;
using System.Collections.Generic;
using System.Reflection;
using Feria;
using UnityEditor;
using UnityEngine;

public static class FairDetailVerification
{
    public static string[] Run()
    {
        if(!EditorApplication.isPlaying)throw new Exception("Entra en Play.");
        var game=UnityEngine.Object.FindFirstObjectByType<FairGame>();if(game.InPractice)throw new Exception("Vuelve a la partida antes de verificar.");
        var original=game.Snapshot();bool paused=game.Paused;game.suppressSave=true;
        var results=new List<string>();int best=game.MarbleBest;
        var late=typeof(FairPlayer).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
        try
        {
            game.TogglePractice();game.SetPaused(false);
            Transform paper=game.litter[1].Find("Papel artesanal");Check(paper&&paper.GetComponent<MeshFilter>().sharedMesh,"Existe papel con malla propia");
            var p=game.litter[1].position;game.Sweep(p,Vector3.forward*.04f,.02f);game.Tick(.02f);
            Check(paper.localPosition.y>0,"El papel levanta visualmente al barrer");Check(Mathf.Abs(game.Snapshot().litter[1].y-p.y)<.00001f,"El aleteo no contamina las coordenadas guardadas");
            game.Apply(original);Check(paper.localPosition==Vector3.zero&&paper.localRotation==Quaternion.identity,"Restaurar reinicia el aleteo");results.Add("PASS · Papel curvado, aleteo y posiciones persistentes separadas");
            var ready=game.Snapshot();Array.Fill(ready.removed,true);Array.Fill(ready.dirt,1f);ready.signFixed=true;ready.restored=true;game.Apply(ready);
            game.player.Place(new Vector3(0,.05f,2.8f),0);game.player.view.transform.LookAt(game.marble.position);
            var camera=game.player.view.transform;Vector3 cameraPosition=camera.localPosition;Quaternion cameraRotation=camera.localRotation;float fov=game.player.view.fieldOfView;
            game.Interact();Check(game.Marble.State==MarbleGame.Phase.Aiming,"E sobre la canica abre el juego");
            late.Invoke(game.player,null);game.SetPaused(true);float meter=game.Marble.Meter;game.Tick(1);Check(Mathf.Abs(meter-game.Marble.Meter)<.001f,"La pausa congela el medidor");game.SetPaused(false);game.Tick(.05f);Check(Mathf.Abs(meter-game.Marble.Meter)>.001f,"Medidor avanza al reanudar");
            game.Interact();Check(game.Marble.State==MarbleGame.Phase.Rolling,"E confirma el lanzamiento");Vector3 launch=game.marble.position;game.Interact();Check(game.marble.position==launch,"E repetida no reinicia una canica en marcha");
            game.TogglePause();late.Invoke(game.player,null);Check(!game.MinigameActive&&!game.Paused,"Escape vuelve a la feria");Check(Vector3.Distance(camera.localPosition,cameraPosition)<.001f&&Quaternion.Angle(camera.localRotation,cameraRotation)<.001f&&Mathf.Abs(game.player.view.fieldOfView-fov)<.001f,"Volver restaura cámara y campo de visión");results.Add("PASS · Entrada, medidor, pausa, antirreinicio y regreso de cámara");
            float[] powers={0,.25f,.5f,.75f,1};int[] scores={10,50,100,50,10};
            for(int test=0;test<powers.Length;test++)
            {
                game.Marble.Drop(powers[test]);for(int i=0;i<350;i++)game.Tick(.02f);
                Check(game.Marble.State==MarbleGame.Phase.Result&&game.Marble.Score==scores[test],"Resultado para lanzamiento "+powers[test]);Check(Vector3.Distance(game.marble.position,game.Marble.Landing)<.001f,"Canica queda sobre su diana");
                game.Interact();Check(game.Marble.State==MarbleGame.Phase.Aiming,"Otra ronda disponible");
            }
            Check(game.Marble.Attempts==5,"Intentos contados una sola vez");Check(game.MarbleBest==best,"La práctica no modifica el récord local");results.Add("PASS · Cinco lanzamientos: 10 / 50 / 100 / 50 / 10, revancha y récord protegido");
            game.Tool=0;game.Marble.Exit();var before=game.player.broom.localPosition;for(int i=0;i<10;i++)game.player.AnimateTool(true,0,.02f);Check(Vector3.Distance(before,game.player.broom.localPosition)>.005f,"Animación de barrido activa");Check(game.player.broom.Find("Escoba artesanal/Fibras flexibles"),"Fibras y cabezal original conectados");results.Add("PASS · Escoba con movimiento de cabezal y fibras flexibles");
        }
        finally{game.suppressSave=true;if(game.InPractice)game.TogglePractice();game.Apply(original);game.SetPaused(paused);}
        return results.ToArray();
    }
    static void Check(bool condition,string text){if(!condition)throw new Exception("FAIL · "+text);}
}
