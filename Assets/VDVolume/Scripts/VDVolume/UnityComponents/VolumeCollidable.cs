using System;
using UnityEngine;
using VDVolume.DataStructure;
using VDVolume.Model;
using VDVolume.Modifiers.Cutting;

namespace VDVolume.UnityComponents
{
    /// This script generates a grid of BoxColliders in the close vicinity of the attached object. The colliders
    /// are aligned with the volume, are only enabled when matching voxels of the volume and provide VoxelData with
    /// the VoxelData component.
    /// <summary>
    /// Allows custom objects to collide with the volume.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class VolumeCollidable : MonoBehaviour
    {
        /// - Solid: This will collide the collidable object with solid voxels. Suitable when cutting voxels.
        /// - Removed: This will collide the collidable object with removed voxels. Suitable when restoring voxels.
        /// - None: This will disable the collision.
        /// <summary>
        /// Sets the collision type of the script.
        /// </summary>
        public VolumeCollisionType CollisionType
        {
            get => _collisionType;
            set
            {
                _collisionType = value;
                switch (value)
                {
                    case VolumeCollisionType.Solid:
                        _checkVolumeCollisionType = voxelState =>
                            voxelState == VoxelState.Visible || voxelState == VoxelState.Solid;
                        break;
                    case VolumeCollisionType.Removed:
                        _checkVolumeCollisionType = voxelState => voxelState == VoxelState.Removed;
                        break;
                    case VolumeCollisionType.None:
                        if (_colliderGrid != null)
                        {
                            for (int i = 0; i < _colliderGrid.Length; i++)
                            {
                                _colliderGrid[i].GetComponent<BoxCollider>().enabled = false;
                            }
                        }
                        break;
                }
            }
        }

        [SerializeField]
        private Volume vdVolume = null;

        private Collider _collider;
        private Vector3 _colliderBoundsSize;
        private Vector3 _colliderBoundsExtents;
        private GameObject[] _colliderGrid;
        private int _colliderXDim;
        private int _colliderYDim;
        private int _colliderZDim;
        private int _colliderCount;
        private float _scale;
        private Func<VoxelState, bool> _checkVolumeCollisionType;
        private VolumeCollisionType _collisionType = VolumeCollisionType.None;
        /// The VolumeCollidable script will create a grid of colliders around your object when voxels are in the close vicinity.
        /// Depending on the resolution of the volume or the scaling of your collidable object the VolumeCollidable script will
        /// create too many colliders which results in bad performance. To prevent this there is a limit to how many of these
        /// colliders can be generated. You can change this with this.
        /// The default is set to 8.000. However you can change this value however you like.
        private const int ColliderLimit = 8000;
        
        public enum VolumeCollisionType
        {
            Solid,
            Removed,
            None
        }
        
        void Start()
        {
            _collider = GetComponent<Collider>();
            
            if (vdVolume.Initialized)
            {
                InitCollidable();
            }
            else
            {
                vdVolume.OnVolumeInitialized.RemoveListener(InitCollidable);
                vdVolume.OnVolumeInitialized.AddListener(InitCollidable);
            }
        }

        /// If you want to see the colliders in the scene hierarchy the VolumeCollidable script generates, you have to
        /// call the method SetHideInHierarchy(). Per default the colliders are hidden.
        /// <summary>
        /// Makes the colliders visible.
        /// </summary>
        /// <param name="hidden">Decides if the colliders are hidden.</param>
        public void SetHideInHierarchy(bool hidden)
        {
            if (_colliderGrid == null) return;

            HideFlags flag = hidden ? HideFlags.HideInHierarchy : HideFlags.None;
            
            for (int i = 0; i < _colliderGrid.Length; i++)
            {
                _colliderGrid[i].hideFlags = flag;
            }
        }
        
        /// <summary>
        /// Inits the collider grid
        /// </summary>
        private void InitCollidable()
        {
            vdVolume.OnVolumeScaleChanged.RemoveListener(InitCollidable);
            vdVolume.OnVolumeScaleChanged.AddListener(InitCollidable);
            vdVolume.OnVolumeSavingStarted.RemoveListener(DeactivateScript);
            vdVolume.OnVolumeSavingStarted.AddListener(DeactivateScript);
            vdVolume.OnVolumeSavingEnded.RemoveListener(ActivateScript);
            vdVolume.OnVolumeSavingEnded.AddListener(ActivateScript);
            _scale = vdVolume.VolumeData.Scale;
            SetColliderData();
            // Calculate how many box colliders would be needed to encapsulate the object collider
            // There should be one more collider box one each side -> 2*_scale
            _colliderXDim = (int) Math.Ceiling((_colliderBoundsSize.x + (2 * _scale)) / _scale);
            _colliderYDim = (int) Math.Ceiling((_colliderBoundsSize.y + (2 * _scale)) / _scale);
            _colliderZDim = (int) Math.Ceiling((_colliderBoundsSize.z + (2 * _scale)) / _scale);
            _colliderCount = _colliderXDim * _colliderYDim * _colliderZDim;

            if (_colliderCount > ColliderLimit)
            {
                Debug.Log("Resolution of the volume is too high or the scaling of the 'VolumeCollidable' object is too big. The" +
                          " current configuration would create " + _colliderCount + " colliders around this object. For preservation" +
                          " of the performance the limit is " + ColliderLimit + ". Your hardware may vary, feel free to change this limit" +
                          " inside 'VolumeCollidable.cs'.");
                gameObject.SetActive(false);
                return;
            }
            
            _colliderGrid = new GameObject[_colliderCount];

            GameObject colliderParent = new GameObject()
            {
                name = "ColliderGrid",
                hideFlags = HideFlags.HideInHierarchy
            };
            
            for (int i = 0; i < _colliderCount; i++)
            {
                GameObject colliderObject = new GameObject()
                {
                    name = "DynamicCollider",
                    hideFlags = HideFlags.HideInHierarchy
                };
                colliderObject.transform.SetParent(colliderParent.transform);
                colliderObject.AddComponent<BoxCollider>();
                BoxCollider boxCollider = colliderObject.GetComponent<BoxCollider>();
                boxCollider.size = new Vector3(_scale,_scale,_scale);
                boxCollider.center = new Vector3(_scale * 0.5f, _scale * 0.5f, _scale * 0.5f);
                boxCollider.enabled = false;
                colliderObject.AddComponent<ColliderData>();
                _colliderGrid[i] = colliderObject;
            }
        }

        /// Use this when the object collider scale has changed to update the local colliders this component creates.
        /// Note: If you scale your object and call this method in the same frame you have to call Physics.SyncTransforms()
        /// before calling this method. Otherwise the collider of your scaled object hasn't changed. Alternatively you
        /// could wait for the next frame and pass the collider inside Unity's Update() function.
        /// <summary>
        /// ReInits the the collider grid. Must be called when the object collider scale has changed.
        /// </summary>
        /// <param name="collidableCollider">The collider of the collidable object.</param>
        public void ReInitCollidable(Collider collidableCollider)
        {
            _collider = collidableCollider;
            InitCollidable();
        }

        /// <summary>
        /// Draws red cubes for colliders when the collidable object is near the volume grid and the colliders matches a voxel.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (_colliderGrid == null || !vdVolume.DrawCollidableGizmo) return;
            for (int i = 0; i < _colliderGrid.Length; i++)
            {
                VoxelData voxelData = _colliderGrid[i].GetComponent<ColliderData>().VoxelData;
                if (_colliderGrid[i].GetComponent<BoxCollider>().enabled && _checkVolumeCollisionType(voxelData.State))
                {
                    Gizmos.color = Color.red;
                    Vector3 pos = _colliderGrid[i].transform.position;
                    Gizmos.matrix = Matrix4x4.TRS(pos, vdVolume.transform.rotation,
                        new Vector3(1f,1f,1f));
                    Gizmos.DrawWireCube(new Vector3(_scale*0.5f,_scale*0.5f,_scale*0.5f),new Vector3(_scale,_scale,_scale));
                }
            }
            Gizmos.matrix = Matrix4x4.identity;
        }

        /// This will also add ColliderData components to the colliders game object.
        /// <summary>
        /// This will update the local colliders according to the current position of the collidable object.
        /// </summary>
        private void FixedUpdate()
        {
            if (!vdVolume.Initialized || _collisionType == VolumeCollisionType.None) return;
            
            // Init values
            Transform volumeTransform = vdVolume.transform;
            Quaternion volumeRotation = volumeTransform.rotation;
            Vector3 volumePos = volumeTransform.position;
            int i = 0;
            
            // Set start and end position of the collider grid
            Vector3 thisPos = _collider.bounds.center;
            Vector3 startPos = thisPos - _colliderBoundsExtents - new Vector3(_scale,_scale,_scale);
            Vector3 endPos = thisPos + _colliderBoundsExtents + new Vector3(_scale,_scale,_scale);
            // Rotate start and end position with the volume rotation
            Vector3 startRotated = volumeRotation * (startPos - thisPos) + thisPos;
            Vector3 endRotated = volumeRotation * (endPos - thisPos) + thisPos;
            
            // Snap start and end position to the corresponding voxel origin
            Vector3 startLocalPoint = vdVolume.VolumeData.WorldToAAVolLocalCoord(startRotated);
            Vector3 endLocalPoint = vdVolume.VolumeData.WorldToAAVolLocalCoord(endRotated);
            Vector3 startLocalAlignedPoint = vdVolume.VolumeData.SnapAAVolLclCoordToLclVxlOrigin(startLocalPoint);
            Vector3 endLocalAlignedPoint = vdVolume.VolumeData.SnapAAVolLclCoordToLclVxlOrigin(endLocalPoint);
            Vector3 startWorldAlignedPoint = startLocalAlignedPoint + volumePos;
            Vector3 endWorldAlignedPoint = endLocalAlignedPoint + volumePos;
            
            // Iterate through all colliders in the grid surrounding this object
            for (float x = startWorldAlignedPoint.x; x < endWorldAlignedPoint.x; x+= _scale)
            {
                for (float y = startWorldAlignedPoint.y; y < endWorldAlignedPoint.y; y+= _scale)
                {
                    for (float z = startWorldAlignedPoint.z; z < endWorldAlignedPoint.z; z+= _scale)
                    {
                        if (i >= _colliderCount) return;

                        // Get the voxel index
                        Vector3 localHitPoint = new Vector3(x,y,z) - volumePos;
                        Vector3Int hitVoxelIndex = vdVolume.VolumeData.GetVxlIdxFromAAVolLocalCoord(localHitPoint);
                        BoxCollider boxCollider = _colliderGrid[i].GetComponent<BoxCollider>();
                        boxCollider.enabled = false;
                        // Look if the voxel is visible or is not removed
                        if (vdVolume.VolumeData.TryGetVoxel(hitVoxelIndex.x, hitVoxelIndex.y, hitVoxelIndex.z,
                            out VoxelData voxelData) && _checkVolumeCollisionType(voxelData.State))
                        {
                            // Update collider and set it to active
                            _colliderGrid[i].transform.position =
                                volumeRotation * (new Vector3(x, y, z) - volumePos) +
                                volumePos; // Rotate point back to the modified origin
                            _colliderGrid[i].transform.rotation = volumeRotation;
                            boxCollider.enabled = true;
                            _colliderGrid[i].GetComponent<ColliderData>().VoxelData = voxelData;
                        }

                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Sets all needed collider data.
        /// </summary>
        private void SetColliderData()
        {
            _colliderBoundsSize = Vector3.Max(_collider.bounds.size, new Vector3(_scale, _scale, _scale));
            _colliderBoundsExtents = Vector3.Scale(_colliderBoundsSize, new Vector3(0.5f,0.5f,0.5f));
        }

        /// <summary>
        /// Callback method that deactivates the script when volume is saving.
        /// </summary>
        private void DeactivateScript()
        {
            enabled = false;
        }
        
        /// <summary>
        /// Callback method that activates the script when volume has saved.
        /// </summary>
        private void ActivateScript()
        {
            enabled = true;
        }
    }
}