using UnityEngine;

namespace Feria
{
    public sealed class FairAudio : System.IDisposable
    {
        readonly AudioSource brush,fx;
        readonly AudioClip leafBrush,clothBrush,pickup,bell;
        readonly AudioLowPassFilter brushFilter;
        readonly GameObject brushHost;
        float clock;
        public FairAudio(GameObject host)
        {
            brushHost=new GameObject("Audio · Herramienta");brushHost.transform.SetParent(host.transform,false);brush=brushHost.AddComponent<AudioSource>();brush.loop=true;brush.volume=0;
            leafBrush=Sound("Escoba sobre hojas",.72f,0,true,74,.45f);clothBrush=Sound("Paño sobre madera",.72f,0,true,231,.30f);brush.clip=leafBrush;brush.Play();brushFilter=brushHost.AddComponent<AudioLowPassFilter>();brushFilter.cutoffFrequency=7600;
            fx=host.AddComponent<AudioSource>();fx.volume=.22f;pickup=Sound("Recogida",.09f,660,false);bell=Sound("Campana",.9f,1046,false);
        }
        public void Brush(float volume,bool softSurface=false){var clip=softSurface?clothBrush:leafBrush;if(brush.clip!=clip){brush.clip=clip;}brushFilter.cutoffFrequency=softSurface?1750:7600;brush.volume=volume;}
        public void Tick(float dt){clock+=dt;brush.volume=Mathf.MoveTowards(brush.volume,0,dt*.2f);brush.pitch=1+.018f*Mathf.Sin(clock*2.1f)+.008f*Mathf.Sin(clock*3.7f);}
        public void Pickup(int count=1){fx.pitch=1;fx.PlayOneShot(pickup,Mathf.Min(.7f,.12f+count*.03f));}
        public void Bell(){fx.pitch=1;fx.PlayOneShot(bell);}
        public void MarbleNote(int step){fx.pitch=Mathf.Pow(1.059463f,Mathf.Clamp(step,0,12)-5);fx.PlayOneShot(pickup,.65f); }
        static AudioClip Sound(string name,float seconds,float frequency,bool noise,int seed=74,float amplitude=.45f)
        {
            const int rate=22050;var data=new float[(int)(rate*seconds)];var random=new System.Random(seed);float smooth=0;
            for(int i=0;i<data.Length;i++){float t=(float)i/rate;smooth=smooth*.65f+((float)random.NextDouble()*2-1)*.35f;data[i]=noise?smooth*amplitude:Mathf.Sin(t*frequency*Mathf.PI*2)*Mathf.Exp(-t*9)*.35f;}
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        public void Dispose(){Object.Destroy(leafBrush);Object.Destroy(clothBrush);Object.Destroy(pickup);Object.Destroy(bell);Object.Destroy(brushHost);}
    }
}
