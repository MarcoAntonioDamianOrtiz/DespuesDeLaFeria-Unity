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
        float pitch=20,verticalSpeed,stroke,toolBlend;
        Vector3 broomRest,normalCameraPosition;
        Quaternion broomRotation,normalCameraRotation;
        Transform fibres;
        float normalFov;
        bool closeView;
        public float Pitch=>pitch;
        public bool Active=>game!=null&&!game.Paused;
        void Awake()
        {
            controller=GetComponent<CharacterController>();broomRest=broom.localPosition;broomRotation=broom.localRotation;
            fibres=broom.Find("Escoba artesanal/Fibras flexibles");sensitivity=PlayerPrefs.GetFloat("feria.sensitivity",.095f);
        }
        void Update()
        {
            var kb=Keyboard.current;var mouse=Mouse.current;if(kb==null||mouse==null)return;
            if(kb.escapeKey.wasPressedThisFrame)game.TogglePause();
            if(game.Paused&&kb.enterKey.wasPressedThisFrame)game.SetPaused(false);
            if(!Active)return;
            if(game.MinigameActive)
            {
                if(game.IsDucks){game.Ducks.Aim(mouse.delta.ReadValue());if(mouse.leftButton.wasPressedThisFrame||kb.eKey.wasPressedThisFrame)game.Interact();if(kb.rKey.wasPressedThisFrame&&game.Ducks.State==DuckGame.Phase.Result)game.Ducks.Begin();}
                else if(kb.eKey.wasPressedThisFrame)game.Interact();
                if(!game.IsDucks&&kb.rKey.wasPressedThisFrame&&game.Marble.State==MarbleGame.Phase.Result)game.Marble.Begin();
                broom.gameObject.SetActive(false);if(cloth)cloth.gameObject.SetActive(false);return;
            }
            var delta=mouse.delta.ReadValue()*sensitivity;transform.Rotate(0,delta.x,0);pitch=Mathf.Clamp(pitch-delta.y,-55,78);
            view.transform.localRotation=Quaternion.Euler(pitch,0,0);
            Vector2 movement=Vector2.zero;
            if(kb.wKey.isPressed)movement.y++;if(kb.sKey.isPressed)movement.y--;if(kb.dKey.isPressed)movement.x++;if(kb.aKey.isPressed)movement.x--;
            movement=Vector2.ClampMagnitude(movement,1);verticalSpeed=controller.isGrounded?-2:verticalSpeed-18*Time.deltaTime;
            controller.Move(((transform.right*movement.x+transform.forward*movement.y)*(game.MovingCart?2.6f:3.7f)+Vector3.up*verticalSpeed)*Time.deltaTime);
            var min=game.park?game.park.definition.worldMin:game.definition.playerMin;var max=game.park?game.park.definition.worldMax:game.definition.playerMax;var p=transform.position;p.x=Mathf.Clamp(p.x,min.x,max.x);p.z=Mathf.Clamp(p.z,min.y,max.y);transform.position=p;
            if(kb.digit1Key.wasPressedThisFrame)game.Tool=0;if(kb.digit2Key.wasPressedThisFrame)game.Tool=1;
            if(mouse.scroll.ReadValue().y!=0)game.Tool=1-game.Tool;
            if(kb.eKey.wasPressedThisFrame)game.Interact();if(kb.f5Key.wasPressedThisFrame)game.Save();if(kb.tabKey.wasPressedThisFrame)game.RequestHint();
            bool working=mouse.leftButton.isPressed&&!game.MovingCart;game.UseTool(working);AnimateTool(working,movement.magnitude,Time.deltaTime);
        }
        public void AnimateTool(bool working,float walking,float dt)
        {
            toolBlend=Mathf.MoveTowards(toolBlend,working?1:0,dt*7);stroke+=dt*(working?9:2);
            float swing=Mathf.Sin(stroke)*toolBlend;
            var offset=new Vector3(swing*.115f,Mathf.Abs(swing)*.022f,-Mathf.Cos(stroke)*.045f*toolBlend);
            offset+=new Vector3(Mathf.Sin(Time.time*5)*.008f,Mathf.Cos(Time.time*10)*.006f,0)*walking;
            broom.localPosition=Vector3.Lerp(broom.localPosition,broomRest+offset,1-Mathf.Exp(-dt*18));
            broom.localRotation=broomRotation*Quaternion.Euler(toolBlend*5,swing*9,-swing*6);
            if(fibres)fibres.localRotation=Quaternion.Euler(toolBlend*8,0,-swing*9);
            broom.gameObject.SetActive(game.Tool==0&&!game.MovingCart&&!game.MinigameActive);
            if(cloth){cloth.gameObject.SetActive(game.Tool==1&&!game.MovingCart&&!game.MinigameActive);cloth.localPosition=new Vector3(.42f+swing*.04f,-.34f,.65f);cloth.localRotation=Quaternion.Euler(-12,-15,swing*8);}
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
        }
        public void Place(Vector3 position,float yaw,float cameraPitch=20)
        {
            if(closeView){view.transform.localPosition=normalCameraPosition;view.fieldOfView=normalFov;closeView=false;}
            if(!controller)controller=GetComponent<CharacterController>();controller.enabled=false;transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));controller.enabled=true;
            pitch=Mathf.Clamp(cameraPitch,-55,78);verticalSpeed=0;if(view)view.transform.localRotation=Quaternion.Euler(pitch,0,0);
        }
    }
}
