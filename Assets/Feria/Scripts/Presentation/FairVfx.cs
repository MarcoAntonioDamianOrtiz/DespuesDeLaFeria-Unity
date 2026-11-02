using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Feria
{
    /// <summary>Small pooled particle bursts for tactile sweeping feedback.</summary>
    public sealed class FairVfx : System.IDisposable
    {
        readonly FairGame game;
        readonly ParticleSystem dust;
        readonly ParticleSystem pickup;
        readonly Material dustMaterial;
        float cooldown;
        public FairVfx(FairGame owner)
        {
            game=owner;
            var shader=Shader.Find("Universal Render Pipeline/Particles/Unlit");if(!shader)shader=Shader.Find("Sprites/Default");
            dustMaterial=new Material(shader){name="Polvo suave de barrido (instancia)"};dustMaterial.color=new Color(.78f,.66f,.48f,.65f);
            dust=Create("Polvo de escoba",dustMaterial,16,.38f,.07f);
            pickup=Create("Destello de recogida",dustMaterial,12,.48f,.09f);
        }
        ParticleSystem Create(string name,Material material,int capacity,float life,float size)
        {
            var host=new GameObject(name);host.transform.SetParent(game.transform,false);var ps=host.AddComponent<ParticleSystem>();
            var main=ps.main;main.playOnAwake=false;main.loop=false;main.maxParticles=capacity;main.startLifetime=life;main.startSpeed=new ParticleSystem.MinMaxCurve(.2f,.7f);main.startSize=new ParticleSystem.MinMaxCurve(size*.55f,size);main.startColor=new Color(1,.82f,.55f,.6f);main.simulationSpace=ParticleSystemSimulationSpace.World;main.gravityModifier=.05f;
            var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.material=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            return ps;
        }
        public void Sweep(Vector3 point,Vector3 direction,int contacts)
        {
            if(contacts<=0||cooldown>0)return;cooldown=.085f;direction.y=0;if(direction.sqrMagnitude<.001f)direction=game.player.transform.forward;direction.Normalize();
            dust.transform.position=point+Vector3.up*.035f;dust.transform.rotation=Quaternion.LookRotation(direction);dust.Emit(Mathf.Clamp(contacts/3,2,5));
        }
        public void PickedUp(Vector3 point,int count){pickup.transform.position=point+Vector3.up*.12f;pickup.Emit(Mathf.Clamp(count,2,6));}
        public void Tick(float dt){cooldown=Mathf.Max(0,cooldown-dt);}
        public void Dispose(){if(dustMaterial)Object.Destroy(dustMaterial);}
    }

    /// <summary>Low-cost breeze, distant leaves and gentle tree movement for the shared park.</summary>
    public sealed class FairParkAmbience : MonoBehaviour
    {
        struct Sway { public Transform target; public Quaternion rest; public float phase,amount; }
        readonly System.Collections.Generic.List<Sway> trees=new System.Collections.Generic.List<Sway>();
        ParticleSystem leaves;
        void Awake()
        {
            foreach(var t in GetComponentsInChildren<Transform>(true))
            {
                float amount=t.name.Contains("Copa")||t.name.Contains("Árbol")?.025f:0;
                if(amount>0)trees.Add(new Sway{target=t,rest=t.localRotation,phase=(t.position.x*1.71f+t.position.z*.63f),amount=amount});
            }
            MakeLeaves();
        }
        void Update()
        {
            float time=Time.time;
            for(int i=0;i<trees.Count;i++){var s=trees[i];if(!s.target)continue;float wave=Mathf.Sin(time*.47f+s.phase)+.35f*Mathf.Sin(time*.83f+s.phase*1.7f);s.target.localRotation=s.rest*Quaternion.Euler(wave*s.amount*32,0,wave*s.amount*44);}
        }
        void MakeLeaves()
        {
            var host=new GameObject("Hojas al viento · parque");host.transform.SetParent(transform,false);leaves=host.AddComponent<ParticleSystem>();
            var main=leaves.main;main.loop=true;main.playOnAwake=true;main.maxParticles=52;main.startLifetime=new ParticleSystem.MinMaxCurve(13,21);main.startSpeed=new ParticleSystem.MinMaxCurve(.05f,.22f);main.startSize=new ParticleSystem.MinMaxCurve(.10f,.19f);main.startColor=new ParticleSystem.MinMaxGradient(new Color(.78f,.34f,.16f,.78f),new Color(.92f,.67f,.28f,.72f));main.gravityModifier=-.012f;main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=leaves.emission;emission.rateOverTime=2.0f;
            var shape=leaves.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(175,1,185);shape.position=new Vector3(0,15,73);
            var velocity=leaves.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=new ParticleSystem.MinMaxCurve(-.18f,.18f);velocity.y=new ParticleSystem.MinMaxCurve(-.08f,-.02f);velocity.z=new ParticleSystem.MinMaxCurve(-.1f,.1f);
            var rotation=leaves.rotationOverLifetime;rotation.enabled=true;rotation.z=new ParticleSystem.MinMaxCurve(-1.2f,1.2f);
            var renderer=leaves.GetComponent<ParticleSystemRenderer>();var shader=Shader.Find("Universal Render Pipeline/Particles/Unlit");if(!shader)shader=Shader.Find("Sprites/Default");renderer.material=new Material(shader){name="Hojas de otoño (instancia)"};renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
        }
        void OnDestroy(){if(leaves&&leaves.GetComponent<ParticleSystemRenderer>().sharedMaterial)Destroy(leaves.GetComponent<ParticleSystemRenderer>().sharedMaterial);}
    }
}
