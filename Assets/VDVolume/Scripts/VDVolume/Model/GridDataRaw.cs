using Unity.Collections;

namespace VDVolume.Model
{
    /// <summary>
    /// Helper struct to store grid data. Mainly used to reduce the
    /// parameter list of Unity Job constructors.
    /// </summary>
    internal struct GridDataRaw
    {
        internal int GridXDim;
        internal int GridYDim;
        internal int GridZDim;
        internal int GridVoxelCount;
        internal int GlobalChunkXDim;
        internal int GlobalChunkYDim;
        internal int GlobalChunkZDim;
        internal int GlobalChunkCount;
        internal int LocalChunkDim;
        internal int LocalChunkVoxelCount;
        internal float Scale;
    }
}