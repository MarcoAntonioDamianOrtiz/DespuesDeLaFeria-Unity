using System;
using System.Linq;
using Feria;
using UnityEditor;
using UnityEngine;
public static class FairV012StainVerification
{
    public static string[] Run()
    {
        var catalog=AssetDatabase.LoadMainAssetAtPath("Assets/Feria/Definitions/ManchasV012.asset");
        if(!catalog)throw new Exception("FAIL · Falta catálogo de siete familias");
        var get=catalog.GetType().GetMethod("Get");
        var names=Enumerable.Range(0,7).Select(i=>get.Invoke(catalog,new object[]{i}).GetType().GetField("displayName").GetValue(get.Invoke(catalog,new object[]{i})).ToString()).ToArray();
        if(names.Distinct().Count()!=7)throw new Exception("FAIL · Deben existir siete familias distintas");
        if(!Shader.Find("Feria/Spill"))throw new Exception("FAIL · Falta shader de líquido");
        return new[]{"PASS · Siete familias distintas y shader URP presente"};
    }
}
