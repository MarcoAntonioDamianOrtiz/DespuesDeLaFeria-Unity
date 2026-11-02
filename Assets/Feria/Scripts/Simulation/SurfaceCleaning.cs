using UnityEngine;

namespace Feria
{
    public sealed class SurfaceCleaning
    {
        readonly Renderer[] patches;
        readonly float[] progress;
        readonly Vector3[] originalScale;
        readonly Bounds[] localBounds;
        public event System.Action<int,float> Changed;
        public event System.Action<int> Completed;
        public int Count=>progress.Length;
        public int CleanedCount {get{int count=0;foreach(float value in progress)if(value>=1)count++;return count;}}
        public float At(int index)=>index>=0&&index<progress.Length?progress[index]:0;
        public bool Complete {get {foreach(float value in progress) if(value<1)return false;return true;}}
        public float Progress {get {float sum=0;foreach(float value in progress)sum+=value;return progress.Length==0?1:sum/progress.Length;}}
        public SurfaceCleaning(Renderer[] patches)
        {
            this.patches=patches;progress=new float[patches.Length];originalScale=new Vector3[patches.Length];localBounds=new Bounds[patches.Length];
            for(int i=0;i<patches.Length;i++){originalScale[i]=patches[i].transform.localScale;var filter=patches[i].GetComponent<MeshFilter>();localBounds[i]=filter&&filter.sharedMesh?filter.sharedMesh.bounds:new Bounds(Vector3.zero,Vector3.one);}
        }
        // Intersect the visible plane instead of a different collider underneath it.
        // A small world-space margin stays comfortable as a stain becomes smaller.
        public bool Raycast(Ray ray,float range,out int index,out Vector3 point)
        {
            index=-1;point=default;float nearest=range;
            for(int i=0;i<patches.Length;i++)
            {
                if(progress[i]>=1)continue;var t=patches[i].transform;var bounds=localBounds[i];
                var plane=new Plane(t.up,t.TransformPoint(bounds.center));
                if(!plane.Raycast(ray,out float distance)||distance<0||distance>nearest)continue;
                var hit=ray.GetPoint(distance);var local=t.InverseTransformPoint(hit)-bounds.center;var scale=t.lossyScale;
                if(Mathf.Abs(local.x)*Mathf.Abs(scale.x)>bounds.extents.x*Mathf.Abs(scale.x)+.055f||Mathf.Abs(local.z)*Mathf.Abs(scale.z)>bounds.extents.z*Mathf.Abs(scale.z)+.055f)continue;
                if(Physics.Raycast(ray,out var blocker,distance,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)&&blocker.distance<distance-.015f&&blocker.collider.transform!=t)continue;
                nearest=distance;index=i;point=hit;
            }
            return index>=0;
        }
        public bool CleanIndex(int index,float dt)
        {
            if(index<0||index>=progress.Length||progress[index]>=1||dt<=0)return false;
            progress[index]=Mathf.Clamp01(progress[index]+dt*.9f);Refresh(index);Changed?.Invoke(index,progress[index]);if(progress[index]>=1)Completed?.Invoke(index);return true;
        }
        public int Clean(Vector3 point,float dt)
        {
            int count=0;
            for(int i=0;i<progress.Length;i++) if(progress[i]<1 && Vector3.Distance(patches[i].transform.position,point)<=.36f)
            {if(CleanIndex(i,dt))count++;}
            return count;
        }
        void Refresh(int i){patches[i].gameObject.SetActive(progress[i]<1);patches[i].transform.localScale=originalScale[i]*(1-progress[i]);}
        public Vector3 Pending(){for(int i=0;i<progress.Length;i++)if(progress[i]<1)return patches[i].transform.position;return Vector3.zero;}
        public float[] Capture()=>(float[])progress.Clone();
        public void Restore(float[] data){for(int i=0;i<data.Length;i++){progress[i]=data[i];Refresh(i);}}
    }
}
