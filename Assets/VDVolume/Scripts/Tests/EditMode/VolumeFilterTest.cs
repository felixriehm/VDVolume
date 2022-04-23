using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VDVolume.DataStructure;
using VDVolume.Model;
using VDVolume.UnityComponents;
using System.Collections;
using VDVolume.Modifiers;
using VDVolume.Serialization;

namespace Tests
{
    public class VolumeFilterTest
    {
        private VolumeModifiers _modifiers;
        private VolumeData _volumeData;
        
        [SetUp]
        public void Setup()
        {
            _volumeData = new VolumeData(4, 4, 4, 0.125f, 16, null);
            VolumeFile file = VolumeSerializer.ReadFile(Application.streamingAssetsPath + "/VDVolume/TestData/4x4/4x4.vdvolume");
            AsyncSerializationOperation operation = VolumeSerializer.AsyncLoadVolume(file, _volumeData);
            operation.Thread.Join();
            _modifiers = new VolumeModifiers(_volumeData);
        }
        
        [TearDown]
        public void Teardown()
        {
            _volumeData.CleanUp();
            _modifiers.CleanUp();
        }
        
        [Test]
        public void FilterValueTest()
        {
            _modifiers.Filter.FilterValue(0);
            VoxelData voxelData = _volumeData.GetVoxel(0, 0, 0);
            VoxelData voxelData2 = _volumeData.GetVoxel(3, 3, 0);
            VoxelData voxelData3 = _volumeData.GetVoxel(0, 1, 0);
            VoxelData voxelData4 = _volumeData.GetVoxel(2, 0, 0);
            
            Assert.AreEqual(VoxelState.Removed, voxelData.State);
            Assert.AreEqual(VoxelState.Removed, voxelData2.State);
            Assert.AreEqual(VoxelState.Visible, voxelData3.State);
            Assert.AreEqual(VoxelState.Visible, voxelData4.State);
            Assert.AreEqual(0, _modifiers.Filter.LastFilterValue);
        }
        
        [Test]
        public void FilterValueRangeTest()
        {
            _modifiers.Filter.FilterValueRange(0,255);
            VoxelData voxelData = _volumeData.GetVoxel(0, 0, 0);
            VoxelData voxelData2 = _volumeData.GetVoxel(3, 3, 0);
            VoxelData voxelData3 = _volumeData.GetVoxel(0, 1, 0);
            VoxelData voxelData4 = _volumeData.GetVoxel(2, 0, 0);
            
            Assert.AreEqual(VoxelState.Removed, voxelData.State);
            Assert.AreEqual(VoxelState.Removed, voxelData2.State);
            Assert.AreEqual(VoxelState.Removed, voxelData3.State);
            Assert.AreEqual(VoxelState.Removed, voxelData4.State);
            Assert.AreEqual(0, _modifiers.Filter.LastFilterRangeBegin);
            Assert.AreEqual(255, _modifiers.Filter.LastFilterRangeEnd);
        }
    }
}