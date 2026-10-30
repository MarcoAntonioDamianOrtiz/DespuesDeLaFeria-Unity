using System;
using UnityEngine;

namespace Feria
{
    [Serializable] public class FairSave
    {
        public int version=1;
        public string zoneId;
        public Vector3[] litter;
        public bool[] removed;
        public float[] dirt;
        public Vector3 cart,player;
        public float cartYaw,playerYaw;
        public bool signFixed,restored;
        public float playSeconds;
    }

}
