using Unity.Collections;

namespace VDVolume.Model
{
    /// <summary>
    /// Helper struct to store sparse storage data structures. Mainly used to reduce the
    /// parameter list of Unity Job constructors.
    /// </summary>
    internal struct SparseDataRaw
    {
        internal NativeMultiHashMap<int, VoxelData> DirtyChunkVisibleVoxels;
        internal NativeHashMap<int, bool> TmpModifiedVisibleChunks;
        internal NativeHashMap<int, bool> TmpModifiedVisChunksWithRemoval;
        internal NativeHashMap<int, bool> VisibleChunks;
    }
}