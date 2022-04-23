using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VDVolume.Serialization
{
    /// <summary>
    /// Billboard game object for showing a progressbar when loading or saving the volume.
    /// </summary>
    internal class SerializationBillboard : MonoBehaviour
    {
        private Transform _cameraTtransform;
        private Transform _progressBar;
        private TextMeshPro _serializationText;
        private AsyncSerializationOperation _asyncSerializationOperation;

        private void Awake()
        {
            _cameraTtransform = Camera.main == null ? new GameObject().transform : Camera.main.transform;
            
            _progressBar = transform.Find("ProgressBar");
            _serializationText = transform.Find("SerializationText").GetComponent<TextMeshPro>();
        }

        internal void SetAsyncSerializationOperation(AsyncSerializationOperation operation, bool isLoading)
        {
            _serializationText.text = isLoading ? "Loading..." : "Saving...";
            _asyncSerializationOperation = operation;
        }
        
        private void Update()
        {
            // to get the billboard effect where the plane always looks at the camera
            transform.LookAt(_cameraTtransform.position, Vector3.up);
            
            if(_asyncSerializationOperation == null) return;
            
            // update the progress bar, with the scale of 10f it reaches the end of the background plane
            _progressBar.localScale = new Vector3(_asyncSerializationOperation.Progress * 10.0f,1.0f,1.0f);
        }
    }
}