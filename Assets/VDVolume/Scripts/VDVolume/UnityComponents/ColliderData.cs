using UnityEngine;
using VDVolume.Model;

namespace VDVolume.UnityComponents
{
    /// When using VolumeCollidable this component will be added to the grid
    /// colliders. This way you can get this component and receive the voxel data.
    /// <summary>
    /// Data container component for colliders of VolumeCollidable. 
    /// </summary>
    public class ColliderData : MonoBehaviour
    {
        /// <summary>
        /// The voxel data associated with the collider.
        /// </summary>
        public VoxelData VoxelData { get; internal set; }
    }
}