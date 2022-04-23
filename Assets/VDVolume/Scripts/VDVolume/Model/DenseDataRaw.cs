using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace VDVolume.Model
{
    /// <summary>
    /// Helper struct to store dense storage data structures. Mainly used to reduce the
    /// parameter list of Unity Job constructors.
    /// </summary>
    internal struct DenseDataRaw
    {
        [NativeDisableContainerSafetyRestriction]
        internal NativeArray<VoxelData> Voxels;
    }
}