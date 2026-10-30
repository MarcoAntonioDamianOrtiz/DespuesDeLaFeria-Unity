using UnityEngine;

namespace Feria
{
    public sealed class RestorationState
    {
        readonly Transform sign,marble;
        readonly Light[] lights;
        readonly Renderer[] bulbs;
        readonly Material lit,unlit;
        readonly Vector3 signStart,marbleStart;
        readonly Quaternion signRotation;
        readonly FairZoneDefinition rules;
        float marbleTime=-1;
        public bool SignFixed {get;private set;}
        public bool Restored {get;private set;}
        public event System.Action Chime;
        public RestorationState(Transform sign,Transform marble,Light[] lights,Renderer[] bulbs,Material lit,FairZoneDefinition rules)
        {
            this.sign=sign;this.marble=marble;this.lights=lights;this.bulbs=bulbs;this.lit=lit;this.rules=rules;
            unlit=bulbs.Length>0?bulbs[0].sharedMaterial:lit;signStart=sign.position;signRotation=sign.rotation;marbleStart=marble.position;
        }
        public void FixSign(){SignFixed=true;sign.SetPositionAndRotation(rules.placedSign,Quaternion.identity);}
        public bool Activate(bool floor,bool counter)
        {
            if(Restored)return true;if(!floor||!counter||!SignFixed)return false;
            Restored=true;Refresh();Chime?.Invoke();return true;
        }
        void Refresh(){foreach(var l in lights)l.intensity=Restored?2.2f:.15f;foreach(var b in bulbs)b.sharedMaterial=Restored?lit:unlit;}
        public void Restore(bool fixedSign,bool restored)
        {
            SignFixed=fixedSign;if(fixedSign)FixSign();else sign.SetPositionAndRotation(signStart,signRotation);
            Restored=restored;Refresh();marbleTime=-1;marble.position=marbleStart;
        }
        public void Release(){if(Restored&&marbleTime<0){marbleTime=0;marble.position=Waypoint(0);}}
        public void Tick(float dt)
        {
            if(marbleTime<0)return;marbleTime+=dt;int segments=rules.marbleWaypoints.Length>=2?rules.marbleWaypoints.Length-1:11;
            float duration=Mathf.Max(.1f,rules.marbleSeconds);float path=Mathf.Clamp01(marbleTime/duration)*segments;
            int step=Mathf.Min(segments-1,Mathf.FloorToInt(path));
            marble.position=Vector3.Lerp(Waypoint(step),Waypoint(step+1),path-step);marble.Rotate(70*dt,0,180*dt);
            if(marbleTime>=duration){marbleTime=-1;Chime?.Invoke();}
        }
        Vector3 Waypoint(int i){if(rules.marbleWaypoints.Length>=2)return rules.marbleWaypoints[i];int row=i/2;float x=i%2==0?.95f:-.95f;if(row%2==1)x=-x;return new Vector3(x,1.68f-row*.027f,5.8f-row*.24f);}
    }
}
