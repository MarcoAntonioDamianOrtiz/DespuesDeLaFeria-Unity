using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Feria
{
    /// <summary>Draw the existing simulated pieces in shared GPU batches; transforms and save IDs stay unchanged.</summary>
    public sealed class FairLitterRenderer : IDisposable
    {
        struct Instance { public Matrix4x4 objectToWorld; }
        sealed class Group
        {
            public Mesh mesh;
            public Material material;
            public int submesh;
            public readonly List<Transform> transforms=new();
            public readonly Instance[] instances=new Instance[1023];
        }
        readonly List<Group> groups=new();
        readonly List<MeshRenderer> sources=new();
        readonly Dictionary<Material,Material> materials=new();
        readonly Bounds worldBounds;
        public int VisibleCount {get;private set;}
        public int BatchCount {get;private set;}
        public bool Instanced=>groups.Count>0;
        public FairLitterRenderer(Transform[] pieces)
        {
            worldBounds=pieces.Length>0?new Bounds(pieces[0].position,Vector3.one):new Bounds(Vector3.zero,Vector3.one);foreach(var piece in pieces)worldBounds.Encapsulate(piece.position);worldBounds.Expand(new Vector3(16,16,16));
            if(!SystemInfo.supportsInstancing)return;
            var lookup=new Dictionary<(Mesh,Material,int),Group>();
            foreach(var piece in pieces)foreach(var source in piece.GetComponentsInChildren<MeshRenderer>(true))
            {
                if(!source.enabled||!source.gameObject.activeInHierarchy)continue;var filter=source.GetComponent<MeshFilter>();if(!filter||!filter.sharedMesh)continue;
                var materials=source.sharedMaterials;
                for(int sub=0;sub<filter.sharedMesh.subMeshCount&&sub<materials.Length;sub++)
                {
                    if(!materials[sub])continue;var key=(filter.sharedMesh,materials[sub],sub);
                    if(!lookup.TryGetValue(key,out var group))
                    {
                        if(!this.materials.TryGetValue(materials[sub],out var shared)){shared=new Material(materials[sub]){enableInstancing=true,name=materials[sub].name+" · lotes"};this.materials.Add(materials[sub],shared);}
                        group=new Group{mesh=filter.sharedMesh,material=shared,submesh=sub};lookup.Add(key,group);groups.Add(group);
                    }
                    group.transforms.Add(source.transform);
                }
                sources.Add(source);source.enabled=false;
            }
        }
        public void Draw()
        {
            VisibleCount=BatchCount=0;
            foreach(var group in groups)
            {
                var parameters=new RenderParams(group.material){worldBounds=worldBounds,shadowCastingMode=ShadowCastingMode.Off,receiveShadows=true,lightProbeUsage=LightProbeUsage.Off};
                int count=0;
                foreach(var transform in group.transforms)
                {
                    if(!transform||!transform.gameObject.activeInHierarchy)continue;
                    group.instances[count++].objectToWorld=transform.localToWorldMatrix;VisibleCount++;
                    if(count==group.instances.Length){Graphics.RenderMeshInstanced(parameters,group.mesh,group.submesh,group.instances,count);BatchCount++;count=0;}
                }
                if(count>0){Graphics.RenderMeshInstanced(parameters,group.mesh,group.submesh,group.instances,count);BatchCount++;}
            }
        }
        public void Dispose(){foreach(var source in sources)if(source)source.enabled=true;foreach(var material in materials.Values)UnityEngine.Object.Destroy(material);}
    }
}
