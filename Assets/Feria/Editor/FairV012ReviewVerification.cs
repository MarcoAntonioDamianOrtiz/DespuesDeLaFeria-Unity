using System;
using System.Reflection;
using Feria;
using UnityEngine;
public static class FairV012ReviewVerification
{
    const BindingFlags Fields=BindingFlags.Instance|BindingFlags.NonPublic;
    public static string[] Run(string check)
    {
        var park=UnityEngine.Object.FindFirstObjectByType<ParkSession>();var saved=park.Snapshot();bool suppressed=park.suppressSave,paused=park.Paused,enabled=park.player.enabled;
        var player=park.player;var camera=player.view.transform;var cp=camera.localPosition;var cr=camera.localRotation;var controller=player.GetComponent<CharacterController>();bool collision=controller.enabled;
        var clothPos=player.cloth.localPosition;var clothRot=player.cloth.localRotation;var broomPos=player.broom.localPosition;var broomRot=player.broom.localRotation;
        try
        {
            park.suppressSave=true;player.enabled=false;park.SetPaused(false);var zone=park.zones[0];zone.Apply(zone.InitialState);
            player.Place(zone.World(new Vector3(-2.5f,.05f,2.15f)),zone.origin.eulerAngles.y,25);park.RefreshArea();controller.enabled=false;park.Tool=1;
            if(check=="label")
            {
                camera.LookAt(zone.dirtPatches[0].bounds.center);zone.UseTool(true,2);
                if(zone.CleanedSurfaces!=1)throw new Exception("FAIL · Preparación: completar primera mancha");
                camera.LookAt(zone.dirtPatches[1].bounds.center);zone.UseTool(false);
                park.hud.game=zone;
                typeof(FairHud).GetMethod("ShowCleaningTarget",Fields).Invoke(park.hud,null);
                var title=(TMPro.TextMeshProUGUI)typeof(FairHud).GetField("contextTitle",Fields).GetValue(park.hud);
                if(!title.text.Contains(zone.stainCatalog.Get(0).displayName))throw new Exception("FAIL · Confirmación nombra la mancha siguiente sin limpiar");
            }
            if(check=="cloth")
            {
                var point=zone.dirtPatches[0].transform.position;
                typeof(FairGame).GetProperty("ContactPoint").SetValue(zone,point);
                var sound=typeof(FairGame).GetField("sound",Fields).GetValue(zone);
                var source=(AudioSource)typeof(FairAudio).GetField("cloth",Fields).GetValue(sound);
                foreach(float pitch in new[]{25f,45f,60f})foreach(float phase in new[]{0f,.125f,.25f,.375f,.5f,.625f,.75f,.875f})
                {
                    camera.rotation=Quaternion.Euler(pitch,zone.origin.eulerAngles.y,0);source.time=source.clip.length*phase;
                    player.AnimateTool(true,0,1);float height=Vector3.Dot(player.cloth.position-point,zone.origin.up);
                    if(height<.019f||height>.031f)throw new Exception("FAIL · Paño abandona el plano: pitch="+pitch+" altura="+height);
                }
            }
            if(check=="range")
            {
                park.Tool=0;var middle=(zone.definition.litterMin+zone.definition.litterMax)*.5f;var point=zone.World(new Vector3(middle.x,.04f,middle.y));camera.position=point+Vector3.up*3.3f;camera.rotation=Quaternion.Euler(90,0,0);Physics.SyncTransforms();
                zone.UseTool(false);if(!zone.Prompt().Contains("Acércate"))throw new Exception("FAIL · Falta aviso de acercamiento fuera de alcance");
            }
            return new[]{"PASS · Revisión "+check};
        }
        finally {park.Apply(saved);camera.localPosition=cp;camera.localRotation=cr;controller.enabled=collision;player.cloth.localPosition=clothPos;player.cloth.localRotation=clothRot;player.broom.localPosition=broomPos;player.broom.localRotation=broomRot;park.SetPaused(paused);player.enabled=enabled;park.suppressSave=suppressed;}
    }
}
