#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace VDVolume.Editor
{
    [InitializeOnLoad]
    [ExcludeFromCoverage]
    internal class Autorun
    {
        static Autorun()
        {
            // Check if cdc.rsp exists
            if (!System.IO.File.Exists(Application.dataPath + "/csc.rsp"))
            {
                Debug.Log("File 'csc.rsp' should exist in the 'Assets' folder. It should contain '-r:System.Drawing.dll -unsafe'.");
            }

            // Check if unsafe code is allowed
            if (!PlayerSettings.allowUnsafeCode)
            {
                Debug.Log("AllowUnsafe code was not set to 'true'. It is now set to 'true'.");
                PlayerSettings.allowUnsafeCode = true;
            }
            
            // Check for api compatibility level
            if (PlayerSettings.GetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup) != ApiCompatibilityLevel.NET_4_6)
            {
                Debug.Log("ApiCompatibilityLevel was not set to '4.x'. It is now set to '4.x'.");
                PlayerSettings.SetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup, ApiCompatibilityLevel.NET_4_6);
            }
        }
    }
}
#endif