using System;

namespace VDVolume.Model
{
    /// If a voxel is visible it is also solid. The 'Unknown' state is the default state when an array of VoxelState is allocated.
    /// A visible voxel is a voxel which has at least one removed voxel as neighbor and therefore can be 'seen' from that
    /// empty space.
    /// <summary>
    /// The state a voxel can have.
    /// </summary>
    public enum VoxelState
    {
        Unknown = 0,
        Solid = 1,
        Removed = 2,
        Visible = 3
    }
}