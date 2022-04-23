using UnityEngine;

namespace VDVolume.Editor
{
    /// <summary>
    /// Serves as a container for the data used to initialize a volume.
    /// </summary>
    public class VolumeInitData : ScriptableObject
    {
        /// This can be a absolute path or a relative path. When using a relative path one has to begin the path
        /// with '@relative:'. The relative starts in the 'StreamingAssets' folder of Unity.
        /// <example>@relative:myFolder/myVolume.vdvolume</example>
        /// <summary>
        /// Path to the .vdvolume file.
        /// </summary>
        public string pathToVolumeFile;
        /// This value is specified as one dimension of a chunk and therefore tells how many voxels are on one axis.
        /// <summary>
        /// Chunk size of the volume.
        /// </summary>
        public int chunkSize;
        /// <summary>
        /// The scale of the volume.
        /// </summary>
        public float scale;
    }
}