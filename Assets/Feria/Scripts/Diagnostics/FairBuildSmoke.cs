using System;
using System.Collections;
using UnityEngine;

namespace Feria
{
    /// <summary>Opt-in startup check for a built player. Normal launches create no diagnostic object.</summary>
    public sealed class FairBuildSmoke : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-feria-smoke")<0)return;
            new GameObject("Verificación de arranque").AddComponent<FairBuildSmoke>();
        }
        IEnumerator Start()
        {
            var loaded=FindFirstObjectByType<ParkSession>();
            if(loaded)
            {
                loaded.suppressSave=true;yield return null;loaded.TogglePractice();loaded.player.enabled=false;
                var zone=loaded.zones[1];loaded.player.Place(zone.World(new Vector3(0,.05f,-5)),zone.origin.eulerAngles.y,25);loaded.RefreshArea();loaded.SetPaused(false);loaded.player.broom.gameObject.SetActive(false);if(loaded.player.cloth)loaded.player.cloth.gameObject.SetActive(false);
            }
            int start=Time.frameCount;var times=new System.Collections.Generic.List<float>();for(int i=0;i<150;i++){yield return null;if(i>=30)times.Add(Time.unscaledDeltaTime*1000);}
            var park=FindFirstObjectByType<ParkSession>();bool valid=park&&park.zones.Length==2&&FindObjectsByType<FairPlayer>(FindObjectsSortMode.None).Length==1&&Time.frameCount>start+100;
            float sum=0;foreach(float ms in times)sum+=ms;times.Sort();
            if(valid)Debug.Log("FERIA_SMOKE_PASS · "+Application.version+" · ParquePrincipal · frames="+Time.frameCount+" · mean_ms="+(sum/times.Count)+" · p95_ms="+times[(int)(times.Count*.95f)]+" · "+SystemInfo.graphicsDeviceType+" · "+Screen.width+"x"+Screen.height);
            else Debug.LogError("FERIA_SMOKE_FAIL · Inicialización incompleta");
            var args=Environment.GetCommandLineArgs();int output=Array.IndexOf(args,"-feria-smoke-output");
            if(output>=0&&output+1<args.Length&&park)
            {
                foreach(var zone in park.zones)zone.DrawLitter();Capture(park.player.view,args[output+1]);yield return null;
            }
            Application.Quit(valid?0:2);
        }
        static void Capture(Camera camera,string path)
        {
            var rt=RenderTexture.GetTemporary(1280,720,24);var previous=camera.targetTexture;var active=RenderTexture.active;Texture2D image=null;
            try
            {
                camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();System.IO.File.WriteAllBytes(path,image.EncodeToPNG());Debug.Log("FERIA_SMOKE_CAPTURE · Cámara renderizada");
            }
            finally{camera.targetTexture=previous;RenderTexture.active=active;if(image)Destroy(image);RenderTexture.ReleaseTemporary(rt);}
        }
    }
}
