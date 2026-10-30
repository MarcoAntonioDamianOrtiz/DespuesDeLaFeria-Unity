using System;
using System.IO;
using UnityEngine;
namespace Feria
{
    /// <summary>One physical player/cart in a continuous park; attraction progress remains local to its origin.</summary>
    public sealed class ParkSaveStore
    {
        public string Path {get;}
        readonly ParkDefinition definition;
        readonly CampaignSaveStore legacy;
        readonly Func<string,Transform> origin;
        public bool ReadOnly {get;private set;}
        public ParkSaveStore(string path,string campaignPath,string v1Path,ParkDefinition definition,Func<string,Transform> origin)
        {
            Path=path;this.definition=definition;this.origin=origin;legacy=new CampaignSaveStore(campaignPath,v1Path,definition.catalog);
        }
        public ParkSave Fresh()=>new ParkSave{parkId=definition.parkId,world=new ParkWorldState{player=definition.entrance,cart=definition.cartStart,pitch=8}};
        public ParkSave Read(out bool recovered,out bool migrated,Action<string> warning=null)
        {
            recovered=migrated=false;bool exists=false;
            foreach(string candidate in new[]{Path,Path+".bak"})
            {
                if(!File.Exists(candidate))continue;exists=true;
                try
                {
                    var data=JsonUtility.FromJson<ParkSave>(File.ReadAllText(candidate));
                    if(data!=null&&data.version>3){ReadOnly=true;warning?.Invoke("La partida del parque pertenece a una versión posterior.");return Fresh();}
                    Validate(data);if(candidate!=Path){File.Copy(candidate,Path,true);recovered=true;}ReadOnly=false;return data;
                }
                catch(Exception e){warning?.Invoke("No se pudo recuperar el parque: "+e.Message);}
            }
            if(exists){ReadOnly=true;return Fresh();}
            var old=legacy.Read(out _,out _,warning);if(legacy.ReadOnly){ReadOnly=true;return Fresh();}
            var result=Fresh();result.zones=old.zones;
            if(old.zones.Count>0)
            {
                var last=old.zones.Find(z=>z.zoneId==old.activeZone)??old.zones[0];var basis=origin(last.zoneId);
                if(!basis){ReadOnly=true;warning?.Invoke("Falta el puesto de la partida anterior.");return Fresh();}
                result.world.player=basis.TransformPoint(last.player);result.world.cart=basis.TransformPoint(last.cart);
                result.world.playerYaw=basis.eulerAngles.y+last.playerYaw;result.world.cartYaw=basis.eulerAngles.y+last.cartYaw;migrated=true;
            }
            Validate(result);ReadOnly=false;return result;
        }
        public void Write(ParkSave data)
        {
            if(ReadOnly)throw new InvalidDataException("El archivo original del parque está protegido.");
            Validate(data);Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Path)));string temp=Path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(data));
            if(File.Exists(Path))File.Replace(temp,Path,Path+".bak");else File.Move(temp,Path);
        }
        public void Validate(ParkSave data)
        {
            if(data==null||data.version!=3||data.parkId!=definition.parkId||data.world==null||data.zones==null)throw new InvalidDataException("Formato del parque incompatible.");
            var w=data.world;
            if(!Position(w.player,-.2f,3)||!Position(w.cart,-.2f,.5f)||!float.IsFinite(w.playerYaw)||!float.IsFinite(w.cartYaw)||!float.IsFinite(w.pitch)||w.pitch< -55||w.pitch>78||w.tool<0||w.tool>1)throw new InvalidDataException("Posición del parque inválida.");
            legacy.Validate(new CampaignSave{activeZone=definition.catalog.zones[0].zoneId,zones=data.zones});
        }
        bool Position(Vector3 p,float low,float high)=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z)&&p.x>=definition.worldMin.x&&p.x<=definition.worldMax.x&&p.z>=definition.worldMin.y&&p.z<=definition.worldMax.y&&p.y>=low&&p.y<=high;
    }
}
