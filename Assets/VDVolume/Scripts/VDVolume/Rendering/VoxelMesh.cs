using System.Dynamic;
using UnityEngine;
using UnityEngine.TestTools;

namespace VDVolume.Rendering
{
    /// Helper class to construct the voxel mesh used for the GPU instancing.
    [ExcludeFromCoverage]
    internal static class VoxelMesh
    {
        /// The cube mesh has the origin at the lower left front corner, not in the center of the cube.
        /// <summary>
        /// Creates the voxel mesh used for the GPU instancing.
        /// </summary>
        /// <returns>The voxel mesh as a Unity mesh.</returns>
        internal static Mesh Create()
        {
            Mesh mesh = new Mesh();
            mesh.Clear();
            Vector3[] vertices = {
                new Vector3 (0, 0, 0),
                new Vector3 (1, 0, 0),
                new Vector3 (1, 1, 0),
                new Vector3 (0, 1, 0),
                new Vector3 (0, 1, 1),
                new Vector3 (1, 1, 1),
                new Vector3 (1, 0, 1),
                new Vector3 (0, 0, 1),
            };
        
            int[] triangles = {
                0, 2, 1, //face front
                0, 3, 2,
                2, 3, 4, //face top
                2, 4, 5,
                1, 2, 5, //face right
                1, 5, 6,
                0, 7, 4, //face left
                0, 4, 3,
                5, 4, 7, //face back
                5, 7, 6,
                0, 6, 7, //face bottom
                0, 1, 6
            };
        
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.Optimize ();
            mesh.RecalculateNormals ();
            return mesh;
        }
    }
}