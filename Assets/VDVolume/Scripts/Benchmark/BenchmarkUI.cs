using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VDVolume.UnityComponents;

namespace Benchmark
{
    public class BenchmarkUI : MonoBehaviour
    {
        public DrillHead DrillHead;
        public Volume Volume;
        public Button BenchmarkOneButton;
        public Button undoButton;
        public Button FilterVolumeButton;
        public ToggleGroup toolHeadSizeSelection;
        public Text timeText;
        public Text filterTimeText;
        public Text deletedVoxelsText;
        public Text frameCountText;
        public Text benchmarkTypeText;
        public Text undoTimeText;
        public Text voxelGridCreationTimeText;
        public ConicalHelix conicalHelix;

        private Text benchmarkOneButtonText;
        private float conicalHelixStep;
        private Transform drillHeadTransform;
        private bool benchmarkOne = false;
        private int benchmarkFrameCount;
        private float benchmarkTime;
        private float headDrillTimer;
        private float gridSyncTime;
        private float headDrillTimerStep = 0.011111f;//0.013333f;//0.02f;//

        // Start is called before the first frame update
        void Start()
        {
            if (RawBenchmark.ArgExists("-raw"))
            {
                RawBenchmark.Run();
                return;
            }

            drillHeadTransform = DrillHead.GetComponent<Transform>();
            benchmarkOneButtonText = BenchmarkOneButton.transform.GetChild(0).gameObject.GetComponent<Text>();
            BenchmarkOneButton.onClick.AddListener(RunBenchmark);
            FilterVolumeButton.onClick.AddListener(RunFiltering);
            undoButton.onClick.AddListener(UndoBenchmark);
            Volume.OnVolumeInitialized.AddListener(DisplayVoxelGridCreationTime);
            gridSyncTime = Time.realtimeSinceStartup;
            Volume.InitVolume();
        }

        public void OneCut()
        {
            DrillHead.SetSize(0.03f);
            DrillHead.transform.position = new Vector3(0.633f,0.3f,0.618f);
        }

        public void PrintUndoBufferSize()
        {
            Debug.Log(Volume.VolumeModifiers.Undo.GetVoxelCmdBuffer().GetSize());
        }

        public void RunFiltering()
        {
            float beforeFilter = Time.realtimeSinceStartup;
            
            Volume.VolumeModifiers.Filter.FilterValueRange(0,255);
            
            float afterFilter = Time.realtimeSinceStartup - beforeFilter;
            filterTimeText.text = (int) afterFilter + ":" + ((afterFilter*100) % 100).ToString("00") + " seconds";;
        }

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 90;
        }

        private void DisplayVoxelGridCreationTime()
        {
            float timeStamp = Time.realtimeSinceStartup - gridSyncTime;
            voxelGridCreationTimeText.text = (int) timeStamp + ":" + ((timeStamp*100) % 100).ToString("00") + " seconds";
        }

        // Update is called once per frame
        void Update()
        {
            if (benchmarkOne && Time.realtimeSinceStartup - headDrillTimer > headDrillTimerStep)
            {
                float timeStamp = Time.realtimeSinceStartup - benchmarkTime;
                headDrillTimer = Time.realtimeSinceStartup;
                timeText.text = (int) timeStamp + ":" + ((timeStamp*100) % 100).ToString("00") + " seconds";
                if (conicalHelixStep < conicalHelix.stepLowerLimit)
                {
                    StopBenchmarkOne();
                    return;
                }
                
                drillHeadTransform.position = conicalHelix.CalcOneStep(conicalHelixStep);;
                conicalHelixStep -= conicalHelix.GetStepSize();
            }
        }

        public void RunBenchmark()
        {
            Debug.Log("Button clicked.");
            RunBenchmarkOne();
        }

        void RunBenchmarkOne()
        {
            Debug.Log("Benchmark one started.");
            benchmarkFrameCount = Time.frameCount;
            benchmarkTime = Time.realtimeSinceStartup;
            headDrillTimer = Time.realtimeSinceStartup;
            //DrillHead.transform.position = new Vector3(0.0452f, 0.5f, 0.4941f);
            conicalHelixStep = conicalHelix.stepUpperLimit;
                
            string selectedValue =
                toolHeadSizeSelection.ActiveToggles().FirstOrDefault().GetComponentInChildren<Text>().text;
            switch (selectedValue)
            {
                case "10":
                    DrillHead.SetSize(0.05f);
                    break;
                case "5":
                    DrillHead.SetSize(0.06f);
                    break;
                case "2":
                    DrillHead.SetSize(0.0125f);
                    break;
                default:
                    break;
            }

            DrillHead.ResetDeletedVoxelsCount();
            benchmarkOne = true;
            BenchmarkOneButton.enabled = false;
            undoButton.enabled = false;
            benchmarkOneButtonText.text = "Benchmark running...";
            timeText.text = "0:00";
            deletedVoxelsText.text = "calculating...";
            frameCountText.text = "calculating...";
            benchmarkTypeText.text = "Benchmark one";
            undoTimeText.text = "0:00";
        }

        void StopBenchmarkOne()
        {
            benchmarkOne = false;
            
            DrillHead.transform.position = new Vector3(0.0452f, 0.5952f, 0.4941f);
            benchmarkFrameCount = Time.frameCount - benchmarkFrameCount;
            deletedVoxelsText.text = DrillHead.GetDeletedVoxelsCount().ToString();;
            Debug.Log(Volume?.VolumeData.SparseDataRaw.DirtyChunkVisibleVoxels.Count().ToString());
            frameCountText.text = benchmarkFrameCount.ToString();
            undoButton.enabled = true;
            BenchmarkOneButton.enabled = true;
            benchmarkOneButtonText.text = "Run benchmark";
        }

        void UndoBenchmark()
        {
            float startTimeStamp = Time.realtimeSinceStartup;
            
            Volume.VolumeModifiers.Undo.UndoVoxels();
            
            float timeStamp = Time.realtimeSinceStartup - startTimeStamp;
            
            undoTimeText.text = (int) timeStamp + ":" + ((timeStamp*100) % 100).ToString("00") + " seconds";
        }
    }
}