using System;
using System.Collections.Generic;
using UnityEngine;
namespace Feria
{
    [Serializable] public sealed class ParkWorldState
    {
        public Vector3 player,cart;
        public float playerYaw,cartYaw,pitch=20;
        public int tool;
    }
    [Serializable] public sealed class ParkSave
    {
        public int version=3;
        public string parkId="feria-manana";
        public ParkWorldState world;
        public List<ZoneProgress> zones=new List<ZoneProgress>();
    }
}
