using Unity.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VDVolume.DataStructure;
using VDVolume.Model;

namespace VDVolume.Rendering
{
    [ExcludeFromCoverage]
    internal class VolumeRenderer
    {
        private SurfaceChunkRenderData[] _preAllocSurfaceChunkRenderData;
        private Mesh _instanceMesh;
        private uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
        private Transform _origin;
        private VolumeData _volumeData;
        private Bounds _chunkBounds;
        // VoxelProperties struct: a float vector3 for position and a float for color
        private const int StrideSize = sizeof(float) * 3 + sizeof(float);
        private SparseDataRaw _sparseData;
        private static readonly int VolumeTransform = Shader.PropertyToID("volumeTransform");
        private static readonly int VoxelProperties = Shader.PropertyToID("voxelProperties");

        internal void Initialize(Transform origin, VolumeData volumeData)
        {
            _volumeData = volumeData;
            _preAllocSurfaceChunkRenderData = new SurfaceChunkRenderData[_volumeData.GlobalChunkCount];
            _instanceMesh = VoxelMesh.Create();
            _origin = origin;
            // The chunks bounds should be fixed for early frustum culling. It should correspond to the chunk position
            // in world space with the corresponding chunk bounds size. Since the center of the bounding box
            // serves as origin for the rendering inside the shader, this creates a lot of afford when considering
            // rotations. However the volume might be in the sight of the camera the most of the time anyway
            // when viewed with a virtual reality headset while cutting.
            float boundsSize = _volumeData.LocalChunkDim * _volumeData.Scale;
            float boundsPaddingX = _volumeData.GlobalChunkXDim * boundsSize * 7;
            float boundsPaddingY = _volumeData.GlobalChunkYDim * boundsSize * 7;
            float boundsPaddingZ = _volumeData.GlobalChunkZDim * boundsSize * 7;
            Vector3 volumePos = _origin.position;
            _chunkBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(volumePos.x + boundsPaddingX,volumePos.y +boundsPaddingY,volumePos.z +boundsPaddingZ));
            Material instanceMaterial = Resources.Load<Material>("Material/InstancedVoxelMaterial");
            for (int i = 0; i < _preAllocSurfaceChunkRenderData.Length; i++)
            {
                _preAllocSurfaceChunkRenderData[i].ArgsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
                _preAllocSurfaceChunkRenderData[i].InstanceMaterial = UnityEngine.Object.Instantiate(instanceMaterial);
            }

            Shader.SetGlobalMatrix(VolumeTransform,
                Matrix4x4.TRS(_origin.position, _origin.rotation,
                    new Vector3(_volumeData.Scale, _volumeData.Scale, _volumeData.Scale)));

            _sparseData = _volumeData.SparseDataRaw;
        }

        /// <summary>
        /// Only visible voxels will be rendered. See 'SparseStorage.cs' for more information.
        /// </summary>
        internal void Render()
        {
            if (_origin.hasChanged)
            {
                // When the volume transform has change update the transform matrix 'volumeTransform' in the shader.
                // The matrix is used to transform the voxel positions (which are in index space) and mesh vertices,
                // in the shader to the correct world space coordinate.
                Shader.SetGlobalMatrix(VolumeTransform,
                    Matrix4x4.TRS(_origin.position, _origin.rotation,
                        new Vector3(_volumeData.Scale, _volumeData.Scale, _volumeData.Scale)));
            }
            
            int instanceCount;
            var surfaceChunks = _sparseData.VisibleChunks.GetKeyArray(Allocator.Temp);
            foreach (int chunkIndex1D in surfaceChunks)
            {
                // update compute buffers of modified surface chunks
                if (_sparseData.TmpModifiedVisibleChunks.ContainsKey(chunkIndex1D))
                {
                    if (_preAllocSurfaceChunkRenderData[chunkIndex1D].MeshPropertiesBuffer != null)
                        _preAllocSurfaceChunkRenderData[chunkIndex1D].MeshPropertiesBuffer.Release();
                    instanceCount = _sparseData.DirtyChunkVisibleVoxels.CountValuesForKey(chunkIndex1D);
                    _preAllocSurfaceChunkRenderData[chunkIndex1D].MeshPropertiesBuffer = new ComputeBuffer(instanceCount, StrideSize);
                    VoxelProperties[] properties = new VoxelProperties[instanceCount];
                    int i = 0;
                    foreach (var voxelData in _sparseData.DirtyChunkVisibleVoxels.GetValuesForKey(chunkIndex1D))
                    {
                        properties[i] = new VoxelProperties()
                        {
                            Position = new Vector3(voxelData.X,voxelData.Y,voxelData.Z),
                            Color = voxelData.Color
                        };
                        i++;
                    }
                    _preAllocSurfaceChunkRenderData[chunkIndex1D].MeshPropertiesBuffer.SetData(properties);
                    _preAllocSurfaceChunkRenderData[chunkIndex1D].InstanceMaterial.SetBuffer(VoxelProperties, _preAllocSurfaceChunkRenderData[chunkIndex1D].MeshPropertiesBuffer);
                    // Indirect args
                    _args[0] = (uint)_instanceMesh.GetIndexCount(0);
                    _args[1] = (uint)instanceCount;
                    _args[2] = (uint)_instanceMesh.GetIndexStart(0);
                    _args[3] = (uint)_instanceMesh.GetBaseVertex(0);
                    _preAllocSurfaceChunkRenderData[chunkIndex1D].ArgsBuffer.SetData(_args);
                }

                // render chunk buffer of surface chunks
                Graphics.DrawMeshInstancedIndirect(_instanceMesh, 0,
                    _preAllocSurfaceChunkRenderData[chunkIndex1D].InstanceMaterial,
                    _chunkBounds,
                    _preAllocSurfaceChunkRenderData[chunkIndex1D].ArgsBuffer);
            }
            surfaceChunks.Dispose();
        }

        internal void CleanUp()
        {
            for (int i = 0; i < _preAllocSurfaceChunkRenderData.Length; i++)
            {
                if (_preAllocSurfaceChunkRenderData[i].MeshPropertiesBuffer != null)
                    _preAllocSurfaceChunkRenderData[i].MeshPropertiesBuffer.Release();
                _preAllocSurfaceChunkRenderData[i].MeshPropertiesBuffer = null;

                if (_preAllocSurfaceChunkRenderData[i].ArgsBuffer != null)
                    _preAllocSurfaceChunkRenderData[i].ArgsBuffer.Release();
                _preAllocSurfaceChunkRenderData[i].ArgsBuffer = null;
            }
        }
    }
}