using UnityEngine;

namespace Feria
{
    /// <summary>Original, lightweight foley made from generated audio for the quiet morning fair.</summary>
    public sealed class FairAudio : System.IDisposable
    {
        readonly AudioSource broom,cloth,fx,steps;
        readonly AudioClip broomBrush,clothBrush,pickup,bell,footstep,cartStep;
        readonly AudioLowPassFilter clothFilter;
        readonly GameObject toolHost;
        readonly FairUiAudio ui=Resources.Load<FairUiAudio>("AudioV012");
        float clock,broomTarget,clothTarget;
        readonly AudioClip[,] surfaceSteps=new AudioClip[4,3];
        int nextStep;
        public int StepCount=>nextStep;
        public float Phase(bool soft)=>((soft?cloth.time:broom.time)/(soft?clothBrush.length:broomBrush.length))*Mathf.PI*2;
        public static void Route(AudioSource source,string group)
        {
            var mixer=Resources.Load<UnityEngine.Audio.AudioMixer>("FeriaAudio");if(!mixer)return;var groups=mixer.FindMatchingGroups(group);if(groups.Length>0)source.outputAudioMixerGroup=groups[0];
        }

        public FairAudio(GameObject host)
        {
            toolHost=new GameObject("Audio · Herramientas artesanales");toolHost.SetActive(false);toolHost.transform.SetParent(host.transform,false);
            var broomHost=new GameObject("Cerdas · barrido corto");broomHost.transform.SetParent(toolHost.transform,false);
            broom=broomHost.AddComponent<AudioSource>();broom.loop=true;broom.spatialBlend=0;broom.volume=0;
            broomBrush=MakeBrush("Escoba · cerdas sobre las tablas",1.12f,74,.34f,.19f);broom.clip=broomBrush;broom.playOnAwake=false;
            var clothHost=new GameObject("Paño · roce suave");clothHost.transform.SetParent(toolHost.transform,false);
            cloth=clothHost.AddComponent<AudioSource>();cloth.loop=true;cloth.spatialBlend=0;cloth.volume=0;
            clothBrush=MakeBrush("Paño · madera encerada",.96f,231,.22f,.11f);cloth.clip=clothBrush;cloth.playOnAwake=false;
            clothFilter=clothHost.AddComponent<AudioLowPassFilter>();clothFilter.cutoffFrequency=3900;
            var fxHost=new GameObject("Campanillas del puesto");fxHost.transform.SetParent(toolHost.transform,false);fxHost.transform.localPosition=new Vector3(0,1.4f,4);
            fx=fxHost.AddComponent<AudioSource>();fx.playOnAwake=false;fx.spatialBlend=.65f;fx.minDistance=5;fx.maxDistance=30;fx.rolloffMode=AudioRolloffMode.Linear;fx.volume=.24f;
            pickup=MakeTone("Recogida",.11f,690,false);bell=MakeTone("Campana de feria",.9f,1046,true);
            footstep=MakeFootstep("Paso · suela en adoquín",.12f,81);cartStep=MakeFootstep("Paso · suela al empujar",.14f,113);
            var stepsHost=new GameObject("Pasos del paseo");stepsHost.transform.SetParent(toolHost.transform,false);
            steps=stepsHost.AddComponent<AudioSource>();steps.playOnAwake=false;steps.spatialBlend=0;steps.volume=.24f;
            for(int surface=0;surface<4;surface++)for(int variant=0;variant<3;variant++)surfaceSteps[surface,variant]=MakeFootstep("Paso original · "+(WalkSurface)surface+" "+variant,.19f,850+surface*31+variant,(WalkSurface)surface);
            Route(broom,"Herramientas");Route(cloth,"Herramientas");Route(steps,"Pasos");Route(fx,"Interacciones");
            toolHost.SetActive(true);broom.Play();cloth.Play();
        }

        public void Brush(float volume,bool softSurface=false)
        {
            if(softSurface){clothTarget=Mathf.Max(clothTarget,volume);broomTarget=0;}
            else{broomTarget=Mathf.Max(broomTarget,volume);clothTarget=0;}
        }

        public void Tick(float dt)
        {
            clock+=dt;float fade=dt*1.8f;
            broom.volume=Mathf.MoveTowards(broom.volume,broomTarget,fade);cloth.volume=Mathf.MoveTowards(cloth.volume,clothTarget,fade);
            broom.pitch=1+.025f*Mathf.Sin(clock*1.7f);cloth.pitch=.98f+.018f*Mathf.Sin(clock*1.1f);
            broomTarget=Mathf.MoveTowards(broomTarget,0,dt*.35f);clothTarget=Mathf.MoveTowards(clothTarget,0,dt*.35f);
        }

        public void Step(bool pushingCart,float intensity,WalkSurface surface=WalkSurface.Stone)
        {
            steps.pitch=Random.Range(.97f,1.03f)*(pushingCart?.96f:1);steps.PlayOneShot(surfaceSteps[(int)surface,nextStep++%3],Mathf.Clamp01(intensity)*.78f);
        }

        public void SilenceTools(){broomTarget=clothTarget=0;broom.volume=cloth.volume=0;}
        public void Pause(){SilenceTools();steps.Stop();fx.Pause();}
        public void Resume(){fx.UnPause();}
        public void ToolChanged(){fx.pitch=1;fx.PlayOneShot(ui&&ui.select?ui.select:pickup,.18f);}
        public void ObjectiveComplete(){fx.pitch=1;fx.PlayOneShot(ui&&ui.confirm?ui.confirm:pickup,.25f);}

        public void Pickup(int count=1){fx.pitch=1;fx.PlayOneShot(pickup,Mathf.Min(.7f,.12f+count*.03f));}
        public void Bell(){fx.pitch=1;fx.PlayOneShot(ui&&ui.complete?ui.complete:bell,.6f);}
        public void MarbleNote(int step){fx.pitch=Mathf.Pow(1.059463f,Mathf.Clamp(step,0,12)-5);fx.PlayOneShot(pickup,.65f);}

        static AudioClip MakeBrush(string name,float seconds,int seed,float softness,float grit)
        {
            const int rate=22050;int length=Mathf.RoundToInt(rate*seconds);var data=new float[length];var random=new System.Random(seed);float low=0,mid=0;
            for(int i=0;i<length;i++)
            {
                float t=(float)i/rate,phase=t/seconds;float stroke=Mathf.Max(Stroke(phase,.22f,.20f),Stroke(phase,.68f,.20f));
                float white=(float)random.NextDouble()*2-1;low=low*.91f+white*.09f;mid=mid*.58f+white*.42f;
                float bristles=(low*softness+(mid-low)*.55f+white*grit);
                float woodRattle=low*stroke*.07f;
                float texture=.82f+.18f*Mathf.Sin(t*31f+Mathf.Sin(t*7f)*1.4f);
                float fade=Mathf.Min(1,Mathf.Min(t/.035f,(seconds-t)/.035f));
                data[i]=(bristles*stroke*texture+woodRattle)*fade*.68f;
            }
            return Clip(name,data,rate);
        }

        static float Stroke(float phase,float center,float width)
        {
            float distance=Mathf.Abs(Mathf.DeltaAngle(phase*360,center*360))/360f;
            return Mathf.Pow(Mathf.Clamp01(1-distance/width),.72f);
        }

        static AudioClip MakeFootstep(string name,float seconds,int seed,WalkSurface surface=WalkSurface.Stone)
        {
            const int rate=22050;int length=Mathf.RoundToInt(rate*seconds);var data=new float[length];var random=new System.Random(seed);float body=0;
            for(int i=0;i<length;i++)
            {
                float t=(float)i/rate;float white=(float)random.NextDouble()*2-1;
                float smoothing=surface==WalkSurface.Grass?.91f:surface==WalkSurface.Earth?.86f:surface==WalkSurface.Wood?.95f:.81f;body=body*smoothing+white*(1-smoothing);
                float heel=Mathf.Exp(-t*34f)*Mathf.Min(1,t/.006f),toe=t>.055f?Mathf.Exp(-(t-.055f)*43)*.48f:0;
                float grit=surface==WalkSurface.Grass?.12f:surface==WalkSurface.Earth?.20f:.045f;
                data[i]=(body*1.7f+white*grit)*(heel+toe)*Mathf.Clamp01((seconds-t)/.02f);
            }
            return Clip(name,data,rate);
        }

        static AudioClip MakeTone(string name,float seconds,float frequency,bool bellLike)
        {
            const int rate=22050;int length=Mathf.RoundToInt(rate*seconds);var data=new float[length];
            for(int i=0;i<length;i++)
            {
                float t=(float)i/rate,env=bellLike?Mathf.Exp(-t*3.5f):Mathf.Exp(-t*24f);
                float tone=Mathf.Sin(t*frequency*Mathf.PI*2)+.28f*Mathf.Sin(t*frequency*2.01f*Mathf.PI*2);
                data[i]=tone*env*(bellLike ? .16f : .12f);
            }
            return Clip(name,data,rate);
        }

        static AudioClip Clip(string name,float[] data,int rate)
        {
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }

        public void Dispose()
        {
            Object.Destroy(broomBrush);Object.Destroy(clothBrush);Object.Destroy(pickup);Object.Destroy(bell);Object.Destroy(footstep);Object.Destroy(cartStep);Object.Destroy(toolHost);
            foreach(var clip in surfaceSteps)Object.Destroy(clip);
        }
    }
}
