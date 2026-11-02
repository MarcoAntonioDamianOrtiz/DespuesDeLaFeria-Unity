using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Feria;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Physical controller traversal and rotated attractions, using isolated save files.</summary>
public static class ContinuousParkVerification
{
    public static string[] Run(bool full=true)
    {
        if(!EditorApplication.isPlaying)throw new Exception("Entra en Play en ParquePrincipal.");
        var park=UnityEngine.Object.FindFirstObjectByType<ParkSession>();if(!park||park.InPractice)throw new Exception("Hace falta el parque, fuera de práctica.");
        var original=park.Snapshot();bool paused=park.Paused,enabled=park.player.enabled,suppressed=park.suppressSave;park.suppressSave=true;park.player.enabled=false;
        var reports=new List<string>();
        try
        {
            Check(SceneManager.sceneCount==1&&SceneManager.GetActiveScene().name=="ParquePrincipal","Una sola escena recorrible");
            Check(UnityEngine.Object.FindObjectsByType<FairPlayer>(FindObjectsSortMode.None).Length==1&&UnityEngine.Object.FindObjectsByType<FairHud>(FindObjectsSortMode.None).Length==1&&UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length==1,"Jugador, HUD y oyente únicos");
            Check(park.definition.futurePlots.Length>=4,"Parcelas de ampliación definidas");reports.Add("PASS · Una escena, jugador/carrito compartidos y cuatro parcelas ampliables");
            var first=Array.Find(park.zones,z=>!z.IsDucks);var ducks=Array.Find(park.zones,z=>z.IsDucks);
            park.player.view.transform.localPosition=new Vector3(0,1.65f,0);park.player.view.fieldOfView=68;park.player.Place(park.definition.entrance,0,8);park.cart.SetPositionAndRotation(park.definition.cartStart,Quaternion.Euler(0,180,0));park.RefreshArea();park.SetPaused(false);park.ToggleCart();
            int steps=Walk(park,new[]{new Vector3(0,0,10),new Vector3(-10,0,24),new Vector3(-22,0,29),first.Entrance,first.World(new Vector3(0,0,-6))});
            Check(park.ActiveZone==first&&park.cart.position.x< -10,"Carrito y jugador abandonan los límites del antiguo recinto");park.ToggleCart();
            steps+=Walk(park,new[]{first.Entrance,new Vector3(-22,0,29),new Vector3(-10,0,24),new Vector3(0,0,18),new Vector3(10,0,24),new Vector3(22,0,29),ducks.Entrance,ducks.World(new Vector3(0,0,-6))});Check(park.ActiveZone==ducks,"El área cambia al caminar hasta los patitos");
            steps+=Walk(park,new[]{ducks.Entrance,new Vector3(40,0,42),new Vector3(40,0,60),new Vector3(21,0,76),new Vector3(0,0,79),new Vector3(-21,0,76),new Vector3(-40,0,60),new Vector3(-40,0,42),first.Entrance});
            Check(park.ActiveZone==first&&SceneManager.sceneCount==1,"El paseo posterior vuelve a las canicas sin cargar escena");reports.Add("PASS · Recorrido físico con CharacterController, carrito y vuelta completa; "+steps+" pasos sin saltos de posición");
            if(!full)return reports.ToArray();
            int sweepSteps=0;
            foreach(var zone in park.zones)
            {
                zone.Apply(zone.InitialState);park.player.Place(zone.Entrance,zone.origin.eulerAngles.y);park.cart.SetPositionAndRotation(zone.World(new Vector3(2.7f,0,-4.4f)),zone.origin.rotation);park.RefreshArea();park.SetPaused(false);
                for(int piece=0;piece<zone.litter.Length;piece++)while(zone.litter[piece].gameObject.activeSelf&&!zone.FloorDone&&sweepSteps<50000)
                {
                    var point=zone.litter[piece].position;var direction=park.cart.position+park.cart.forward*.95f-point;direction.y=0;zone.Sweep(point,direction.normalized*.04f,.02f);park.Tick(.02f);sweepSteps++;
                }
                Check(zone.FloorDone,"El barrido funciona con el puesto desplazado y girado: "+zone.definition.zoneId);
                foreach(var surface in zone.dirtPatches)zone.CleanAt(surface.transform.position,2);zone.FixSign();Check(zone.TryRestore(),"Las tareas reactivan la atracción");
                Check(Vector3.Distance(zone.sign.position,zone.World(zone.definition.placedSign))<.001f,"El cartel usa el origen del puesto");
                if(zone.IsDucks)
                {
                    zone.Ducks.Begin();for(int shot=0;shot<3;shot++){zone.Ducks.SetAim(zone.duckTargets[0].position);zone.Ducks.Confirm();for(int i=0;i<50;i++)park.Tick(.02f);}
                    Check(zone.Ducks.State==DuckGame.Phase.Result&&zone.Ducks.Score==300,"Aros y blancos móviles en coordenadas del nuevo puesto");zone.Ducks.Exit();
                }
                else
                {
                    zone.ReleaseMarble();for(int i=0;i<400;i++)park.Tick(.02f);Check(zone.Marble.State==MarbleGame.Phase.Result&&zone.Marble.Score==100&&Vector3.Distance(zone.marble.position,zone.World(new Vector3(0,1.55f,4.24f)))<.01f,"La canica alcanza la diana en el puesto girado");zone.Marble.Exit();
                }
                var body=park.player.transform.position;var view=park.player.view;var oldLocal=view.transform.localPosition;var oldRotation=view.transform.localRotation;float fov=view.fieldOfView;
                if(zone.IsDucks)zone.Ducks.Begin();else zone.Marble.Begin();var late=typeof(FairPlayer).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic);for(int i=0;i<150;i++)late.Invoke(park.player,null);
                Check(Vector3.Distance(body,park.player.transform.position)<.001f&&Vector3.Distance(view.transform.position,zone.World(zone.definition.activityViewPosition))<.2f,"El enfoque mueve la cámara, sin transportar al jugador");zone.TogglePause();late.Invoke(park.player,null);Check(Vector3.Distance(oldLocal,view.transform.localPosition)<.001f&&Quaternion.Angle(oldRotation,view.transform.localRotation)<.001f&&Mathf.Abs(view.fieldOfView-fov)<.001f,"Salir recupera la vista normal");
            }
            reports.Add("PASS · 1200 residuos, tareas, canica y tres aros con orígenes desplazados/girados; "+sweepSteps+" pasos de barrido");
            var campaign=park.Snapshot();var before=park.player.transform.position;park.TogglePractice();Check(park.InPractice&&Array.TrueForAll(park.zones,z=>!z.Restored),"Práctica reinicia las atracciones");Check(Vector3.Distance(before,park.player.transform.position)<.001f,"Práctica no transporta al jugador");park.TogglePractice();Check(!park.InPractice&&Array.TrueForAll(park.zones,z=>z.Restored)&&Vector3.Distance(before,park.player.transform.position)<.001f,"Volver conserva el lugar y recupera las atracciones");
            reports.AddRange(Persistence(park,campaign));
            return reports.ToArray();
        }
        finally{park.suppressSave=true;if(park.InPractice)park.TogglePractice();park.Apply(original);park.player.enabled=enabled;park.SetPaused(paused);park.suppressSave=suppressed;}
    }
    static int Walk(ParkSession park,Vector3[] points)
    {
        var controller=park.player.GetComponent<CharacterController>();int steps=0;
        foreach(var destination in points)
        {
            int limit=0;while(Flat(park.player.transform.position,destination)>.20f&&limit++<3000)
            {
                var before=park.player.transform.position;var cart=park.cart.position;var delta=destination-before;delta.y=0;park.player.transform.rotation=Quaternion.LookRotation(delta);controller.Move(delta.normalized*.052f+Vector3.down*.06f);park.Tick(.02f);
                Check(Flat(before,park.player.transform.position)<.09f&&Flat(cart,park.cart.position)<.091f,"El cambio de área no causa saltos del jugador ni del carrito");steps++;
            }
            Check(limit<3000,"Obstáculo en el recorrido hacia "+destination+" desde "+park.player.transform.position);
        }
        return steps;
    }
    static string[] Persistence(ParkSession park,ParkSave data)
    {
        string folder=Path.GetFullPath("../../work/v06/store-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);string path=Path.Combine(folder,"park.json"),v2=Path.Combine(folder,"campaign.json");
        var old=new CampaignSave{activeZone="galeria-patitos",zones=data.zones};var last=old.zones.Find(z=>z.zoneId==old.activeZone);last.player=new Vector3(1,.05f,-6);last.cart=new Vector3(-2,0,-4);File.WriteAllText(v2,JsonUtility.ToJson(old));string bytes=File.ReadAllText(v2);
        var store=new ParkSaveStore(path,v2,null,park.definition,park.FindOrigin);var migrated=store.Read(out _,out bool migratedFlag);Check(migratedFlag&&migrated.zones.Count==2&&Vector3.Distance(migrated.world.player,park.FindOrigin(old.activeZone).TransformPoint(last.player))<.001f,"Migra v2 a posición global");
        store.Write(data);store.Write(data);Check(File.ReadAllText(v2)==bytes,"La migración conserva v2");var loaded=store.Read(out _,out _);Check(Vector3.Distance(loaded.world.player,data.world.player)<.001f&&Vector3.Distance(loaded.world.cart,data.world.cart)<.001f,"Jugador y carrito globales se recuperan");
        File.WriteAllText(path,"{corrupto");Check(store.Read(out bool recovered,out _).zones.Count==2&&recovered,"Respaldo recupera parque y atracciones");
        string valid=File.ReadAllText(path);var invalid=JsonUtility.FromJson<ParkSave>(JsonUtility.ToJson(data));invalid.world.player.x=float.NaN;bool rejected=false;try{store.Write(invalid);}catch(InvalidDataException){rejected=true;}Check(rejected&&File.ReadAllText(path)==valid,"No escribe posiciones inválidas");
        File.WriteAllText(path,"{\"version\":99}");string future=File.ReadAllText(path);store.Read(out _,out _);rejected=false;try{store.Write(data);}catch(InvalidDataException){rejected=true;}Check(store.ReadOnly&&rejected&&File.ReadAllText(path)==future,"Versiones posteriores protegidas");
        return new[]{"PASS · Limpieza libre conserva las atracciones sin mover jugador/carrito","PASS · Guardado v3 global, migración v2 intacta, respaldo y datos inválidos/versiones posteriores protegidos"};
    }
    static float Flat(Vector3 a,Vector3 b){a.y=b.y=0;return Vector3.Distance(a,b);}
    static void Check(bool condition,string text){if(!condition)throw new Exception("FAIL · "+text);}
}
