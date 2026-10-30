using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Feria
{
    /// <summary>Atomic multi-zone saves. The previous v1 file remains untouched during migration.</summary>
    public sealed class CampaignSaveStore
    {
        public string Path {get;}
        readonly string legacyPath;
        readonly FairZoneCatalog catalog;
        public bool ReadOnly {get;private set;}
        public CampaignSaveStore(string path,string legacyPath,FairZoneCatalog catalog){Path=path;this.legacyPath=legacyPath;this.catalog=catalog;}
        public CampaignSave Read(out bool recovered,out bool migrated,Action<string> warning=null)
        {
            recovered=migrated=false;bool hadFile=false;
            foreach(var candidate in new[]{Path,Path+".bak"})
            {
                if(!File.Exists(candidate))continue;hadFile=true;
                try{var data=JsonUtility.FromJson<CampaignSave>(File.ReadAllText(candidate));if(data!=null&&data.version>2){ReadOnly=true;warning?.Invoke("La campaña pertenece a una versión posterior. Se conserva sin cambios.");return new CampaignSave();}Validate(data);if(candidate!=Path){File.Copy(candidate,Path,true);recovered=true;}ReadOnly=false;return data;}
                catch(Exception e){warning?.Invoke("No se pudo leer la campaña: "+e.Message);}
            }
            // Never replace an unreadable campaign with a new, apparently valid empty file.
            if(hadFile){ReadOnly=true;return new CampaignSave();}
            if(!string.IsNullOrEmpty(legacyPath))foreach(var candidate in new[]{legacyPath,legacyPath+".bak"})
            {
                if(!File.Exists(candidate))continue;
                try
                {
                    var old=JsonUtility.FromJson<FairSave>(File.ReadAllText(candidate));var first=catalog.Find("rincon-canicas");
                    SaveValidation.Validate(old,720,16,first);old.zoneId=first.zoneId;
                    var data=new CampaignSave();data.zones.Add(CampaignMapping.Capture(old,CampaignMapping.Ids("litter",720),CampaignMapping.Ids("surface",16)));migrated=true;return data;
                }
                catch(Exception e){warning?.Invoke("No se pudo migrar la partida anterior: "+e.Message);}
            }
            ReadOnly=false;return new CampaignSave();
        }
        public void Upsert(ZoneProgress zone,string activeZone=null)
        {
            var campaign=Read(out _,out _);if(ReadOnly)throw new InvalidDataException("La campaña necesita recuperar un respaldo; se conserva el archivo original.");
            campaign.zones.RemoveAll(z=>z.zoneId==zone.zoneId);campaign.zones.Add(zone);campaign.activeZone=activeZone??zone.zoneId;Write(campaign);
        }
        public void Write(CampaignSave data)
        {
            Validate(data);Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Path)));string temp=Path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(data));
            if(File.Exists(Path))File.Replace(temp,Path,Path+".bak");else File.Move(temp,Path);ReadOnly=false;
        }
        public void Validate(CampaignSave data)
        {
            if(data==null||data.version!=2||data.zones==null||data.zones.Count>32||!catalog||!catalog.Find(data.activeZone))throw new InvalidDataException("Formato de campaña incompatible.");
            var seen=new HashSet<string>();
            foreach(var zone in data.zones)
            {
                if(zone==null||!seen.Add(zone.zoneId)||!catalog.Find(zone.zoneId)||zone.litter==null||zone.surfaces==null||zone.litter.Length>10000||zone.surfaces.Length>1000)throw new InvalidDataException("Puesto inválido en la campaña.");
                var definition=catalog.Find(zone.zoneId);var ids=new string[zone.litter.Length];var surfaces=new string[zone.surfaces.Length];
                for(int i=0;i<ids.Length;i++)ids[i]=zone.litter[i]?.id;for(int i=0;i<surfaces.Length;i++)surfaces[i]=zone.surfaces[i]?.id;
                var snapshot=CampaignMapping.Restore(zone,ids,surfaces);SaveValidation.Validate(snapshot,definition.litterCount,definition.surfaceCount,definition);
            }
        }
    }
}
