using System.Collections;
using Unity.Collections;
using UnityEngine;
using VDVolume.Model;

namespace VDVolume.DataStructure
{
    public partial class VolumeData
    {
        /// The sparse storage houses multiple data structures to manage the visible voxels of the volume. Only the
        /// visible voxels will be rendered. See 'VoxelState.cs' for more information.
        /// <summary>
        /// Sparse storage of the volume data.
        /// </summary>
        private class SparseStorage
        {
            internal SparseDataRaw SparseDataRaw => _sparseData;
            /// Visible voxels (values) are grouped in chunks (keys). The MultiHashMap is dirty because voxels only get
            /// added to it. This is because removal of a voxel in the values list is not supported by the MultiHashMap.
            /// Even if it would be supported it it would be inefficient since one would have to search the list (O(N)) for the
            /// element and then delete it.
            /// The method 'CleanDirtyData()' inside 'VolumeData.cs' cleans up this data before rendering. Therefore the data structure
            /// can be seen as dirty while the dense storage holds always true data.
            /// <summary>
            /// Contains 1D chunks indices as keys and (usually) multiple corresponding visible voxels as values.
            /// </summary>
            internal NativeMultiHashMap<int,VoxelData> DirtyChunkVisibleVoxels => _sparseData.DirtyChunkVisibleVoxels;
            /// This data structure is used to identify which visible chunks has been change when removing or adding voxels.
            /// This is important when rendering the voxels with GPU instancing since one wants only to upload new position
            /// and color data to the GPU when there are modified voxels inside the chunk. GPU instancing is chunk based.
            /// Further notes: Contains 1D chunk indices as keys while the value is irrelevant. The HashMap data structure provides
            /// the helpful property that only a key is stored once which is important when iterating the key list.
            /// It is 'Tmp' because the data gets cleared every frame.
            /// <summary>
            /// Stores modified visible chunks within a render update cycle.
            /// </summary>
            internal NativeHashMap<int,bool> TmpModifiedVisibleChunks => _sparseData.TmpModifiedVisibleChunks; // cant be array because of resetModifiedSurfaces
            /// This is only needed for some optimizations. When not considering this array 'TmpModifiedVisibleChunks' would
            /// suffice. However when no voxels has been removed (when restoring voxels, adding voxels back (undo) or when
            /// initializing the grid) there is no need to clean up 'DirtyChunkVisibleVoxels' which is usually done
            /// with the method 'CleanDirtyData()' inside 'VolumeData.cs'. The clean up process will only consider chunks
            /// which are stored inside this HashMap because for all other visible chunks it is not needed.
            /// Further notes: Contains 1D chunk indices as keys while the value is irrelevant. The HashMap data structure provides
            /// the helpful property that only a key is stored once which is important when iterating the key list.
            /// It is 'Tmp' because the data gets cleared every frame.
            /// <summary>
            /// Stores modified visible chunks within a render update cycle where a visible voxel was removed.
            /// </summary>
            internal NativeHashMap<int,bool> TmpModifiedVisChunksWithRemoval => _sparseData.TmpModifiedVisChunksWithRemoval;
            /// Since 'DirtyChunkVisibleVoxels' is allocated with the total amount of chunks the volume has. One has to store the
            /// information which chunk index of 'DirtyChunkVisibleVoxels' has visible voxels as values. This is needed
            /// later for rendering where one wants to iterate only over visible chunks and their visible voxels.
            /// Further notes: Chunks are stored as 1D chunk indices (keys) while their value is irrelevant. The HashMap data structure provides
            /// the helpful property that only a key is stored once which is important for later in the rendering when iterating
            /// over all visible chunks. 
            /// <summary>
            /// Stores chunks which hold visible voxels.
            /// </summary>
            internal NativeHashMap<int,bool> VisibleChunks => _sparseData.VisibleChunks;

            private SparseDataRaw _sparseData;
            private VolumeData _volumeData;

            internal SparseStorage(VolumeData volumeData)
            {
                _volumeData = volumeData;
                _sparseData = new SparseDataRaw()
                {
                    DirtyChunkVisibleVoxels = new NativeMultiHashMap<int,VoxelData>(_volumeData.GridVoxelCount,Allocator.Persistent),
                    TmpModifiedVisibleChunks = new NativeHashMap<int,bool>(_volumeData.GlobalChunkCount,Allocator.Persistent),
                    TmpModifiedVisChunksWithRemoval = new NativeHashMap<int,bool>(_volumeData.GlobalChunkCount,Allocator.Persistent),
                    VisibleChunks = new NativeHashMap<int,bool>(_volumeData.GlobalChunkCount,Allocator.Persistent)
                };
            }

            internal void ResetModifiedChunks()
            {
                TmpModifiedVisibleChunks.Clear();
                TmpModifiedVisChunksWithRemoval.Clear();
            }

            internal void CleanUp()
            {
                DirtyChunkVisibleVoxels.Dispose();
                TmpModifiedVisibleChunks.Dispose();
                TmpModifiedVisChunksWithRemoval.Dispose();
                VisibleChunks.Dispose();
            }
        }
    }
    
}