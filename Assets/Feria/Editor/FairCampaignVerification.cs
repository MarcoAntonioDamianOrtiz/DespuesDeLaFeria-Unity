using System;
using System.Collections.Generic;
using System.IO;
using Feria;
using UnityEditor;
using UnityEngine;

public static class FairCampaignVerification
{
    public static string[] Run()
    {
        var catalog=AssetDatabase.LoadAssetAtPath<FairZoneCatalog>("Assets/Feria/Definitions/FeriaCatalog.asset");Check(catalog&&catalog.zones.Length==2,"Catálogo con dos puestos");
        string folder=System.IO.Path.GetFullPath("../../work/v05/campaign-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
        var first=Baseline(catalog.Find("rincon-canicas"));first.removed[17]=true;first.dirt[3]=.64f;
        string legacy=System.IO.Path.Combine(folder,"old.json"),path=System.IO.Path.Combine(folder,"campaign.json");File.WriteAllText(legacy,JsonUtility.ToJson(first));string legacyBytes=File.ReadAllText(legacy);
        var store=new CampaignSaveStore(path,legacy,catalog);var migrated=store.Read(out _,out bool didMigrate);Check(didMigrate&&migrated.zones.Count==1&&migrated.zones[0].litter[17].removed,"Migra el progreso v1");
        var duck=Baseline(catalog.Find("galeria-patitos"));duck.dirt[1]=.25f;duck.removed[5]=true;
        var duckRecord=Record(duck);store.Upsert(duckRecord);var campaign=store.Read(out _,out _);Check(campaign.zones.Count==2&&campaign.activeZone==duck.zoneId&&campaign.zones.Find(z=>z.zoneId==first.zoneId).litter[17].removed,"Añadir patitos conserva canicas");Check(File.ReadAllText(legacy)==legacyBytes,"El archivo v1 queda intacto");
        first.removed[21]=true;store.Upsert(Record(first),duck.zoneId);campaign=store.Read(out _,out _);Check(campaign.activeZone==duck.zoneId&&campaign.zones.Find(z=>z.zoneId==duck.zoneId).surfaces[1].progress==.25f,"Volver a guardar canicas conserva patitos");
        var ids=CampaignMapping.Ids("litter",duck.litter.Length);Array.Reverse(ids);var reordered=CampaignMapping.Restore(duckRecord,ids,CampaignMapping.Ids("surface",duck.dirt.Length));Check(reordered.removed[duck.litter.Length-6]&&!reordered.removed[5],"Identidad estable al reordenar objetos");
        File.WriteAllText(path,"{corrupto");campaign=store.Read(out bool recovered,out _);Check(recovered&&campaign.zones.Count==2&&JsonUtility.FromJson<CampaignSave>(File.ReadAllText(path)).version==2,"Respaldo recupera una campaña completa");
        string valid=File.ReadAllText(path);var bad=Record(duck);bad.litter[1].id=bad.litter[0].id;bool rejected=false;try{store.Upsert(bad);}catch(InvalidDataException){rejected=true;}Check(rejected&&File.ReadAllText(path)==valid,"Duplicados no sustituyen el guardado");
        bad=Record(duck);bad.player.x=float.NaN;rejected=false;try{store.Upsert(bad);}catch(InvalidDataException){rejected=true;}Check(rejected&&File.ReadAllText(path)==valid,"Coordenadas inválidas rechazadas antes de escribir");
        File.WriteAllText(path,"{\"version\":99,\"activeZone\":\"rincon-canicas\",\"zones\":[]}");string future=File.ReadAllText(path);store.Read(out _,out _);Check(store.ReadOnly,"Versión posterior protegida");rejected=false;try{store.Upsert(Record(first));}catch(InvalidDataException){rejected=true;}Check(rejected&&File.ReadAllText(path)==future,"No rebaja una campaña posterior aunque exista un respaldo v2");
        return new[]{"PASS · Migración v1 a v2 sin alterar el archivo antiguo","PASS · Dos puestos conservan progreso independiente y zona activa","PASS · Residuos identificados conservan progreso al reordenar","PASS · Respaldo, duplicados, posiciones no finitas y versiones posteriores protegidas"};
    }
    static ZoneProgress Record(FairSave data)=>CampaignMapping.Capture(data,CampaignMapping.Ids("litter",data.litter.Length),CampaignMapping.Ids("surface",data.dirt.Length));
    static FairSave Baseline(FairZoneDefinition zone)
    {
        var data=new FairSave{zoneId=zone.zoneId,litter=new Vector3[zone.litterCount],removed=new bool[zone.litterCount],dirt=new float[zone.surfaceCount],cart=new Vector3(2.7f,0,-4.4f),player=new Vector3(0,.05f,-7.7f)};
        for(int i=0;i<data.litter.Length;i++)data.litter[i]=new Vector3((i%30)*.10f-1.5f,.03f,(i/30)*.12f-4);return data;
    }
    static void Check(bool condition,string message){if(!condition)throw new Exception("FAIL · "+message);}
}
