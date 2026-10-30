using TMPro;
using UnityEngine;

namespace Feria
{
    public sealed class FairFeedback : System.IDisposable
    {
        readonly FairGame game;
        readonly GameObject root;
        readonly LineRenderer ring;
        readonly TextMeshPro marker;
        readonly Material material;
        float hintSeconds;
        public FairFeedback(FairGame game)
        {
            this.game=game;root=new GameObject("Feedback de herramientas");root.transform.SetParent(game.transform,false);
            var go=new GameObject("Área de barrido");go.transform.SetParent(root.transform,false);
            ring=go.AddComponent<LineRenderer>();ring.loop=true;ring.useWorldSpace=true;ring.positionCount=48;ring.widthMultiplier=.012f;
            material=new Material(game.feedbackMaterial?game.feedbackMaterial:game.litBulb);material.color=new Color(.85f,.72f,.40f);ring.sharedMaterial=material;
            ring.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;ring.receiveShadows=false;
            var hint=new GameObject("Ayuda contextual");hint.transform.SetParent(root.transform,false);marker=hint.AddComponent<TextMeshPro>();
            marker.font=game.hud.font;marker.text="↓";marker.fontSize=7;marker.color=new Color(1,.82f,.32f);marker.alignment=TextAlignmentOptions.Center;hint.SetActive(false);
        }
        public void ShowHint(Vector3 point){marker.transform.position=point+Vector3.up*.55f;hintSeconds=6;}
        public void Tick(float dt)
        {
            bool visible=!game.Paused&&!game.MovingCart&&game.Tool==0&&game.TrySweepPoint(out _);
            ring.gameObject.SetActive(visible);
            if(visible&&game.TrySweepPoint(out var p))for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;ring.SetPosition(i,p+new Vector3(Mathf.Sin(a)*game.definition.sweepRadius,.008f,Mathf.Cos(a)*game.definition.sweepRadius));}
            if(!game.Paused)hintSeconds-=dt;
            marker.transform.rotation=game.player.view.transform.rotation;marker.gameObject.SetActive(!game.Paused&&hintSeconds>0);
        }
        public void Dispose(){Object.Destroy(material);Object.Destroy(root);}
    }
}
