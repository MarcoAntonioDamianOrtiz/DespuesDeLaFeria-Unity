using UnityEngine;

namespace Feria
{
    public sealed class CartMotor
    {
        readonly Transform cart,player;
        readonly Vector2 min,max;
        readonly Collider[] obstacles=new Collider[24];
        public bool Moving {get;private set;}
        public Vector3 Mouth=>cart.position+cart.forward*.95f;
        public Vector3 Forward=>cart.forward;
        public CartMotor(Transform cart,Transform player,FairZoneDefinition rules):this(cart,player,rules.cartMin,rules.cartMax){}
        public CartMotor(Transform cart,Transform player,Vector2 min,Vector2 max){this.cart=cart;this.player=player;this.min=min;this.max=max;}
        public void Toggle(){Moving=!Moving;}
        public void Stop(){Moving=false;}
        public void Tick(float dt)
        {
            if(!Moving)return;
            Vector3 desired=player.position+player.forward*1.65f;desired.y=0;
            desired.x=Mathf.Clamp(desired.x,min.x,max.x);desired.z=Mathf.Clamp(desired.z,min.y,max.y);
            var next=Vector3.MoveTowards(cart.position,Vector3.Lerp(cart.position,desired,1-Mathf.Exp(-10*dt)),4.2f*dt);
            int count=Physics.OverlapBoxNonAlloc(next+Vector3.up*.55f,new Vector3(.48f,.4f,.32f),obstacles,Quaternion.Euler(0,player.eulerAngles.y+180,0),~0,QueryTriggerInteraction.Ignore);
            bool blocked=false;
            for(int i=0;i<count;i++) if(!obstacles[i].transform.IsChildOf(cart) && !obstacles[i].transform.IsChildOf(player)){blocked=true;break;}
            if(!blocked)cart.position=next;
            cart.rotation=Quaternion.Slerp(cart.rotation,Quaternion.Euler(0,player.eulerAngles.y+180,0),1-Mathf.Exp(-10*dt));
        }
    }
}
