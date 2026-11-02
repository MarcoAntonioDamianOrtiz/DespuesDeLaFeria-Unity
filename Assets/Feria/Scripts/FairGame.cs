using System;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Feria
{
    /// <summary>Scene composition root. Coordinates services while preserving existing scene references.</summary>
    public sealed class FairGame : MonoBehaviour
    {
        public FairPlayer player;
        public ParkSession park;
        public Transform origin;
        public Vector3 Entrance=>World(definition.entryPoint);
        public Vector3 World(Vector3 point)=>origin?origin.TransformPoint(point):point;
        public Vector3 Local(Vector3 point)=>origin?origin.InverseTransformPoint(point):point;
        public FairSave InitialState=>initialState;
        public bool ContainsPlayer(float margin=0){var p=Local(player.transform.position);return p.x>definition.areaMin.x-margin&&p.x<definition.areaMax.x+margin&&p.z>definition.areaMin.y-margin&&p.z<definition.areaMax.y+margin;}
        public Transform cart,sign,switchBox,marble,broomHead;
        public Transform[] litter;
        public Transform[] duckTargets;
        public Transform aimRing,flyingRing,activityAnchor,travelSign;
        public FairZoneCatalog catalog;
        public string[] litterIds,surfaceIds;
        public Renderer[] dirtPatches;
        public FairStainCatalog stainCatalog;
        public const float SweepReach=2.8f;
        public string SurfaceLabelFor(int index)=>stainCatalog?stainCatalog.Get(index).displayName:"Mancha";
        public string SurfaceLabel=>SurfaceLabelFor(SurfaceTarget>=0?SurfaceTarget:LastSurfaceCompleted);
        public bool SweepOutOfReach {get;private set;}
        public Light[] fairLights;
        public Renderer[] bulbs;
        public Material litBulb,feedbackMaterial;
        public FairHud hud;
        public FairZoneDefinition definition;
        public bool suppressSave;
        bool paused=true;
        public bool Paused=>park?park.Paused:paused;
        public bool MovingCart=>park?park.MovingCart:cartMotor!=null&&cartMotor.Moving;
        int tool;
        public int Tool {get=>park?park.Tool:tool;set{if(park)park.Tool=value;else tool=value;}}
        public bool SignFixed=>restoration.SignFixed;
        public bool Restored=>restoration.Restored;
        public float PlaySeconds {get;private set;}
        public int Collected=>floor.Collected;
        public int SweepContacts=>floor.Contacts;
        public bool FloorDone=>floor.Complete;
        public bool CounterDone=>counter.Complete;
        public int CleanedSurfaces=>counter.CleanedCount;
        public int SurfaceTotal=>counter.Count;
        public int SurfaceTarget {get;private set;}=-1;
        public float SurfaceProgress=>counter.At(SurfaceTarget);
        public bool ToolContact {get;private set;}
        public float ToolPhase=>sound.Phase(Tool==1);
        public int FootstepCount=>sound.StepCount;
        public Vector3 ContactPoint {get;private set;}
        public int LastSurfaceCompleted {get;private set;}=-1;
        public float SurfaceCompletedAt {get;private set;}=-100;
        public float CleanProgress=>counter.Progress;
        bool inPractice;
        public bool InPractice=>park?park.InPractice:inPractice;
        public MarbleGame Marble=>restoration.Marble;
        public DuckGame Ducks {get;private set;}
        public bool IsDucks=>definition.activity==FairActivity.Ducks;
        public bool MinigameActive=>Marble.Active||Ducks.Active;
        public bool ChangingZones {get;private set;}
        public float Fade {get;private set;}=1;
        public int DuckBest=>PlayerPrefs.GetInt("feria.ducks.best",0);
        public string NextZoneName=>catalog&&catalog.Find(definition.nextZoneId)?catalog.Find(definition.nextZoneId).displayName:"Otro puesto";
        Transform ActivityTarget=>IsDucks&&activityAnchor?activityAnchor:marble;
        public int MarbleBest=>PlayerPrefs.GetInt("feria.marble.best",0);
        public PlaySessionMetrics Metrics {get;private set;}=new PlaySessionMetrics();
        string notice="";
        public string Notice=>park?park.Notice:notice;
        public string SavePath=>park?park.SavePath:campaignStore!=null?campaignStore.Path:Path.Combine(FairSavePaths.Directory,"feria-campaign.json");
        CampaignSaveStore campaignStore;
        CampaignSave campaign;
        bool saveBlocked;
        LitterSimulation floor;
        SurfaceCleaning counter;
        CartMotor cartMotor;
        RestorationState restoration;
        FairAudio sound;
        FairFeedback feedback;
        FairVfx vfx;
        FairLitterRenderer litterRenderer;
        public int DrawnLitter=>litterRenderer?.VisibleCount??0;
        public int LitterBatches=>litterRenderer?.BatchCount??0;
        FairSave initialState,practiceReturn;
        Vector3 lastSweep,lastPlayer;
        bool hasSweep,toolHeld;
        float autosave,noticeTime;
        bool audioPaused;

        void Awake()
        {
            if(!definition)definition=ScriptableObject.CreateInstance<FairZoneDefinition>();
            if(!catalog)throw new InvalidOperationException("Falta el catálogo de puestos de la escena.");
            campaignStore=new CampaignSaveStore(Path.Combine(FairSavePaths.Directory,"feria-campaign.json"),Path.Combine(FairSavePaths.Directory,"feria-slot-1.json"),catalog);
            Ducks=new DuckGame(duckTargets,aimRing,flyingRing,origin);
            floor=new LitterSimulation(litter,definition,origin);counter=new SurfaceCleaning(dirtPatches);
            if(!park)cartMotor=new CartMotor(cart,player.transform,definition);
            restoration=new RestorationState(sign,marble,fairLights,bulbs,litBulb,definition,origin);
            sound=new FairAudio(gameObject);feedback=new FairFeedback(this);vfx=new FairVfx(this);
            litterRenderer=new FairLitterRenderer(litter);
            counter.Completed+=index=>{sound.ObjectiveComplete();vfx.Cleaned(dirtPatches[index].transform.position);};
            floor.PickedUp+=amount=>{sound.Pickup(amount);Metrics.Pickup(amount);vfx.PickedUp(cart.position+cart.forward*.8f,amount);};
            floor.FloorCompleted+=()=>Tell("¡Explanada despejada! Recuperaste los colores del suelo.");
            restoration.Chime+=sound.Bell;
            Marble.RailTouched+=sound.MarbleNote;
            Marble.Finished+=score=>{sound.MarbleNote(score==100?9:7);if(!InPractice&&score>MarbleBest){PlayerPrefs.SetInt("feria.marble.best",score);PlayerPrefs.Save();}};
            Ducks.Landed+=score=>{if(score>0)sound.MarbleNote(score==100?9:5);else sound.Pickup();};
            Ducks.Finished+=score=>{if(!InPractice&&score>DuckBest){PlayerPrefs.SetInt("feria.ducks.best",score);PlayerPrefs.Save();}};
            AudioListener.volume=PlayerPrefs.GetFloat("feria.volume",.7f);
            initialState=Snapshot();if(!park){Load();SetPaused(true);}lastPlayer=player.transform.position;
        }
        void Start(){if(park)return;if(!Application.isEditor&&campaign!=null&&campaign.activeZone!=definition.zoneId)BeginTravel(campaign.activeZone,false);}
        void LateUpdate(){DrawLitter();}
        public void DrawLitter(){litterRenderer?.Draw();}
        void Update(){if(park)return;if(!ChangingZones)Fade=Mathf.MoveTowards(Fade,0,Time.unscaledDeltaTime*4);Tick(Time.deltaTime);}
        public void Tick(float elapsed)
        {
            if(noticeTime>0){noticeTime-=elapsed;if(noticeTime<=0)notice="";}
            bool show=!park||park.CurrentGame==this;feedback.SetVisible(show);if(show)feedback.Tick(elapsed);vfx.Tick(elapsed);if(Paused){if(!audioPaused){sound.Pause();audioPaused=true;}ToolContact=false;SurfaceTarget=-1;return;}if(audioPaused){sound.Resume();audioPaused=false;}
            float dt=Mathf.Min(elapsed,.05f);
            if(!park||park.ActiveZone==this){PlaySeconds+=elapsed;Metrics.seconds+=elapsed;}
            Metrics.walkingMetres+=Vector3.Distance(player.transform.position,lastPlayer);lastPlayer=player.transform.position;
            if(!park)cartMotor.Tick(dt);floor.Tick(dt,cart.position+cart.forward*.95f,cart.forward);restoration.Tick(dt);if(Restored&&IsDucks)Ducks.Tick(dt);sound.Tick(dt);
            if(park)return;autosave+=elapsed;if(autosave>=definition.autosaveSeconds){autosave=0;Save(false);}
        }
        public bool TrySurfaceTarget(Ray ray,out int index,out Vector3 point)=>counter.Raycast(ray,3.5f,out index,out point);
        public void UseTool(bool working,float elapsed=-1)
        {
            ToolContact=false;SurfaceTarget=-1;SweepOutOfReach=false;
            if(Paused||MinigameActive||MovingCart||(park&&park.ActiveZone!=this)){hasSweep=false;toolHeld=false;sound.SilenceTools();return;}
            var ray=player.view.ViewportPointToRay(new Vector3(.5f,.5f));
            if(TrySurfaceTarget(ray,out int target,out var surfacePoint)){SurfaceTarget=target;ContactPoint=surfacePoint;}
            var sweepPoint=Vector3.zero;bool sweepValid=Tool==0&&TrySweepPoint(out sweepPoint);
            if(!working){hasSweep=false;toolHeld=false;sound.SilenceTools();return;}
            float dt=elapsed>=0?elapsed:Time.deltaTime;
            bool first=!toolHeld;toolHeld=true;
            if(Tool==0)
            {
                if(first)Metrics.sweepActions++;
                if(!sweepValid){hasSweep=false;sound.SilenceTools();if(first)Metrics.emptySweeps++;return;}var p=sweepPoint;
                int contacts=Sweep(p,hasSweep?p-lastSweep:player.transform.forward*.02f,dt);ToolContact=contacts>0;ContactPoint=p;
                if(first&&contacts==0)Metrics.emptySweeps++;
                Metrics.sweepingSeconds+=Time.deltaTime;lastSweep=p;hasSweep=true;
            }
            else if(SurfaceTarget>=0&&counter.CleanIndex(SurfaceTarget,dt))
            {
                ToolContact=true;sound.Brush(.09f,true);Metrics.cleaningSeconds+=dt;
                if(counter.At(SurfaceTarget)>=1){LastSurfaceCompleted=SurfaceTarget;SurfaceCompletedAt=Time.unscaledTime;}
            }
            if(!ToolContact)sound.SilenceTools();
        }
        public int Sweep(Vector3 point,Vector3 movement,float dt)
        {
            if(MovingCart)return 0;
            int count=floor.Sweep(point,movement,player.transform.forward,dt);if(count>0){sound.Brush(.13f,false);vfx.Sweep(point,movement,count);}return count;
        }
        public void Footstep(bool pushingCart,float intensity,WalkSurface surface=WalkSurface.Stone){if(Paused||MinigameActive||(park&&park.ActiveZone&&park.ActiveZone!=this))return;sound.Step(pushingCart,intensity,surface);}
        public void SelectTool(int value){if(Tool==value)return;Tool=value;sound.SilenceTools();sound.ToolChanged();}
        public int CleanAt(Vector3 point,float dt){int count=counter.Clean(point,dt);if(count>0)sound.Brush(.09f,true);return count;}
        public bool TrySweepPoint(out Vector3 point)
        {
            point=default;SweepOutOfReach=false;if(park&&park.ActiveZone!=this)return false;var ray=player.view.ViewportPointToRay(new Vector3(.5f,.5f));var plane=new Plane(origin?origin.up:Vector3.up,World(new Vector3(0,.04f,0)));
            if(!plane.Raycast(ray,out float distance)||distance<0)return false;
            var candidate=ray.GetPoint(distance);var local=Local(candidate);if(!(local.x>definition.litterMin.x-.2f&&local.x<definition.litterMax.x+.2f&&local.z>definition.litterMin.y-.2f&&local.z<definition.litterMax.y+.1f))return false;
            if(Physics.Raycast(ray,out var hit,distance)&&hit.collider.gameObject.name!="Explanada")return false;
            if(distance+Vector3.Distance(ray.origin,player.view.transform.position)>SweepReach){SweepOutOfReach=true;return false;}
            point=candidate;return true;
        }
        public Vector3 RequestHint()
        {
            Metrics.hints++;if(park&&park.ActiveZone!=this){var entrance=park.NextEntrance();feedback.ShowHint(entrance);Tell("Sigue el paseo hasta el próximo puesto.");return entrance;}Vector3 target=switchBox.position;
            string text="Acércate al interruptor del puesto.";
            if(!FloorDone){target=floor.Nearest(player.transform.position);text="Barre estos residuos hacia el recogedor amarillo.";}
            else if(!CounterDone){target=counter.Pending();text="Usa el paño (2) sobre esta mancha del mostrador.";}
            else if(!SignFixed){target=sign.position;text="Mira el cartel y pulsa E para colocarlo.";}
            else if(Restored){target=ActivityTarget.position;text=IsDucks?"El canal está listo. Mira los patitos y pulsa E.":"El puesto está listo. Acércate a la canica y pulsa E.";}
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
            if(MinigameActive&&IsDucks)return Ducks.State==DuckGame.Phase.Aiming?"Ratón · Apuntar   ·   Clic o E · Lanzar aro   ·   Esc · Volver":Ducks.State==DuckGame.Phase.Throwing?"¡Ahí va el aro!   ·   Esc · Volver":"E o R · Otros tres aros   ·   Esc · Volver a la feria";
            if(MinigameActive)return Marble.State==MarbleGame.Phase.Aiming?"E · Lanzar al pasar por el centro   ·   Esc · Salir":Marble.State==MarbleGame.Phase.Rolling?"Sigue tu canica…   ·   Esc · Salir":"E o R · Otra canica   ·   Esc · Volver a la feria";
            if(MovingCart)return "E · Estacionar carrito";
            if(!park&&travelSign&&LookingAt(travelSign,3.5f))return "E · Visitar "+NextZoneName;
            if(LookingAt(cart,3.4f))return "E · Mover carrito";
            if(park&&park.ActiveZone!=this)return "Recorre los caminos · Tab para orientarte";
            if(!SignFixed&&LookingAt(sign,3.5f))return "E · Colocar cartel";
            if(LookingAt(switchBox,3.5f))return Restored?"El puesto vuelve a brillar":(FloorDone&&CounterDone&&SignFixed?"E · ¡Encender el puesto!":"Termina las tres tareas para encender");
            if(Restored&&LookingAt(ActivityTarget,4))return IsDucks?"E · Jugar a los patitos":"E · Jugar a las canicas";
            if(LookingAt(cart,3.4f))return "E · Mover carrito";
            return Tool==0?(SweepOutOfReach?"Acércate · La escoba no llega hasta ahí":"Mantén clic · Barrer hacia el recogedor"):"Mantén clic · Limpiar el mostrador";
        }
        public void Interact()
        {
            if(MinigameActive){if(IsDucks)Ducks.Confirm();else Marble.Confirm();return;}
            if(MovingCart){if(park)park.ToggleCart();else{cartMotor.Stop();Save(false);}return;}
            if(park&&LookingAt(cart,3.4f)){park.ToggleCart();return;}
            if(park&&park.ActiveZone!=this)return;
            if(!park&&travelSign&&LookingAt(travelSign,3.5f)){Travel();return;}
            if(!SignFixed&&LookingAt(sign,3.5f)){FixSign();Save(false);return;}
            if(LookingAt(switchBox,3.5f)){if(!TryRestore())Tell("Falta despejar, limpiar o colocar el cartel.");return;}
            if(Restored&&LookingAt(ActivityTarget,4)){if(IsDucks)Ducks.Begin();else Marble.Begin();return;}
            if(LookingAt(cart,3.4f)){cartMotor.Toggle();Metrics.cartMoves++;Tell("Acerca el recogedor al montón. E para estacionar.");}
        }
        public void FixSign(){restoration.FixSign();sound.Pickup();}
        public bool TryRestore()
        {
            bool previous=Restored;if(!restoration.Activate(FloorDone,CounterDone))return false;
            if(!previous){Metrics.finishedSeconds=Metrics.seconds;Tell("¡"+definition.displayName+" vuelve a la vida!");Save(false);}return true;
        }
        public void ReleaseMarble(){restoration.Release();}
        public void SetVolume(float value){AudioListener.volume=Mathf.Clamp01(value);PlayerPrefs.SetFloat("feria.volume",AudioListener.volume);}
        public void SetSensitivity(float value){player.sensitivity=Mathf.Clamp(value,.035f,.18f);PlayerPrefs.SetFloat("feria.sensitivity",player.sensitivity);}
        public void TogglePause(){if(MinigameActive&&!Paused){Marble.Exit();Ducks.Exit();return;}SetPaused(!Paused); }
        public void SetPaused(bool paused)
        {
            if(park){park.SetPaused(paused);return;}this.paused=paused;Cursor.lockState=paused?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=paused;
            if(paused){sound?.Brush(0);PlayerPrefs.Save();}
        }
        public void Tell(string message){if(park){park.Tell(message);return;}notice=message;noticeTime=7;}
        public FairSave Snapshot()=>new FairSave {zoneId=definition.zoneId,litter=floor.CapturePositions(),removed=floor.CaptureRemoved(),dirt=counter.Capture(),cart=park?definition.localCart:cart.position,cartYaw=park?0:cart.eulerAngles.y,player=park?definition.localSpawn:player.transform.position,playerYaw=park?0:player.transform.eulerAngles.y,signFixed=SignFixed,restored=Restored,playSeconds=PlaySeconds};
        public void Apply(FairSave data)
        {
            SaveValidation.Validate(data,litter.Length,dirtPatches.Length,definition);
            floor.Restore(data.litter,data.removed);counter.Restore(data.dirt);if(!park){cartMotor.Stop();
            cart.SetPositionAndRotation(data.cart,Quaternion.Euler(0,data.cartYaw,0));player.Place(data.player,data.playerYaw);}
            restoration.Restore(data.signFixed,data.restored);Ducks.Reset();PlaySeconds=data.playSeconds;lastPlayer=player.transform.position;hasSweep=false;toolHeld=false;SurfaceTarget=-1;ToolContact=false;LastSurfaceCompleted=-1;SurfaceCompletedAt=-100;
        }
        public void TogglePractice()
        {
            if(park){park.TogglePractice();return;}
            if(InPractice){Apply(practiceReturn);practiceReturn=null;inPractice=false;Metrics=new PlaySessionMetrics();SetPaused(true);Tell("Has vuelto a tu partida guardada.");return;}
            Save(false);practiceReturn=Snapshot();Apply(initialState);inPractice=true;Metrics=new PlaySessionMetrics();Tool=0;SetPaused(false);
            Tell("Limpieza libre: puedes repetir esta zona sin cambiar tu partida.");
        }
        public bool Save(bool notify=true,string activeZone=null)
        {
            if(park)return park.Save(notify);
            if(suppressSave||InPractice||saveBlocked){if(notify)Tell(InPractice?"La limpieza libre no modifica tu partida guardada.":saveBlocked?"Se conserva el archivo de campaña para recuperarlo.":"Sesión de verificación: guardado desactivado.");return false;}
            try{campaignStore.Upsert(CampaignMapping.Capture(Snapshot(),litterIds,surfaceIds),activeZone);if(notify)Tell("Partida guardada en este equipo.");return true;}
            catch(Exception e){Debug.LogWarning("No se pudo guardar: "+e.Message);Tell("No se pudo guardar. Se conserva el progreso anterior.");return false;}
        }
        void Load()
        {
            campaign=campaignStore.Read(out bool recovered,out bool migrated,message=>Debug.LogWarning(message));saveBlocked=campaignStore.ReadOnly;
            var zone=campaign.zones.Find(z=>z.zoneId==definition.zoneId);
            if(zone!=null)try{Apply(CampaignMapping.Restore(zone,litterIds,surfaceIds));}catch(Exception e){saveBlocked=true;Debug.LogWarning("No se pudo aplicar el puesto: "+e.Message);}
            if(saveBlocked)Tell("No se pudo recuperar la campaña. Su archivo se conserva sin cambios.");
            else if(recovered)Tell("Campaña recuperada desde el respaldo.");else if(migrated)Tell("Tu primera zona se conserva al ampliar la feria.");
        }
        public void Travel(){if(park){Tell("Sigue las señales y camina hasta el siguiente puesto.");return;}BeginTravel(definition.nextZoneId,true);}
        void BeginTravel(string zoneId,bool saveCurrent)
        {
            if(ChangingZones||!catalog)return;var next=catalog.Find(zoneId);
            if(!next||!Application.CanStreamedLevelBeLoaded(next.sceneName)){Tell("Ese puesto todavía no está disponible.");return;}
            if(saveCurrent){if(InPractice)TogglePractice();if(!Save(false,next.zoneId)){Tell("Primero necesitamos guardar para cambiar de puesto.");return;}}
            StartCoroutine(TravelRoutine(next.sceneName));
        }
        IEnumerator TravelRoutine(string sceneName)
        {
            ChangingZones=true;SetPaused(true);
            while(Fade<1){Fade=Mathf.MoveTowards(Fade,1,Time.unscaledDeltaTime*4);yield return null;}
            var operation=SceneManager.LoadSceneAsync(sceneName);while(!operation.isDone)yield return null;
        }
        void OnApplicationQuit(){if(!park&&floor!=null)Save(false);PlayerPrefs.Save();Cursor.lockState=CursorLockMode.None;}
        void OnApplicationFocus(bool focus){if(!park&&!focus&&!Paused)SetPaused(true);}
        void OnDestroy(){litterRenderer?.Dispose();feedback?.Dispose();sound?.Dispose();vfx?.Dispose();}
    }
}
