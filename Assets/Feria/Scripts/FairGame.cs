using System;
using System.IO;
using UnityEngine;

namespace Feria
{
    /// <summary>Scene composition root. Coordinates services while preserving existing scene references.</summary>
    public sealed class FairGame : MonoBehaviour
    {
        public FairPlayer player;
        public Transform cart,sign,switchBox,marble,broomHead;
        public Transform[] litter;
        public Renderer[] dirtPatches;
        public Light[] fairLights;
        public Renderer[] bulbs;
        public Material litBulb,feedbackMaterial;
        public FairHud hud;
        public FairZoneDefinition definition;
        public bool suppressSave;
        public bool Paused {get;private set;}=true;
        public bool MovingCart=>cartMotor!=null&&cartMotor.Moving;
        public int Tool {get;set;}
        public bool SignFixed=>restoration.SignFixed;
        public bool Restored=>restoration.Restored;
        public float PlaySeconds {get;private set;}
        public int Collected=>floor.Collected;
        public int SweepContacts=>floor.Contacts;
        public bool FloorDone=>floor.Complete;
        public bool CounterDone=>counter.Complete;
        public float CleanProgress=>counter.Progress;
        public bool InPractice {get;private set;}
        public MarbleGame Marble=>restoration.Marble;
        public bool MinigameActive=>Marble.Active;
        public int MarbleBest=>PlayerPrefs.GetInt("feria.marble.best",0);
        public PlaySessionMetrics Metrics {get;private set;}=new PlaySessionMetrics();
        public string Notice {get;private set;}="";
        public string SavePath=>saveStore!=null?saveStore.Path:Path.Combine(Application.persistentDataPath,"feria-slot-1.json");
        LocalSaveStore saveStore;
        LitterSimulation floor;
        SurfaceCleaning counter;
        CartMotor cartMotor;
        RestorationState restoration;
        FairAudio sound;
        FairFeedback feedback;
        FairSave initialState,practiceReturn;
        Vector3 lastSweep,lastPlayer;
        bool hasSweep,toolHeld;
        float autosave,noticeTime;

        void Awake()
        {
            if(!definition)definition=ScriptableObject.CreateInstance<FairZoneDefinition>();
            saveStore=new LocalSaveStore(Path.Combine(Application.persistentDataPath,"feria-slot-1.json"));
            floor=new LitterSimulation(litter,definition);counter=new SurfaceCleaning(dirtPatches);
            cartMotor=new CartMotor(cart,player.transform,definition);
            restoration=new RestorationState(sign,marble,fairLights,bulbs,litBulb,definition);
            sound=new FairAudio(gameObject);feedback=new FairFeedback(this);
            floor.PickedUp+=amount=>{sound.Pickup(amount);Metrics.Pickup(amount);};
            floor.FloorCompleted+=()=>Tell("¡Explanada despejada! Recuperaste los colores del suelo.");
            restoration.Chime+=sound.Bell;
            Marble.RailTouched+=sound.MarbleNote;
            Marble.Finished+=score=>{sound.MarbleNote(score==100?9:7);if(!InPractice&&score>MarbleBest){PlayerPrefs.SetInt("feria.marble.best",score);PlayerPrefs.Save();}};
            AudioListener.volume=PlayerPrefs.GetFloat("feria.volume",.7f);
            initialState=Snapshot();Load();SetPaused(true);lastPlayer=player.transform.position;
        }
        void Update(){Tick(Time.deltaTime);}
        public void Tick(float elapsed)
        {
            if(noticeTime>0){noticeTime-=elapsed;if(noticeTime<=0)Notice="";}
            feedback.Tick(elapsed);if(Paused)return;
            float dt=Mathf.Min(elapsed,.05f);
            PlaySeconds+=elapsed;Metrics.seconds+=elapsed;
            Metrics.walkingMetres+=Vector3.Distance(player.transform.position,lastPlayer);lastPlayer=player.transform.position;
            cartMotor.Tick(dt);floor.Tick(dt,cartMotor.Mouth,cartMotor.Forward);restoration.Tick(dt);sound.Tick(dt);
            autosave+=elapsed;if(autosave>=definition.autosaveSeconds){autosave=0;Save(false);}
        }
        public void UseTool(bool working)
        {
            if(!working||Paused||MinigameActive){hasSweep=false;toolHeld=false;return;}
            bool first=!toolHeld;toolHeld=true;
            var ray=player.view.ViewportPointToRay(new Vector3(.5f,.5f));
            if(Tool==0)
            {
                if(first)Metrics.sweepActions++;
                if(!TrySweepPoint(out var p)){hasSweep=false;if(first)Metrics.emptySweeps++;return;}
                int contacts=Sweep(p,hasSweep?p-lastSweep:player.transform.forward*.02f,Time.deltaTime);
                if(first&&contacts==0)Metrics.emptySweeps++;
                Metrics.sweepingSeconds+=Time.deltaTime;lastSweep=p;hasSweep=true;
            }
            else if(Physics.Raycast(ray,out var hit,3.5f)){CleanAt(hit.point,Time.deltaTime);Metrics.cleaningSeconds+=Time.deltaTime;}
        }
        public int Sweep(Vector3 point,Vector3 movement,float dt)
        {
            if(MovingCart)return 0;
            int count=floor.Sweep(point,movement,player.transform.forward,dt);if(count>0)sound.Brush(.13f);return count;
        }
        public int CleanAt(Vector3 point,float dt){int count=counter.Clean(point,dt);if(count>0)sound.Brush(.09f);return count;}
        public bool TrySweepPoint(out Vector3 point)
        {
            point=default;var ray=player.view.ViewportPointToRay(new Vector3(.5f,.5f));var plane=new Plane(Vector3.up,new Vector3(0,.04f,0));
            if(!plane.Raycast(ray,out float distance)||distance<0||distance>5)return false;
            if(Physics.Raycast(ray,out var hit,distance)&&hit.collider.gameObject.name!="Explanada")return false;
            point=ray.GetPoint(distance);return point.x>definition.litterMin.x-.2f&&point.x<definition.litterMax.x+.2f&&point.z>definition.litterMin.y-.2f&&point.z<definition.litterMax.y+.1f;
        }
        public Vector3 RequestHint()
        {
            Metrics.hints++;Vector3 target=switchBox.position;
            string text="Acércate al interruptor del puesto.";
            if(!FloorDone){target=floor.Nearest(player.transform.position);text="Barre estos residuos hacia el recogedor amarillo.";}
            else if(!CounterDone){target=counter.Pending();text="Usa el paño (2) sobre esta mancha del mostrador.";}
            else if(!SignFixed){target=sign.position;text="Mira el cartel y pulsa E para colocarlo.";}
            else if(Restored){target=marble.position;text="El puesto está listo. Acércate a la canica y pulsa E.";}
            feedback.ShowHint(target);Tell(text);return target;
        }
        bool LookingAt(Transform target,float range)
        {
            Vector3 position=target==cart?cart.position+Vector3.up*.65f:target.position;
            Vector3 delta=position-player.view.transform.position;
            return delta.magnitude<range&&Vector3.Dot(delta.normalized,player.view.transform.forward)>.94f;
        }
        public string Prompt()
        {
            if(MinigameActive)return Marble.State==MarbleGame.Phase.Aiming?"E · Lanzar al pasar por el centro   ·   Esc · Salir":Marble.State==MarbleGame.Phase.Rolling?"Sigue tu canica…   ·   Esc · Salir":"E o R · Otra canica   ·   Esc · Volver a la feria";
            if(MovingCart)return "E · Estacionar carrito";
            if(!SignFixed&&LookingAt(sign,3.5f))return "E · Colocar cartel";
            if(LookingAt(switchBox,3.5f))return Restored?"El puesto vuelve a brillar":(FloorDone&&CounterDone&&SignFixed?"E · ¡Encender el puesto!":"Termina las tres tareas para encender");
            if(Restored&&LookingAt(marble,4))return "E · Jugar a las canicas";
            if(LookingAt(cart,3.4f))return "E · Mover carrito";
            return Tool==0?"Mantén clic · Barrer hacia el recogedor":"Mantén clic · Limpiar el mostrador";
        }
        public void Interact()
        {
            if(MinigameActive){Marble.Confirm();return;}
            if(MovingCart){cartMotor.Stop();Save(false);return;}
            if(!SignFixed&&LookingAt(sign,3.5f)){FixSign();Save(false);return;}
            if(LookingAt(switchBox,3.5f)){if(!TryRestore())Tell("Falta despejar, limpiar o colocar el cartel.");return;}
            if(Restored&&LookingAt(marble,4)){Marble.Begin();return;}
            if(LookingAt(cart,3.4f)){cartMotor.Toggle();Metrics.cartMoves++;Tell("Acerca el recogedor al montón. E para estacionar.");}
        }
        public void FixSign(){restoration.FixSign();sound.Pickup();}
        public bool TryRestore()
        {
            bool previous=Restored;if(!restoration.Activate(FloorDone,CounterDone))return false;
            if(!previous){Metrics.finishedSeconds=Metrics.seconds;Tell("¡El Rincón de las Canicas vuelve a la vida!");Save(false);}return true;
        }
        public void ReleaseMarble(){restoration.Release();}
        public void SetVolume(float value){AudioListener.volume=Mathf.Clamp01(value);PlayerPrefs.SetFloat("feria.volume",AudioListener.volume);}
        public void SetSensitivity(float value){player.sensitivity=Mathf.Clamp(value,.035f,.18f);PlayerPrefs.SetFloat("feria.sensitivity",player.sensitivity);}
        public void TogglePause(){if(MinigameActive&&!Paused){Marble.Exit();return;}SetPaused(!Paused); }
        public void SetPaused(bool paused)
        {
            Paused=paused;Cursor.lockState=paused?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=paused;
            if(paused){sound?.Brush(0);PlayerPrefs.Save();}
        }
        public void Tell(string message){Notice=message;noticeTime=7;}
        public FairSave Snapshot()=>new FairSave {zoneId=definition.zoneId,litter=floor.CapturePositions(),removed=floor.CaptureRemoved(),dirt=counter.Capture(),cart=cart.position,cartYaw=cart.eulerAngles.y,player=player.transform.position,playerYaw=player.transform.eulerAngles.y,signFixed=SignFixed,restored=Restored,playSeconds=PlaySeconds};
        public void Apply(FairSave data)
        {
            SaveValidation.Validate(data,litter.Length,dirtPatches.Length,definition);
            floor.Restore(data.litter,data.removed);counter.Restore(data.dirt);cartMotor.Stop();
            cart.SetPositionAndRotation(data.cart,Quaternion.Euler(0,data.cartYaw,0));player.Place(data.player,data.playerYaw);
            restoration.Restore(data.signFixed,data.restored);PlaySeconds=data.playSeconds;lastPlayer=player.transform.position;hasSweep=false;toolHeld=false;
        }
        public void TogglePractice()
        {
            if(InPractice){Apply(practiceReturn);practiceReturn=null;InPractice=false;Metrics=new PlaySessionMetrics();SetPaused(true);Tell("Has vuelto a tu partida guardada.");return;}
            Save(false);practiceReturn=Snapshot();Apply(initialState);InPractice=true;Metrics=new PlaySessionMetrics();Tool=0;SetPaused(false);
            Tell("Limpieza libre: puedes repetir esta zona sin cambiar tu partida.");
        }
        public void Save(bool notify=true)
        {
            if(suppressSave||InPractice){if(notify&&InPractice)Tell("La limpieza libre no modifica tu partida guardada.");return;}
            try{saveStore.Write(Snapshot());if(notify)Tell("Partida guardada en este equipo.");}
            catch(Exception e){Debug.LogWarning("No se pudo guardar: "+e.Message);Tell("No se pudo guardar. Comprueba el espacio y los permisos.");}
        }
        void Load(){saveStore.TryLoad(Apply,out bool recovered,message=>Debug.LogWarning(message));if(recovered)Tell("Partida recuperada desde el respaldo.");}
        void OnApplicationQuit(){if(floor!=null)Save(false);PlayerPrefs.Save();Cursor.lockState=CursorLockMode.None;}
        void OnApplicationFocus(bool focus){if(!focus&&!Paused)SetPaused(true);}
        void OnDestroy(){feedback?.Dispose();sound?.Dispose();}
    }
}
