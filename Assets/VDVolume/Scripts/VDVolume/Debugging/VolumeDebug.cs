using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VDVolume.DataStructure;
using VDVolume.Model;
using VDVolume.UnityComponents;

namespace VDVolume.Debugging
{
    [ExcludeFromCoverage]
    internal class VolumeDebug : Volume
    {
        private string _debugGridXDim = "24";
        private string _debugGridYDim = "24";
        private string _debugGridZDim = "24";
        private string _debugGridScale = "0.25";
        private string _debugChangeGridScale = "0.5";
        private string _debugChunkDim = "16";
        private string _debugFilterValue = "22";
        private string _debugFilterRangeBegin = "22";
        private string _debugFilterRangeEnd = "42";
        private string _debugCuttingX = "7";
        private string _debugCuttingY = "7";
        private string _debugCuttingZ = "7";
        private string _debugCuttingRadius = "2";
        private string _debugRestoringX = "7";
        private string _debugRestoringY = "7";
        private string _debugRestoringZ = "7";
        private string _debugRestoringRadius = "2";
        private string _debugCuttingVoxelX = "7";
        private string _debugCuttingVoxelY = "7";
        private string _debugCuttingVoxelZ = "7";
        private string _debugRestoringVoxelX = "7";
        private string _debugRestoringVoxelY = "7";
        private string _debugRestoringVoxelZ = "7";
        private string _debugCuttinSphereX = "7";
        private string _debugCuttinSphereY = "7";
        private string _debugCuttinSphereZ = "7";
        private string _debugCuttingSphereRadius = "2";
        private bool _drawVisibleChunks = false;
        private bool _drawVoxelStateInside = false;
        private bool _drawVoxelStateOutside = false;
        private bool _drawVoxelStateSurface = false;

        private void OnGUI()
        {
            // Info: No user input check applied for debug menu
            GUILayout.BeginArea (new Rect (0,0,300,470), "Grid information", GUI.skin.window);
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Grid dimension:");
            GUILayout.Label (VolumeData?.GridXDim + "x" + VolumeData?.GridYDim + "x" + VolumeData?.GridZDim);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Voxel count:");
            GUILayout.Label (VolumeData?.GridVoxelCount.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Chunk count:");
            GUILayout.Label (VolumeData?.GlobalChunkCount.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Chunk dimension:");
            GUILayout.Label (VolumeData?.GlobalChunkXDim + "x" + VolumeData?.GlobalChunkYDim + "x" + VolumeData?.GlobalChunkZDim);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Chunk dimension:");
            GUILayout.Label (VolumeData?.LocalChunkDim.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Chunk voxel count:");
            GUILayout.Label (VolumeData?.LocalChunkVoxelCount.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Visible chunks:");
            GUILayout.Label (VolumeData?.SparseDataRaw.VisibleChunks.Count().ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Visible voxels:");
            GUILayout.Label (VolumeData?.SparseDataRaw.DirtyChunkVisibleVoxels.Count().ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Scale:");
            GUILayout.Label (VolumeData?.Scale.ToString());
            GUILayout.EndHorizontal();
            GUILayout.Label ("Create grid:");
            GUILayout.BeginHorizontal();
            GUILayout.Label ("XDim:");
            _debugGridXDim = GUILayout.TextField(_debugGridXDim.ToString());
            GUILayout.Label ("YDim:");
            _debugGridYDim = GUILayout.TextField(_debugGridYDim.ToString());
            GUILayout.Label ("ZDim:");
            _debugGridZDim = GUILayout.TextField(_debugGridZDim.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Scale:");
            _debugGridScale = GUILayout.TextField(_debugGridScale.ToString());
            GUILayout.Label ("Chunk dim:");
            _debugChunkDim = GUILayout.TextField(_debugChunkDim.ToString());
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Run"))
                TestAndDebugInitRandomVolume(int.Parse(_debugGridXDim), int.Parse(_debugGridYDim), int.Parse(_debugGridZDim),
                    float.Parse(_debugGridScale), int.Parse(_debugChunkDim));
            if (GUILayout.Button("Save")) 
                AsyncSaveVolume();
            if (GUILayout.Button("Init from user data")) 
                InitVolume();
            GUILayout.Label ("Change scale:");
            _debugChangeGridScale = GUILayout.TextField(_debugChangeGridScale.ToString());
            if (GUILayout.Button("Run"))
                SetScale(float.Parse(_debugChangeGridScale));
            GUILayout.EndArea();
            
            GUILayout.BeginArea (new Rect (0,490,300,340), "Filtering", GUI.skin.window);
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Last range filter:");
            GUILayout.Label ("Begin: " + VolumeModifiers?.Filter.LastFilterRangeBegin+" End: " + VolumeModifiers?.Filter.LastFilterRangeEnd);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Last value filter:");
            GUILayout.Label ("Value: " + VolumeModifiers?.Filter.LastFilterValue);
            GUILayout.EndHorizontal();
            GUILayout.Label ("Range filter:");
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Begin:");
            _debugFilterRangeBegin = GUILayout.TextField(_debugFilterRangeBegin);
            GUILayout.Label ("End:");
            _debugFilterRangeEnd = GUILayout.TextField(_debugFilterRangeEnd);
            GUILayout.EndHorizontal();
            if(GUILayout.Button("Run") && Initialized)
                VolumeModifiers?.Filter.FilterValueRange(int.Parse(_debugFilterRangeBegin),int.Parse(_debugFilterRangeEnd));
            GUILayout.Label ("Value filter:");
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Value:");
            _debugFilterValue = GUILayout.TextField(_debugFilterValue.ToString());
            GUILayout.EndHorizontal();
            if(GUILayout.Button("Run") && Initialized)
                VolumeModifiers?.Filter.FilterValue(int.Parse(_debugFilterValue));
            GUILayout.EndArea();
            
            // Draw calls
            GUILayout.BeginArea (new Rect (340,0,300,200), "Debug draws", GUI.skin.window);
            DrawCollidableGizmo = GUILayout.Toggle (DrawCollidableGizmo, "Collidable boxes");
            _drawVisibleChunks = GUILayout.Toggle (_drawVisibleChunks, "Visible chunks");
            _drawVoxelStateInside = GUILayout.Toggle (_drawVoxelStateInside, "Inside voxels");
            _drawVoxelStateOutside = GUILayout.Toggle (_drawVoxelStateOutside, "Outside voxels");
            _drawVoxelStateSurface = GUILayout.Toggle (_drawVoxelStateSurface, "Surface voxels");
            GUILayout.EndArea();

			// Application
            GUILayout.BeginArea (new Rect (670,0,100,60), "Application", GUI.skin.window);
            if(GUILayout.Button("Quit"))
                Application.Quit();
            GUILayout.EndArea();
            
            // Cutting and restoring
            GUILayout.BeginArea (new Rect (Screen.width - 300,0,300,470), "Cutting/Restoring", GUI.skin.window);
            // Cube
            GUILayout.Label ("Cut cube:");
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Center:");
            GUILayout.Label ("X:");
            _debugCuttingX = GUILayout.TextField(_debugCuttingX.ToString());
            GUILayout.Label ("Y:");
            _debugCuttingY = GUILayout.TextField(_debugCuttingY.ToString());
            GUILayout.Label ("Z:");
            _debugCuttingZ = GUILayout.TextField(_debugCuttingZ.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Radius:");
            _debugCuttingRadius = GUILayout.TextField(_debugCuttingRadius.ToString());
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Run"))
                VolumeModifiers?.Cutting.RemoveCubeParallel(new Vector3Int(int.Parse(_debugCuttingX), int.Parse(_debugCuttingY), int.Parse(_debugCuttingZ)),
                    int.Parse(_debugCuttingRadius));
            GUILayout.Label ("Restore cube:");
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Center:");
            GUILayout.Label ("X:");
            _debugRestoringX = GUILayout.TextField(_debugRestoringX.ToString());
            GUILayout.Label ("Y:");
            _debugRestoringY = GUILayout.TextField(_debugRestoringY.ToString());
            GUILayout.Label ("Z:");
            _debugRestoringZ = GUILayout.TextField(_debugRestoringZ.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Radius:");
            _debugRestoringRadius = GUILayout.TextField(_debugRestoringRadius.ToString());
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Run"))
                VolumeModifiers?.Restoring.AddCubeParallel(new Vector3Int(int.Parse(_debugRestoringX), int.Parse(_debugRestoringY), int.Parse(_debugRestoringZ)),
                    int.Parse(_debugRestoringRadius));

            // Voxel
            GUILayout.Label ("Cut voxel:");
            GUILayout.BeginHorizontal();
            GUILayout.Label ("X:");
            _debugCuttingVoxelX = GUILayout.TextField(_debugCuttingVoxelX.ToString());
            GUILayout.Label ("Y:");
            _debugCuttingVoxelY = GUILayout.TextField(_debugCuttingVoxelY.ToString());
            GUILayout.Label ("Z:");
            _debugCuttingVoxelZ = GUILayout.TextField(_debugCuttingVoxelZ.ToString());
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Run"))
                VolumeModifiers?.Cutting.RemoveVoxel(int.Parse(_debugCuttingVoxelX), int.Parse(_debugCuttingVoxelY), int.Parse(_debugCuttingVoxelZ));
            GUILayout.Label ("Restore voxel:");
            GUILayout.BeginHorizontal();
            GUILayout.Label ("X:");
            _debugRestoringVoxelX = GUILayout.TextField(_debugRestoringVoxelX.ToString());
            GUILayout.Label ("Y:");
            _debugRestoringVoxelY = GUILayout.TextField(_debugRestoringVoxelY.ToString());
            GUILayout.Label ("Z:");
            _debugRestoringVoxelZ = GUILayout.TextField(_debugRestoringVoxelZ.ToString());
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Run"))
                VolumeModifiers?.Restoring.AddVoxel(int.Parse(_debugRestoringVoxelX), int.Parse(_debugRestoringVoxelY), int.Parse(_debugRestoringVoxelZ));
            
            // Sphere
            GUILayout.Label ("Cut sphere:");
            GUILayout.BeginHorizontal();
            GUILayout.Label ("X:");
            _debugCuttinSphereX = GUILayout.TextField(_debugCuttinSphereX.ToString());
            GUILayout.Label ("Y:");
            _debugCuttinSphereY = GUILayout.TextField(_debugCuttinSphereY.ToString());
            GUILayout.Label ("Z:");
            _debugCuttinSphereZ = GUILayout.TextField(_debugCuttinSphereZ.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Radius:");
            _debugCuttingSphereRadius = GUILayout.TextField(_debugCuttingSphereRadius.ToString());
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Run"))
                VolumeModifiers?.Cutting.RemoveSphere(
                    new Vector3Int(int.Parse(_debugCuttinSphereX), int.Parse(_debugCuttinSphereY),
                        int.Parse(_debugCuttinSphereZ)), int.Parse(_debugCuttingSphereRadius));

            // Undo
            GUILayout.Label ("Undo:");
            if (GUILayout.Button("Run"))
                VolumeModifiers?.Undo.UndoVoxels();
            GUILayout.EndArea();
        }
        
        private void OnDrawGizmos()
        {
            if (!_drawVisibleChunks) return;
            var visibleChunks = VolumeData.SparseDataRaw.VisibleChunks.GetKeyArray(Allocator.Temp);
            Vector3 volumePos = _transform.position;
            Quaternion volumeRotation = _transform.rotation;
            Gizmos.color = Color.blue;
            for (int i = 0; i < visibleChunks.Length; i++)
            {
                float boundsSize = VolumeData.LocalChunkDim * VolumeData.Scale;
                Vector3Int chunkIndex3D = VolumeDataUtil.ChunkCoord.To3D(visibleChunks[i],VolumeData.GlobalChunkXDim,VolumeData.GlobalChunkYDim);
                Vector3 localChunkPos = new Vector3(chunkIndex3D.x*boundsSize,chunkIndex3D.y*boundsSize,chunkIndex3D.z*boundsSize);

                if (_drawVoxelStateInside || _drawVoxelStateOutside || _drawVoxelStateSurface)
                {
                    for (int x = 0; x < _gridData.LocalChunkDim; x++)
                    {
                        for (int y = 0; y < _gridData.LocalChunkDim; y++)
                        {
                            for (int z = 0; z < _gridData.LocalChunkDim; z++)
                            {
                                Vector3Int voxelIndex = new Vector3Int((chunkIndex3D.x * _gridData.LocalChunkDim) + x,
                                    (chunkIndex3D.y * _gridData.LocalChunkDim) + y,
                                    (chunkIndex3D.z * _gridData.LocalChunkDim) + z);

                                if (voxelIndex.x >= _gridData.GridXDim || voxelIndex.y >= _gridData.GridYDim ||
                                    voxelIndex.z >= _gridData.GridZDim)
                                {
                                    continue;
                                }
                                
                                VoxelData voxelData = _denseData.Voxels[
                                    VolumeDataUtil.GridCoord.To1D(voxelIndex.x, voxelIndex.y, voxelIndex.z,
                                        _gridData.GridXDim, _gridData.GridYDim)];
                                
                                Vector3 voxelLocal = new Vector3(voxelIndex.x * VolumeData.Scale,
                                    voxelIndex.y * VolumeData.Scale, voxelIndex.z * VolumeData.Scale);
                                Vector3 voxelWorld = voxelLocal + volumePos;
                                Vector3 voxelRotated = volumeRotation * (voxelWorld - volumePos) + volumePos;

                                switch (voxelData.State)
                                {
                                    case VoxelState.Solid:
                                        if(_drawVoxelStateInside)
                                            Gizmos.DrawIcon(voxelRotated, "sv_icon_dot3_pix16_gizmo", false);
                                        break;
                                    case VoxelState.Removed:
                                        if(_drawVoxelStateOutside)
                                            Gizmos.DrawIcon(voxelRotated, "sv_icon_dot6_pix16_gizmo", false);
                                        break;
                                    case VoxelState.Visible:
                                        if(_drawVoxelStateSurface)
                                            Gizmos.DrawIcon(voxelRotated, "sv_icon_dot1_pix16_gizmo", false);
                                        break;
                                }
                            }
                        }
                    }
                }

                Vector3 worldChunkPos = localChunkPos + volumePos;
                Vector3 rotatedChunkPos = volumeRotation * (worldChunkPos - volumePos) + volumePos;
                
                Gizmos.matrix = Matrix4x4.TRS(rotatedChunkPos, volumeRotation, Vector3.one);
                
                Gizmos.DrawWireCube(new Vector3(boundsSize * 0.5f,boundsSize * 0.5f,boundsSize * 0.5f), new Vector3(boundsSize,boundsSize,boundsSize));
                #if UNITY_EDITOR
                Handles.Label(rotatedChunkPos, "(" + chunkIndex3D.x + "," + chunkIndex3D.y+ "," + chunkIndex3D.z + ")");
                #endif
            }
            Gizmos.matrix = Matrix4x4.identity;

            visibleChunks.Dispose();
        }
        
        internal void TestAndDebugInitRandomVolume(int x, int y, int z, float scale, int chunkSize)
        {
            BeforeVolumeDataInit();
            
            InitVolumeData(x, y, z, scale, chunkSize);
            DebugManager.InitRandomDataParallel(VolumeData);
            
            AfterVolumeDataInit();
            
            //float timeNow = Time.realtimeSinceStartup;
            /*float timeThen = Time.realtimeSinceStartup;
            String time = (timeThen - timeNow).ToString();
            File.WriteAllText("vdvolume.log", time);*/
            //Debug.Log((timeThen - timeNow));

            //VolumeModifier.RemoveCube(new Vector3Int(0,1,2),2);

            //VolumeModifiers.Filter.FilterValueRange(0, 0);
            //VolumeModifiers.Filter.FilterValueRange(70, 200);
        }
    }
}