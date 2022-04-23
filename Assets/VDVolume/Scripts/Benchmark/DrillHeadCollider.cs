using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VDVolume.Model;
using VDVolume.UnityComponents;

namespace Benchmark
{
    public class DrillHeadCollider : DrillHead
    {
        private VolumeCollidable _volumeCollidable;
        private Action<VoxelData> _volumeCollisionTypeAction;

        private void Awake()
        {
            _volumeCollidable = GetComponent<VolumeCollidable>();
            _volumeCollisionTypeAction = voxelData =>
                vdVolume.VolumeModifiers.Cutting.RemoveVoxel(voxelData.X, voxelData.Y, voxelData.Z);
            _volumeCollidable.CollisionType = VolumeCollidable.VolumeCollisionType.Solid;
        }

        public void SetCollisionType(VolumeCollidable.VolumeCollisionType collisionType)
        {
            switch (_volumeCollidable.CollisionType)
            {
                case VolumeCollidable.VolumeCollisionType.Solid:
                    _volumeCollisionTypeAction = voxelData =>
                        vdVolume.VolumeModifiers.Restoring.AddVoxel(voxelData.X, voxelData.Y, voxelData.Z);
                    _volumeCollidable.CollisionType = VolumeCollidable.VolumeCollisionType.Removed;
                    break;
                case VolumeCollidable.VolumeCollisionType.Removed:
                    _volumeCollisionTypeAction = voxelData => { };
                    _volumeCollidable.CollisionType = VolumeCollidable.VolumeCollisionType.None;
                    break;
                case VolumeCollidable.VolumeCollisionType.None:
                    _volumeCollisionTypeAction = voxelData =>
                        vdVolume.VolumeModifiers.Cutting.RemoveVoxel(voxelData.X, voxelData.Y, voxelData.Z);
                    _volumeCollidable.CollisionType = VolumeCollidable.VolumeCollisionType.Solid;
                    break;
            }
        }

        public override void SetSize(float size)
        {
            transform.localScale = new Vector3(size, size, size);;
            Physics.SyncTransforms();
            _volumeCollidable.ReInitCollidable(GetComponent<Collider>());
        }

        private void OnCollisionEnter(Collision collisionInfo)
        {
            for (int i = 0; i < collisionInfo.contacts.Length; i++)
            {
                // Alternatively you can get the center of the collider (collisionInfo.contacts[i].otherCollider.bounds.center)
                // and then convert the world position to index space and get the voxel with VolumeData.GetVoxel
                if (collisionInfo.contacts[i].otherCollider.gameObject
                    .TryGetComponent<ColliderData>(out ColliderData colliderData))
                {
                    _volumeCollisionTypeAction(colliderData.VoxelData);
                    _deletedVoxels++;
                }
            }
        }
    }
}
