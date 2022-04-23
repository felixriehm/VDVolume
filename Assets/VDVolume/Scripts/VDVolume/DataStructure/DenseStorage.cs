using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using VDVolume.Model;

namespace VDVolume.DataStructure
{
    public partial class VolumeData
    {
        /// The dense storage houses a single 1D array with all voxel data. All voxels of the volume are stored inside this array.
        /// The location of a voxel inside this 1D array, given a three dimensional index vector, can be calculated with the VolumeDataUtil class.
        /// <summary>
        /// Dense storage of the volume data.
        /// </summary>
        private class DenseStorage
        {
            internal DenseDataRaw DenseDataRaw => _denseData;
            // all voxels
            private DenseDataRaw _denseData;
            // custom data
            //private static NativeArray _customData;
            //private static CustomDataType CustomDataType = CustomDataType.None;
            
            private VolumeData _volumeData;

            internal DenseStorage(VolumeData volumeData)
            {
                _volumeData = volumeData;
                DenseDataRaw denseData = new DenseDataRaw();
                denseData.Voxels = new NativeArray<VoxelData>(_volumeData.GridVoxelCount, Allocator.Persistent);
                _denseData = denseData;
            }

            internal VoxelData GetVoxel(int x, int y, int z)
            {
                return _denseData.Voxels[VolumeDataUtil.GridCoord.To1D(x, y, z,_volumeData.GridXDim,_volumeData.GridYDim)];
            }

            internal void CleanUp()
            {
                _denseData.Voxels.Dispose();
            }

            internal NativeArray<VoxelData> GetVoxels()
            {
                return _denseData.Voxels;
            }

            internal void SetVoxel(int x, int y, int z, byte color)
            {
                int index = VolumeDataUtil.GridCoord.To1D(x, y, z, _volumeData.GridXDim, _volumeData.GridYDim);
                VoxelData voxelData = _denseData.Voxels[index];
                voxelData.Color = color;
                _denseData.Voxels[index] = voxelData;
            }
        }
    }
    
}