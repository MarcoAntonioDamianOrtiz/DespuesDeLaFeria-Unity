using System;
using System.Linq;
using UnityEngine;
public static class FairV012EnvironmentVerification
{
    public static string[] Run()
    {
        var root=GameObject.Find("Decoracion v012");if(!root)throw new Exception("FAIL · Falta integración de entorno v012");
        if(root.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("Copa externa"))<6)throw new Exception("FAIL · Faltan seis variantes de copa");
        return new[]{"PASS · Entorno integrado con seis variantes naturales"};
    }
}
