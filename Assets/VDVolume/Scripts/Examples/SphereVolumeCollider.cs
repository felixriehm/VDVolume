using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using VDVolume.Model;
using VDVolume.Modifiers.Cutting;
using VDVolume.UnityComponents;

namespace Examples
{
    // This example does not requires the volume to have a collider because the voxel colliders are locally
    // generated around this game object.
    // This game object has to have a rigidbody while the volume doesn't need it. This game object
    // also needs a collider with 'IsTrigger' disabled. 
    [RequireComponent(typeof(VolumeCollidable))]
    public class SphereVolumeCollider : MonoBehaviour
    {
        [SerializeField]
        private Volume vdVolume = null;

        private VolumeCollidable _volumeCollidable;
        private Action<VoxelData> _volumeCollisionTypeAction;

        private void Awake()
        {
            _volumeCollidable = GetComponent<VolumeCollidable>();
            _volumeCollisionTypeAction = voxelData => vdVolume.VolumeModifiers.Cutting.RemoveVoxel(voxelData.X,voxelData.Y,voxelData.Z);
            _volumeCollidable.CollisionType = VolumeCollidable.VolumeCollisionType.Solid;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                switch (_volumeCollidable.CollisionType)
                {
                    case VolumeCollidable.VolumeCollisionType.Solid:
                        _volumeCollisionTypeAction = voxelData => vdVolume.VolumeModifiers.Restoring.AddVoxel(voxelData.X,voxelData.Y,voxelData.Z);
                        _volumeCollidable.CollisionType = VolumeCollidable.VolumeCollisionType.Removed;
                        break;
                    case VolumeCollidable.VolumeCollisionType.Removed:
                        _volumeCollisionTypeAction = voxelData => {};
                        _volumeCollidable.CollisionType = VolumeCollidable.VolumeCollisionType.None;
                        break;
                    case VolumeCollidable.VolumeCollisionType.None:
                        _volumeCollisionTypeAction = voxelData => vdVolume.VolumeModifiers.Cutting.RemoveVoxel(voxelData.X,voxelData.Y,voxelData.Z);
                        _volumeCollidable.CollisionType = VolumeCollidable.VolumeCollisionType.Solid;
                        break;
                }
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                vdVolume.VolumeModifiers.Undo.UndoVoxels();
            }
            
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
                Physics.SyncTransforms();
                _volumeCollidable.ReInitCollidable(GetComponent<Collider>());
            }

            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                transform.localScale = new Vector3(0.5f,0.5f,0.5f);
                Physics.SyncTransforms();
                _volumeCollidable.ReInitCollidable(GetComponent<Collider>());
            }
        }

        private void OnCollisionEnter(Collision collisionInfo)
        {
            for (int i = 0; i < collisionInfo.contacts.Length; i++)
            {
                // Alternatively you can get the center of the collider (collisionInfo.contacts[i].otherCollider.bounds.center)
                // and then convert the world position to index space and get the voxel with VolumeData.GetVoxel
                if(collisionInfo.contacts[i].otherCollider.gameObject.TryGetComponent<ColliderData>(out ColliderData colliderData))
                {
                    _volumeCollisionTypeAction(colliderData.VoxelData);
                }
            }
        }
    }
}