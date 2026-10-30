using System;
using UnityEngine;
namespace Feria
{
    /// <summary>Three relaxed ring throws. Moving targets reward anticipating their path.</summary>
    public sealed class DuckGame
    {
        public enum Phase { Idle, Aiming, Throwing, Result }
        readonly Transform[] ducks;
        readonly Vector3[] origins;
        readonly Quaternion[] rotations;
        readonly Vector3[] scales;
        readonly float[] pulses;
        bool judged;
        int caught=-1;
        public bool HasLanded=>judged;
        readonly Transform aimRing,flyingRing;
        Vector3 aim=new Vector3(0,1.54f,5),landing;
        float clock,flight;
        public Phase State {get;private set;}
        public bool Active=>State!=Phase.Idle;
        public int Throws {get;private set;}
        public int Score {get;private set;}
        public int LastScore {get;private set;}
        public Vector3 AimPoint=>aim;
        public event Action<int> Landed;
        public event Action<int> Finished;
        public DuckGame(Transform[] targets,Transform aimRing,Transform flyingRing)
        {
            ducks=targets??Array.Empty<Transform>();this.aimRing=aimRing;this.flyingRing=flyingRing;
            origins=new Vector3[ducks.Length];rotations=new Quaternion[ducks.Length];scales=new Vector3[ducks.Length];pulses=new float[ducks.Length];
            for(int i=0;i<ducks.Length;i++){origins[i]=ducks[i].position;rotations[i]=ducks[i].rotation;scales[i]=ducks[i].localScale;}Exit();
        }
        public void Begin(){Throws=Score=LastScore=0;aim=new Vector3(0,1.54f,5);State=Phase.Aiming;ShowRings(true,false);}
        public void Aim(Vector2 delta){if(State!=Phase.Aiming)return;aim.x=Mathf.Clamp(aim.x+delta.x*.0038f,-1.04f,1.04f);aim.z=Mathf.Clamp(aim.z+delta.y*.0038f,4.47f,5.59f);}
        public void SetAim(Vector3 point){if(State!=Phase.Aiming)return;aim=new Vector3(Mathf.Clamp(point.x,-1.04f,1.04f),1.54f,Mathf.Clamp(point.z,4.47f,5.59f));}
        public void Confirm(){if(State==Phase.Result){Begin();return;}if(State!=Phase.Aiming)return;landing=aim;flight=0;LastScore=0;judged=false;caught=-1;State=Phase.Throwing;ShowRings(false,true);}
        public void Tick(float dt)
        {
            if(dt<=0)return;clock+=dt;
            for(int i=0;i<ducks.Length;i++)
            {
                pulses[i]=Mathf.MoveTowards(pulses[i],0,dt*2);var p=origins[i];p.y+=Mathf.Sin(pulses[i]*Mathf.PI)*.07f;ducks[i].localScale=scales[i]*(1+Mathf.Sin(pulses[i]*Mathf.PI)*.09f);p.x+=Mathf.Sin(clock*.85f+i*1.8f)*.29f;p.y+=Mathf.Sin(clock*2.4f+i)*.012f;
                ducks[i].position=p;ducks[i].rotation=rotations[i]*Quaternion.Euler(0,Mathf.Cos(clock*.85f+i*1.8f)*12,Mathf.Sin(clock*2.4f+i)*3);
            }
            if(aimRing)aimRing.position=aim;
            if(State==Phase.Result&&caught>=0&&flyingRing){var held=ducks[caught].position;held.y=1.57f;flyingRing.position=held;}
            if(State!=Phase.Throwing)return;
            flight+=dt;float t=Mathf.Clamp01(flight/.62f);var from=new Vector3(.15f,1.93f,3.9f);
            if(flyingRing){flyingRing.position=Vector3.Lerp(from,landing,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*.48f;flyingRing.rotation=Quaternion.Euler(0,t*240,0);}
            if(t<1)return;
            if(!judged)
            {
                judged=true;float best=.23f;caught=-1;
                for(int i=0;i<ducks.Length;i++){var d=ducks[i].position-landing;d.y=0;if(d.magnitude<best){best=d.magnitude;caught=i;}}
                LastScore=caught<0?0:caught==0?100:50;Score+=LastScore;Throws++;if(caught>=0)pulses[caught]=1;Landed?.Invoke(LastScore);
            }
            if(caught>=0&&flyingRing){var p=ducks[caught].position;p.y=1.57f;flyingRing.position=p;}
            if(flight<.98f)return;
            if(Throws>=3){State=Phase.Result;ShowRings(false,true);Finished?.Invoke(Score);}else{State=Phase.Aiming;ShowRings(true,false);}
        }
        void ShowRings(bool aiming,bool flying){if(aimRing)aimRing.gameObject.SetActive(aiming);if(flyingRing)flyingRing.gameObject.SetActive(flying);}
        public void Exit(){State=Phase.Idle;ShowRings(false,false);}
        public void Reset(){Exit();clock=0;Throws=Score=LastScore=0;for(int i=0;i<ducks.Length;i++){ducks[i].SetPositionAndRotation(origins[i],rotations[i]);ducks[i].localScale=scales[i];pulses[i]=0;}}
    }
}
