using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Model;

namespace VDVolume.Modifiers.Filter
{
    /// <summary>
    /// For a single color value, this job will set 'visible' and 'solid' voxels in the dense storage to 'removed'.
    /// </summary>
    internal struct FilterValueJob : IJob
    {
        private Vector3Int _start;
        private Vector3Int _end;
        private GridDataRaw _gridData;
        private DenseDataRaw _denseData;
        private int _filterValue;
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<bool> _modifiedChunks;
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<bool> _modifiedChunksWithSurfaceRemoval;
        private int _chunkIndex;

        internal FilterValueJob(Vector3Int start, Vector3Int end, GridDataRaw gridData, DenseDataRaw denseData, int filterValue, int chunkIndex, NativeArray<bool> modifiedChunks,NativeArray<bool> modifiedChunksWithSurfaceRemoval)
        {
            _start = start;
            _end = end;
            _gridData = gridData;
            _denseData = denseData;
            _filterValue = filterValue;
            _modifiedChunks = modifiedChunks;
            _chunkIndex = chunkIndex;
            _modifiedChunksWithSurfaceRemoval = modifiedChunksWithSurfaceRemoval;
        }

        public void Execute()
        {
            for (int x = _start.x; x < _end.x; x++)
            {
                if (x >= _gridData.GridXDim)
                {
                    continue;
                }
                for (int y = _start.y; y < _end.y; y++)
                {
                    if (y >= _gridData.GridYDim)
                    {
                        continue;
                    }
                    for (int z = _start.z; z < _end.z; z++)
                    {
                        if (z >= _gridData.GridZDim)
                        {
                            continue;
                        }

                        // get voxel
                        VoxelData voxelData = _denseData.Voxels[VolumeDataUtil.GridCoord.To1D(x, y, z,_gridData.GridXDim,_gridData.GridYDim)];
                        // check if value should be filtered
                        if (voxelData.Color == _filterValue)
                        {
                            _modifiedChunks[_chunkIndex] = true;
                            if (voxelData.State == VoxelState.Visible)
                            {
                                _modifiedChunksWithSurfaceRemoval[_chunkIndex] = true;
                            }
                            // set voxel state
                            int index = VolumeDataUtil.GridCoord.To1D(x, y, z, _gridData.GridXDim, _gridData.GridYDim);
                            VoxelData voxelDataCopy = _denseData.Voxels[index];
                            voxelDataCopy.State = VoxelState.Removed;
                            _denseData.Voxels[index] = voxelDataCopy;

                            int leftVoxelChunkIndex = VolumeDataUtil.GridCoord.To1DGlobalChunkCoord(x-1, y, z,
                                _gridData.LocalChunkDim, _gridData.GlobalChunkXDim, _gridData.GlobalChunkYDim);
                            int rightVoxelChunkIndex = VolumeDataUtil.GridCoord.To1DGlobalChunkCoord(x+1, y, z,
                                _gridData.LocalChunkDim, _gridData.GlobalChunkXDim, _gridData.GlobalChunkYDim);
                            int topVoxelChunkIndex= VolumeDataUtil.GridCoord.To1DGlobalChunkCoord(x, y+1, z,
                                _gridData.LocalChunkDim, _gridData.GlobalChunkXDim, _gridData.GlobalChunkYDim);
                            int bottomVoxelChunkIndex = VolumeDataUtil.GridCoord.To1DGlobalChunkCoord(x, y-1, z,
                                _gridData.LocalChunkDim, _gridData.GlobalChunkXDim, _gridData.GlobalChunkYDim);
                            int frontVoxelChunkIndex = VolumeDataUtil.GridCoord.To1DGlobalChunkCoord(x, y, z+1,
                                _gridData.LocalChunkDim, _gridData.GlobalChunkXDim, _gridData.GlobalChunkYDim);
                            int backVoxelChunkIndex = VolumeDataUtil.GridCoord.To1DGlobalChunkCoord(x, y, z-1,
                                _gridData.LocalChunkDim, _gridData.GlobalChunkXDim, _gridData.GlobalChunkYDim);

                            // this needs to be done because of the neighborhood cases where a removed voxel is
                            // on the edge of another chunk and a surface voxel on the other chunks has to be added
                            // (which happens in 'RebuildVisibleVoxelsJob-cs'). We predefine the modified value here.
                            if (leftVoxelChunkIndex != -1 && leftVoxelChunkIndex != _chunkIndex)
                                _modifiedChunks[leftVoxelChunkIndex] = true;
                            if (rightVoxelChunkIndex != -1 && rightVoxelChunkIndex != _chunkIndex)
                                _modifiedChunks[rightVoxelChunkIndex] = true;
                            if (topVoxelChunkIndex != -1 && topVoxelChunkIndex != _chunkIndex)
                                _modifiedChunks[topVoxelChunkIndex] = true;
                            if (bottomVoxelChunkIndex != -1 && bottomVoxelChunkIndex != _chunkIndex)
                                _modifiedChunks[bottomVoxelChunkIndex] = true;
                            if (frontVoxelChunkIndex != -1 && frontVoxelChunkIndex != _chunkIndex)
                                _modifiedChunks[frontVoxelChunkIndex] = true;
                            if (backVoxelChunkIndex != -1 && backVoxelChunkIndex != _chunkIndex)
                                _modifiedChunks[backVoxelChunkIndex] = true;
                        }
                    }   
                }
            }
        }
    }
}