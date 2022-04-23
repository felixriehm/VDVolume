using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Model;

namespace VDVolume.Modifiers.Cutting
{
    /// <summary>
    /// Removes voxels from the volume for a inner hull layer of a cube by setting 'visible' and 'solid' voxels in the
    /// dense storage to 'removed'.
    /// </summary>
    internal struct RemoveCubeInnerJob : IJob
    {
        private GridDataRaw _gridData;
        private DenseDataRaw _denseData;
        private Vector3Int _center;
        private readonly int _iterationNumber;
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<int> _modifiedChunksFromDenseJob;


        internal RemoveCubeInnerJob(Vector3Int center, int iterationNumber,NativeArray<int> modifiedChunksFromDenseJob, GridDataRaw gridData,DenseDataRaw denseData)
        {
            _gridData = gridData;
            _denseData = denseData;
            _center = center;
            _iterationNumber = iterationNumber;
            _modifiedChunksFromDenseJob = modifiedChunksFromDenseJob;
        }

        public void Execute()
        {
            // front and back side
            for (int x = _center.x - _iterationNumber; x <= _center.x + _iterationNumber; x++)
            {
                for (int y = _center.y - _iterationNumber; y <= _center.y + _iterationNumber; y++)
                {
                    RemoveVoxel(x,y,_center.z + _iterationNumber);
                    RemoveVoxel(x,y,_center.z - _iterationNumber);
                }
            }
            
            // left and right side
            for (int z = _center.z - _iterationNumber + 1; z <= _center.z + _iterationNumber - 1; z++)
            {
                for (int y = _center.y - _iterationNumber; y <= _center.y + _iterationNumber; y++)
                {
                    RemoveVoxel(_center.x + _iterationNumber,y,z);
                    RemoveVoxel(_center.x - _iterationNumber,y,z);
                }
            }
            
            // top and bottom side
            for (int z = _center.z - _iterationNumber + 1; z <= _center.z + _iterationNumber - 1; z++)
            {
                for (int x = _center.x - _iterationNumber + 1; x <= _center.y + _iterationNumber - 1; x++)
                {
                    RemoveVoxel(x,_center.y + _iterationNumber,z);
                    RemoveVoxel(x,_center.y - _iterationNumber,z);
                }
            }
        }
        
        private void RemoveVoxel(int x, int y, int z)
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
                    _denseData.Voxels[voxelIndex] = new VoxelData()
                    {
                        Color = voxelData.Color,
                        State = VoxelState.Removed,
                        X = voxelData.X,
                        Y = voxelData.Y,
                        Z = voxelData.Z
                    };
                    break;
                case VoxelState.Visible:
                    _denseData.Voxels[voxelIndex] = new VoxelData()
                    {
                        Color = voxelData.Color,
                        State = VoxelState.Removed,
                        X = voxelData.X,
                        Y = voxelData.Y,
                        Z = voxelData.Z
                    };
                    int itemCounterIndex = _iterationNumber * ((6*_gridData.LocalChunkVoxelCount) + 1);
                    int itemCount = _modifiedChunksFromDenseJob[itemCounterIndex];
                    
                    _modifiedChunksFromDenseJob[itemCounterIndex + itemCount + 1] = chunkIndex;
                    _modifiedChunksFromDenseJob[itemCounterIndex] = itemCount + 1;
                    break;
            }
        }
    }
}