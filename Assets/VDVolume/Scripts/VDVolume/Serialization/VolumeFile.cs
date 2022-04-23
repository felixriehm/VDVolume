namespace VDVolume.Serialization
{
    /// <summary>
    /// Object for storing .vdvolume file data. This object is used e.g. when loading or saving a volume.
    /// </summary>
    internal class VolumeFile
    {
        /// <summary>
        /// Absolute file path to the .vdvolume file.
        /// </summary>
        internal readonly string FilePathToVDVolume;
        /// <summary>
        /// The full data which is stored in the associated .vdvolume file. This includes everything before the
        /// voxel data (e.g. grid dimensions) and the voxel data itself.
        /// </summary>
        internal readonly byte[] Data;
        /// <summary>
        /// The X dimension of the volume stored in the file.
        /// </summary>
        internal readonly int XDim;
        /// <summary>
        /// The Y dimension of the volume stored in the file.
        /// </summary>
        internal readonly int YDim;
        /// <summary>
        /// The Z dimension of the volume stored in the file.
        /// </summary>
        internal readonly int ZDim;
        
        /// <summary>
        /// Constructs a VolumeFile object.
        /// </summary>
        internal VolumeFile(string filePathToVDVolume, byte[] data, int xDim, int yDim, int zDim)
        {
            FilePathToVDVolume = filePathToVDVolume;
            Data = data;
            XDim = xDim;
            YDim = yDim;
            ZDim = zDim;
        }
    }
}