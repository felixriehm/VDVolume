using NUnit.Framework;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Debugging;
using VDVolume.Model;
using VDVolume.Modifiers;

namespace Tests
{
    public class VolumeRestoringTest
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
        public void AddVoxelTest()
        {
            
            VoxelData oldVoxelData = _volumeData.GetVoxel(2,2,2);

            _volumeModifiers.Cutting.RemoveVoxel(2,2,2);
            _volumeModifiers.Restoring.AddVoxel(2,2,2);

            VoxelData newVoxelData = _volumeData.GetVoxel(2,2,2);
            VoxelData leftVoxel = _volumeData.GetVoxel(1,2,2);
            VoxelData rightVoxel = _volumeData.GetVoxel(3,2,2);
            VoxelData topVoxel = _volumeData.GetVoxel(2,3,2);
            VoxelData bottomVoxel = _volumeData.GetVoxel(2,1,2);
            VoxelData backVoxel = _volumeData.GetVoxel(2,2,3);
            VoxelData frontVoxel = _volumeData.GetVoxel(2,2,1);
            
            // Check new voxel data
            Assert.AreEqual( oldVoxelData.Color,newVoxelData.Color);
            Assert.AreEqual( VoxelState.Solid,newVoxelData.State);
            Assert.AreEqual( oldVoxelData.X,newVoxelData.X);
            Assert.AreEqual( oldVoxelData.Y,newVoxelData.Y);
            Assert.AreEqual( oldVoxelData.Z,newVoxelData.Z);
            
            // Check neighbour voxels
            Assert.AreEqual( VoxelState.Solid,leftVoxel.State);
            Assert.AreEqual( VoxelState.Solid,rightVoxel.State);
            Assert.AreEqual( VoxelState.Solid,topVoxel.State);
            Assert.AreEqual( VoxelState.Solid,bottomVoxel.State);
            Assert.AreEqual( VoxelState.Solid,backVoxel.State);
            Assert.AreEqual( VoxelState.Solid,frontVoxel.State);
            
        }

        [Test]
        public void AddCubeParallelTest()
        {
            _volumeModifiers.Cutting.RemoveCubeParallel(new Vector3Int(2,2,2),2);
            _volumeModifiers.Restoring.AddCubeParallel(new Vector3Int(2,2,2),2);
            
            VoxelData voxelData = _volumeData.GetVoxel(1,1,1);
            Assert.AreEqual( VoxelState.Solid,voxelData.State);
            VoxelData voxelData2 = _volumeData.GetVoxel(0,0,0);
            Assert.AreEqual( VoxelState.Visible,voxelData2.State);
        }
        
        [Test]
        public void AddSphereTest()
        {
            _volumeModifiers.Cutting.RemoveSphere(new Vector3Int(2,2,2),2);
            _volumeModifiers.Restoring.AddSphere(new Vector3Int(2,2,2),2);
            VoxelData voxelData = _volumeData.GetVoxel(1, 2, 2);
            Assert.AreEqual( VoxelState.Solid,voxelData.State);
        }
    }
}