using UnityEngine;
using UnityEngine.TestTools;

namespace VDVolume.Rendering
{
    /// <summary>
    /// Struct for sending the voxel properties to the GPU.
    /// </summary>
    [ExcludeFromCoverage]
    internal struct VoxelProperties
    {
        /// <summary>
        /// Position of the voxel in index space. See VoxelMesh.cs for information where the origin is.
        /// </summary>
        internal Vector3 Position;
        /// <summary>
        /// Grayscale color with range of 0-255. It is a 'float' since a 'byte' data type does not exist in the Cg language and
        /// because it will be divided by a integer in the shader.
        /// </summary>
        internal float Color;
    }
}