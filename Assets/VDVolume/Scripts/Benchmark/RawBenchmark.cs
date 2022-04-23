using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Debugging;
using VDVolume.Model;
using VDVolume.Modifiers;

namespace Benchmark
{
    public static class RawBenchmark
    {
        public static void Run()
        {
             int gridX = 512;
             int gridY = 176;
             int gridZ = 348;
             int xMax = 64;
             int yMax = 64;
             int zMax = 64;
             String log = "";
        
            var cSize = GetArg ("-cSize");
            var gridXYZ = GetArgs ("-gridXYZ", 3);

            if (cSize != null)
            {
                int sizeInt = Int32.Parse(cSize);
                xMax = sizeInt;
                yMax = sizeInt;
                zMax = sizeInt;
                log += "Received cSize: " + xMax + Environment.NewLine;
            }
            else
            {
                log += "No cSize received. Taking default cSize: " + xMax + Environment.NewLine;
            }
            
            if (gridXYZ != null)
            {
                gridX = Int32.Parse(gridXYZ[0]);
                gridY = Int32.Parse(gridXYZ[1]);
                gridZ = Int32.Parse(gridXYZ[2]);
                log += "Received grid size x: " + gridX + " y: " + gridY + " z: " + gridZ + Environment.NewLine;
            }
            else
            {
                log += "No grid size received. Taking default size x: " + gridX + " y: " + gridY + " z: " + gridZ + Environment.NewLine;
            }
            
            // create voxel grid iterative 
            float timeNow = Time.realtimeSinceStartup;
            
            VolumeData volumeData = new VolumeData(gridX,gridY,gridZ,1.0f,16,null);
            VolumeModifiers modifiers = new VolumeModifiers(volumeData);
            DebugManager.InitRandomData(volumeData);
            
            float timeThen = Time.realtimeSinceStartup;
            log += "Create time (sec): " + (timeThen - timeNow) + Environment.NewLine;

            // delete voxels
            timeNow = Time.realtimeSinceStartup;

            for (int x = 0; x < xMax; x++)
            {
                for (int y = 0; y < yMax; y++)
                {
                    for (int z = 0; z < zMax; z++)
                    {
                        modifiers.Cutting.RemoveVoxel(x,y,z);
                    }
                }
            }
            timeThen = Time.realtimeSinceStartup;
            log += "Delete time (sec): " + (timeThen - timeNow) + Environment.NewLine;
            
            // undo voxels
            timeNow = Time.realtimeSinceStartup;
            for (int x = 0; x < xMax; x++)
            {
                for (int y = 0; y < yMax; y++)
                {
                    for (int z = 0; z < zMax; z++)
                    {
                        modifiers.Restoring.AddVoxel(x,y,z);
                    }
                }
            }
            timeThen = Time.realtimeSinceStartup;
            log += "Add/Undo time (sec): " + (timeThen - timeNow) + Environment.NewLine;
            
            // modify voxels
            byte color;
            timeNow = Time.realtimeSinceStartup;
            for (int x = 0; x < xMax; x++)
            {
                for (int y = 0; y < yMax; y++)
                {
                    for (int z = 0; z < zMax; z++)
                    {
                        color = Convert.ToByte((x + y + z + 2) % 256); 
                        volumeData.SetVoxelColor(x,y,z, color);
                    }
                }
            }
            timeThen = Time.realtimeSinceStartup;
            log += "Modify time (sec): " + (timeThen - timeNow) + Environment.NewLine;
            
            // Remove cube parallel. Cube will be removed in the middle of the volume.
            timeNow = Time.realtimeSinceStartup;
            modifiers.Cutting.RemoveCubeParallel(new Vector3Int(gridX/2, gridY/2,gridZ/2), xMax/2); // xMax == cSize
            timeThen = Time.realtimeSinceStartup;
            log += "Remove cube parallel time (sec): " + (timeThen - timeNow) + Environment.NewLine;
            
            // Add cube parallel. Cube will be added in the middle of the volume.
            timeNow = Time.realtimeSinceStartup;
            modifiers.Restoring.AddCubeParallel(new Vector3Int(gridX/2, gridY/2,gridZ/2), xMax/2); // xMax == cSize
            timeThen = Time.realtimeSinceStartup;
            log += "Add cube parallel time (sec): " + (timeThen - timeNow) + Environment.NewLine;
            
            
            File.WriteAllText("vdvolume.log", log);
            
            volumeData.CleanUp();
            modifiers.CleanUp();
            Application.Quit();
        }

        private static string[] GetEnvCmdLineArgs()
        {
            //return new string[] {"Application.exe", "-raw"};
            return System.Environment.GetCommandLineArgs();
        }
        
        public static bool ArgExists(string name)
        {
            var args = GetEnvCmdLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name)
                {
                    return true;
                }
            }
            return false;
        }
        
        private static string GetArg(string name)
        {
            var args = GetEnvCmdLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    
        private static List<string> GetArgs(string name, int number)
        {
            var args = GetEnvCmdLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    List<string> list = new List<string>();
                    for (int j = 1; j < number + 1; j++)
                    {
                        list.Add(args[i + j]);
                    }
                    return list;
                }
            }
            return null;
        }
    }
}