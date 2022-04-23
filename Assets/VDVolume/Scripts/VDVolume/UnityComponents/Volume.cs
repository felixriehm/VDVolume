using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using VDVolume;
using VDVolume.DataStructure;
using VDVolume.Editor;
using VDVolume.Model;
using VDVolume.Modifiers;
using VDVolume.Rendering;
using VDVolume.Serialization;

[assembly: InternalsVisibleTo("VDVolume.Tests.PlayMode"), InternalsVisibleTo("VDVolume.Tests.EditMode"),
           InternalsVisibleTo("VDVolume.Benchmark"), InternalsVisibleTo("VDVolume.Editor")]

namespace VDVolume.UnityComponents
{
    /// <summary>
    /// Defines a volume game object.
    /// </summary>
    public class Volume : MonoBehaviour
    {
        /// This will be overwritten if VolumeInitData isn't null. This can be an absolute or relative path (see VolumeInitData.cs) 
        /// <summary>
        /// Path to the .vdvolume file.
        /// </summary>
        [SerializeField]
        private string FilePathToVDVolume = "";
        /// <summary>
        /// Scale of the volume. This will be overwritten if VolumeInitData isn't null.
        /// </summary>
        [SerializeField]
        private float VolumeScale = 0.25f; 
        /// For more see information see VolumeInitData.cs. 
        /// <summary>
        /// Chunk size of the volume. This will be overwritten if VolumeInitData isn't null.
        /// </summary>
        [SerializeField]
        private int VolumeChunkSize = 16;
        /// <summary>
        /// Defines if the volume initializes with Unity's Start() method.
        /// </summary>
        [SerializeField]
        private bool InitializeOnStart = false;
        /// <summary>
        /// Defines if collider of the volume will be a trigger.
        /// </summary>
        [SerializeField]
        private bool ColliderIsTrigger = true;
        /// <summary>
        /// Defines the init data for the volume.
        /// </summary>
        [SerializeField]
        private VolumeInitData VolumeInitData = null;
        /// <summary>
        /// VolumeModifier object of this volume.
        /// </summary>
        public VolumeModifiers VolumeModifiers { get; private set; }
        /// <summary>
        /// VolumeData object of this volume.
        /// </summary>
        public VolumeData VolumeData { get; private set; }
        /// <summary>
        /// This will trigger when the volume data has been fully initialized.
        /// </summary>
        public UnityEvent OnVolumeInitialized { get; private set; }
        /// <summary>
        /// This will trigger when the scale of the volume has changed.
        /// </summary>
        public UnityEvent OnVolumeScaleChanged { get; private set; }
        /// <summary>
        /// This will trigger when the the saving process has started.
        /// </summary>
        public UnityEvent OnVolumeSavingStarted { get; private set; }
        /// <summary>
        /// This will trigger when the the saving process has ended.
        /// </summary>
        public UnityEvent OnVolumeSavingEnded { get; private set; }
        /// <summary>
        /// Tells if the volume is initialized.
        /// </summary>
        public bool Initialized { get; private set; } = false;
        
        private VolumeRenderer VolumeRenderer { get; set; }
        internal bool DrawCollidableGizmo { get; set; } = false;
        private VolumeFile _volumeFile;
        // this is for doxygen to ignore private protected members
        //! @cond
        private protected Transform _transform;
        private protected SparseDataRaw _sparseData;
        private protected DenseDataRaw _denseData;
        private protected GridDataRaw _gridData;
        //! @endcond
        private bool _isSaving;
        private const string RelativePathIdentifier = "@relative:";

        /// Note: If a VolumeInitData is provided the manually typed input in the inspector will be overwritten.
        /// <summary>
        /// Inits the volume.
        /// </summary>
        public void InitVolume()
        {
            string path = FilePathToVDVolume;
            float scale = VolumeScale;
            int chunkSize = VolumeChunkSize;
            if (VolumeInitData != null)
            {
                path = VolumeInitData.pathToVolumeFile;
                scale = VolumeInitData.scale;
                chunkSize = VolumeInitData.chunkSize;
            }
            
            InitFromPath(path, scale, chunkSize);
        }

        /// This Method will ignore all data provided by the game object inspector in the Unity editor.
        /// All notes described in VolumeInitData.cs apply for the arguments of this method.
        /// <summary>
        /// Inits the volume from a given file path.
        /// </summary>
        /// <param name="filePathToVDVolume">File path to the .vdvolume file.</param>
        /// <param name="scale">The scale of the volume.</param>
        /// <param name="chunkSize">The chunk size of the volume.</param>
        public void InitFromPath(string filePathToVDVolume, float scale, int chunkSize)
        {
            GameObject loadVolumeBillboard = Instantiate(Resources.Load("Prefabs/SerializationBillboard")) as GameObject;
            loadVolumeBillboard.transform.position = _transform.position;

            BeforeVolumeDataInit();

            string path = filePathToVDVolume;
            if (filePathToVDVolume.StartsWith(RelativePathIdentifier))
            {
                string relativePath = path.Substring(RelativePathIdentifier.Length);
                if (!relativePath.StartsWith("/"))
                    relativePath = relativePath.Insert(0, "/");
                path = Application.streamingAssetsPath + relativePath;
            }
            _volumeFile = VolumeSerializer.ReadFile(path);
            InitVolumeData(_volumeFile.XDim, _volumeFile.YDim, _volumeFile.ZDim, scale, chunkSize);
            
            AsyncSerializationOperation operation = VolumeSerializer.AsyncLoadVolume(_volumeFile, VolumeData);
            loadVolumeBillboard.GetComponent<SerializationBillboard>().SetAsyncSerializationOperation(operation, true);
            StartCoroutine(CheckIfLoadingIsDone(operation, loadVolumeBillboard));
        }

        /// <summary>
        /// Changes the scale of the volume.
        /// </summary>
        /// <param name="scale">New scale of the volume.</param>
        public void SetScale(float scale)
        {
            VolumeData.Scale = scale;
            OnVolumeScaleChanged.Invoke();
            _transform.hasChanged = true;
        }

        /// A .Net thread is created for the process.
        /// <summary>
        /// Asynchronous saves a volume. This spawns a billboard object into the scene which shows a progress bar.
        /// </summary>
        public void AsyncSaveVolume()
        {
            if (Initialized)
            {
                _isSaving = true;
                GameObject loadVolumeBillboard = Instantiate(Resources.Load("Prefabs/SerializationBillboard")) as GameObject;
                loadVolumeBillboard.transform.position = _transform.position;
                AsyncSerializationOperation operation = VolumeSerializer.AsyncSaveVolume(_volumeFile.FilePathToVDVolume,VolumeData);
                loadVolumeBillboard.GetComponent<SerializationBillboard>().SetAsyncSerializationOperation(operation, false);
                OnVolumeSavingStarted.Invoke();
                StartCoroutine(CheckIfSavingIsDone(operation, loadVolumeBillboard));
            }
        }

        private void Awake()
        {
            OnVolumeInitialized = new UnityEvent();
            OnVolumeScaleChanged = new UnityEvent();
            OnVolumeSavingStarted = new UnityEvent();
            OnVolumeSavingEnded = new UnityEvent();

            _transform = transform;
            
            if (Application.isPlaying && InitializeOnStart)
            {
                InitVolume();
            }
        }

        void Update()
        {
            if (Initialized && !_isSaving)
            {
                // Fix dirty surface voxels and update surface chunks
                VolumeData.CleanDirtyData();

                // Render
                VolumeRenderer.Render();
            
                // Reset modified chunks
                if (_sparseData.TmpModifiedVisibleChunks.Count() > 0) 
                    VolumeData.ResetModifiedChunks();

                // This has to be set manually
                transform.hasChanged = false;
            }
        }

        private void OnDestroy()
        {
            if (Initialized)
            {
                CleanUp();
            }
        }

        private IEnumerator CheckIfLoadingIsDone(AsyncSerializationOperation operation, GameObject billboard)
        {
            while (!operation.IsDone)
            {
                yield return null;
            }
            AfterVolumeDataInit();
            Destroy(billboard);
        }

        private void CleanUp()
        {
            VolumeData.CleanUp();
            VolumeRenderer.CleanUp();
            VolumeModifiers.CleanUp();
            VolumeRenderer = null;
            VolumeModifiers = null;
            VolumeData = null;
            Initialized = false;
        }

        // this is for doxygen to ignore private protected members
        //! @cond
        private protected void InitVolumeData(int gridX, int gridY, int gridZ, float scale, int localChunkDim)
        {
            VolumeData = new VolumeData(gridX, gridY, gridZ, scale, localChunkDim, transform);
            _sparseData = VolumeData.SparseDataRaw;
            _denseData = VolumeData.DenseDataRaw;
            _gridData = VolumeData.GridDataRaw;
        }

        private protected void BeforeVolumeDataInit()
        {
            if (Initialized)
            {
                CleanUp();
            }
        }

        private protected void AfterVolumeDataInit()
        {
            AddBoxCollider();
            VolumeModifiers = new VolumeModifiers(VolumeData);
            VolumeRenderer = new VolumeRenderer();
            VolumeRenderer.Initialize(transform, VolumeData);
            Initialized = true;
            OnVolumeInitialized.Invoke();
        }
        //! @endcond

        private void AddBoxCollider()
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            Vector3 colliderSize = new Vector3(VolumeData.GridXDim * VolumeData.Scale, VolumeData.GridYDim * VolumeData.Scale, VolumeData.GridZDim * VolumeData.Scale);
            boxCollider.center = Vector3.Scale(colliderSize, new Vector3(0.5f,0.5f,0.5f));
            boxCollider.size = colliderSize;
            boxCollider.isTrigger = ColliderIsTrigger;
        }

        private IEnumerator CheckIfSavingIsDone(AsyncSerializationOperation operation, GameObject billboard)
        {
            while (!operation.IsDone)
            {
                yield return null;
            }

            _isSaving = false;
            Destroy(billboard);
            OnVolumeSavingEnded.Invoke();
        }
    }
}

