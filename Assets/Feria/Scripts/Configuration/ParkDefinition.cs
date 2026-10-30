using System;
using UnityEngine;
namespace Feria
{
    [Serializable] public sealed class ParkPlot
    {
        public string id,label;
        public Vector3 center,connection;
        public Vector2 size=new Vector2(30,28);
    }
    [CreateAssetMenu(menuName="Feria/Plano del parque")]
    public sealed class ParkDefinition : ScriptableObject
    {
        public string parkId="feria-manana";
        public FairZoneCatalog catalog;
        public Vector2 worldMin=new Vector2(-100,-20),worldMax=new Vector2(100,180);
        public Vector3 entrance=new Vector3(0,.05f,-3),cartStart=new Vector3(2,0,9);
        public ParkPlot[] futurePlots;
    }
}
