using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Model;
using VDVolume.Modifiers.Cutting;
using VDVolume.Modifiers.Filter;
using VDVolume.Modifiers.Restoring;
using VDVolume.Modifiers.Undo;

namespace VDVolume.Modifiers
{
    /// <summary>
    /// Holds and manges references to modifier objects for cutting, restoring etc.
    /// </summary>
    public class VolumeModifiers
    {
        /// <summary>
        /// Reference to the VolumeFilter object
        /// </summary>
        public VolumeFilter Filter { get; }
        /// <summary>
        /// Reference to the VolumeCutting object
        /// </summary>
        public VolumeCutting Cutting { get; }
        /// <summary>
        /// Reference to the VolumeRestoring object
        /// </summary>
        public VolumeRestoring Restoring { get; }
        /// <summary>
        /// Reference to the UndoManager object
        /// </summary>
        public UndoManager Undo { get; }

        private VolumeData _volumeData;
        

        internal VolumeModifiers(VolumeData volumeData)
        {
            _volumeData = volumeData;
            Undo = new UndoManager();
            Filter = new VolumeFilter(_volumeData);
            Cutting = new VolumeCutting(_volumeData, Undo.GetVoxelCmdBuffer());
            Restoring = new VolumeRestoring(_volumeData, Undo.GetVoxelCmdBuffer());
            Undo.Init(Cutting, Restoring);
        }
        
        internal void CleanUp()
        {
            Undo.CleanUp();
        }
    }
}