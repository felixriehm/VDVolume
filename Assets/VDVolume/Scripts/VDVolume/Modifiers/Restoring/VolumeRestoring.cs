using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Model;
using VDVolume.Modifiers.Filter;
using VDVolume.Modifiers.Undo;

namespace VDVolume.Modifiers.Restoring
{
    /// <summary>
    /// This class provides functions for restoring voxels which were earlier cut from the volume. Restoring means
    /// adding voxels back to the volume.
    /// </summary>
    public class VolumeRestoring
    {
        private VolumeData _volumeData;
        private DenseDataRaw _denseData;
        private GridDataRaw _gridData;
        private SparseDataRaw _sparseData;
        private readonly VoxelCmdBuffer _voxelCmdBuffer;
        
        internal VolumeRestoring(VolumeData volumeData, VoxelCmdBuffer voxelCmdBuffer)
        {
            _volumeData = volumeData;
            _denseData = volumeData.DenseDataRaw;
            _gridData = volumeData.GridDataRaw;
            _sparseData = volumeData.SparseDataRaw;
            _voxelCmdBuffer = voxelCmdBuffer;
        }

        /// <summary>
        /// Adds a previously cut voxel back.
        /// </summary>
        /// <param name="x">X dimension in index space.</param>
        /// <param name="y">Y dimension in index space.</param>
        /// <param name="z">Z dimension in index space.</param>
        public void AddVoxel(int x, int y, int z)
        {
            // Add voxel
            if (x >= _gridData.GridXDim || x < 0 || y >= _gridData.GridYDim || y < 0 || z >= _gridData.GridZDim || z < 0)
            {
                return;
            }

            int chunkIndex = VolumeDataUtil.GridCoord.To1DGlobalChunkCoord(x, y, z, _gridData.LocalChunkDim, _gridData.GlobalChunkXDim, _gridData.GlobalChunkYDim);
            int voxelIndex = VolumeDataUtil.GridCoord.To1D(x, y, z, _gridData.GridXDim, _gridData.GridYDim);
            VoxelData voxelData = _denseData.Voxels[voxelIndex];

            // In advance set this voxels state state to inside so that the neighbourhood check with IsRemovedAndProcess
            // sees this voxel as solid. Also if all voxels around are solid, this voxel will just be
            // a solid voxel without converting it to a surface voxel
            if (voxelData.State != VoxelState.Visible)
            {
                voxelData.State = VoxelState.Solid;
                _denseData.Voxels[voxelIndex] = voxelData;
            }

            bool leftVoxelRemoved = IsRemovedAndProcess(x - 1, y, z);
            bool rightVoxelRemoved = IsRemovedAndProcess(x + 1, y, z);
            bool topVoxelRemoved = IsRemovedAndProcess(x, y + 1, z);
            bool bottomVoxelRemoved = IsRemovedAndProcess(x, y - 1, z);
            bool frontVoxelRemoved = IsRemovedAndProcess(x, y, z + 1);
            bool backVoxelRemoved = IsRemovedAndProcess(x, y, z - 1);
            
            if (leftVoxelRemoved || rightVoxelRemoved || topVoxelRemoved || bottomVoxelRemoved || 
                frontVoxelRemoved || backVoxelRemoved)
            {
                // Only make it a visible voxel when one of the neighbour voxels is removed
                voxelData.State = VoxelState.Visible;
                _sparseData.DirtyChunkVisibleVoxels.Add(chunkIndex, voxelData);
                _sparseData.TmpModifiedVisibleChunks.TryAdd(chunkIndex, true);
                _sparseData.VisibleChunks.TryAdd(chunkIndex, true);
                
            }
            else
            {
                if (voxelData.State == VoxelState.Visible )
                {
                    // All neighbour voxels are solid or visible. Remove this visible voxel.
                    voxelData.State = VoxelState.Solid;
                    _sparseData.TmpModifiedVisibleChunks.TryAdd(chunkIndex, true);
                    _sparseData.TmpModifiedVisChunksWithRemoval.TryAdd(chunkIndex, true);
                }
            }
            
            _denseData.Voxels[voxelIndex] = voxelData;

            // Add to undo buffer
            _voxelCmdBuffer.Write(new VoxelCmdData()
            {
                CmdType = VoxelCmdType.Restoring,
                X = voxelData.X,
                Y = voxelData.Y,
                Z = voxelData.Z
            });

            bool IsRemovedAndProcess(int neighbourX, int neighbourY, int neighbourZ)
            {
                if (neighbourX >= _gridData.GridXDim || neighbourX < 0 || neighbourY >= _gridData.GridYDim || neighbourY < 0 || neighbourZ >= _gridData.GridZDim || neighbourZ < 0)
                {
                    return true;
                }
                
                int nbChunkIndex = VolumeDataUtil.GridCoord.To1DGlobalChunkCoord(neighbourX, neighbourY, neighbourZ, _gridData.LocalChunkDim, _gridData.GlobalChunkXDim, _gridData.GlobalChunkYDim);
                int nbVoxelIndex = VolumeDataUtil.GridCoord.To1D(neighbourX, neighbourY, neighbourZ, _gridData.GridXDim, _gridData.GridYDim);
                VoxelData nbVoxelData = _denseData.Voxels[nbVoxelIndex];
                
                // if this voxel is visible and all neighbours are visible or solid voxels than delete his visible voxel
                if (nbVoxelData.State == VoxelState.Visible)
                {
                    if ( IsRemoved(neighbourX - 1, neighbourY, neighbourZ) ||
                         IsRemoved(neighbourX + 1, neighbourY, neighbourZ) ||
                         IsRemoved(neighbourX, neighbourY + 1, neighbourZ) ||
                         IsRemoved(neighbourX, neighbourY - 1, neighbourZ) ||
                         IsRemoved(neighbourX, neighbourY, neighbourZ + 1) ||
                         IsRemoved(neighbourX, neighbourY, neighbourZ - 1))
                    {
                        return false;
                    }
                    // remove visible voxel
                    nbVoxelData.State = VoxelState.Solid;
                    _denseData.Voxels[nbVoxelIndex] = nbVoxelData;
                    _sparseData.TmpModifiedVisibleChunks.TryAdd(nbChunkIndex, true);
                    _sparseData.TmpModifiedVisChunksWithRemoval.TryAdd(nbChunkIndex, true);
                }

                return nbVoxelData.State == VoxelState.Removed;

                bool IsRemoved(int neighbour2X, int neighbour2Y, int neighbour2Z)
                {
                    if (neighbour2X >= _gridData.GridXDim || neighbour2X < 0 || neighbour2Y >= _gridData.GridYDim || neighbour2Y < 0 || neighbour2Z >= _gridData.GridZDim || neighbour2Z < 0)
                    {
                        return true;
                    }
                    
                    int nb2VoxelIndex = VolumeDataUtil.GridCoord.To1D(neighbour2X, neighbour2Y, neighbour2Z, _gridData.GridXDim, _gridData.GridYDim);
                    return _denseData.Voxels[nb2VoxelIndex].State == VoxelState.Removed;
                }
            }
        }

        /// This is copied from the 'VDVoxels' prototype script 'ClickToDestroyPhantom.cs'.
        /// There is actually no center with this method. It will always create an even sphere diameter of voxels.
        /// If the center is 2 and radius 2 in an one dimensional world, it would add 0,1,2,3, where the
        /// 'center' is '2'.
        /// <summary>
        /// Adds a sphere from the volume.
        /// </summary>
        /// <param name="center">The center of the sphere in index space.</param>
        /// <param name="radius">The radius of the sphere. Radius must be greater than 0.</param>
        public void AddSphere(Vector3Int center, int radius)
        {
            if (radius < 1) return;
            
            int rangeSquared = radius * radius;
            
            if (radius == 1)
            {
                AddVoxel(center.x,center.y,center.z);
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
                                AddVoxel(x,y,z);
                            }
                        }
                    }
                }
            }
        }

        /// This methods runs with a Unity Job and does operations in parallel. A radius of 0 would mean that
        /// only one voxel is restored. While a radius of 2 would remove two more voxels on each side of the center.
        /// Each Unity Job will handle a inner/outer hull layer of the cube.
        /// <summary>
        /// Adds previously cut voxels back. The region for adding is defined as a cube.
        /// </summary>
        /// <param name="center">The center of the cube in index space.</param>
        /// <param name="radius">The radius of the cube.</param>
        public void AddCubeParallel(Vector3Int center, int radius)
        {
            NativeArray<int> modifiedChunksFromDenseJob = new NativeArray<int>(radius * (_volumeData.LocalChunkVoxelCount + 1),Allocator.TempJob);
            
            // Restore voxels
            NativeArray<JobHandle> denseInnerJobHandles = new NativeArray<JobHandle>(radius,Allocator.Temp);
            for (int layer = 0; layer < radius; layer++)
            {
                AddCubeInnerJob job = new AddCubeInnerJob(center, layer, modifiedChunksFromDenseJob, _gridData, _denseData);
                denseInnerJobHandles[layer] = job.Schedule();
            }
            AddCubeHullJob hullJob = new AddCubeHullJob(center, radius , _gridData, _denseData, _sparseData);
            JobHandle hullJobHandle = hullJob.Schedule();
            hullJobHandle.Complete();
            
            // Add modified chunks information from the inner cube job
            for (int layer = 0; layer < radius; layer++)
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
    }
}