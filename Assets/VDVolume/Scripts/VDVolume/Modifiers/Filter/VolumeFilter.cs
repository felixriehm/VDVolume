using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Model;

namespace VDVolume.Modifiers.Filter
{
    /// <summary>
    /// Provides functionalities for filtering a volume.
    /// </summary>
    public class VolumeFilter
    {
        /// <summary>
        /// The range start value of the last range filter operation.
        /// </summary>
        public int LastFilterRangeBegin { get; private set; }
        /// <summary>
        /// The range end value of the last range filter operation.
        /// </summary>
        public int LastFilterRangeEnd { get; private set;}
        /// <summary>
        /// The value of the last single value filter operation.
        /// </summary>
        public int LastFilterValue { get; private set;}
        
        private VolumeData _volumeData;
        private DenseDataRaw _denseData;
        private SparseDataRaw _sparseData;
        private GridDataRaw _gridData;

        internal VolumeFilter(VolumeData volumeData)
        {
            _volumeData = volumeData;
            _denseData = _volumeData.DenseDataRaw;
            _sparseData = _volumeData.SparseDataRaw;
            _gridData = _volumeData.GridDataRaw;
        }
        /// The method will group operations in chunks and run them in parallel with Unity Jobs.
        /// <summary>
        /// Removes voxels within a color value range.
        /// </summary>
        /// <param name="rangeBeginIncl">The start value of the color value range.</param>
        /// <param name="rangeEndIncl">The end value of the color value range.</param>
        public void FilterValueRange(int rangeBeginIncl, int rangeEndIncl)
        {
            LastFilterRangeBegin = rangeBeginIncl;
            LastFilterRangeEnd = rangeEndIncl;
            // Filter values
            // this will contain chunks where voxels are removed and where later surface voxel will be added
            NativeArray<bool> modifiedChunks = new NativeArray<bool>(_volumeData.GlobalChunkCount,Allocator.TempJob);
            NativeArray<bool> modifiedChunksWithSurfaceRemoval = new NativeArray<bool>(_volumeData.GlobalChunkCount,Allocator.TempJob);
            NativeArray<JobHandle> filterJobHandles = new NativeArray<JobHandle>(_volumeData.GlobalChunkCount,Allocator.Temp);
            for (int chunkX = 0; chunkX < _volumeData.GlobalChunkXDim; chunkX++)
            {
                for (int chunkY = 0; chunkY < _volumeData.GlobalChunkYDim; chunkY++)
                {
                    for (int chunkZ = 0; chunkZ < _volumeData.GlobalChunkZDim; chunkZ++)
                    {
                        int chunkIndex = VolumeDataUtil.ChunkCoord.To1D(chunkX, chunkY, chunkZ,
                            _volumeData.GlobalChunkXDim,
                            _volumeData.GlobalChunkYDim);
                        FilterValueRangeJob job = new FilterValueRangeJob(new Vector3Int(chunkX * _volumeData.LocalChunkDim, chunkY * _volumeData.LocalChunkDim, chunkZ * _volumeData.LocalChunkDim),
                            new Vector3Int((chunkX + 1) * _volumeData.LocalChunkDim, (chunkY + 1) * _volumeData.LocalChunkDim, (chunkZ + 1) * _volumeData.LocalChunkDim),_gridData,_denseData, rangeBeginIncl, rangeEndIncl, chunkIndex, modifiedChunks, modifiedChunksWithSurfaceRemoval);
                        filterJobHandles[chunkIndex] = job.Schedule();
                    }
                }
            }
            JobHandle.CompleteAll(filterJobHandles);
            filterJobHandles.Dispose();
            
            RebuildVisibleVoxels(modifiedChunks, modifiedChunksWithSurfaceRemoval);
            
            modifiedChunks.Dispose();
            modifiedChunksWithSurfaceRemoval.Dispose();
        }
        /// The method will group operations in chunks and run them in parallel with Unity Jobs.
        /// <summary>
        /// Removes voxels which have the same color value as the provided value.
        /// </summary>
        /// <param name="value">The color value to be removed from the volume.</param>
        public void FilterValue(int value)
        {
            LastFilterValue = value;
            // Filter values
            // this will contain chunks where voxels are removed and where later surface voxel will be added
            NativeArray<bool> modifiedChunks = new NativeArray<bool>(_volumeData.GlobalChunkCount,Allocator.TempJob);
            NativeArray<bool> modifiedChunksWithSurfaceRemoval = new NativeArray<bool>(_volumeData.GlobalChunkCount,Allocator.TempJob);
            NativeArray<JobHandle> filterJobHandles = new NativeArray<JobHandle>(_volumeData.GlobalChunkCount,Allocator.Temp);
            for (int chunkX = 0; chunkX < _volumeData.GlobalChunkXDim; chunkX++)
            {
                for (int chunkY = 0; chunkY < _volumeData.GlobalChunkYDim; chunkY++)
                {
                    for (int chunkZ = 0; chunkZ < _volumeData.GlobalChunkZDim; chunkZ++)
                    {
                        int chunkIndex = VolumeDataUtil.ChunkCoord.To1D(chunkX, chunkY, chunkZ,
                            _volumeData.GlobalChunkXDim,
                            _volumeData.GlobalChunkYDim);
                        FilterValueJob job = new FilterValueJob(new Vector3Int(chunkX * _volumeData.LocalChunkDim, chunkY * _volumeData.LocalChunkDim, chunkZ * _volumeData.LocalChunkDim),
                            new Vector3Int((chunkX + 1) * _volumeData.LocalChunkDim, (chunkY + 1) * _volumeData.LocalChunkDim, (chunkZ + 1) * _volumeData.LocalChunkDim),_gridData,_denseData, value, chunkIndex, modifiedChunks, modifiedChunksWithSurfaceRemoval);
                        filterJobHandles[chunkIndex] = job.Schedule();
                    }
                }
            }
            JobHandle.CompleteAll(filterJobHandles);
            filterJobHandles.Dispose();
            RebuildVisibleVoxels(modifiedChunks, modifiedChunksWithSurfaceRemoval);
            
            modifiedChunks.Dispose();
            modifiedChunksWithSurfaceRemoval.Dispose();
        }

        internal void RebuildVisibleVoxels(NativeArray<bool> modifiedChunks, NativeArray<bool> modifiedChunksWithSurfaceRemoval)
        {
            // Identifying visible voxels
            NativeArray<JobHandle> rebuildJobHandles = new NativeArray<JobHandle>(_volumeData.GlobalChunkCount,Allocator.Temp);
            NativeList<int> modifiedChunksAsList = new NativeList<int>(_volumeData.GlobalChunkCount,Allocator.Temp);
            NativeArray<int> addedSurfaceVoxels = new NativeArray<int>(_volumeData.GlobalChunkCount * (_volumeData.LocalChunkVoxelCount + 1),Allocator.TempJob);;
            for (int chunkIndex1D = 0; chunkIndex1D < modifiedChunks.Length; chunkIndex1D++)
            {
                if (modifiedChunks[chunkIndex1D])
                { 
                    modifiedChunksAsList.Add(chunkIndex1D);

                    Vector3Int chunkIndex = VolumeDataUtil.ChunkCoord.To3D(chunkIndex1D, _volumeData.GlobalChunkXDim, _volumeData.GlobalChunkYDim);
                    IdentifyVisibleVoxelsJob job = new IdentifyVisibleVoxelsJob(new Vector3Int(chunkIndex.x * _volumeData.LocalChunkDim,
                        chunkIndex.y * _volumeData.LocalChunkDim, chunkIndex.z * _volumeData.LocalChunkDim), new Vector3Int(
                        (chunkIndex.x + 1) * _volumeData.LocalChunkDim, (chunkIndex.y + 1) * _volumeData.LocalChunkDim,
                        (chunkIndex.z + 1) * _volumeData.LocalChunkDim),_gridData,_denseData, chunkIndex1D, addedSurfaceVoxels);
                    rebuildJobHandles[chunkIndex1D] = job.Schedule();
                }
            }
            JobHandle.CompleteAll(rebuildJobHandles);
            rebuildJobHandles.Dispose();
            
            // Add surface voxels
            for (int i = 0; i < modifiedChunksAsList.Length; i++)
            {
                int modifiedChunk1DIndex = modifiedChunksAsList[i];
                int itemCounterIndex = modifiedChunk1DIndex * (_volumeData.LocalChunkVoxelCount + 1);
                int itemCount = addedSurfaceVoxels[itemCounterIndex];
                for (int voxelIndex = 1; voxelIndex < itemCount + 1; voxelIndex++)
                {
                    int voxelPos1D = addedSurfaceVoxels[
                        itemCounterIndex +
                        voxelIndex];
                    Vector3Int voxelPos = VolumeDataUtil.GridCoord.To3D(voxelPos1D, _volumeData.GridXDim, _volumeData.GridYDim);
                    
                    // dense storage
                    VoxelData voxelDataCopy = _denseData.Voxels[voxelPos1D];
                    voxelDataCopy.State = VoxelState.Visible;
                    _denseData.Voxels[voxelPos1D] = voxelDataCopy;
                    // sparse storage
                    int chunkIndex = VolumeDataUtil.GridCoord.To1DGlobalChunkCoord(voxelPos.x, voxelPos.y, voxelPos.z, _volumeData.LocalChunkDim, _volumeData.GlobalChunkXDim, _volumeData.GlobalChunkYDim);
                    _sparseData.DirtyChunkVisibleVoxels.Add(chunkIndex, voxelDataCopy);
                    _sparseData.TmpModifiedVisibleChunks.TryAdd(chunkIndex, true);
                    _sparseData.VisibleChunks.TryAdd(chunkIndex, true);
                }
                
                if (modifiedChunksWithSurfaceRemoval[modifiedChunk1DIndex])
                {
                    _sparseData.TmpModifiedVisibleChunks.TryAdd(modifiedChunk1DIndex, true);
                    _sparseData.TmpModifiedVisChunksWithRemoval.TryAdd(modifiedChunk1DIndex, true);
                }
            }

            modifiedChunksAsList.Dispose();
            addedSurfaceVoxels.Dispose();
        }
    }
}