using System;
using NUnit.Framework;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Debugging;
using VDVolume.Model;

namespace Tests
{
    public class VolumeDataTest
    {
        private VolumeData _volumeData;
        private DenseDataRaw _denseData;
        
        [SetUp]
        public void Setup()
        {
            GameObject volume = new GameObject();
            volume.transform.position = new Vector3(0, 0, 0);
            _volumeData = new VolumeData(7, 7, 7, 0.125f, 16, volume.transform);
            DebugManager.InitRandomDataParallel(_volumeData);
            _denseData = _volumeData.DenseDataRaw;
        }
        
        [TearDown]
        public void Teardown()
        {
            _volumeData.CleanUp();
        }
        
        
        [Test]
        public void VolumePropertiesTest()
        {
            // Grid
            Assert.AreEqual(_volumeData.GridXDim, 7);
            Assert.AreEqual(_volumeData.GridYDim, 7);
            Assert.AreEqual(_volumeData.GridZDim, 7);
            Assert.AreEqual(_volumeData.GridVoxelCount, 7*7*7);
            // Chunk
            Assert.AreEqual(_volumeData.GlobalChunkCount, 1);
            Assert.AreEqual(_volumeData.LocalChunkDim, 16);
            Assert.AreEqual(_volumeData.GlobalChunkXDim, 1);
            Assert.AreEqual(_volumeData.GlobalChunkYDim, 1);
            Assert.AreEqual(_volumeData.GlobalChunkZDim, 1);
            Assert.AreEqual(_volumeData.LocalChunkVoxelCount, 16*16*16);
            // Scale
            Assert.AreEqual(_volumeData.Scale, 0.125f);
        }

        [Test]
        public void DenseStorageWithoutInitTest()
        {
            _volumeData.CleanUp();
            _volumeData = new VolumeData(7, 7, 7, 0.125f, 16, null);
            _denseData = _volumeData.DenseDataRaw;
            
            // state should be unknown
            int voxelIndex = VolumeDataUtil.GridCoord.To1D(2, 2, 2,_volumeData.GridXDim,_volumeData.GridYDim);
            VoxelData voxelData = _denseData.Voxels[voxelIndex];
            Assert.AreEqual( 0,voxelData.Color);
            Assert.AreEqual( VoxelState.Unknown,voxelData.State);
            Assert.AreEqual( 0,voxelData.X);
            Assert.AreEqual( 0,voxelData.Y);
            Assert.AreEqual( 0,voxelData.Z);
        }

        [Test]
        public void TryGetVoxelFromWorldCoordTest()
        {
            // voxel exists
            VoxelData voxelData;
            bool result = _volumeData.TryGetVoxelFromWorldCoord(new Vector3(0.2f,0.2f,0.2f), out voxelData);
            Assert.AreEqual(true, result);
            Assert.AreEqual(VoxelState.Solid, voxelData.State);
            
            // voxel does not exist
            result = _volumeData.TryGetVoxelFromWorldCoord(new Vector3(-0.1f,-0.1f,-0.1f), out voxelData);
            Assert.AreEqual(false, result);
            Assert.AreEqual(VoxelState.Unknown, voxelData.State);
        }

        [Test]
        public void GetVoxelFromWorldCoordTest()
        {
            // Voxel exists
            VoxelData voxelData =  _volumeData.GetVoxelFromWorldCoord(new Vector3(0.2f,0.2f,0.2f));
            Assert.AreEqual(VoxelState.Solid, voxelData.State);
            
            // Voxel does not exist
            try
            {
                _volumeData.GetVoxelFromWorldCoord(new Vector3(-0.1f,-0.1f,-0.1f));
                Assert.Fail();
            } catch(Exception e){};
        }

        [Test]
        public void GetVoxelIndexFromWorldCoord()
        {
            // Voxel does exist
            Vector3Int voxelIndex = _volumeData.GetVoxelIndexFromWorldCoord(new Vector3(0.1f, 0.1f, 0.1f));
            Assert.IsTrue(voxelIndex.Equals(new Vector3Int(0,0,0)));
            
            // Voxel does not exist
            voxelIndex = _volumeData.GetVoxelIndexFromWorldCoord(new Vector3(-0.1f, -0.1f, -0.1f));
            Assert.AreNotEqual(new Vector3Int(0,0,0), voxelIndex);
        }

        [Test]
        public void GetVoxel()
        {
            // Voxel exists
            VoxelData voxelData = _volumeData.GetVoxel(2, 2, 2);
            Assert.AreEqual(VoxelState.Solid, voxelData.State);
            
            // Voxel does not exist
            try
            {
                _volumeData.GetVoxel(-1, -1, -1);
                Assert.Fail();
            } catch(Exception e){};
        }

        [Test]
        public void TryGetVoxel()
        {
            // Voxel exists
            VoxelData voxelData;
            bool result = _volumeData.TryGetVoxel(2, 2, 2, out voxelData);
            Assert.AreEqual(true, result);
            Assert.AreEqual(VoxelState.Solid, voxelData.State);
            
            // Voxel does not exist
            result = _volumeData.TryGetVoxel(-1, -1, -1, out voxelData);
            Assert.AreEqual(false, result);
            Assert.AreEqual(VoxelState.Unknown, voxelData.State);
        }

        [Test]
        public void PickFirstSolidVoxel()
        {
            // Voxel hit
            VoxelData voxelData;
            Ray ray = new Ray(new Vector3(0.1f, 0.1f, -0.1f), Vector3.forward);
            bool result = _volumeData.PickFirstSolidVoxel(ray, 2f, out voxelData);
            Assert.IsTrue(result);
            Assert.AreEqual(0, voxelData.X);
            Assert.AreEqual(0, voxelData.Y);
            Assert.AreEqual(0, voxelData.Z);
            Assert.AreEqual(VoxelState.Visible, voxelData.State);
            
            // No voxel hit
            ray = new Ray(new Vector3(0.1f, 0.1f, -10.0f), Vector3.forward);
            result = _volumeData.PickFirstSolidVoxel(ray, 2f, out voxelData);
            Assert.IsFalse(result);
            Assert.AreEqual(VoxelState.Unknown, voxelData.State);
            
            // No voxel hit
            ray = new Ray(new Vector3(0.1f, 0.1f, -0.1f), Vector3.forward);
            result = _volumeData.PickFirstSolidVoxel(ray, _volumeData.GridZDim * _volumeData.Scale + 20.0f,
                out voxelData, _volumeData.GridZDim * _volumeData.Scale + 10.0f);
            Assert.IsFalse(result);
            Assert.AreEqual(VoxelState.Unknown, voxelData.State);

        }

        [Test]
        public void SetVoxelColor()
        {
            // Voxel exists
            _volumeData.SetVoxelColor(1,1,1,21);
            VoxelData newVoxel = _volumeData.GetVoxel(1, 1, 1);
            Assert.AreEqual(21, newVoxel.Color);

            // Voxel does not exist
            try
            {
                _volumeData.SetVoxelColor(-1,-1,-1,21);
                Assert.Fail();
            }
            catch (Exception e) { }
        }

        [Test]
        public void TrySetVoxelColor()
        {
            // Voxel exists
            bool result = _volumeData.TrySetVoxelColor(1, 1, 1,21);
            VoxelData newVoxel = _volumeData.GetVoxel(1, 1, 1);
            Assert.IsTrue(result);
            Assert.AreEqual(21, newVoxel.Color);
            
            // Voxel does not exist
            result = _volumeData.TrySetVoxelColor(-1, -1, -1,21);
            Assert.IsFalse(result);
        }
        
        
    }
}