using Unity.Jobs;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Model;

namespace VDVolume.Modifiers.Cutting
{
    /// <summary>
    /// This Job makes the voxels of the outer hull of a cube visible.
    /// </summary>
    internal struct RemoveCubeHullJob : IJob
    {
        private GridDataRaw _gridData;
        private DenseDataRaw _denseData;
        private SparseDataRaw _sparseData;
        private Vector3Int _center;
        private readonly int _iterationNumber;

        internal RemoveCubeHullJob(Vector3Int center, int iterationNumber,GridDataRaw gridData,DenseDataRaw denseData,SparseDataRaw sparseData)
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
            switch (voxelData.State)
            {
                case VoxelState.Solid:
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
                    break;
            }
        }
    }
}