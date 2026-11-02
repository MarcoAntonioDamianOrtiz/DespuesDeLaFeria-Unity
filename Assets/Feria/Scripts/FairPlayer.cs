using UnityEngine;
using UnityEngine.InputSystem;

namespace Feria
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FairPlayer : MonoBehaviour
    {
        public Camera view;
        public Transform broom,cloth;
        public FairGame game;
        public float sensitivity=.095f;
        CharacterController controller;
        float pitch=20,verticalSpeed,stroke,toolBlend,bobPhase,stepDistance;
        Vector2 moveVelocity;
        Vector3 broomRest,normalCameraPosition,cameraRestPosition;
        Quaternion broomRotation,normalCameraRotation;
        Transform fibres;
        float normalFov;
        bool closeView,cameraRestKnown;
        public float Pitch=>pitch;
        public bool Active=>game!=null&&!game.Paused;
        void Awake()
        {
            controller=GetComponent<CharacterController>();broomRest=broom.localPosition;broomRotation=broom.localRotation;if(view){cameraRestPosition=view.transform.localPosition;cameraRestKnown=true;}
            fibres=broom.Find("Escoba artesanal/Fibras flexibles");sensitivity=PlayerPrefs.GetFloat("feria.sensitivity",.095f);
        }
        void Update()
        {
            var kb=Keyboard.current;var mouse=Mouse.current;if(kb==null||mouse==null)return;
            if(kb.escapeKey.wasPressedThisFrame)game.TogglePause();
            if(game.Paused&&kb.enterKey.wasPressedThisFrame)game.SetPaused(false);
            if(!Active){moveVelocity=Vector2.zero;return;}
            if(game.MinigameActive)
            {
                if(game.IsDucks){game.Ducks.Aim(mouse.delta.ReadValue());if(mouse.leftButton.wasPressedThisFrame||kb.eKey.wasPressedThisFrame)game.Interact();if(kb.rKey.wasPressedThisFrame&&game.Ducks.State==DuckGame.Phase.Result)game.Ducks.Begin();}
                else if(kb.eKey.wasPressedThisFrame)game.Interact();
                if(!game.IsDucks&&kb.rKey.wasPressedThisFrame&&game.Marble.State==MarbleGame.Phase.Result)game.Marble.Begin();
                moveVelocity=Vector2.zero;stepDistance=0;broom.gameObject.SetActive(false);if(cloth)cloth.gameObject.SetActive(false);return;
            }
            var delta=mouse.delta.ReadValue()*sensitivity;transform.Rotate(0,delta.x,0);pitch=Mathf.Clamp(pitch-delta.y,-55,78);
            view.transform.localRotation=Quaternion.Euler(pitch,0,0);
            Vector2 movement=Vector2.zero;
            if(kb.wKey.isPressed)movement.y++;if(kb.sKey.isPressed)movement.y--;if(kb.dKey.isPressed)movement.x++;if(kb.aKey.isPressed)movement.x--;
            float dt=Time.deltaTime;Move(movement,dt);
            if(kb.digit1Key.wasPressedThisFrame)game.SelectTool(0);if(kb.digit2Key.wasPressedThisFrame)game.SelectTool(1);
            if(mouse.scroll.ReadValue().y!=0)game.SelectTool(1-game.Tool);
            if(kb.eKey.wasPressedThisFrame)game.Interact();if(kb.f5Key.wasPressedThisFrame)game.Save();if(kb.tabKey.wasPressedThisFrame)game.RequestHint();
            bool working=mouse.leftButton.isPressed&&!game.MovingCart;game.UseTool(working);AnimateTool(working&&game.ToolContact,moveVelocity.magnitude/3.7f,dt);
        }
        public void Move(Vector2 movement,float dt)
        {
            if(!Active||game.MinigameActive){moveVelocity=Vector2.zero;return;}
            movement=Vector2.ClampMagnitude(movement,1);float maxSpeed=game.MovingCart?2.6f:3.7f;
            moveVelocity=Vector2.MoveTowards(moveVelocity,movement*maxSpeed,(movement.sqrMagnitude>.01f?13f:19f)*dt);
            bool grounded=controller.isGrounded;verticalSpeed=grounded?-2:verticalSpeed-18*dt;Vector3 before=transform.position;
            controller.Move(((transform.right*moveVelocity.x+transform.forward*moveVelocity.y)+Vector3.up*verticalSpeed)*dt);
            var min=game.park?game.park.definition.worldMin:game.definition.playerMin;var max=game.park?game.park.definition.worldMax:game.definition.playerMax;var p=transform.position;p.x=Mathf.Clamp(p.x,min.x,max.x);p.z=Mathf.Clamp(p.z,min.y,max.y);transform.position=p;
            Vector3 travelled=transform.position-before;travelled.y=0;float distance=travelled.magnitude;
            if(grounded&&distance>.001f){stepDistance+=distance;float stride=game.MovingCart ? 1.25f : 1.45f;while(stepDistance>=stride){stepDistance-=stride;game.Footstep(game.MovingCart,Mathf.Clamp01(moveVelocity.magnitude/3.7f),GroundSurface());}}
        }
        WalkSurface GroundSurface()
        {
            if(Physics.Raycast(transform.position+Vector3.up*.2f,Vector3.down,out var hit,.7f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))
            {
                var marker=hit.collider.GetComponent<FairGroundSurface>();if(marker)return marker.surface;
                if(hit.collider.name.Contains("Explanada")||hit.collider.name.Contains("Tarima"))return WalkSurface.Wood;
                if(hit.collider.name.Contains("Suelo continuo")||hit.collider.name.Contains("Loma"))return WalkSurface.Grass;
            }
            return WalkSurface.Stone;
        }
        public void AnimateTool(bool working,float walking,float dt)
        {
            toolBlend=Mathf.MoveTowards(toolBlend,working?1:0,dt*7);if(working)stroke=game.ToolPhase;else stroke+=dt*2;
            float swing=Mathf.Sin(stroke)*toolBlend;
            var offset=new Vector3(swing*.115f,Mathf.Abs(swing)*.022f,-Mathf.Cos(stroke)*.045f*toolBlend);
            offset+=new Vector3(Mathf.Sin(Time.time*5)*.008f,Mathf.Cos(Time.time*10)*.006f,0)*walking;
            offset+=new Vector3(.12f,-.10f,.06f)*(1-toolBlend);
            var broomContact=view.transform.InverseTransformPoint(game.ContactPoint)-new Vector3(0,.05f,.20f)+offset;
            broom.localPosition=Vector3.Lerp(broom.localPosition,Vector3.Lerp(broomRest+offset,broomContact,toolBlend),1-Mathf.Exp(-dt*18));
            broom.localRotation=broomRotation*Quaternion.Euler(toolBlend*5,swing*9,-swing*6);
            if(fibres)fibres.localRotation=Quaternion.Euler(toolBlend*8,0,-swing*9);
            broom.gameObject.SetActive(game.Tool==0&&!game.MovingCart&&!game.MinigameActive);
            if(cloth)
            {
                cloth.gameObject.SetActive(game.Tool==1&&!game.MovingCart&&!game.MinigameActive);
                var rest=new Vector3(.42f,-.34f,.65f);var normal=game.origin?game.origin.up:Vector3.up;var right=game.origin?game.origin.right:Vector3.right;var forward=game.origin?game.origin.forward:Vector3.forward;
                var world=game.ContactPoint+normal*.025f+right*(Mathf.Sin(stroke)*.06f)+forward*(Mathf.Cos(stroke)*.025f);
                var contact=cloth.parent.InverseTransformPoint(world);
                cloth.localPosition=Vector3.Lerp(cloth.localPosition,Vector3.Lerp(rest,contact,toolBlend),1-Mathf.Exp(-dt*16));
                cloth.rotation=Quaternion.Slerp(view.transform.rotation*Quaternion.Euler(-12,-15,0),Quaternion.LookRotation(forward,normal)*Quaternion.Euler(0,swing*9,0),toolBlend);
            }
        }
        void LateUpdate()
        {
            if(game.MinigameActive)
            {
                if(!closeView){normalCameraPosition=view.transform.localPosition;normalCameraRotation=view.transform.localRotation;normalFov=view.fieldOfView;closeView=true;}
                float blend=1-Mathf.Exp(-Time.unscaledDeltaTime*7);
                Vector3 position=game.World(game.definition.activityViewPosition),target=game.World(game.definition.activityViewTarget);
                view.transform.position=Vector3.Lerp(view.transform.position,position,blend);
                view.transform.rotation=Quaternion.Slerp(view.transform.rotation,Quaternion.LookRotation(target-position),blend);
                view.fieldOfView=Mathf.Lerp(view.fieldOfView,53,blend);
            }
            else if(closeView){view.transform.localPosition=normalCameraPosition;view.transform.localRotation=normalCameraRotation;view.fieldOfView=normalFov;closeView=false;}
            else
            {
                float moving=Mathf.Clamp01(moveVelocity.magnitude/2.2f);bobPhase+=Time.deltaTime*moveVelocity.magnitude*(Mathf.PI*2/1.45f);
                float settle=1-Mathf.Exp(-Time.deltaTime*12);Vector3 bob=new Vector3(Mathf.Sin(bobPhase*.5f)*.007f,Mathf.Sin(bobPhase)*.014f,0)*moving;
                view.transform.localPosition=Vector3.Lerp(view.transform.localPosition,cameraRestPosition+bob,settle);
                view.transform.localRotation=Quaternion.Euler(pitch+Mathf.Sin(bobPhase+1f)*.30f*moving,0,0);
            }
        }
        public void Place(Vector3 position,float yaw,float cameraPitch=20)
        {
            if(closeView){view.transform.localPosition=normalCameraPosition;view.fieldOfView=normalFov;closeView=false;}
            if(!controller)controller=GetComponent<CharacterController>();controller.enabled=false;transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));controller.enabled=true;
            pitch=Mathf.Clamp(cameraPitch,-55,78);verticalSpeed=0;moveVelocity=Vector2.zero;stepDistance=0;if(view){if(!cameraRestKnown){cameraRestPosition=view.transform.localPosition;cameraRestKnown=true;}view.transform.localPosition=cameraRestPosition;view.transform.localRotation=Quaternion.Euler(pitch,0,0);}
        }
    }
}
