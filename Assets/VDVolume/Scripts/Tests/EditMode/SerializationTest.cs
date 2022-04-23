using System.IO;
using NUnit.Framework;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Model;
using VDVolume.Modifiers;
using VDVolume.Serialization;

namespace Tests
{
    public class SerializationTest
    {
        [Test]
        public void ReadFileTest()
        {
            VolumeFile file = VolumeSerializer.ReadFile(Application.streamingAssetsPath + "/VDVolume/TestData/smiley/example.vdvolume");
            Assert.AreEqual(24, file.XDim);
            Assert.AreEqual(24, file.YDim);
            Assert.AreEqual(7, file.ZDim);
            // 24*24*7 -> amount of voxels, 10 -> bits per voxel data, 8 -> to get bytes from bits, 12 -> the file
            // begins with the 3 dimensions of the volume which are each 4 bytes
            Assert.AreEqual(((24 * 24 * 7 * 10) / 8) + 12, file.Data.Length);
        }
        
        [Test]
        public void LoadVolumeTest()
        {
            VolumeData _volumeData = new VolumeData(24, 24, 7, 0.125f, 16, null);
            VolumeFile file = VolumeSerializer.ReadFile(Application.streamingAssetsPath + "/VDVolume/TestData/smiley/example.vdvolume");
            AsyncSerializationOperation operation = VolumeSerializer.AsyncLoadVolume(file, _volumeData);
            operation.Thread.Join();
            VoxelData voxelData = _volumeData.GetVoxel(0, 0, 0);
            Assert.AreEqual(VoxelState.Visible, voxelData.State);
            Assert.AreEqual(254, voxelData.Color);
            _volumeData.CleanUp();
        }
        
        // [Test]
        // public void SaveVolumeTest()
        // {
        //     File.Delete(Application.streamingAssetsPath + "/VDVolume/TestData/temp/4x4.vdvolume");
        //     VolumeData _volumeData = new VolumeData(24, 24, 7, 0.125f, 16, null);
        //     File.Copy(Application.streamingAssetsPath + "/VDVolume/TestData/4x4/4x4.vdvolume", Application.streamingAssetsPath + "/VDVolume/TestData/temp/4x4.vdvolume");
        //     
        //     // Load volume
        //     VolumeFile file = VolumeSerializer.ReadFile(Application.streamingAssetsPath + "/VDVolume/TestData/temp/4x4.vdvolume");
        //     AsyncSerializationOperation operation = VolumeSerializer.AsyncLoadVolume(file, _volumeData);
        //     operation.Thread.Join();
        //     VolumeModifiers modifiers = new VolumeModifiers(_volumeData);
        //     // Modify volume
        //     modifiers.Cutting.RemoveVoxel(0,0,0);
        //     // Save volume
        //     operation = VolumeSerializer.AsyncSaveVolume(file.FilePathToVDVolume, _volumeData);
        //     operation.Thread.Join();
        //     // Load volume again
        //     _volumeData = new VolumeData(24, 24, 7, 0.125f, 16, null);
        //     VolumeFile file2 = VolumeSerializer.ReadFile(Application.streamingAssetsPath + "/VDVolume/TestData/temp/4x4.vdvolume");
        //     operation = VolumeSerializer.AsyncLoadVolume(file2, _volumeData);
        //     operation.Thread.Join();
        //     
        //     // The voxel should be removed
        //     VoxelData voxelData = _volumeData.GetVoxel(0, 0, 0);
        //     Assert.AreEqual(VoxelState.Removed, voxelData.State);
        //     // The file data sizes should be the same
        //     Assert.AreEqual(24, file2.XDim);
        //     Assert.AreEqual(24, file2.YDim);
        //     Assert.AreEqual(7, file2.ZDim);
        //     // 24*24*7 -> amount of voxels, 10 -> bits per voxel data, 8 -> to get bytes from bits, 12 -> the file
        //     // begins with the 3 dimensions of the volume which are each 4 bytes
        //     Assert.AreEqual(((24 * 24 * 7 * 10) / 8) + 12, file2.Data.Length);
        // }
    }
}