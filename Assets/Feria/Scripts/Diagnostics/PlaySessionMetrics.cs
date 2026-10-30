using System;
using UnityEngine;

namespace Feria
{
    [Serializable] public sealed class PlaySessionMetrics
    {
        public float seconds,sweepingSeconds,cleaningSeconds,walkingMetres;
        public int sweepActions,emptySweeps,collected,cartMoves,hints;
        public float firstPickupSeconds=-1,finishedSeconds=-1;
        public void Pickup(int amount){collected+=amount;if(firstPickupSeconds<0)firstPickupSeconds=seconds;}
        public string Summary=>$"{Mathf.FloorToInt(seconds/60):00}:{Mathf.FloorToInt(seconds%60):00} · {collected} residuos · {cartMoves} movimientos del carrito";
    }
}
