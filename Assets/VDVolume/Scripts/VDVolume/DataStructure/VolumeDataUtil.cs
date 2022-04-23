using System;
using UnityEngine;

namespace VDVolume.DataStructure
{
    /// <summary>
    /// Util class to convert grid or chunk coordinates.
    /// </summary>
    internal static class VolumeDataUtil
    {
        /// <summary>
        /// Converts grid coordinates.
        /// </summary>
        internal static class GridCoord
        {
            internal static int To1DGlobalChunkCoord(int x, int y, int z, int localChunkDim, int globalChunkXDim, int globalChunkYDim)
            {
                if (x < 0 || y < 0 || z < 0)
                {
                    return -1;
                }
                return Array3DIndexTo1D((int) Math.Floor((double) x / localChunkDim),
                    (int) Math.Floor((double) y / localChunkDim), (int) Math.Floor((double) z / localChunkDim),
                    globalChunkXDim, globalChunkYDim);
            }
            
            internal static int To1D(int x, int y, int z, int xMax, int yMax)
            {
                if (x < 0 || y < 0 || z < 0)
                {
                    return -1;
                }
                return Array3DIndexTo1D(x, y, z,xMax,yMax);
            }
            
            internal static Vector3Int To3D(int index, int xMax, int yMax)
            {
                if (index < 0)
                {
                    return new Vector3Int(-1,-1,-1);
                }
                return Array1DIndexTo3D(index, xMax, yMax);
            }
        }

        /// <summary>
        /// Converts chunk coordinates.
        /// </summary>
        internal static class ChunkCoord
        {
            internal static int To1D(int x, int y, int z, int xMax, int yMax)
            {
                if (x < 0 || y < 0 || z < 0)
                {
                    return -1;
                }
                return Array3DIndexTo1D(x, y, z,xMax,yMax);
            }

            internal static Vector3Int To3D(int index, int xMax, int yMax)
            {
                if (index < 0)
                {
                    return new Vector3Int(-1,-1,-1);
                }
                return Array1DIndexTo3D(index, xMax, yMax);
            }
        }

        internal static int Array3DIndexTo1D( int x, int y, int z, int xMax, int yMax) {
            return (z * xMax * yMax) + (y * xMax) + x;
        }

        internal static Vector3Int Array1DIndexTo3D( int idx, int xMax, int yMax) {
            int z = idx / (xMax * yMax);
            idx -= (z * xMax * yMax);
            int y = idx / xMax;
            int x = idx % xMax;
            return new Vector3Int(x, y, z);
        }
    }
}