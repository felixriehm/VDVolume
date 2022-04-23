using System;
using UnityEngine;
using VDVolume.Model;
using VDVolume.UnityComponents;

namespace Benchmark
{
    public class DrillHeadRayCast : DrillHead
    {
        private Collider _collider;
        private Ray[] _rays;
        private Transform _transform;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _rays = new Ray[5];
            _transform = transform;
        }

        public override void SetSize(float size)
        {
            Vector3 colliderBoundsSize = new Vector3(size, size, size);
            transform.localScale = colliderBoundsSize;
        }

        void OnTriggerStay(Collider other)
        {
            if(!other.gameObject.TryGetComponent<Volume>(out Volume volume)) return;
            
            Vector3 pos = _transform.position;
            // build some rays facing z direction
            _rays[0] = new Ray(pos, _transform.forward);
            _rays[1] = new Ray(pos, _transform.right);
            _rays[2] = new Ray(pos, -_transform.right);
            _rays[3] = new Ray(pos, _transform.up);
            _rays[4] = new Ray(pos, -_transform.up);

            for (int i = 0; i < _rays.Length; i++)
            {
                if (vdVolume.VolumeData.PickFirstSolidVoxel(_rays[i], _collider.bounds.extents.z, out VoxelData voxelData, _collider.bounds.extents.z))
                {
                    Vector3Int centerOfCube = vdVolume.VolumeData.GetVoxelIndexFromWorldCoord(transform.position);
                    vdVolume.VolumeModifiers.Cutting.RemoveSphere(centerOfCube, (int) Math.Floor(_collider.bounds.extents.z/vdVolume.VolumeData.Scale));
                    return;
                }
            }
            
        }
    }
}