using System;
using System.Drawing;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using VDVolume.Editor.Import;
using VDVolume.Serialization;

namespace Tests
{
    public class ImporterTest
    {
        [Test]
        public void GenerateVolumeTest()
        {
            File.Delete(Application.streamingAssetsPath + "/VDVolume/TestData/temp/UnitTestTemp.vdvolume");
            // LoadFolder() should succeed
            Importer.LoadFolderData folderData = new Importer.LoadFolderData()
            {
                folderPath = Application.streamingAssetsPath + "/VDVolume/TestData/smiley",
                fileNamePattern = "example"
            };
            AsyncImportOperation operation = Importer.AsyncLoadFolder(folderData);
            operation.Thread.Join();
            operation = Importer.AsyncGenerateVolume(folderData.bitmaps, Application.streamingAssetsPath + "/VDVolume/TestData/temp/UnitTestTemp");
            operation.Thread.Join();
            // Test: File exists
            VolumeFile file = VolumeSerializer.ReadFile(Application.streamingAssetsPath + "/VDVolume/TestData/temp/UnitTestTemp.vdvolume");
            // Test: File has the right dimensions
            Assert.AreEqual(24, file.XDim);
            Assert.AreEqual(24, file.YDim);
            Assert.AreEqual(7, file.ZDim);
            // Test: File has the right data length
            // 24*24*7 -> amount of voxels, 10 -> bits per voxel data, 8 -> to get bytes from bits, 12 -> the file
            // begins with the 3 dimensions of the volume which are each 4 bytes
            Assert.AreEqual(((24 * 24 * 7 * 10) / 8) + 12, file.Data.Length);
        }

        [Test]
        public void LoadFolder_FolderPathDoesNotExist()
        {
            AsyncImportOperation operation;
            Importer.LoadFolderData folderData = new Importer.LoadFolderData()
            {
                folderPath = Application.streamingAssetsPath + "/VDVolume/Unknown",
                fileNamePattern = "unknown",
                unkownValue = "testUnknown"
            };
            try
            {
                operation = Importer.AsyncLoadFolder(folderData);
                operation.Thread.Join();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(true, folderData.loadImageStackError);
            }
        }

        [Test]
        public void LoadFolder_FolderPathIsEmpty()
        {
            AsyncImportOperation operation;
            Importer.LoadFolderData folderData = new Importer.LoadFolderData()
            {
                folderPath = Application.streamingAssetsPath,
                fileNamePattern = "example",
                unkownValue = "testUnknown"
            };

            operation = Importer.AsyncLoadFolder(folderData);
            operation.Thread.Join();
            Assert.AreEqual("testUnknown", folderData.targetImagedim);
            Assert.AreEqual("0", folderData.recognizedImages);
        }
    }
}