using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VDVolume.DataStructure;
using VDVolume.Debugging;
using VDVolume.Model;
using VDVolume.Modifiers;

namespace Tests
{
    public class VolumeCuttingTest
    {
        private VolumeData _volumeData;
        private VolumeModifiers _volumeModifiers;
        
        [SetUp]
        public void Setup()
        {
            _volumeData = new VolumeData(7, 7, 7, 0.125f, 16, null);
            DebugManager.InitRandomDataParallel(_volumeData);
            _volumeModifiers = new VolumeModifiers(_volumeData);
        }
        
        [TearDown]
        public void Teardown()
        {
            _volumeData.CleanUp();
            _volumeModifiers.CleanUp();
        }

        [Test]
        public void TestVoxelAfterRemoval()
        {
            VoxelData oldVoxelData = _volumeData.GetVoxel(2,2,2);

            _volumeModifiers.Cutting.RemoveVoxel(2,2,2);

            VoxelData newVoxelData = _volumeData.GetVoxel(2,2,2);
            
            Assert.AreEqual( oldVoxelData.Color,newVoxelData.Color);
            Assert.AreEqual( VoxelState.Removed,newVoxelData.State);
            Assert.AreEqual( oldVoxelData.X,newVoxelData.X);
            Assert.AreEqual( oldVoxelData.Y,newVoxelData.Y);
            Assert.AreEqual( oldVoxelData.Z,newVoxelData.Z);
        }

        [Test]
        public void TestNeighbourVoxelsAfterRemoval()
        {
            _volumeModifiers.Cutting.RemoveVoxel(2,2,2);
            
            VoxelData leftVoxel = _volumeData.GetVoxel(1,2,2);
            VoxelData rightVoxel = _volumeData.GetVoxel(3,2,2);
            VoxelData topVoxel = _volumeData.GetVoxel(2,3,2);
            VoxelData bottomVoxel = _volumeData.GetVoxel(2,1,2);
            VoxelData backVoxel = _volumeData.GetVoxel(2,2,3);
            VoxelData frontVoxel = _volumeData.GetVoxel(2,2,1);
            
            Assert.AreEqual( VoxelState.Visible,leftVoxel.State);
            Assert.AreEqual( VoxelState.Visible,rightVoxel.State);
            Assert.AreEqual( VoxelState.Visible,topVoxel.State);
            Assert.AreEqual( VoxelState.Visible,bottomVoxel.State);
            Assert.AreEqual( VoxelState.Visible,backVoxel.State);
            Assert.AreEqual( VoxelState.Visible,frontVoxel.State);
        }

        [Test]
        public void RemoveCubeParallelTest()
        {
            _volumeModifiers.Cutting.RemoveCubeParallel(new Vector3Int(2,2,2),2);
            VoxelData voxelData = _volumeData.GetVoxel(0, 0, 0);
            Assert.AreEqual( VoxelState.Removed,voxelData.State);
        }
        
        [Test]
        public void RemoveSphereTest()
        {
            _volumeModifiers.Cutting.RemoveSphere(new Vector3Int(2,2,2),2);
            VoxelData voxelData = _volumeData.GetVoxel(1, 2, 2);
            Assert.AreEqual( VoxelState.Removed,voxelData.State);
        }
    }
}