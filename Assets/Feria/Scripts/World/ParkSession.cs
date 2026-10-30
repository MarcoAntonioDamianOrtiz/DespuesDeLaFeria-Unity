using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Feria
{
    [DefaultExecutionOrder(-1000)]
    public sealed class ParkSession : MonoBehaviour
    {
        public ParkDefinition definition;
        public FairPlayer player;
        public Transform cart;
        public FairHud hud;
        public FairGame[] zones;
        public FairGame ActiveZone {get;private set;}
        public FairGame CurrentGame {get;private set;}
        public bool Paused {get;private set;}=true;
        public bool InPractice {get;private set;}
        public bool suppressSave;
        public int Tool {get;set;}
        public string Notice {get;private set;}="";
        public bool MovingCart=>cartMotor!=null&&cartMotor.Moving;
        public string SavePath=>store.Path;
        public int RestoredCount {get{int count=0;foreach(var zone in zones)if(zone.Restored)count++;return count;}}
        public string District=>ActiveZone?ActiveZone.definition.displayName:player.transform.position.z<9?"Atrio de bienvenida":player.transform.position.z>65?"Paseo de las guirnaldas":"Plaza de los faroles";
        CartMotor cartMotor;
        ParkSaveStore store;
        ParkSave practiceReturn;
        float autosave,noticeSeconds;
        bool saveBlocked,ready;
        void Awake()
        {
            if(!definition||!definition.catalog)throw new InvalidOperationException("Falta el plano del parque.");
            zones=GetComponentsInChildren<FairGame>(true);if(zones.Length==0)throw new InvalidOperationException("El parque no tiene atracciones.");
            foreach(var zone in zones){zone.park=this;zone.player=player;zone.cart=cart;zone.hud=hud;zone.catalog=definition.catalog;}
            CurrentGame=zones[0];player.game=CurrentGame;hud.game=CurrentGame;hud.park=this;
            cartMotor=new CartMotor(cart,player.transform,definition.worldMin,definition.worldMax);
            store=new ParkSaveStore(Path.Combine(FairSavePaths.Directory,"feria-park.json"),Path.Combine(FairSavePaths.Directory,"feria-campaign.json"),Path.Combine(FairSavePaths.Directory,"feria-slot-1.json"),definition,FindOrigin);
        }
        void Start()
        {
            var saved=store.Read(out bool recovered,out bool migrated,message=>Debug.LogWarning(message));saveBlocked=store.ReadOnly;
            try{Apply(saved);}catch(Exception e){saveBlocked=true;Debug.LogWarning("No se pudo aplicar el parque: "+e.Message);}
            ready=true;RefreshArea();SetPaused(true);
            if(saveBlocked)Tell("El archivo de partida se conserva para recuperarlo.");else if(recovered)Tell("Parque recuperado desde su respaldo.");else if(migrated)Tell("Tus puestos recuperados te esperan en el parque.");
        }
        void Update(){Tick(Time.deltaTime);}
        public void Tick(float elapsed)
        {
            if(!ready)return;
            noticeSeconds-=elapsed;if(noticeSeconds<=0)Notice="";
            RefreshArea();if(!Paused)cartMotor.Tick(Mathf.Min(elapsed,.05f));
            foreach(var zone in zones)zone.Tick(elapsed);
            if(Paused)return;autosave+=elapsed;if(autosave>=25){autosave=0;Save(false);}
        }
        public Transform FindOrigin(string zoneId){foreach(var zone in zones)if(zone.definition.zoneId==zoneId)return zone.origin;return null;}
        public void RefreshArea()
        {
            FairGame area=ActiveZone&&ActiveZone.ContainsPlayer(.8f)?ActiveZone:null;if(!area)foreach(var zone in zones)if(zone.ContainsPlayer()){area=zone;break;}
            ActiveZone=area;if(area)CurrentGame=area;player.game=CurrentGame;
        }
        public Vector3 NextEntrance()
        {
            FairGame nearest=null;float distance=float.MaxValue;
            foreach(var zone in zones){if(zone.Restored)continue;float d=Vector3.Distance(player.transform.position,zone.Entrance);if(d<distance){distance=d;nearest=zone;}}
            if(!nearest)foreach(var zone in zones){float d=Vector3.Distance(player.transform.position,zone.Entrance);if(d<distance){distance=d;nearest=zone;}}
            return nearest?nearest.Entrance:definition.entrance;
        }
        public void ToggleCart(){cartMotor.Toggle();if(!MovingCart)Save(false);else Tell("Lleva el recogedor por los caminos. E para estacionar.");}
        public void SetPaused(bool value){Paused=value;Cursor.lockState=value?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=value;if(value)PlayerPrefs.Save();}
        public void Tell(string text){Notice=text;noticeSeconds=6;}
        public ParkSave Snapshot()
        {
            var save=new ParkSave{parkId=definition.parkId,world=new ParkWorldState{player=player.transform.position,playerYaw=player.transform.eulerAngles.y,pitch=player.Pitch,cart=cart.position,cartYaw=cart.eulerAngles.y,tool=Tool}};
            foreach(var zone in zones)save.zones.Add(CampaignMapping.Capture(zone.Snapshot(),zone.litterIds,zone.surfaceIds));return save;
        }
        public void Apply(ParkSave save)
        {
            store.Validate(save);var pending=new List<(FairGame game,FairSave data)>();
            foreach(var zone in zones)
            {
                var state=save.zones.Find(z=>z.zoneId==zone.definition.zoneId);var snapshot=state==null?zone.InitialState:CampaignMapping.Restore(state,zone.litterIds,zone.surfaceIds);
                SaveValidation.Validate(snapshot,zone.litter.Length,zone.dirtPatches.Length,zone.definition);pending.Add((zone,snapshot));
            }
            cartMotor.Stop();foreach(var state in pending)state.game.Apply(state.data);
            var w=save.world;cart.SetPositionAndRotation(w.cart,Quaternion.Euler(0,w.cartYaw,0));player.Place(w.player,w.playerYaw,w.pitch);Tool=w.tool;RefreshArea();
        }
        public bool Save(bool notify=true)
        {
            if(!ready||suppressSave||InPractice||saveBlocked){if(notify&&InPractice)Tell("La limpieza libre conserva tu partida.");return false;}
            try{store.Write(Snapshot());if(notify)Tell("Parque guardado en este equipo.");return true;}
            catch(Exception e){Debug.LogWarning("No se pudo guardar el parque: "+e.Message);Tell("No se pudo guardar. Se conserva el archivo anterior.");return false;}
        }
        public void TogglePractice()
        {
            if(!InPractice)
            {
                Save(false);practiceReturn=Snapshot();InPractice=true;foreach(var zone in zones)zone.Apply(zone.InitialState);SetPaused(false);Tell("Limpieza libre del parque. Tu partida se conserva.");
            }
            else
            {
                // Keep the physical player/cart where they are; only restore the campaign's attractions.
                var position=player.transform.position;float yaw=player.transform.eulerAngles.y,pitch=player.Pitch;var cartPosition=cart.position;var cartRotation=cart.rotation;
                Apply(practiceReturn);player.Place(position,yaw,pitch);cart.SetPositionAndRotation(cartPosition,cartRotation);practiceReturn=null;InPractice=false;SetPaused(true);Tell("Has vuelto a tu partida en este mismo lugar.");
            }
            RefreshArea();
        }
        void OnApplicationFocus(bool focus){if(!focus&&ready)SetPaused(true);}
        void OnApplicationQuit(){if(ready)Save(false);Cursor.lockState=CursorLockMode.None;PlayerPrefs.Save();}
    }
}
