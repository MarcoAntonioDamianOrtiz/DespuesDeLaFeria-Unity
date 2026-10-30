using System;
using UnityEngine;

namespace Feria
{
    /// <summary>A repeatable timing toy. No dependency on input, UI, currency or saves.</summary>
    public sealed class MarbleGame
    {
        public enum Phase { Idle, Aiming, Rolling, Result }
        readonly Transform ball;
        readonly FairZoneDefinition rules;
        readonly Vector3 start;
        float clock,rollTime,power;
        public Phase State {get;private set;}
        public bool Active=>State!=Phase.Idle;
        public float Meter=>.5f+.5f*Mathf.Sin(clock*2.15f);
        public float Accuracy=>1-Mathf.Abs(power-.5f)*2;
        public int Score {get;private set;}
        public int Attempts {get;private set;}
        public Vector3 Landing=>new Vector3(Mathf.Lerp(-.78f,.78f,power),1.55f,4.24f);
        public event Action<int> Finished;
        public event Action<int> RailTouched;
        int lastRail;
        public MarbleGame(Transform ball,FairZoneDefinition rules){this.ball=ball;this.rules=rules;start=ball.position;}
        public void Begin(){clock=-.65f;rollTime=0;Score=0;State=Phase.Aiming;ball.position=Waypoint(0);}
        public void Confirm(){if(State==Phase.Aiming)Drop(Meter);else if(State==Phase.Result)Begin();}
        public void Drop(float strength)
        {
            if(State==Phase.Rolling)return;
            power=Mathf.Clamp01(strength);rollTime=0;lastRail=0;Score=0;State=Phase.Rolling;ball.position=Waypoint(0);
        }
        public void Tick(float dt)
        {
            if(dt<=0)return;
            if(State==Phase.Aiming){clock+=dt;return;}
            if(State!=Phase.Rolling)return;
            rollTime+=dt;float duration=Mathf.Max(.1f,rules.marbleSeconds);
            int segments=Mathf.Max(1,rules.marbleWaypoints.Length-1);
            float path=Mathf.Clamp01(rollTime/duration)*segments;int step=Mathf.Min(segments-1,Mathf.FloorToInt(path));
            if(rollTime<duration)ball.position=Vector3.Lerp(Waypoint(step),Waypoint(step+1),path-step);
            else {float t=Mathf.Clamp01((rollTime-duration)/.85f);ball.position=Vector3.Lerp(Waypoint(segments),Landing,Mathf.SmoothStep(0,1,t))+Vector3.up*Mathf.Sin(t*Mathf.PI)*.055f;}
            ball.Rotate(130*dt,0,240*dt,Space.Self);
            int rail=Mathf.FloorToInt(path/2);if(rail>lastRail){lastRail=rail;RailTouched?.Invoke(rail);}
            if(rollTime>=duration+.85f)
            {
                ball.position=Landing;float offset=Mathf.Abs(Landing.x);Score=offset<=.18f?100:offset<=.57f?50:10;Attempts++;State=Phase.Result;Finished?.Invoke(Score);
            }
        }
        Vector3 Waypoint(int i)
        {
            if(rules.marbleWaypoints.Length>=2)return rules.marbleWaypoints[Mathf.Clamp(i,0,rules.marbleWaypoints.Length-1)];
            int row=i/2;float x=i%2==0?.88f:-.88f;if(row%2==1)x=-x;return new Vector3(x,1.9f-row*.075f,5.85f-row*.22f);
        }
        public void Exit(){State=Phase.Idle;ball.position=start;}
        public void Reset(){Exit();Attempts=0;Score=0;clock=rollTime=0;}
    }
}
