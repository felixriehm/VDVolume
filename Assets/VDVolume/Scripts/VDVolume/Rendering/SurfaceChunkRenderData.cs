using UnityEngine;
using UnityEngine.TestTools;

namespace VDVolume.Rendering
{
    /// <summary>
    /// Struct which is used to store the data per chunk which is needed for chunk based GPU instancing.
    /// </summary>
    [ExcludeFromCoverage]
    internal struct SurfaceChunkRenderData
    {
        /// <summary>
        /// Args buffer differs only in instance count. The number of how many voxels will be drawn in this chunk.
        /// </summary>
        internal ComputeBuffer ArgsBuffer { get; set; }
        /// <summary>
        /// Contains a VoxelProperties struct per voxel.
        /// </summary>
        internal ComputeBuffer MeshPropertiesBuffer { get; set; }
        /// <summary>
        /// Contains a material per voxel. Each Material differs only in the material 'voxelProperties' property which will be assigned
        /// with the MeshPropertiesBuffer. Through this the material 'voxelProperties' property is available in the shader and instanced.
        /// </summary>
        internal Material InstanceMaterial { get; set; }
    }
}