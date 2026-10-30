using UnityEngine;

namespace Feria
{
    public sealed class SurfaceCleaning
    {
        readonly Renderer[] patches;
        readonly float[] progress;
        public bool Complete {get {foreach(float value in progress) if(value<1)return false;return true;}}
        public float Progress {get {float sum=0;foreach(float value in progress)sum+=value;return progress.Length==0?1:sum/progress.Length;}}
        public SurfaceCleaning(Renderer[] patches){this.patches=patches;progress=new float[patches.Length];}
        public int Clean(Vector3 point,float dt)
        {
            int count=0;
            for(int i=0;i<progress.Length;i++) if(progress[i]<1 && Vector3.Distance(patches[i].transform.position,point)<=.48f)
            {progress[i]=Mathf.Clamp01(progress[i]+dt*.9f);Refresh(i);count++;}
            return count;
        }
        void Refresh(int i){patches[i].gameObject.SetActive(progress[i]<1);patches[i].transform.localScale=new Vector3(.34f*(1-progress[i]),.008f,.32f*(1-progress[i]));}
        public Vector3 Pending(){for(int i=0;i<progress.Length;i++)if(progress[i]<1)return patches[i].transform.position;return Vector3.zero;}
        public float[] Capture()=>(float[])progress.Clone();
        public void Restore(float[] data){for(int i=0;i<data.Length;i++){progress[i]=data[i];Refresh(i);}}
    }
}
