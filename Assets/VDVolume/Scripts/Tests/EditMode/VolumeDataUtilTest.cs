using NUnit.Framework;
using UnityEngine;
using VDVolume.DataStructure;

namespace Tests
{
    public class VolumeDataUtilTest
    {
        [Test]
        public void Array3DIndexTo1DTest()
        {
            Assert.IsTrue(VolumeDataUtil.Array1DIndexTo3D(3, 2, 3).Equals(new Vector3Int(1,1,0)));
            Assert.IsTrue(VolumeDataUtil.Array1DIndexTo3D(9, 2, 3).Equals(new Vector3Int(1,1,1)));
            Assert.IsTrue(VolumeDataUtil.Array1DIndexTo3D(12, 2, 3).Equals(new Vector3Int(0,0,2)));
            Assert.IsTrue(VolumeDataUtil.Array1DIndexTo3D(19, 2, 3).Equals(new Vector3Int(1,0,3)));
        }
        
        [Test]
        public void Array1DIndexTo3D()
        {
            Assert.AreEqual(5, VolumeDataUtil.Array3DIndexTo1D(1, 1, 0,4,3));
            Assert.AreEqual(4, VolumeDataUtil.Array3DIndexTo1D(0, 1, 0,4,3));
            Assert.AreEqual(23, VolumeDataUtil.Array3DIndexTo1D(3, 2, 1,4,3));
            Assert.AreEqual(9, VolumeDataUtil.Array3DIndexTo1D(1, 2, 0,4,3));
        }
    }
}