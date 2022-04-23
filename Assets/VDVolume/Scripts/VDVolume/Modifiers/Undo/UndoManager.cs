using Unity.Collections;
using VDVolume.Modifiers.Cutting;
using VDVolume.Modifiers.Restoring;

namespace VDVolume.Modifiers.Undo
{
    /// <summary>
    /// Manages the undo process of the application.
    /// </summary>
    public class UndoManager
    {
        private VoxelCmdBuffer _voxelCmdBuffer;
        private VolumeCutting _cutting;
        private VolumeRestoring _restoring;
        internal UndoManager()
        {
            _voxelCmdBuffer = new VoxelCmdBuffer();
        }

        internal void Init(VolumeCutting cutting, VolumeRestoring restoring)
        {
            _cutting = cutting;
            _restoring = restoring;
        }

        internal VoxelCmdBuffer GetVoxelCmdBuffer()
        {
            return _voxelCmdBuffer;
        }
        
        /// Only voxels cut or restored with the methods AddVoxel() (VolumeRestoring.cs) and RemoveVoxel() (VolumeCutting.cs) are considered.
        /// The amount of the undoed voxels depends on the VoxelCmdBuffer size.
        /// <summary>
        /// Undo the last voxels which cut or restored the volume.
        /// </summary>
        public void UndoVoxels()
        {
            foreach (VoxelCmdData voxelCmdData in _voxelCmdBuffer.GetBuffer())
            {
                switch (voxelCmdData.CmdType)
                {
                    case VoxelCmdType.Cutting:
                        _restoring.AddVoxel(voxelCmdData.X, voxelCmdData.Y, voxelCmdData.Z);
                        break;
                    case VoxelCmdType.Restoring:
                        _cutting.RemoveVoxel(voxelCmdData.X, voxelCmdData.Y, voxelCmdData.Z);
                        break;
                }
            }

            _voxelCmdBuffer.Clear();
        }

        internal void CleanUp()
        {
            _voxelCmdBuffer.CleanUp();
        }
    }
}