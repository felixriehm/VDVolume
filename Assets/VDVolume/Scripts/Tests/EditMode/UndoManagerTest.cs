using NUnit.Framework;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Debugging;
using VDVolume.Model;
using VDVolume.Modifiers;

namespace Tests
{
    public class UndoManagerTest
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
        public void UndoVoxelsTest()
        {
            _volumeModifiers.Cutting.RemoveVoxel(2,2,2);
            _volumeModifiers.Cutting.RemoveVoxel(4,4,4);
            _volumeModifiers.Restoring.AddVoxel(4,4,4);

            Assert.AreEqual(3, _volumeModifiers.Undo.GetVoxelCmdBuffer().GetSize());
            _volumeModifiers.Undo.UndoVoxels();
            Assert.AreEqual(0, _volumeModifiers.Undo.GetVoxelCmdBuffer().GetSize());

            VoxelData voxelData = _volumeData.GetVoxel(2, 2, 2);
            VoxelData voxelData2 = _volumeData.GetVoxel(2, 2, 2);
            Assert.AreEqual(VoxelState.Solid, voxelData.State);
            Assert.AreEqual(VoxelState.Solid, voxelData2.State);
        }
    }
}