namespace VDVolume.Modifiers.Undo
{
    /// <summary>
    /// Enum for the undo process that helps to clarify if a voxel was cut or restored.
    /// </summary>
    internal enum VoxelCmdType
    {
        Cutting,
        Restoring
    }
}