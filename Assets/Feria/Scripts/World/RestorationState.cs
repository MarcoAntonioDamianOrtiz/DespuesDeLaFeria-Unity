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
        public MarbleGame Marble {get;}
        public bool SignFixed {get;private set;}
        public bool Restored {get;private set;}
        public event System.Action Chime;
        public RestorationState(Transform sign,Transform marble,Light[] lights,Renderer[] bulbs,Material lit,FairZoneDefinition rules)
        {
            this.sign=sign;this.marble=marble;this.lights=lights;this.bulbs=bulbs;this.lit=lit;this.rules=rules;
            Marble=new MarbleGame(marble,rules);
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
            Restored=restored;Refresh();Marble.Reset();
        }
        public void Release(){if(Restored)Marble.Drop(.5f);}
        public void Tick(float dt){if(Restored)Marble.Tick(dt);}
    }
}
