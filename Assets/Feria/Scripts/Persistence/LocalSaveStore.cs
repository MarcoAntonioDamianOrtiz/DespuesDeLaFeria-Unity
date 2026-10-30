using System;
using System.IO;
using UnityEngine;

namespace Feria
{
    /// <summary>File operations are separate so recovery can be verified without touching a player's saves.</summary>
    public sealed class LocalSaveStore
    {
        public string Path { get; }
        public LocalSaveStore(string path) { Path = path ?? throw new ArgumentNullException(nameof(path)); }
        public void Write(FairSave data)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Path)));
            string temp=Path+".tmp";
            File.WriteAllText(temp,JsonUtility.ToJson(data));
            if(File.Exists(Path)) File.Replace(temp,Path,Path+".bak"); else File.Move(temp,Path);
        }
        public bool TryLoad(Action<FairSave> apply,out bool recovered,Action<string> warning=null)
        {
            recovered=false;
            foreach(string candidate in new[]{Path,Path+".bak"})
            {
                if(!File.Exists(candidate)) continue;
                try
                {
                    apply(JsonUtility.FromJson<FairSave>(File.ReadAllText(candidate)));
                    if(candidate!=Path) { File.Copy(candidate,Path,true); recovered=true; }
                    return true;
                }
                catch(Exception e) { warning?.Invoke("No se pudo leer "+System.IO.Path.GetFileName(candidate)+": "+e.Message); }
            }
            return false;
        }
    }
}
