using UnityEngine;
namespace Feria
{
    public static class FairSavePaths
    {
        public static string Directory
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.SessionState.GetString("Feria.VerificationRoot",Application.persistentDataPath);
#else
                return Application.persistentDataPath;
#endif
            }
        }
    }
}
