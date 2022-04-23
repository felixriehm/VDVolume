using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using VDVolume.DataStructure;
using VDVolume.Model;
using VDVolume.Serialization;

namespace VDVolume.Editor.Export
{
    internal static class Exporter
    {
        internal static AsyncExportOperation AsyncExportVolumeToPng(string filePath, string destinationFolder)
        {
            AsyncExportOperation operation = new AsyncExportOperation();
            
            Thread t = new Thread(() => ExportVolumeToPNG(filePath, destinationFolder, operation));
            t.Start();
            operation.Thread = t;

            return operation;
        }
        
        // https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f
        private static void ExportVolumeToPNG(string filePath, string destinationFolder, AsyncExportOperation operation)
        {
            VolumeFile volumeFile = VolumeSerializer.ReadFile(filePath);
            BitArray dataInBits = new BitArray(volumeFile.Data);
            BitArray stateInBits = new BitArray(2);
            BitArray colorInBits = new BitArray(8);
            int[] intState = new int[1];
            byte[] byteColor = new byte[1];
            int gridVoxelCount = volumeFile.XDim * volumeFile.YDim * volumeFile.ZDim;
            int voxelIndex = 0;
            int progressCounter = 0;
            
            byte[] imageData = new byte[volumeFile.XDim * volumeFile.YDim * 4];
            GCHandle imageDataHandle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            Bitmap bitmap = new Bitmap(volumeFile.XDim, volumeFile.YDim, volumeFile.XDim * 4, PixelFormat.Format32bppArgb, imageDataHandle.AddrOfPinnedObject());

            int imageCounter = 0;
            int imagePixelCount = volumeFile.XDim * volumeFile.YDim;
            for (int voxelStride = 12*8; voxelStride < dataInBits.Length; voxelStride+=10)
            {
                // update async operation progress
                operation.Progress = progressCounter / (float) gridVoxelCount;
                
                // retrieve voxel state data
                for (int state = 0; state < 2; state++)
                {
                    stateInBits[state] = dataInBits[voxelStride + state];
                }
                stateInBits.CopyTo(intState, 0);
                VoxelState voxelState = (VoxelState) intState[0];
                
                // retrieve voxel color data
                for (int color = 0; color < 8; color++)
                {
                    colorInBits[color] = dataInBits[voxelStride + 2 + color];
                }
                colorInBits.CopyTo(byteColor, 0);
                
                // convert pixel data
                byte grayscaleValue = byteColor[0];
                byte alpha;
                alpha = voxelState == VoxelState.Solid || voxelState == VoxelState.Visible
                    ? (byte) 255
                    : (byte) 0;
                
                // needed to invert y-axis, otherwise voxelIndex would suffice
                int row = voxelIndex / volumeFile.XDim;
                int pixel = (volumeFile.XDim - 1) - (voxelIndex % volumeFile.XDim);
                int imageDataIndex = ((imagePixelCount - 1) - (row * volumeFile.XDim + pixel)) * 4;
                
                // set pixel data
                imageData[imageDataIndex] = grayscaleValue;
                imageData[imageDataIndex + 1] = grayscaleValue;
                imageData[imageDataIndex + 2] = grayscaleValue;
                imageData[imageDataIndex + 3] = alpha;

                // update progress
                progressCounter++;
                
                // save image file
                if (voxelIndex != 0 && voxelIndex % (imagePixelCount - 1) == 0)
                {
                    string path = destinationFolder + "/" +
                                  Convert.ToString(imageCounter)
                                      .PadLeft((int) Math.Floor(Math.Log10(volumeFile.ZDim) + 1), '0') + "_" +
                                  Path.GetFileNameWithoutExtension(filePath) + ".png";
                    bitmap.Save(path, ImageFormat.Png);

                    imageCounter++;
                    voxelIndex = 0;
                    continue;
                }

                voxelIndex++;
            }
            
            // clean up
            bitmap.Dispose();
            imageDataHandle.Free();
            
            // update async operation state
            operation.IsDone = true;
        }
    }
}