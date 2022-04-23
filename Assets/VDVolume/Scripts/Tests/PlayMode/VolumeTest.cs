using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VDVolume;
using VDVolume.DataStructure;
using VDVolume.Debugging;
using VDVolume.Model;

namespace Tests
{
    public class VolumeTest
    {
        private VolumeDebug _vdVolume;
        private SparseDataRaw _sparseData;
        private VolumeData _volumeData;
        private DenseDataRaw _denseData;
        
        [SetUp]
        public void Setup()
        {
            GameObject volume = new GameObject("testVolume");
            _vdVolume = volume.AddComponent<VolumeDebug>();
        }
        
        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_vdVolume.gameObject);
        }

        private void InitVolumeData(int x,int y,int z,float scale,int chunkSize)
        {
            _vdVolume.TestAndDebugInitRandomVolume(x,y,z, scale, chunkSize);
            _sparseData = _vdVolume.VolumeData.SparseDataRaw;
            _volumeData = _vdVolume.VolumeData;
            _denseData = _volumeData.DenseDataRaw;
        }
        
        [UnityTest]
        public IEnumerator InitializationTest()
        {
            InitVolumeData(7,7,7, 1f, 4);
            yield return null;
            // Grid
            Assert.AreEqual( 7,_vdVolume.VolumeData.GridXDim);
            Assert.AreEqual( 7,_vdVolume.VolumeData.GridYDim);
            Assert.AreEqual( 7,_vdVolume.VolumeData.GridZDim);
            Assert.AreEqual( 7*7*7,_vdVolume.VolumeData.GridVoxelCount);
            // Chunk
            Assert.AreEqual( 8,_vdVolume.VolumeData.GlobalChunkCount);
            Assert.AreEqual( 4,_vdVolume.VolumeData.LocalChunkDim);
            Assert.AreEqual( 2,_vdVolume.VolumeData.GlobalChunkXDim);
            Assert.AreEqual( 2,_vdVolume.VolumeData.GlobalChunkYDim);
            Assert.AreEqual( 2,_vdVolume.VolumeData.GlobalChunkZDim);
            // Scale
            Assert.AreEqual( 1f,_vdVolume.VolumeData.Scale);
        }
        
        [UnityTest]
        public IEnumerator TestSparseStorageAfterInitialization()
        {
            InitVolumeData(7,7,7, 1f, 4);

            int surfaceVoxels = (7 * 7 * 2) + (5 * 7 * 2) + (5 * 5 * 2);
            Assert.AreEqual(8,_sparseData.TmpModifiedVisibleChunks.Count());
            Assert.AreEqual( 0,_sparseData.TmpModifiedVisChunksWithRemoval.Count());
            Assert.AreEqual(8,_sparseData.VisibleChunks.Count());
            Assert.AreEqual( surfaceVoxels,_sparseData.DirtyChunkVisibleVoxels.Count()); // face&back, left&right, top&bottom
            yield return null;
            
            Assert.AreEqual(0,_sparseData.TmpModifiedVisibleChunks.Count());
            Assert.AreEqual(0,_sparseData.TmpModifiedVisChunksWithRemoval.Count());
            Assert.AreEqual(8,_sparseData.VisibleChunks.Count());
            Assert.AreEqual(surfaceVoxels,_sparseData.DirtyChunkVisibleVoxels.Count());
        }

        [UnityTest]
        public IEnumerator SetScaleTest()
        {
            InitVolumeData(7,7,7, 1f, 4);
            List<string> receivedEvents = new List<string>();
            _vdVolume.OnVolumeScaleChanged.AddListener(() => receivedEvents.Add("triggered"));
            
            _vdVolume.SetScale(0.25f);
            Assert.AreEqual(0.25f,_vdVolume.VolumeData.Scale);
            Assert.AreEqual(1, receivedEvents.Count);
            yield return null;
        }

        [UnityTest]
        public IEnumerator InitFromPathTest()
        {
            _vdVolume.InitFromPath(Application.streamingAssetsPath + "/VDVolume/TestData/4x4/4x4.vdvolume", 0.25f, 16);
            Assert.AreEqual( 4,_vdVolume.VolumeData.GridXDim);
            Assert.AreEqual( 4,_vdVolume.VolumeData.GridYDim);
            Assert.AreEqual( 4,_vdVolume.VolumeData.GridZDim);
            yield return null;
        }
    }
}
