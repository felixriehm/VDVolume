using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.TestTools;
using VDVolume.DataStructure;
using VDVolume.Model;
using Random = UnityEngine.Random;

namespace VDVolume.Debugging
{
    [ExcludeFromCoverage]
    internal struct InitDenseVolumeJob : IJob
    {
        private Vector3Int _start;
        private Vector3Int _end;
        private DenseDataRaw _denseData;
        private GridDataRaw _gridData;

        internal InitDenseVolumeJob(Vector3Int start, Vector3Int end, DenseDataRaw denseData, GridDataRaw gridData)
        {
            _start = start;
            _end = end;
            _denseData = denseData;
            _gridData = gridData;
        }

        public void Execute()
        {
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(15);
            
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
                        // Instructions copied from VolumeData methods since references to objects are not allowed inside Jobs
                        // Set dense voxel
                        int index = VolumeDataUtil.GridCoord.To1D(x, y, z, _gridData.GridXDim, _gridData.GridYDim);
                        VoxelData voxelDataCopy = _denseData.Voxels[index];
                        voxelDataCopy.X = x;
                        voxelDataCopy.Y = y;
                        voxelDataCopy.Z = z;
                        voxelDataCopy.State = VoxelState.Solid;
                        voxelDataCopy.Color = (byte) random.NextInt(0, 255);
                        _denseData.Voxels[index] = voxelDataCopy;
                    }   
                }
            }
        }
    }
}