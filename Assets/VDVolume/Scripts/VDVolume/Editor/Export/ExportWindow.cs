using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace VDVolume.Editor.Export
{
    [ExcludeFromCoverage]
    public class ExportWindow : EditorWindow
    {
        private string _filePath;
        private string _folderPath;
        private AsyncExportOperation _operation;
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "Export volume",
                EditorStyles.largeLabel);

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "\nSelect a .vdvolume file and a destination folder and press 'Export' to export volume data to PNG images." +
                " The file names be are the same as the .vdvolume file. Files will have a leading number and will overwrite existing files.\n",
                MessageType.Info);
            EditorGUILayout.Space();

            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////// EXPORT VOLUME ///////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = _operation == null;
            if (GUILayout.Button("Select volume file",GUILayout.Width(170.0f))) 
                _filePath = EditorUtility.OpenFilePanel("Select volume file", "", "vdvolume");
            EditorStyles.label.wordWrap = true;
            EditorGUILayout.LabelField(
                "File path: " + _filePath,
                EditorStyles.label);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select destination",GUILayout.MaxWidth(170.0f))) 
                _folderPath = EditorUtility.OpenFolderPanel("Select destination", "", "vdvolume");
            EditorGUILayout.LabelField(
            "Folder path: " + _folderPath,
            EditorStyles.label);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUIStyle sad = EditorStyles.helpBox;
            sad.margin.left = 20;
            sad.margin.right = 20;
            
            GUI.enabled = !string.IsNullOrEmpty(_filePath) && !string.IsNullOrEmpty(_folderPath);
            if (GUILayout.Button("Export"))
            {
                _operation = Exporter.AsyncExportVolumeToPng(_filePath, _folderPath);
                _folderPath = "";
                _filePath = "";
            }
            GUI.enabled = true;
            if (_operation != null)
            {
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r,_operation.Progress,"Exporting volume...");
                EditorGUILayout.Space(24);
                EditorGUILayout.EndVertical();
                if (_operation.IsDone)
                {
                    _operation = null;
                }
            }
        }
        
        private void OnDestroy()
        {
            if (_operation != null && !_operation.IsDone && _operation.Thread != null)
            {
                _operation.Thread.Abort();
                Debug.Log("Thread for exporting volume aborted.");
            }
        }
    }
}