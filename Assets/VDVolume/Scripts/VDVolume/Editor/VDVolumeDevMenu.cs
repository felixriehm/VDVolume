using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VDVolume.Editor
{
    public class VDVolumeDevMenu
    {
        [MenuItem("VDVolume/Export Unity package", false, 100)]
        private static void CreateVolumeInitData()
        {
            var exportedPackageAssetList = new List<string>();

            foreach (var guid in AssetDatabase.FindAssets("l:Export"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }

            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(),
                "VDVolume.unitypackage");
        }
    }
}