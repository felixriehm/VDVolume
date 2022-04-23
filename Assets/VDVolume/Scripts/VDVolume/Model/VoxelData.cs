namespace VDVolume.Model
{
    /// <summary>
    /// The data which is stored for each voxel 
    /// </summary>
    public struct VoxelData
    {
        /// <summary>
        /// The grayscale color of the voxel with range 0-255.
        /// </summary>
        public byte Color;
        /// <summary>
        /// The X dimension of the position in index space.
        /// </summary>
        public int X;
        /// <summary>
        /// The Y dimension of the position in index space.
        /// </summary>
        public int Y;
        /// <summary>
        /// The Z dimension of the position in index space.
        /// </summary>
        public int Z;
        /// <summary>
        /// The state of the voxel.
        /// </summary>
        public VoxelState State;
    }
}