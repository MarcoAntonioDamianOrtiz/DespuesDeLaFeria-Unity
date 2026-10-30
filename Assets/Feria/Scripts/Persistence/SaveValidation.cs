using System;
using System.IO;
using UnityEngine;

namespace Feria
{
    public static class SaveValidation
    {
        public static void Validate(FairSave d,int litterCount,int dirtCount,FairZoneDefinition zone)
        {
            if(d==null||d.version!=1||d.litter==null||d.removed==null||d.dirt==null||d.litter.Length!=litterCount||d.removed.Length!=litterCount||d.dirt.Length!=dirtCount)Fail("Formato de partida incompatible.");
            if(!string.IsNullOrEmpty(d.zoneId)&&d.zoneId!=zone.zoneId)Fail("La partida pertenece a otra zona.");
            if(string.IsNullOrEmpty(d.zoneId)&&zone.zoneId!="rincon-canicas")Fail("La partida antigua pertenece a la primera zona.");
            if(!Finite(d.player)||!Finite(d.cart)||!float.IsFinite(d.playerYaw)||!float.IsFinite(d.cartYaw)||!float.IsFinite(d.playSeconds)||d.playSeconds<0)Fail("Posición no válida.");
            foreach(var p in d.litter)if(!Finite(p)||Mathf.Abs(p.x)>7||Mathf.Abs(p.z)>9||p.y<0||p.y>1)Fail("Residuo no válido.");
            foreach(float v in d.dirt)if(!float.IsFinite(v)||v<0||v>1)Fail("Limpieza no válida.");
            if(Mathf.Abs(d.player.x)>6.6f||Mathf.Abs(d.player.z)>8.4f||d.player.y<-.2f||d.player.y>3||Mathf.Abs(d.cart.x)>6||d.cart.z< -7.5f||d.cart.z>3.7f)Fail("Partida fuera del recinto.");
            if(d.restored&&(!d.signFixed||Array.Exists(d.removed,v=>!v)||Array.Exists(d.dirt,v=>v<1)))Fail("Objetivos incoherentes.");
        }
        static bool Finite(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z);
        static void Fail(string message){throw new InvalidDataException(message);}
    }
}
