using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor;
using UnityEngine;
using VDVolume.Model;
using VDVolume.Serialization;

[assembly: InternalsVisibleTo("VDVolume.Tests.PlayMode"), InternalsVisibleTo("VDVolume.Tests.EditMode")]
namespace VDVolume.Editor.Import
{
    internal static class Importer
    {
        internal static AsyncImportOperation AsyncGenerateVolume(Bitmap[] bitmaps, string filePath)
        {
            AsyncImportOperation operation = new AsyncImportOperation();
            
            Thread t = new Thread(() => GenerateVolume(bitmaps,filePath, operation));
            t.Start();
            operation.Thread = t;

            return operation;

        }
        
        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/0e079998-ffd8-4828-879b-9a09e7b8bc97/pass-from-jpeg-to-rgb-and-read-pixels
        private static void GenerateVolume(Bitmap[] bitmaps, string filePath,AsyncImportOperation operation)
        {
            string finalFilePath = String.Concat(filePath, ".vdvolume");
            int xDim = bitmaps[0].Width;
            int yDim = bitmaps[0].Height;
            int zDim = bitmaps.Length;
            // We multiply by 10 because the bitmask returns 10 bits
            BitArray bits = new BitArray(xDim * yDim * zDim * 10);
            int bitsIndex = 0;
            BitArray bitmask = new BitArray(10);
            int progressCounter = 0;
            float progressMax = xDim * yDim * zDim;
            
            for (int i = 0; i < zDim; i++)
            {
                // lock bits so the garbage collector doesnt move the data around
                BitmapData data = bitmaps[i].LockBits(new Rectangle(0, 0, xDim, yDim),
                    ImageLockMode.ReadOnly, bitmaps[i].PixelFormat);

                // we have to use this because the width of the allocated memory for the bitmap 
                // is usually a little bit bigger than the actual image size (e.g. for resizing purposes)
                int bmpStride = data.Stride;
                unsafe
                {
                    byte* bmpPtr = (byte*) data.Scan0.ToPointer();

                    for (int row = yDim - 1; row >= 0; row--) // TODO: first colors then rows
                    {
                        for (int color = 0; color < xDim; color++)
                        {
                            operation.Progress =  progressCounter / progressMax;

                            bitmask.SetAll(false);

                            VoxelUtil.AddColorToBitMask(bitmask, bmpPtr[row * bmpStride + color]);

                            if (i == 0 || i == zDim - 1 || row == 0 || row == yDim - 1 || color == 0 || color == xDim - 1)
                            {
                                VoxelUtil.AddStateToBitMask(bitmask, VoxelState.Visible);
                            }
                            else
                            {
                                VoxelUtil.AddStateToBitMask(bitmask, VoxelState.Solid);
                            }
                            
                            for (int j = 0; j < bitmask.Length; j++)
                            {
                                bits[bitsIndex + j] = bitmask[j];
                            }

                            bitsIndex += bitmask.Length;
                            
                            progressCounter++;
                        }
                    }
                }
                bitmaps[i].UnlockBits(data);
            }
       
            byte[] bitsInBytes = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(bitsInBytes, 0);
            byte[] xDimInBytes = BitConverter.GetBytes(xDim);
            byte[] yDimInBytes = BitConverter.GetBytes(yDim);
            byte[] zDimInBytes = BitConverter.GetBytes(zDim);
            
            using (var fs = new FileStream(finalFilePath, FileMode.CreateNew, FileAccess.Write))
            {
                // Write grid dimensions
                fs.Write(xDimInBytes, 0, xDimInBytes.Length);
                fs.Write(yDimInBytes, 0, yDimInBytes.Length);
                fs.Write(zDimInBytes, 0, zDimInBytes.Length);
                
                // Write grid data
                fs.Write(bitsInBytes, 0, bitsInBytes.Length);
            }

            operation.IsDone = true;
        }

        internal class LoadFolderData
        {
            public string folderPath;
            public string fileNamePattern;
            public string recognizedImages;
            public Bitmap[] bitmaps;
            public string targetImagedim;
            public bool loadImageStackError;
            public bool imageFolderSelected;
            public string importErrorMsg;
            public string unkownValue;
        }
        
        internal static AsyncImportOperation AsyncLoadFolder(LoadFolderData data)
        {
            AsyncImportOperation operation = new AsyncImportOperation();

            Thread t = new Thread(() => LoadFolder(data, operation));
            t.Start();
            operation.Thread = t;

            return operation;
        }
        
        private static void LoadFolder(LoadFolderData loadData, AsyncImportOperation operation)
        {
            try
            {
                string[] files = Directory.GetFiles(loadData.folderPath, "*_" + loadData.fileNamePattern + ".jpg");
                if (files.Length == 0)
                {
                    loadData.recognizedImages = "0";
                    loadData.targetImagedim = loadData.unkownValue;
                    throw new Exception("No images found in folder '" + loadData.folderPath + "' with file name pattern '001_" + loadData.fileNamePattern + ".jpg'.");
                }

                loadData.recognizedImages = files.Length.ToString();
                Array.Sort(files, new OrderedImageComparer());
                loadData.bitmaps = new Bitmap[files.Length];
                int firstImageWidth = -1, firstImageHeight = -1;
                for (int i = 0; i < files.Length; i++)
                {
                    loadData.bitmaps[i] = new Bitmap(files[i]);
                    if (firstImageWidth == -1 || firstImageHeight == -1)
                    {
                        firstImageWidth = loadData.bitmaps[i].Width;
                        firstImageHeight = loadData.bitmaps[i].Height;
                        continue;
                    }

                    if (loadData.bitmaps[i].Width != firstImageWidth || loadData.bitmaps[i].Height != firstImageHeight)
                    {
                        throw new ImportException.InvalidImageDimensionException(
                            "At least one of your file has a different image dimension than the first one." + 
                            " That one file has the dimension: Width " + loadData.bitmaps[i].Width + " | " + "Height: " + loadData.bitmaps[i].Height + 
                            ". While the first one has: Width " + firstImageWidth + " | " + "Height: " + firstImageHeight);
                    }

                    if (loadData.bitmaps[i].PixelFormat != PixelFormat.Format8bppIndexed)
                    {
                        throw new ImportException.InvalidImageFormatException(
                            "At least one of your file has a wrong pixel format. Only 8 bit grayscale values are allowed." + 
                            " File '" + Path.GetFileName(files[i]) + "' has the image format: " + loadData.bitmaps[i].PixelFormat.ToString() +
                            " (System.Drawing.Imaging.PixelFormat Enumeration).");
                    }
                    
                    operation.Progress = i / (float)files.Length;
                }
                loadData.targetImagedim = "Height: " + firstImageHeight.ToString() + " | Width: " +
                                          firstImageWidth.ToString();

                loadData.loadImageStackError = false;
                loadData.imageFolderSelected = true;
                loadData.importErrorMsg = "";
            }
            catch (ImportException.InvalidImageDimensionException e)
            {
                loadData.importErrorMsg = "Error while reading the images: " + e.Message;
                loadData.loadImageStackError = true;
                loadData.imageFolderSelected = false;
            }
            catch (ImportException.InvalidImageFormatException e)
            {
                loadData.importErrorMsg = "Error while reading the images: " + e.Message;
                loadData.loadImageStackError = true;
                loadData.imageFolderSelected = false;
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.GetType() == typeof(ImportException.OrderedImageComparerException))
                {
                    loadData.importErrorMsg = "Error while comparing order numbers of two file names: " + e.InnerException.Message;
                    loadData.loadImageStackError = true;
                    loadData.imageFolderSelected = false;
                    return;
                }
                loadData.importErrorMsg = e.Message;
                loadData.loadImageStackError = true;
                loadData.imageFolderSelected = false;
            }

            operation.IsDone = true;
        }

        private class OrderedImageComparer : IComparer<string>{
            
            public int Compare(string x, string y)
            {
                if (x == null || y == null)
                    throw new ImportException.OrderedImageComparerException("A file name was null.");

                string[] xSplit = Path.GetFileName(x).Split('_');
                string[] ySplit = Path.GetFileName(y).Split('_');

                try
                {
                    if (Int32.Parse(xSplit[0]) < Int32.Parse(ySplit[0]))
                        return -1;
                
                    if (Int32.Parse(xSplit[0]) > Int32.Parse(ySplit[0]))
                        return 1;
                }
                catch (Exception e)
                {
                    throw new ImportException.OrderedImageComparerException("Order number comparison of two files failed: " + Path.GetFileName(x) + " and " + Path.GetFileName(y) + 
                                                                            ". It failed because of: " + e.Message);
                }
                
                throw new ImportException.OrderedImageComparerException("Two file names have the same order: " +
                                                                        Path.GetFileName(x) + " and " + Path.GetFileName(y));
            }
        }
    }
}