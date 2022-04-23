using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Model;

namespace VDVolume.Modifiers.Restoring
{
    /// <summary>
    /// Adds voxels to the volume for the outer hull layer of a cube.
    /// </summary>
    internal struct AddCubeHullJob : IJob
    {
        private GridDataRaw _gridData;
        private DenseDataRaw _denseData;
        private SparseDataRaw _sparseData;
        private Vector3Int _center;
        private readonly int _iterationNumber;

        internal AddCubeHullJob(Vector3Int center, int iterationNumber, GridDataRaw gridData,DenseDataRaw denseData,SparseDataRaw sparseData)
        {
            _gridData = gridData;
            _denseData = denseData;
            _sparseData = sparseData;
            _center = center;
            _iterationNumber = iterationNumber;
        }

        public void Execute()
        {
            // front and back side
            for (int x = _center.x - _iterationNumber; x <= _center.x + _iterationNumber; x++)
            {
                for (int y = _center.y - _iterationNumber; y <= _center.y + _iterationNumber; y++)
                {
                    MakeVoxelVisible(x,y,_center.z + _iterationNumber);
                    MakeVoxelVisible(x,y,_center.z - _iterationNumber);
                }
            }
            
            // left and right side
            for (int z = _center.z - _iterationNumber + 1; z <= _center.z + _iterationNumber - 1; z++)
            {
                for (int y = _center.y - _iterationNumber; y <= _center.y + _iterationNumber; y++)
                {
                    MakeVoxelVisible(_center.x + _iterationNumber,y,z);
                    MakeVoxelVisible(_center.x - _iterationNumber,y,z);
                }
            }
            
            // top and bottom side
            for (int z = _center.z - _iterationNumber + 1; z <= _center.z + _iterationNumber - 1; z++)
            {
                for (int x = _center.x - _iterationNumber + 1; x <= _center.x + _iterationNumber - 1; x++)
                {
                    MakeVoxelVisible(x,_center.y + _iterationNumber,z);
                    MakeVoxelVisible(x,_center.y - _iterationNumber,z);
                }
            }
        }

        private void MakeVoxelVisible(int x, int y, int z)
        {
            if (x >= _gridData.GridXDim || x < 0 || y >= _gridData.GridYDim || y < 0 || z >= _gridData.GridZDim || z < 0)
            {
                return;
            }

            int chunkIndex = VolumeDataUtil.GridCoord.To1DGlobalChunkCoord(x, y, z, _gridData.LocalChunkDim, _gridData.GlobalChunkXDim, _gridData.GlobalChunkYDim);
            int voxelIndex = VolumeDataUtil.GridCoord.To1D(x, y, z, _gridData.GridXDim, _gridData.GridYDim);
            VoxelData voxelData = _denseData.Voxels[voxelIndex];
            if (voxelData.State != VoxelState.Visible)
            {
                VoxelData newVoxelData = new VoxelData()
                {
                    Color = voxelData.Color,
                    State = VoxelState.Visible,
                    X = voxelData.X,
                    Y = voxelData.Y,
                    Z = voxelData.Z
                };
                _denseData.Voxels[voxelIndex] = newVoxelData;

                _sparseData.DirtyChunkVisibleVoxels.Add(chunkIndex, newVoxelData);
                _sparseData.TmpModifiedVisibleChunks.TryAdd(chunkIndex, true);
                _sparseData.VisibleChunks.TryAdd(chunkIndex, true);
            }
        }
    }
}