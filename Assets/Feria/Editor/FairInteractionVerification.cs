using System;
using Feria;
using UnityEngine;
using UnityEditor;

public static class FairInteractionVerification
{
    public static string[] Run()
    {
        if(!EditorApplication.isPlaying)throw new Exception("Entra en Play.");
        var park=UnityEngine.Object.FindFirstObjectByType<ParkSession>();var saved=park.Snapshot();bool paused=park.Paused,suppressed=park.suppressSave,enabled=park.player.enabled;
        GameObject wall=null;park.suppressSave=true;park.player.enabled=false;int rays=0;
        try
        {
            foreach(var zone in park.zones)
            {
                zone.Apply(zone.InitialState);Physics.SyncTransforms();
                for(int i=0;i<zone.dirtPatches.Length;i++)
                {
                    foreach(float progress in new[]{0f,.45f})
                    {
                    var partial=zone.Snapshot();partial.dirt[i]=progress;zone.Apply(partial);
                    var patch=zone.dirtPatches[i];var mesh=patch.GetComponent<MeshFilter>().sharedMesh;var center=patch.transform.TransformPoint(mesh.bounds.center);
                    foreach(float distance in new[]{.85f,1.6f,2.8f})foreach(float side in new[]{-.4f,0,.4f})
                    {
                        var eye=center+Vector3.up*distance*.65f-zone.origin.forward*distance*.70f+zone.origin.right*side;
                        foreach(var local in new[]{mesh.bounds.center,mesh.vertices[1]*.96f,mesh.vertices[4]*.96f,mesh.vertices[8]*.96f})
                        {
                            var point=patch.transform.TransformPoint(local);var ray=new Ray(eye,(point-eye).normalized);
                            bool visible=!Physics.Raycast(ray,out var obstacle,Vector3.Distance(eye,point)-.015f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore);
                            bool found=zone.TrySurfaceTarget(ray,out int index,out _);
                            Check(visible?(found&&index==i):(!found||index!=i),"Centro/borde/ángulo de mancha "+i+" en "+zone.definition.zoneId+" distancia="+distance+" lateral="+side+" visible="+visible+" obstáculo="+(obstacle.collider?obstacle.collider.name:"ninguno"));rays++;
                        }
                    }
                }
                    }
                zone.Apply(zone.InitialState);
                var target=zone.dirtPatches[0].bounds.center;var from=target+Vector3.up*.8f-zone.origin.forward*1.3f;var aim=new Ray(from,(target-from).normalized);
                Check(!zone.TrySurfaceTarget(new Ray(target+Vector3.up*4,Vector3.down),out _,out _),"Fuera de alcance");
                wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=(from+target)*.5f;wall.transform.localScale=Vector3.one*.25f;Physics.SyncTransforms();
                Check(!zone.TrySurfaceTarget(aim,out _,out _),"No limpia detrás de un objeto sólido");UnityEngine.Object.DestroyImmediate(wall);wall=null;
                park.player.Place(zone.World(new Vector3(-2.5f,.05f,2.15f)),zone.origin.eulerAngles.y,25);park.RefreshArea();park.SetPaused(false);park.player.view.transform.LookAt(target);
                zone.Tool=0;zone.UseTool(true,.2f);Check(zone.Snapshot().dirt[0]==0,"Escoba no limpia manchas");
                zone.Tool=1;park.SetPaused(true);zone.UseTool(true,.2f);Check(zone.Snapshot().dirt[0]==0,"Pausa bloquea limpieza");park.SetPaused(false);
                zone.UseTool(true,.2f);Check(Mathf.Abs(zone.Snapshot().dirt[0]-.18f)<.001f&&zone.ToolContact,"Paño limpia el objetivo visible");
                zone.UseTool(false,.4f);Check(Mathf.Abs(zone.Snapshot().dirt[0]-.18f)<.001f,"Soltar clic detiene progreso");
                // Camera moves while the same held action continues.
                park.player.view.transform.LookAt(target+zone.origin.right*.01f);zone.UseTool(true,.2f);
                Check(zone.Snapshot().dirt[0]>.18f,"Limpieza continua con cámara en movimiento");
                zone.UseTool(true,2);Check(zone.CleanedSurfaces==1&&!zone.dirtPatches[0].gameObject.activeSelf,"Contador y visual de completado coherentes");
                var snapshot=zone.Snapshot();zone.Apply(zone.InitialState);zone.Apply(snapshot);Check(zone.CleanedSurfaces==1,"Progreso y contador se reconstruyen al cargar");
            }
            return new[]{"PASS · "+rays+" rayos en centros y bordes, 3 distancias y 3 ángulos de ambos puestos","PASS · Fuera de alcance, oclusión, herramienta incorrecta y pausa rechazados","PASS · Clic continuo, cámara móvil, soltar, completar y recargar preservan progreso individual"};
        }
        finally{if(wall)UnityEngine.Object.DestroyImmediate(wall);park.Apply(saved);park.SetPaused(paused);park.player.enabled=enabled;park.suppressSave=suppressed;}
    }
    static void Check(bool valid,string message){if(!valid)throw new Exception("FAIL · "+message);}
}
