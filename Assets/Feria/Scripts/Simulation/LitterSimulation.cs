using System;
using UnityEngine;

namespace Feria
{
    /// <summary>Lightweight, deterministic movement; no input, UI, audio or file dependencies.</summary>
    public sealed class LitterSimulation
    {
        readonly Transform[] pieces;
        readonly bool[] removed;
        readonly Vector3[] velocities;
        readonly FairZoneDefinition rules;
        public int Collected { get; private set; }
        public int Contacts { get; private set; }
        public bool Complete=>Collected==pieces.Length;
        public event Action<int> PickedUp;
        public event Action FloorCompleted;
        public LitterSimulation(Transform[] pieces,FairZoneDefinition rules)
        { this.pieces=pieces; this.rules=rules; removed=new bool[pieces.Length]; velocities=new Vector3[pieces.Length]; }
        public int Sweep(Vector3 point,Vector3 movement,Vector3 forward,float dt)
        {
            if(dt<=0) return 0;
            Vector3 direction=movement; direction.y=0;
            if(direction.sqrMagnitude<.0001f) direction=forward;
            direction.y=0; direction.Normalize();
            float strength=rules.sweepStrength+Mathf.Min(movement.magnitude/Mathf.Max(dt,.001f),5);
            int count=0;
            for(int i=0;i<pieces.Length;i++)
            {
                if(removed[i]) continue;
                float distance=FlatDistance(pieces[i].position,point);
                if(distance>rules.sweepRadius) continue;
                // A small inward bias keeps a broad stroke together instead of scattering confetti.
                Vector3 inward=point-pieces[i].position; inward.y=0;
                float falloff=1-distance/(rules.sweepRadius+.15f);
                velocities[i]=Vector3.ClampMagnitude(velocities[i]+(direction*strength+inward*.3f)*falloff*Mathf.Min(dt,.05f)*17,4.5f);
                count++;
            }
            Contacts+=count; return count;
        }
        public void Tick(float dt,Vector3 mouth,Vector3 mouthForward)
        {
            int gained=0; bool wasComplete=Complete;
            for(int i=0;i<pieces.Length;i++)
            {
                if(removed[i]) continue;
                var p=pieces[i].position;
                p+=velocities[i]*dt;
                p.x=Mathf.Clamp(p.x,rules.litterMin.x,rules.litterMax.x); p.z=Mathf.Clamp(p.z,rules.litterMin.y,rules.litterMax.y);
                pieces[i].position=p; velocities[i]*=Mathf.Exp(-rules.friction*dt);
                // Pickup is only through the tray, never through the rear wall of the cart.
                var delta=p-mouth; delta.y=0;
                if(delta.magnitude<rules.pickupRadius && Vector3.Dot(delta,mouthForward)>-.34f) {Remove(i);gained++;}
            }
            if(!Complete && Collected>=Mathf.CeilToInt(pieces.Length*rules.completionThreshold))
                for(int i=0;i<pieces.Length;i++) if(!removed[i]) {Remove(i);gained++;}
            if(gained>0) PickedUp?.Invoke(gained);
            if(!wasComplete && Complete) FloorCompleted?.Invoke();
        }
        void Remove(int i) {removed[i]=true;velocities[i]=Vector3.zero;pieces[i].gameObject.SetActive(false);Collected++;}
        public Vector3 Nearest(Vector3 from)
        {
            Vector3 result=from;float best=float.MaxValue;
            for(int i=0;i<pieces.Length;i++) if(!removed[i]) {float d=FlatDistance(from,pieces[i].position);if(d<best){best=d;result=pieces[i].position;}}
            return result;
        }
        public bool[] CaptureRemoved()=>(bool[])removed.Clone();
        public Vector3[] CapturePositions() {var result=new Vector3[pieces.Length];for(int i=0;i<result.Length;i++)result[i]=pieces[i].position;return result;}
        public void Restore(Vector3[] positions,bool[] deleted)
        {
            Collected=0;
            for(int i=0;i<pieces.Length;i++){pieces[i].position=positions[i];removed[i]=deleted[i];velocities[i]=Vector3.zero;pieces[i].gameObject.SetActive(!deleted[i]);if(deleted[i])Collected++;}
        }
        public static float FlatDistance(Vector3 a,Vector3 b) {a.y=b.y=0;return Vector3.Distance(a,b);}
    }
}
