using System;
using System.Collections;
using System.IO;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Model;
using VDVolume.Modifiers;

namespace VDVolume.Serialization
{
    /// <summary>
    /// Class for serializing and deserializing volume data.
    /// </summary>
    internal static class VolumeSerializer
    {
        /// <summary>
        /// Reads file from file path.
        /// </summary>
        /// <param name="filePathToVDVolume">Absolute file path to a .vdvolume file.</param>
        /// <returns>Objects which holds the file information.</returns>
        internal static VolumeFile ReadFile(string filePathToVDVolume)
        {
            byte[] data = File.ReadAllBytes(filePathToVDVolume);
            
            int xDim = BitConverter.ToInt32(data,0);
            int yDim = BitConverter.ToInt32(data,4);
            int zDim = BitConverter.ToInt32(data,8);

            return new VolumeFile(filePathToVDVolume,data,xDim,yDim,zDim);
        }

        /// This will fill the given VolumeData object with voxel data. The loading process will be executed in a
        /// .NET thread.
        /// <summary>
        /// Loads a volume from a VolumeFile asynchronously.
        /// </summary>
        /// <param name="volumeFile">Objects which holds the file information.</param>
        /// <param name="volumeData">VolumeData object in which the file data will be written.</param>
        /// <returns>An object for observing the async loading process.</returns>
        internal static AsyncSerializationOperation AsyncLoadVolume(VolumeFile volumeFile, VolumeData volumeData)
        {
            AsyncSerializationOperation operation = new AsyncSerializationOperation();
            
            Thread t = new Thread(() => LoadVolume(volumeFile,volumeData, operation));
            t.Start();
            operation.Thread = t;

            return operation;
        }
        
        /// <summary>
        /// Loads a volume from a VolumeFile. This will fill the given VolumeData object with voxel data.
        /// </summary>
        /// <param name="volumeFile">Objects which holds the file information.</param>
        /// <param name="volumeData">VolumeData object in which the file data will be written.</param>
        /// <param name="operation">A observer object in which will be written into.</param>
        private static void LoadVolume(VolumeFile volumeFile, VolumeData volumeData, AsyncSerializationOperation operation)
        {
            // init data structures
            DenseDataRaw denseData = volumeData.DenseDataRaw;
            SparseDataRaw sparseData = volumeData.SparseDataRaw;
            BitArray dataInBits = new BitArray(volumeFile.Data);
            int voxelIndex = 0;
            BitArray stateInBits = new BitArray(2);
            BitArray colorInBits = new BitArray(8);
            int[] intState = new int[1];
            byte[] byteColor = new byte[1];

            // we start from 12(bytes)*8(bits) because the first 12 bytes are taken for dim values
            for (int voxelStride = 12*8; voxelStride < dataInBits.Length; voxelStride+=10)
            {
                // update async operation progress
                operation.Progress = voxelIndex / (float) volumeData.GridVoxelCount;
                
                // retrieve voxel state data
                for (int state = 0; state < 2; state++)
                {
                    stateInBits[state] = dataInBits[voxelStride + state];
                }
                stateInBits.CopyTo(intState, 0);
                VoxelState voxelState = (VoxelState) intState[0];
                
                // retrieve voxel colore data
                for (int color = 0; color < 8; color++)
                {
                    colorInBits[color] = dataInBits[voxelStride + 2 + color];
                }
                colorInBits.CopyTo(byteColor, 0);

                Vector3Int pos = VolumeDataUtil.GridCoord.To3D(voxelIndex,volumeFile.XDim,volumeFile.YDim);
                if (voxelState == VoxelState.Visible)
                {
                    // dense storage
                    // store visible voxel
                    VoxelData newVoxel = new VoxelData()
                    {
                        X = pos.x,
                        Y = pos.y,
                        Z = pos.z,
                        State = VoxelState.Visible,
                        Color = byteColor[0],
                    };
                    denseData.Voxels[voxelIndex] = newVoxel;
                    
                    // sparse storage
                    int chunkIndex = VolumeDataUtil.GridCoord.To1DGlobalChunkCoord(pos.x, pos.y, pos.z, volumeData.LocalChunkDim, volumeData.GlobalChunkXDim, volumeData.GlobalChunkYDim);
                    sparseData.DirtyChunkVisibleVoxels.Add(chunkIndex, newVoxel);
                    sparseData.TmpModifiedVisibleChunks.TryAdd(chunkIndex, true);
                    sparseData.VisibleChunks.TryAdd(chunkIndex, true);
                }
                else
                {
                    // dense storage
                    // store any none visible voxel
                    denseData.Voxels[voxelIndex] = new VoxelData()
                    {
                        X = pos.x,
                        Y = pos.y,
                        Z = pos.z,
                        State = VoxelState.Solid,
                        Color = byteColor[0],
                    };
                }

                voxelIndex++;
            }

            // update async operation state
            operation.IsDone = true;
        }

        /// This will write the VolumeData object data into the .vdvolume file defined with the
        /// VolumeFile object. The saving process will be executed in a .NET thread.
        /// <summary>
        /// Saves a volume from a VolumeData object asynchronously.
        /// </summary>
        /// <param name="saveFileLocation">Absolute file path to the save location.</param>
        /// <param name="volumeData">VolumeData object in which the file data will be written.</param>
        /// <returns>An object for observing the async saving process.</returns>
        internal static AsyncSerializationOperation AsyncSaveVolume(string saveFileLocation, VolumeData volumeData)
        {
            AsyncSerializationOperation operation = new AsyncSerializationOperation();
            
            Thread t = new Thread(() => SaveVolume(saveFileLocation, volumeData, operation));
            t.Start();
            operation.Thread = t;

            return operation;
        }

        /// <summary>
        /// Saves a volume from a VolumeData object. This will write the VolumeData object data into the .vdvolume file defined with the
        /// VolumeFile object.
        /// </summary>
        /// <param name="saveFileLocation">Absolute file path to the save location.</param>
        /// <param name="volumeData">VolumeData object in which the file data will be written.</param>
        /// <param name="operation">A observer object in which will be written into.</param>
        private static void SaveVolume(string saveFileLocation, VolumeData volumeData, AsyncSerializationOperation operation)
        {
            // We multiply by 10 because the bitmask returns 10 bits
            BitArray voxelDataBits = new BitArray(volumeData.GridXDim * volumeData.GridYDim * volumeData.GridZDim * 10);
            // holds bit information for a voxel
            BitArray bitmask = new BitArray(10);
            int bitsIndex = 0;
            
            NativeArray<VoxelData> voxels = volumeData.DenseDataRaw.Voxels;
            // fill voxel data BitArray which will be later be written into the file
            for (int i = 0; i < voxels.Length; i++)
            {
                bitmask.SetAll(false);
                VoxelUtil.AddColorToBitMask(bitmask, voxels[i].Color);
                VoxelUtil.AddStateToBitMask(bitmask, voxels[i].State);
                
                for (int j = 0; j < bitmask.Length; j++)
                {
                    voxelDataBits[bitsIndex + j] = bitmask[j];
                }

                bitsIndex += bitmask.Length;
                // update async operation progress
                operation.Progress = i / (float) voxels.Length;
            }
           
            byte[] bitsInBytes = new byte[(voxelDataBits.Length - 1) / 8 + 1];
            // convert voxel data BitArray to ByteArray for the writing operation
            voxelDataBits.CopyTo(bitsInBytes, 0);
            byte[] xDimInBytes = BitConverter.GetBytes(volumeData.GridXDim);
            byte[] yDimInBytes = BitConverter.GetBytes(volumeData.GridYDim);
            byte[] zDimInBytes = BitConverter.GetBytes(volumeData.GridZDim);
            using (var fs = new FileStream(saveFileLocation, FileMode.Create, FileAccess.Write))
            {
                // Write grid dimensions
                fs.Write(xDimInBytes, 0, xDimInBytes.Length);
                fs.Write(yDimInBytes, 0, yDimInBytes.Length);
                fs.Write(zDimInBytes, 0, zDimInBytes.Length);
                
                // Write grid data
                fs.Write(bitsInBytes, 0, bitsInBytes.Length);
            }

            // update async operation state
            operation.IsDone = true;
        }
    }
}