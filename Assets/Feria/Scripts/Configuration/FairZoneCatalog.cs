using UnityEngine;
namespace Feria
{
    [CreateAssetMenu(menuName="Feria/Catálogo de puestos")]
    public sealed class FairZoneCatalog : ScriptableObject
    {
        public FairZoneDefinition[] zones;
        public FairZoneDefinition Find(string id){if(zones!=null)foreach(var zone in zones)if(zone&&zone.zoneId==id)return zone;return null;}
    }
}
