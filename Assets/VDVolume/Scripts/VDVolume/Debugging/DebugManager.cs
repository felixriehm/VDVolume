using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.TestTools;
using VDVolume.DataStructure;
using VDVolume.Model;
using VDVolume.UnityComponents;

namespace VDVolume.Debugging
{
    [ExcludeFromCoverage]
    internal static class DebugManager
    {
        internal static void InitRandomDataParallel(VolumeData volumeData)
        {
            GridDataRaw gridData = volumeData.GridDataRaw;
            SparseDataRaw sparseData = volumeData.SparseDataRaw;
            DenseDataRaw denseData = volumeData.DenseDataRaw;
            
            // Dense grid parallel
            NativeArray<JobHandle> denseJobHandles = new NativeArray<JobHandle>(gridData.GlobalChunkCount,Allocator.Temp);
            for (int chunkX = 0; chunkX < gridData.GlobalChunkXDim; chunkX++)
            {
                for (int chunkY = 0; chunkY < gridData.GlobalChunkYDim; chunkY++)
                {
                    for (int chunkZ = 0; chunkZ < gridData.GlobalChunkZDim; chunkZ++)
                    {
                        InitDenseVolumeJob job = new InitDenseVolumeJob(new Vector3Int(chunkX * gridData.LocalChunkDim, chunkY * gridData.LocalChunkDim, chunkZ * gridData.LocalChunkDim),
                            new Vector3Int((chunkX + 1) * gridData.LocalChunkDim, (chunkY + 1) * gridData.LocalChunkDim, (chunkZ + 1) * gridData.LocalChunkDim),denseData, gridData);
                        denseJobHandles[
                            VolumeDataUtil.ChunkCoord.To1D(chunkX, chunkY, chunkZ, gridData.GlobalChunkXDim,
                                gridData.GlobalChunkYDim)] = job.Schedule();
                    }
                }
            }
            JobHandle.CompleteAll(denseJobHandles);
            denseJobHandles.Dispose();

            // Surface voxels are only in a iterative way possible since you cant access NativeMultiHashMap in parallel
            int x = gridData.GridXDim;
            int y = gridData.GridYDim;
            int z = gridData.GridZDim;
            InitSparseVolumeJob surfaceVoxels1 = new InitSparseVolumeJob(new Vector3Int(0, 0, 0), new Vector3Int(x, y, 1),denseData,sparseData,gridData);
            InitSparseVolumeJob surfaceVoxels2 = new InitSparseVolumeJob(new Vector3Int(0, 0, z - 1), new Vector3Int( x, y, z),denseData,sparseData,gridData);
            InitSparseVolumeJob surfaceVoxels3 = new InitSparseVolumeJob(new Vector3Int(0, 0, 1), new Vector3Int( 1, y, z-1),denseData,sparseData,gridData);
            InitSparseVolumeJob surfaceVoxels4 = new InitSparseVolumeJob(new Vector3Int(x - 1, 0, 1), new Vector3Int( x, y, z-1),denseData,sparseData,gridData);
            InitSparseVolumeJob surfaceVoxels5 = new InitSparseVolumeJob(new Vector3Int(1, 0, 1), new Vector3Int( x-1, 1, z-1),denseData,sparseData,gridData);
            InitSparseVolumeJob surfaceVoxels6 = new InitSparseVolumeJob(new Vector3Int(1, y - 1, 1), new Vector3Int( x-1, y, z-1),denseData,sparseData,gridData);
            // iterative 
            JobHandle handle2 = surfaceVoxels1.Schedule();
            handle2.Complete();
            JobHandle handle3 = surfaceVoxels2.Schedule();
            handle3.Complete();
            JobHandle handle4 = surfaceVoxels3.Schedule();
            handle4.Complete();
             JobHandle handle5 = surfaceVoxels4.Schedule();
             handle5.Complete();
             JobHandle handle6 = surfaceVoxels5.Schedule();
             handle6.Complete();
             JobHandle handle7 = surfaceVoxels6.Schedule();
             handle7.Complete();
             
             // parallel
             /*JobHandle handle2 = surfaceVoxels1.Schedule();
             JobHandle handle3 = surfaceVoxels2.Schedule();
             JobHandle handle4 = surfaceVoxels3.Schedule();
             JobHandle handle5 = surfaceVoxels4.Schedule();
             JobHandle handle6 = surfaceVoxels5.Schedule();
             JobHandle handle7 = surfaceVoxels6.Schedule();
             handle6.Complete();
             handle5.Complete();
             handle4.Complete();
             handle3.Complete();
             handle2.Complete();
             handle7.Complete();*/
        }
        
        internal static void InitRandomData(VolumeData volumeData)
        {
            GridDataRaw gridData = volumeData.GridDataRaw;
            SparseDataRaw sparseData = volumeData.SparseDataRaw;
            DenseDataRaw denseData = volumeData.DenseDataRaw;
            
            // Dense grid iterative
            InitDenseVolumeJob insideVoxels = new InitDenseVolumeJob(new Vector3Int(0, 0, 0),
                new Vector3Int(gridData.GridXDim, gridData.GridYDim, gridData.GridZDim), denseData, gridData);
            JobHandle handle1 = insideVoxels.Schedule();
            handle1.Complete();
            
            // Surface voxels are only in a iterative way possible since you cant access NativeMultiHashMap in parallel
            int x = gridData.GridXDim;
            int y = gridData.GridYDim;
            int z = gridData.GridZDim;
            InitSparseVolumeJob surfaceVoxels1 = new InitSparseVolumeJob(new Vector3Int(0, 0, 0), new Vector3Int(x, y, 1),denseData,sparseData,gridData);
            InitSparseVolumeJob surfaceVoxels2 = new InitSparseVolumeJob(new Vector3Int(0, 0, z - 1), new Vector3Int( x, y, z),denseData,sparseData,gridData);
            InitSparseVolumeJob surfaceVoxels3 = new InitSparseVolumeJob(new Vector3Int(0, 0, 1), new Vector3Int( 1, y, z-1),denseData,sparseData,gridData);
            InitSparseVolumeJob surfaceVoxels4 = new InitSparseVolumeJob(new Vector3Int(x - 1, 0, 1), new Vector3Int( x, y, z-1),denseData,sparseData,gridData);
            InitSparseVolumeJob surfaceVoxels5 = new InitSparseVolumeJob(new Vector3Int(1, 0, 1), new Vector3Int( x-1, 1, z-1),denseData,sparseData,gridData);
            InitSparseVolumeJob surfaceVoxels6 = new InitSparseVolumeJob(new Vector3Int(1, y - 1, 1), new Vector3Int( x-1, y, z-1),denseData,sparseData,gridData);
            // iterative 
            JobHandle handle2 = surfaceVoxels1.Schedule();
            handle2.Complete();
            JobHandle handle3 = surfaceVoxels2.Schedule();
            handle3.Complete();
            JobHandle handle4 = surfaceVoxels3.Schedule();
            handle4.Complete();
             JobHandle handle5 = surfaceVoxels4.Schedule();
             handle5.Complete();
             JobHandle handle6 = surfaceVoxels5.Schedule();
             handle6.Complete();
             JobHandle handle7 = surfaceVoxels6.Schedule();
             handle7.Complete();
        }
    }
}