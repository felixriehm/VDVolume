using VDVolume.Model;

namespace VDVolume.Modifiers.Undo
{
    /// <summary>
    /// Enum for the undo process that stores the position and the VoxelCmdType of a cut or restored voxel.
    /// </summary>
    internal struct VoxelCmdData
    {
        internal int X;
        internal int Y;
        internal int Z;
        internal VoxelCmdType CmdType;
    }
}