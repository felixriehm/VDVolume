using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VDVolume.Editor.Export;
using VDVolume.Editor.Import;

namespace VDVolume.Editor
{
    [ExcludeFromCoverage]
    internal class VDVolumeMenu
    {
        
        [MenuItem("VDVolume/Import image slices", false, 1)]
        private static void LoadImageStack()
        {
            ImportWindow window = (ImportWindow)EditorWindow.GetWindow(typeof(ImportWindow),true,"Import image slices");
            window.position = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 700, 600);
            window.Show();
        }
        
        [MenuItem("VDVolume/Create VolumeInitData", false, 2)]
        private static void CreateVolumeInitData()
        {
            VolumeInitData initData = ScriptableObject.CreateInstance<VolumeInitData>();

            string name = AssetDatabase.GenerateUniqueAssetPath("Assets/VolumeInitData.asset");
            AssetDatabase.CreateAsset(initData, name);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = initData;
        }
        
        [MenuItem("VDVolume/Export volume", false, 5)]
        private static void ExportVolume()
        {
            ExportWindow window = (ExportWindow)EditorWindow.GetWindow(typeof(ExportWindow),true,"Export volume");
            window.position = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 700, 250);
            window.Show();
        }
    }
}