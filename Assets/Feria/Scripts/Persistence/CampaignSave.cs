using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Feria
{
    [Serializable] public sealed class CampaignSave
    {
        public int version=2;
        public string activeZone="rincon-canicas";
        public List<ZoneProgress> zones=new List<ZoneProgress>();
    }
    [Serializable] public sealed class LitterRecord {public string id;public Vector3 position;public bool removed;}
    [Serializable] public sealed class SurfaceRecord {public string id;public float progress;}
    [Serializable] public sealed class ZoneProgress
    {
        public string zoneId;
        public LitterRecord[] litter;
        public SurfaceRecord[] surfaces;
        public Vector3 cart,player;
        public float cartYaw,playerYaw,playSeconds;
        public bool signFixed,restored;
    }
    public static class CampaignMapping
    {
        public static string[] Ids(string prefix,int count){var ids=new string[count];for(int i=0;i<count;i++)ids[i]=prefix+"-"+i.ToString("D4");return ids;}
        public static ZoneProgress Capture(FairSave data,string[] litterIds,string[] surfaceIds)
        {
            CheckIds(litterIds);CheckIds(surfaceIds);
            if(litterIds.Length!=data.litter.Length||surfaceIds.Length!=data.dirt.Length)throw new InvalidDataException("Identidades de zona incompletas.");
            var z=new ZoneProgress{zoneId=data.zoneId,cart=data.cart,player=data.player,cartYaw=data.cartYaw,playerYaw=data.playerYaw,playSeconds=data.playSeconds,signFixed=data.signFixed,restored=data.restored,litter=new LitterRecord[litterIds.Length],surfaces=new SurfaceRecord[surfaceIds.Length]};
            for(int i=0;i<litterIds.Length;i++)z.litter[i]=new LitterRecord{id=litterIds[i],position=data.litter[i],removed=data.removed[i]};
            for(int i=0;i<surfaceIds.Length;i++)z.surfaces[i]=new SurfaceRecord{id=surfaceIds[i],progress=data.dirt[i]};return z;
        }
        public static FairSave Restore(ZoneProgress zone,string[] litterIds,string[] surfaceIds)
        {
            CheckIds(litterIds);CheckIds(surfaceIds);
            if(zone==null||zone.litter==null||zone.surfaces==null||zone.litter.Length!=litterIds.Length||zone.surfaces.Length!=surfaceIds.Length)throw new InvalidDataException("Contenido de puesto incompatible.");
            var pieces=new Dictionary<string,LitterRecord>();foreach(var piece in zone.litter){if(piece==null||string.IsNullOrWhiteSpace(piece.id)||!pieces.TryAdd(piece.id,piece))throw new InvalidDataException("Residuo duplicado o sin identidad.");}
            var surfaces=new Dictionary<string,SurfaceRecord>();foreach(var surface in zone.surfaces){if(surface==null||string.IsNullOrWhiteSpace(surface.id)||!surfaces.TryAdd(surface.id,surface))throw new InvalidDataException("Superficie duplicada o sin identidad.");}
            var data=new FairSave{zoneId=zone.zoneId,cart=zone.cart,player=zone.player,cartYaw=zone.cartYaw,playerYaw=zone.playerYaw,playSeconds=zone.playSeconds,signFixed=zone.signFixed,restored=zone.restored,litter=new Vector3[litterIds.Length],removed=new bool[litterIds.Length],dirt=new float[surfaceIds.Length]};
            for(int i=0;i<litterIds.Length;i++){if(!pieces.TryGetValue(litterIds[i],out var p))throw new InvalidDataException("Falta un residuo identificado.");data.litter[i]=p.position;data.removed[i]=p.removed;}
            for(int i=0;i<surfaceIds.Length;i++){if(!surfaces.TryGetValue(surfaceIds[i],out var p))throw new InvalidDataException("Falta una superficie identificada.");data.dirt[i]=p.progress;}return data;
        }
        public static void CheckIds(string[] ids){if(ids==null)throw new InvalidDataException("Faltan identidades.");var seen=new HashSet<string>();foreach(var id in ids)if(string.IsNullOrWhiteSpace(id)||id.Length>80||!seen.Add(id))throw new InvalidDataException("Identidad inválida o duplicada.");}
    }
}
