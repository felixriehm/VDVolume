using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VDVolume.DataStructure;
using VDVolume.Debugging;
using VDVolume.Model;

namespace Tests
{
    public class SparseStorageTest
    {
        private VolumeDebug _vdVolume;
        private VolumeData _volumeData;
        private SparseDataRaw _sparseData;
        
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
            _volumeData = _vdVolume.VolumeData;
            _sparseData = _vdVolume.VolumeData.SparseDataRaw;
        }
        
        [UnityTest]
        public IEnumerator TestSparseStorageAfterRemoval()
        {
            InitVolumeData(7, 7, 7, 0.125f, 16);
            yield return null;

            int surfaceVoxels = (7 * 7 * 2) + (5 * 7 * 2) + (5 * 5 * 2);
            
            Assert.AreEqual( 0,_sparseData.TmpModifiedVisibleChunks.Count());
            Assert.AreEqual( 0,_sparseData.TmpModifiedVisChunksWithRemoval.Count());
            Assert.AreEqual( surfaceVoxels,_sparseData.DirtyChunkVisibleVoxels.Count());
            Assert.AreEqual( 1,_sparseData.VisibleChunks.Count());
            
            _vdVolume.VolumeModifiers.Cutting.RemoveVoxel(2,2,2);
            
            Assert.AreEqual( 1,_sparseData.TmpModifiedVisibleChunks.Count());
            Assert.AreEqual( 0,_sparseData.TmpModifiedVisChunksWithRemoval.Count());
            Assert.AreEqual( surfaceVoxels + 6,_sparseData.DirtyChunkVisibleVoxels.Count());
            Assert.AreEqual( 1,_sparseData.VisibleChunks.Count());
        }
    }
}