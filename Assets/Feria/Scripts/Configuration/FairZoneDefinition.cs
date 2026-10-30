using UnityEngine;

namespace Feria
{
    [CreateAssetMenu(menuName="Feria/Definición de zona")]
    public sealed class FairZoneDefinition : ScriptableObject
    {
        public string zoneId="rincon-canicas";
        public string displayName="El rincón de las canicas";
        public Vector3 placedSign=new Vector3(-2.95f,1.3f,3.85f);
        public Vector3[] marbleWaypoints=new Vector3[0];
        public float marbleSeconds=8;
        public Vector3 marbleViewPosition=new Vector3(0,2.4f,3.0f),marbleViewTarget=new Vector3(0,1.65f,5.05f);
        [Range(.4f,1.5f)] public float sweepRadius=.9f;
        [Range(1,6)] public float sweepStrength=3.4f;
        [Range(2,12)] public float friction=7;
        [Range(.3f,1.3f)] public float pickupRadius=.85f;
        [Range(.9f,1)] public float completionThreshold=.97f;
        public Vector2 litterMin=new Vector2(-5.8f,-7.1f), litterMax=new Vector2(5.8f,3.4f);
        public Vector2 playerMin=new Vector2(-6.5f,-8.3f), playerMax=new Vector2(6.5f,7.6f);
        public Vector2 cartMin=new Vector2(-5.9f,-7.4f), cartMax=new Vector2(5.9f,3.0f);
        [Range(10,120)] public float autosaveSeconds=25;
    }
}
