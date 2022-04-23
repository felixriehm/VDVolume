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
    internal struct InitSparseVolumeJob : IJob
    {
        private Vector3Int _start;
        private Vector3Int _end;
        private DenseDataRaw _denseData;
        private SparseDataRaw _sparseData;
        private GridDataRaw _gridData;

        internal InitSparseVolumeJob(Vector3Int start, Vector3Int end, DenseDataRaw denseData, SparseDataRaw sparseData, GridDataRaw gridData)
        {
            _start = start;
            _end = end;
            _denseData = denseData;
            _sparseData = sparseData;
            _gridData = gridData;
        }

        public void Execute()
        {
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(15);
            for (int x = _start.x; x < _end.x; x++)
            {
                for (int y = _start.y; y < _end.y; y++)
                {
                    for (int z = _start.z; z < _end.z; z++)
                    {
                        // Instructions copied from VolumeData methods since references to objects are not allowed inside Jobs
                        // Set dense voxel
                        int index = VolumeDataUtil.GridCoord.To1D(x, y, z, _gridData.GridXDim, _gridData.GridYDim);
                        VoxelData voxelDataCopy = _denseData.Voxels[index];
                        voxelDataCopy.X = x;
                        voxelDataCopy.Y = y;
                        voxelDataCopy.Z = z;
                        voxelDataCopy.State = VoxelState.Visible;
                        voxelDataCopy.Color = (byte) random.NextInt(0, 255);
                        _denseData.Voxels[index] = voxelDataCopy;

                        // Add surface voxel
                        int chunkIndex = VolumeDataUtil.GridCoord.To1DGlobalChunkCoord(x, y, z, _gridData.LocalChunkDim, _gridData.GlobalChunkXDim, _gridData.GlobalChunkYDim);
                        _sparseData.DirtyChunkVisibleVoxels.Add(chunkIndex, voxelDataCopy);
                        _sparseData.TmpModifiedVisibleChunks.TryAdd(chunkIndex, true);
                        _sparseData.VisibleChunks.TryAdd(chunkIndex,
                            true);
                    }   
                }
            }
        }
    }
}