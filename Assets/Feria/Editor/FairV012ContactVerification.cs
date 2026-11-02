using System;
using Feria;
using UnityEngine;
public static class FairV012ContactVerification
{
    public static string[] Run()
    {
        var park=UnityEngine.Object.FindFirstObjectByType<ParkSession>();
        var saved=park.Snapshot();bool suppressed=park.suppressSave, enabled=park.player.enabled, paused=park.Paused;
        var controller=park.player.GetComponent<CharacterController>();bool collision=controller.enabled;
        var camera=park.player.view.transform;var cp=camera.localPosition;var cr=camera.localRotation;
        try
        {
            park.suppressSave=true;park.player.enabled=false;park.SetPaused(false);
            var zone=park.zones[0];var middle=(zone.definition.litterMin+zone.definition.litterMax)*.5f;
            var point=zone.World(new Vector3(middle.x,.04f,middle.y));
            park.player.Place(point+Vector3.up*.01f,zone.origin.eulerAngles.y,78);park.RefreshArea();controller.enabled=false;
            camera.position=point+Vector3.up*2.81f;camera.rotation=Quaternion.Euler(90,0,0);Physics.SyncTransforms();
            if(zone.TrySweepPoint(out _))throw new Exception("FAIL · Escoba alcanza 2,81 m; debe rechazar más de 2,8 m");
            camera.position=point+Vector3.up*2.79f;Physics.SyncTransforms();
            if(!zone.TrySweepPoint(out _))throw new Exception("FAIL · Escoba rechaza contacto libre a 2,79 m");
            return new[]{"PASS · Escoba respeta límite de contacto 2,8 m"};
        }
        finally {park.Apply(saved);controller.enabled=collision;camera.localPosition=cp;camera.localRotation=cr;park.SetPaused(paused);park.player.enabled=enabled;park.suppressSave=suppressed;}
    }
}
