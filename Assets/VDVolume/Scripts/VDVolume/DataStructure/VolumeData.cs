using System;
using System.Text;
using Unity.Collections;
using UnityEngine;
using VDVolume.Model;

namespace VDVolume.DataStructure
{
    /// <summary>
    /// Holds and manages the volume data.
    /// </summary>
    public partial class VolumeData
    {
        /// <summary>
        /// The x dimension of the voxel grid.
        /// </summary>
        public int GridXDim => _gridData.GridXDim;
        /// <summary>
        /// The y dimension of the voxel grid.
        /// </summary>
        public int GridYDim => _gridData.GridYDim;
        /// <summary>
        /// The z dimension of the voxel grid.
        /// </summary>
        public int GridZDim => _gridData.GridZDim;
        /// <summary>
        /// The amount of voxels (all voxel states considered) inside the volume.
        /// </summary>
        public int GridVoxelCount => _gridData.GridVoxelCount;
        /// <summary>
        /// The x dimension of the chunk grid.
        /// </summary>
        public int GlobalChunkXDim => _gridData.GlobalChunkXDim;
        /// <summary>
        /// The y dimension of the chunk grid.
        /// </summary>
        public int GlobalChunkYDim => _gridData.GlobalChunkYDim;
        /// <summary>
        /// The z dimension of the chunk grid.
        /// </summary>
        public int GlobalChunkZDim => _gridData.GlobalChunkZDim;
        /// <summary>
        /// The amount of chunks inside the volume.
        /// </summary>
        public int GlobalChunkCount => _gridData.GlobalChunkCount;
        /// <summary>
        /// The dimension (in voxel count) of one axis of a chunk. All dimensions of a chunks are the same. 
        /// </summary>
        public int LocalChunkDim => _gridData.LocalChunkDim;
        /// <summary>
        /// The amount of voxels (all voxel states considered) inside a chunk.
        /// </summary>
        public int LocalChunkVoxelCount => _gridData.LocalChunkVoxelCount;
        /// <summary>
        /// Scale of the volume.
        /// </summary>
        public float Scale
        {
            get => _gridData.Scale;

            internal set
            {
                GridDataRaw gridData = _gridData;
                gridData.Scale = value;
                _gridData = gridData;
            }
        }
        
        internal GridDataRaw GridDataRaw => _gridData;
        internal DenseDataRaw DenseDataRaw => _denseStorage.DenseDataRaw;
        internal SparseDataRaw SparseDataRaw => _sparseStorage.SparseDataRaw;
        
        private GridDataRaw _gridData;

        private readonly DenseStorage _denseStorage;
        private readonly SparseStorage _sparseStorage;

        private readonly Transform _origin;

        internal VolumeData(int gridX, int gridY, int gridZ, float scale, int localChunkDim, Transform origin)
        {
            int tmpGlobalChunkX = (int) Math.Floor((double) gridX / localChunkDim) + 1;
            int tmpGlobalChunkY = (int) Math.Floor((double) gridY / localChunkDim) + 1;
            int tmpGlobalChunkZ = (int) Math.Floor((double) gridZ / localChunkDim) + 1;
            _gridData = new GridDataRaw
            {
                GridXDim = gridX,
                GridYDim = gridY,
                GridZDim = gridZ,
                GridVoxelCount = gridX * gridY * gridZ,
                GlobalChunkXDim = tmpGlobalChunkX,
                GlobalChunkYDim = tmpGlobalChunkY,
                GlobalChunkZDim = tmpGlobalChunkZ,
                GlobalChunkCount = tmpGlobalChunkX * tmpGlobalChunkY * tmpGlobalChunkZ,
                LocalChunkVoxelCount = localChunkDim * localChunkDim * localChunkDim,
                LocalChunkDim = localChunkDim,
                Scale = scale <= 0 ? 1.0f : scale
            };
            _origin = origin;

            _denseStorage = new DenseStorage(this);
            _sparseStorage = new SparseStorage(this);
        }

        /// <summary>
        /// This will take a world position and deliver the voxel which encloses that position. 
        /// </summary>
        /// <param name="worldPos">The world position of the query.</param>
        /// <param name="voxelData">The voxel data of the voxel which encloses the world position. </param>
        /// <returns>True if the world position is enclosed by a voxel.</returns>
        public bool TryGetVoxelFromWorldCoord(Vector3 worldPos, out VoxelData voxelData)
        {
            Vector3Int voxelIndex = GetVoxelIndexFromWorldCoord(worldPos);
            return TryGetVoxel(voxelIndex.x, voxelIndex.y, voxelIndex.z, out voxelData);
        }
        
        /// <summary>
        /// This will take a world position and return the voxel which encloses that position. Throws an exception
        /// if the world position is not enclosed by a voxel.
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns>Voxel data of the voxel that encloses the world position.</returns>
        public VoxelData GetVoxelFromWorldCoord(Vector3 worldPos)
        {
            Vector3Int voxelIndex = GetVoxelIndexFromWorldCoord(worldPos);
            VoxelData voxelData = GetVoxel(voxelIndex.x, voxelIndex.y, voxelIndex.z);
            return voxelData;
        }

        /// May also return negative values which are out of the volume bounds.
        /// <summary>
        /// Given a world position this method will return the voxel position in index space.
        /// </summary>
        /// <param name="worldPos">The world position of the query.</param>
        /// <returns>The voxel position in index space.</returns>
        public Vector3Int GetVoxelIndexFromWorldCoord(Vector3 worldPos)
        {
            Vector3 localPos = WorldToAAVolLocalCoord(worldPos);
            return GetVxlIdxFromAAVolLocalCoord(localPos);
        }

        /// Throws an exception if the requested voxel is out of the volume bounds.
        /// <summary>
        /// Returns the voxel data of the voxel with the given position in index space.
        /// </summary>
        /// <param name="x">The x dimension of the position in index space.</param>
        /// <param name="y">The y dimension of the position in index space.</param>
        /// <param name="z">The z dimension of the position in index space.</param>
        /// <returns>The voxel data for the query.</returns>
        /// <exception cref="Exception">Thrown when the requested voxel is out of the volume bounds. </exception>
        public VoxelData GetVoxel(int x, int y, int z)
        {
            if (x >= GridXDim || x < 0 || y >= GridYDim || y < 0 || z >= GridZDim || z < 0)
            {
                throw new Exception("Voxel x:" + x + " y:" + y + " z:" + z + " is out of the volume bounds.");
            }
            return _denseStorage.GetVoxel(x, y, z);
        }

        /// <summary>
        /// Tries so get the voxel data of the voxel with the given position in index space.
        /// </summary>
        /// <param name="x">The x dimension of the position in index space.</param>
        /// <param name="y">The y dimension of the position in index space.</param>
        /// <param name="z">The z dimension of the position in index space.</param>
        /// <param name="voxelData">The voxel data of the requested voxel.</param>
        /// <returns>False if the requested voxel is out of the volume bounds. True otherwise.</returns>
        public bool TryGetVoxel(int x, int y, int z, out VoxelData voxelData)
        {
            if (x >= GridXDim || x < 0 || y >= GridYDim || y < 0 || z >= GridZDim || z < 0)
            {
                voxelData = new VoxelData();
                return false;
            }
            voxelData = _denseStorage.GetVoxel(x, y, z);
            return true;
        }
        /// Default step size of the ray is the value for the volume scale.
        /// Result can be inaccurate because of the fixed step size. E.g. a ray can step over a corner of a voxel.
        /// <summary>
        /// Delivers the voxel data of the first voxel the ray hits.
        /// </summary>
        /// <param name="ray">The ray the query should consider.</param>
        /// <param name="distance">The maximum distance the ray will travel.</param>
        /// <param name="voxelData">The voxel data of the voxel the ray hits first.</param>
        /// <param name="stepSize">Step size of the ray.</param>
        /// <returns>False if no voxel was hit during the traveled distance. True otherwise.</returns>
        public bool PickFirstSolidVoxel(Ray ray, float distance, out VoxelData voxelData, float stepSize = -1)
        {
            if (stepSize < 0) stepSize = Scale;
            
            for (float rayStep = 0; rayStep <= distance; rayStep+=stepSize)
            {
                if (TryGetVoxelFromWorldCoord(ray.GetPoint(rayStep), out voxelData) &&
                    (voxelData.State == VoxelState.Visible || voxelData.State == VoxelState.Solid))
                {
                    return true;
                }
            }
            voxelData = new VoxelData();
            return false;
        }

        /// <summary>
        /// Sets the color of a voxel.
        /// </summary>
        /// <param name="x">The x dimension of the voxel position in index space.</param>
        /// <param name="y">The y dimension of the voxel position in index space.</param>
        /// <param name="z">The z dimension of the voxel position in index space.</param>
        /// <param name="color">The new grayscale color of the voxel. Must be between 0 and 255.</param>
        /// <exception cref="Exception">Thrown if the requested voxel is out of the volume bounds.</exception>
        public void SetVoxelColor(int x, int y, int z, byte color)
        {
            if (x >= GridXDim || x < 0 || y >= GridYDim || y < 0 || z >= GridZDim || z < 0)
            {
                throw new Exception("Voxel x:" + x + " y:" + y + " z:" + z + " is out of the volume bounds.");
            }
            _denseStorage.SetVoxel(x, y, z, color);
        }
        
        /// <summary>
        /// Sets the color of a voxel.
        /// </summary>
        /// <param name="x">The x dimension of the voxel position in index space.</param>
        /// <param name="y">The y dimension of the voxel position in index space.</param>
        /// <param name="z">The z dimension of the voxel position in index space.</param>
        /// <param name="color">The new grayscale color of the voxel. Must be between 0 and 255.</param>
        /// <returns>False if the requested voxel is out of the volume bounds. True otherwise.</returns>
        public bool TrySetVoxelColor(int x, int y, int z, byte color)
        {
            if (x >= GridXDim || x < 0 || y >= GridYDim || y < 0 || z >= GridZDim || z < 0)
            {
                return false;
            }
            _denseStorage.SetVoxel(x, y, z, color);
            return true;
        }

        internal Vector3Int GetVxlIdxFromAAVolLocalCoord(Vector3 aaVolWorldCoord)
        {
            return new Vector3Int((int)Math.Floor(aaVolWorldCoord.x / Scale),
                (int)Math.Floor(aaVolWorldCoord.y / Scale), (int)Math.Floor(aaVolWorldCoord.z / Scale));
        }
        
        /// For why this is necessary see the 'SparseStorage.cs' file. This methods cleans up the data by comparing the
        /// stored data inside 'DirtyChunkSurfaceVoxels' with the data stored in the dense storage (which always holds true data).
        /// This method only considers visible chunks where visible voxels have been removed because for all other visible chunks
        /// it is not needed. See 'ModifiedChunksWithRemoval' inside the 'SparseStorage.cs' file.
        /// Further notes: CleanDirtyData() could be optimized by putting it into the render function of VolumeRenderer. One would save a iteration over surface voxels. However
        /// a function which modifies the data should not be put in the render part. That part should just be responsible to display the data to the screen.
        /// Therefore CleanDirtyData() is put before the rendering for better software architecture and maintenance.
        /// <summary>
        /// Cleans up the 'DirtyChunkSurfaceVoxels' from the sparse storage. This is needed before rendering.
        /// </summary>
        internal void CleanDirtyData()
        {
            // For all modified chunks
            var modifiedSurfaceChunks = _sparseStorage.TmpModifiedVisibleChunks.GetKeyArray(Allocator.Temp);
            for (int i = 0; i < modifiedSurfaceChunks.Length; i++)
            {
                // Fix dirty voxels only when there are removed voxels inside a modified chunk
                // This mainly applies when init the grid where there is no removed voxels, or restoring/adding back voxels (undo)
                // while cutting this will be mostly irrelevant
                if (_sparseStorage.TmpModifiedVisChunksWithRemoval.ContainsKey(modifiedSurfaceChunks[i]))
                {
                    // Fix dirty surface voxels
                    NativeList<VoxelData> cleanVoxels = new NativeList<VoxelData>(Allocator.Temp);
                    foreach (VoxelData voxelData in _sparseStorage.DirtyChunkVisibleVoxels.GetValuesForKey(modifiedSurfaceChunks[i]))
                    {
                        if (_denseStorage.GetVoxels()[
                                    VolumeDataUtil.GridCoord.To1D(voxelData.X, voxelData.Y, voxelData.Z, GridXDim, GridYDim)]
                                .State == VoxelState.Visible)
                        {
                            cleanVoxels.Add(voxelData);
                        }
                    }

                    _sparseStorage.DirtyChunkVisibleVoxels.Remove(modifiedSurfaceChunks[i]);
                    foreach (VoxelData cleanVoxelData in cleanVoxels)
                    {
                        _sparseStorage.DirtyChunkVisibleVoxels.Add(modifiedSurfaceChunks[i], cleanVoxelData);
                    }

                    // Update surface chunks
                    if (cleanVoxels.Length == 0)
                    {
                        // Modified surface should not be deleted because the methods which calls this methods iterates over
                        // modified surface. Also: in Volume.Render() after the cleanup part the modified values are reset
                        // and in VolumeRenderer.Render it iterates only over surfaceChunks where it checks if surfaceChunk is 
                        // also present in modified chunk, which will not be true since the value is deleted from 
                        // surfaceChunks
                        _sparseStorage.VisibleChunks.Remove(modifiedSurfaceChunks[i]);
                    }

                    cleanVoxels.Dispose();
                }
            }
            modifiedSurfaceChunks.Dispose();
        }

        internal void CleanUp()
        {
            _denseStorage.CleanUp();
            _sparseStorage.CleanUp();
        }
        
        internal Vector3 SnapAAVolLclCoordToLclVxlOrigin(Vector3 localPos)
        {
            // If scale '0.112' this rounds '0.19' to '0.224' and '0.14' to '0.112'
            // Alternatively use 'Floor': rounds '0.19' and '0.14' to '0.112' 
            return new Vector3((float) Math.Round(localPos.x*(1/Scale)) * Scale,
                (float) Math.Round(localPos.y*(1/Scale)) * Scale,
                (float) Math.Round(localPos.z*(1/Scale)) * Scale);
        }

        internal Vector3 WorldToAAVolLocalCoord(Vector3 worldPos)
        {
            Vector3 volumePos = _origin.position;
            // Get local pos and rotate it to a axis aligned volume
            return Quaternion.Inverse(_origin.rotation) * (worldPos - volumePos);
        }

        internal void ResetModifiedChunks()
        {
            _sparseStorage.ResetModifiedChunks();
        }
    }
}