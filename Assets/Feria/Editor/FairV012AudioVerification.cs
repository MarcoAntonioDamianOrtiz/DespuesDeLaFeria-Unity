using System;
using UnityEditor;
public static class FairV012AudioVerification
{
    public static string[] Run()
    {
        var audio=AssetDatabase.LoadMainAssetAtPath("Assets/Feria/Resources/AudioV012.asset");
        if(!audio)throw new Exception("FAIL · Falta configuración de UI Kenney");
        var data=new SerializedObject(audio);
        foreach(var field in new[]{"select","confirm","complete"})if(!data.FindProperty(field).objectReferenceValue)throw new Exception("FAIL · Clip sin asignar: "+field);
        return new[]{"PASS · Tres clips de interfaz configurados"};
    }
}
