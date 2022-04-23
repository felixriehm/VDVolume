using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace VDVolume.Editor.Import
{
    [ExcludeFromCoverage]
    internal class ImportWindow : EditorWindow
    {
        private bool _loadImageStackError = false;
        private const string UnknownValue = "Unknown";
        private string _recognizedImages = UnknownValue;
        private string _targetImagedim = UnknownValue;
        private string _folderPath = UnknownValue;
        private string _fileNamePattern = "example";
        private string _importErrorMsg = "";
        private string _volumeFileName = "myVolume";
        private Bitmap[] _bitmaps;
        private bool _imageFolderSelected = false;
        private string _volumeSavePath = UnknownValue;
        private AsyncImportOperation _operation;
        private AsyncImportOperation _loadFolderOperation;
        private Importer.LoadFolderData _loadData;

        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "Import image slices",
                EditorStyles.largeLabel);
            
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("\nSelect a folder with image files and press 'Generate'. This will generate a volume file which can be" +
                                    " associated with a volume in the scene. Modifications of the volume will be saved into this file.\n\n" +
                                    "Only JPEG files (.jpg, lower case) are supported. Furthermore the files have to be ordered." +
                                    " The file name has to start with the order number followed by a underscore ('_')." + 
                                    " The count number does not need to have leading zeros." +
                                    "\n\nExamples: '001_myImage.jpg' or '4_.jpg'\n\n" +
                                    " The first image in order will be at the bottom of the volume while the last image will be a the top." +
                                    " The images must have the same dimensions, must not have the same order number and must have 8 bit grayscale values.\n", MessageType.Info);
            EditorGUILayout.Space();

            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////// SELECT FOLDER ///////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            EditorGUILayout.LabelField(
                "Select folder",
                EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                "File name pattern:",
                EditorStyles.label);
            EditorGUILayout.LabelField(
                "001_");
            _fileNamePattern = EditorGUILayout.TextField(_fileNamePattern);
            EditorGUILayout.LabelField(
                ".jpg");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUIStyle sad = EditorStyles.helpBox;
            sad.margin.left = 20;
            sad.margin.right = 20;
            EditorGUILayout.BeginVertical(sad);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                "Selected folder path:",
                EditorStyles.label);
            EditorGUILayout.LabelField(
                _folderPath,
                EditorStyles.label);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                "Recognized images:");
            EditorGUILayout.LabelField(
                _recognizedImages);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                "Image dimension:");
            EditorGUILayout.LabelField(
                _targetImagedim);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Select folder"))
            {
                _folderPath = EditorUtility.OpenFolderPanel("Select folder", "", "");
                if (string.IsNullOrEmpty(_folderPath))
                {
                    _folderPath = UnknownValue;
                    _loadImageStackError = false;
                    _imageFolderSelected = false;
                }
                else
                {
                    _loadData = new Importer.LoadFolderData()
                    {
                        bitmaps = _bitmaps,
                        folderPath = _folderPath,
                        recognizedImages = _recognizedImages,
                        targetImagedim = _targetImagedim,
                        unkownValue = UnknownValue,
                        fileNamePattern = _fileNamePattern,
                        imageFolderSelected = _imageFolderSelected,
                        importErrorMsg = _importErrorMsg,
                        loadImageStackError = _loadImageStackError
                    };
                    _loadFolderOperation = Importer.AsyncLoadFolder(_loadData);
                }
            }
            
            if (_loadFolderOperation != null)
            {
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r,_loadFolderOperation.Progress,"Loading folder...");
                EditorGUILayout.Space(24);
                EditorGUILayout.EndVertical();
                if (_loadFolderOperation.IsDone)
                {
                    _bitmaps = _loadData.bitmaps;
                    _recognizedImages = _loadData.recognizedImages;
                    _targetImagedim = _loadData.targetImagedim;
                    _imageFolderSelected = _loadData.imageFolderSelected;
                    _importErrorMsg = _loadData.importErrorMsg;
                    _loadImageStackError = _loadData.loadImageStackError;

                    _loadData = null;
                    _loadFolderOperation = null;
                }
            }

            EditorGUILayout.Space();
            
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////// GENERATE VOLUME /////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            EditorGUILayout.LabelField(
                "Generate volume",
                EditorStyles.boldLabel);
            _volumeFileName = EditorGUILayout.TextField("File name:", _volumeFileName);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                "Save location:",
                EditorStyles.label);
            EditorGUILayout.LabelField(_volumeSavePath);
            if (GUILayout.Button("Select folder"))
            {
                _volumeSavePath = EditorUtility.SaveFolderPanel("Select folder", "", "");
                if (string.IsNullOrEmpty(_volumeSavePath))
                {
                    _volumeSavePath = UnknownValue;
                }
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = _imageFolderSelected && _volumeFileName != "" && _volumeSavePath != UnknownValue && !string.IsNullOrEmpty(_volumeSavePath) && (_operation?.IsDone ?? true);
            if (GUILayout.Button("Generate"))
            {
                try
                {
                    _operation = Importer.AsyncGenerateVolume(_bitmaps, _volumeSavePath + "/" + _volumeFileName);
                    _recognizedImages = UnknownValue;
                    _targetImagedim = UnknownValue;
                    _folderPath = UnknownValue;
                    _imageFolderSelected = false;
                    _loadImageStackError = false;
                }
                catch (Exception e)
                {
                    _importErrorMsg = "While generating a VDVolume file an error occurred: " + e.Message;
                    _loadImageStackError = true;
                }
            }
            GUI.enabled = true;
            
            if (_operation != null)
            {
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r,_operation.Progress,"Generating volume...");
                EditorGUILayout.Space(24);
                EditorGUILayout.EndVertical();
                if (_operation.IsDone)
                {
                    _operation = null;
                }
            }

            EditorGUILayout.Space();

            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////// CLOSE AND ERROR MESSAGES ////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            if (GUILayout.Button("Close"))
                Close();

            if (_loadImageStackError)
            {
                EditorGUILayout.HelpBox(_importErrorMsg, MessageType.Error);
            }
        }
        
        private void OnDestroy()
        {
            if (_operation != null && !_operation.IsDone && _operation.Thread != null)
            {
                _operation.Thread.Abort();
                Debug.Log("Thread for generating volume aborted.");
            }
            
            if (_loadFolderOperation != null && !_loadFolderOperation.IsDone && _loadFolderOperation.Thread != null)
            {
                _loadFolderOperation.Thread.Abort();
                Debug.Log("Thread for loading folder aborted.");
            }
        }

        
    }
}
