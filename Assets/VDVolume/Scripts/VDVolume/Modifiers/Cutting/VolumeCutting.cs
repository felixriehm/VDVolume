using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Model;
using VDVolume.Modifiers.Undo;

namespace VDVolume.Modifiers.Cutting
{
    /// <summary>
    /// This class provides functionalities to cut voxels from a volume. Cutting means removing voxels from the volume.
    /// </summary>
    public class VolumeCutting
    {
        private VolumeData _volumeData;
        private DenseDataRaw _denseData;
        private GridDataRaw _gridData;
        private SparseDataRaw _sparseData;
        private readonly VoxelCmdBuffer _voxelCmdBuffer;
        internal VolumeCutting(VolumeData volumeData, VoxelCmdBuffer voxelCmdBuffer)
        {
            _volumeData = volumeData;
            _denseData = volumeData.DenseDataRaw;
            _gridData = volumeData.GridDataRaw;
            _sparseData = volumeData.SparseDataRaw;
            _voxelCmdBuffer = voxelCmdBuffer;
        }
        
        /// <summary>
        /// Removes a voxel from the volume.
        /// </summary>
        /// <param name="x">X dimension in index space.</param>
        /// <param name="y">Y dimension in index space.</param>
        /// <param name="z">Z dimension in index space.</param>
        public void RemoveVoxel(int x, int y, int z)
        {
            // Remove voxel
            RemoveVoxelInnerIterative(x,y,z);

            // Process hull
            RemoveVoxelHullIterative(x,y,z);
            
            // Add to undo buffer
            _voxelCmdBuffer.Write(new VoxelCmdData()
            {
                CmdType = VoxelCmdType.Cutting,
                X = x,
                Y = y,
                Z = z
            });
        }
        /// This is copied from the 'VDVoxels' prototype script 'ClickToDestroyPhantom.cs'.
        /// There is actually no center with this method. It will always create an even sphere diameter of voxels.
        /// If the center is 2 and radius 2 in an one dimensional world, it would delete 0,1,2,3, where the
        /// 'center' is '2'.
        /// <summary>
        /// Removes a sphere from the volume.
        /// </summary>
        /// <param name="center">The center of the sphere in index space.</param>
        /// <param name="radius">The radius of the sphere. Radius must be greater than 0.</param>
        public void RemoveSphere(Vector3Int center, int radius)
        {
            if (radius < 1) return;
            
            int rangeSquared = radius * radius;
            
            if (radius == 1)
            {
                RemoveVoxel(center.x,center.y,center.z);
            }
            else
            {
                for(int z = center.z - radius; z < center.z + radius; z++) 
                {
                    for(int y = center.y - radius; y < center.y + radius; y++)
                    {
                        for(int x = center.x - radius; x < center.x + radius; x++)
                        {			
                            int xDistance = x - center.x;
                            int yDistance = y - center.y;
                            int zDistance = z - center.z;
					
                            int distSquared = xDistance * xDistance + yDistance * yDistance + zDistance * zDistance;
                        
                            if(distSquared < rangeSquared)
                            {
                                RemoveVoxel(x,y,z);
                            }
                        }
                    }
                }
            }
        }
        /// This method runs with Unity Jobs and does operations in parallel. A radius of 0 would mean that
        /// only one voxel is cut. While a radius of 2 would remove two more voxels on each side of the center.
        /// Each Unity Job will handle a inner/outer hull layer of the cube.
        /// <summary>
        /// Removes voxels in a cube shaped region from the volume.
        /// </summary>
        /// <param name="center">The center of the cube in index space.</param>
        /// <param name="radius">The radius of the cube.</param>
        public void RemoveCubeParallel(Vector3Int center, int radius)
        {
            NativeArray<int> modifiedChunksFromDenseJob = new NativeArray<int>((radius+1) * ((6*_volumeData.LocalChunkVoxelCount) + 1),Allocator.TempJob);
            
            // Delete voxels
            NativeArray<JobHandle> denseInnerJobHandles = new NativeArray<JobHandle>(radius+1,Allocator.Temp);
            for (int layer = 0; layer < radius+1; layer++)
            {
                RemoveCubeInnerJob job = new RemoveCubeInnerJob(center, layer, modifiedChunksFromDenseJob, _gridData, _denseData);
                denseInnerJobHandles[layer] = job.Schedule();
            }
            RemoveCubeHullJob hullJob = new RemoveCubeHullJob(center, radius+1 , _gridData, _denseData, _sparseData);
            JobHandle hullJobHandle = hullJob.Schedule();
            hullJobHandle.Complete();
            
            // Add modified chunks information from the inner cube job
            for (int layer = 0; layer < radius+1; layer++)
            {
                int itemCounterIndex = layer * (_volumeData.LocalChunkVoxelCount + 1);
                int itemCount = modifiedChunksFromDenseJob[itemCounterIndex];
                
                for (int chunkIndex = 1; chunkIndex < itemCount + 1; chunkIndex++)
                {
                    int chunkPos1D = modifiedChunksFromDenseJob[
                        itemCounterIndex +
                        chunkIndex];
                    
                    _sparseData.TmpModifiedVisibleChunks.TryAdd(chunkPos1D, true);
                    _sparseData.TmpModifiedVisChunksWithRemoval.TryAdd(chunkPos1D, true);
                }
            }
            JobHandle.CompleteAll(denseInnerJobHandles);
            
            // Clean up
            denseInnerJobHandles.Dispose();
            modifiedChunksFromDenseJob.Dispose();
        }

        private void RemoveVoxelInnerIterative(int x, int y, int z)
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
                    _sparseData.TmpModifiedVisibleChunks.TryAdd(chunkIndex, true);
                    _sparseData.TmpModifiedVisChunksWithRemoval.TryAdd(chunkIndex, true);
                    break;
            }
        }

        private void RemoveVoxelHullIterative(int centerX, int centerY, int centerZ)
        {
            CheckNeighbour(centerX - 1, centerY, centerZ);
            CheckNeighbour(centerX + 1, centerY, centerZ);
            CheckNeighbour(centerX, centerY + 1, centerZ);
            CheckNeighbour(centerX, centerY - 1, centerZ);
            CheckNeighbour(centerX, centerY, centerZ + 1);
            CheckNeighbour(centerX, centerY, centerZ - 1);
            
            void CheckNeighbour(int x, int y, int z)
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
}