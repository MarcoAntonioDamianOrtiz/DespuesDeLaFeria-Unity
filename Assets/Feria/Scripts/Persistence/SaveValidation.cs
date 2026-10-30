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
            foreach(var p in d.litter)if(!Finite(p)||p.x<zone.litterMin.x-.15f||p.x>zone.litterMax.x+.15f||p.z<zone.litterMin.y-.15f||p.z>zone.litterMax.y+.15f||p.y<0||p.y>1)Fail("Residuo no válido.");
            foreach(float v in d.dirt)if(!float.IsFinite(v)||v<0||v>1)Fail("Limpieza no válida.");
            if(d.player.x<zone.playerMin.x-.15f||d.player.x>zone.playerMax.x+.15f||d.player.z<zone.playerMin.y-.15f||d.player.z>zone.playerMax.y+.15f||d.player.y<-.2f||d.player.y>3||d.cart.x<zone.cartMin.x-.15f||d.cart.x>zone.cartMax.x+.15f||d.cart.z<zone.cartMin.y-.15f||d.cart.z>zone.cartMax.y+.15f)Fail("Partida fuera del recinto.");
            if(d.restored&&(!d.signFixed||Array.Exists(d.removed,v=>!v)||Array.Exists(d.dirt,v=>v<1)))Fail("Objetivos incoherentes.");
        }
        static bool Finite(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z);
        static void Fail(string message){throw new InvalidDataException(message);}
    }
}
