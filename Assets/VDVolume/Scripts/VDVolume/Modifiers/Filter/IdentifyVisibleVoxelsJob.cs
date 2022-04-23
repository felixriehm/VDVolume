using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Model;

namespace VDVolume.Modifiers.Filter
{
    /// <summary>
    /// This Job will identify missing visible voxels of the volume. Missing visible voxels are solid voxel which have
    /// a removed voxel as neighbor but is not marked as visible yet.
    /// </summary>
    internal struct IdentifyVisibleVoxelsJob : IJob
    {
        private Vector3Int _start;
        private Vector3Int _end;
        private GridDataRaw _gridData;
        private DenseDataRaw _denseData;
        private int _chunkIndex;
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<int> _addedSurfaceVoxels;

        internal IdentifyVisibleVoxelsJob(Vector3Int start, Vector3Int end, GridDataRaw gridData, DenseDataRaw denseData, int chunkIndex, NativeArray<int> addedSurfaceVoxels)
        {
            _start = start;
            _end = end;
            _gridData = gridData;
            _denseData = denseData;
            _addedSurfaceVoxels = addedSurfaceVoxels;
            _chunkIndex = chunkIndex;
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

                        VoxelData thisVoxel = GetVoxel(x, y, z);
                        if (thisVoxel.State == VoxelState.Solid)
                        {
                            VoxelData leftVoxel = GetVoxel(x-1, y, z);
                            VoxelData rightVoxel = GetVoxel(x+1, y, z);
                            VoxelData topVoxel = GetVoxel(x, y+1, z);
                            VoxelData bottomVoxel = GetVoxel(x, y-1, z);
                            VoxelData frontVoxel = GetVoxel(x, y, z+1);
                            VoxelData backVoxel = GetVoxel(x, y, z-1);
                            if (leftVoxel.State == VoxelState.Removed ||
                                rightVoxel.State == VoxelState.Removed ||
                                topVoxel.State == VoxelState.Removed ||
                                bottomVoxel.State == VoxelState.Removed ||
                                frontVoxel.State == VoxelState.Removed ||
                                backVoxel.State == VoxelState.Removed)
                            {
                                int itemCounterIndex = _chunkIndex * (_gridData.LocalChunkVoxelCount + 1);
                                int itemCount = _addedSurfaceVoxels[itemCounterIndex];

                                _addedSurfaceVoxels[itemCounterIndex + itemCount + 1] = VolumeDataUtil.GridCoord.To1D(x,y,z,_gridData.GridXDim, _gridData.GridYDim);
                                _addedSurfaceVoxels[itemCounterIndex] = itemCount+1;
                            }
                        }
                    }   
                }
            }
        }
        
        private VoxelData GetVoxel(int x, int y, int z)
        {
            if (x >= _gridData.GridXDim || x < 0 || y >= _gridData.GridYDim || y < 0 || z >= _gridData.GridZDim || z < 0)
            {
                return new VoxelData();
            }
            return _denseData.Voxels[VolumeDataUtil.GridCoord.To1D(x, y, z,_gridData.GridXDim,_gridData.GridYDim)];
        }
    }
}