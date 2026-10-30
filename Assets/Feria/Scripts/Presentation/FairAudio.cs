using UnityEngine;

namespace Feria
{
    public sealed class FairAudio : System.IDisposable
    {
        readonly AudioSource brush,fx;
        readonly AudioClip brushClip,pickup,bell;
        public FairAudio(GameObject host)
        {
            brush=host.AddComponent<AudioSource>();brush.loop=true;brush.volume=0;
            brushClip=Sound("Escoba",.55f,0,true);brush.clip=brushClip;brush.Play();
            fx=host.AddComponent<AudioSource>();fx.volume=.22f;pickup=Sound("Recogida",.09f,660,false);bell=Sound("Campana",.9f,1046,false);
        }
        public void Brush(float volume){brush.volume=volume;}
        public void Tick(float dt){brush.volume=Mathf.MoveTowards(brush.volume,0,dt*.2f);}
        public void Pickup(int count=1){fx.PlayOneShot(pickup,Mathf.Min(.7f,.12f+count*.03f));}
        public void Bell(){fx.PlayOneShot(bell);}
        static AudioClip Sound(string name,float seconds,float frequency,bool noise)
        {
            const int rate=22050;var data=new float[(int)(rate*seconds)];var random=new System.Random(74);float smooth=0;
            for(int i=0;i<data.Length;i++){float t=(float)i/rate;smooth=smooth*.65f+((float)random.NextDouble()*2-1)*.35f;data[i]=noise?smooth*.45f:Mathf.Sin(t*frequency*Mathf.PI*2)*Mathf.Exp(-t*9)*.35f;}
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        public void Dispose(){Object.Destroy(brushClip);Object.Destroy(pickup);Object.Destroy(bell);}
    }
}
