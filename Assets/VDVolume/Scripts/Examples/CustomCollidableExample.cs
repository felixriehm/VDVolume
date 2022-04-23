using UnityEngine;
using VDVolume.Model;
using VDVolume.UnityComponents;

namespace VDVolume.Scripts.Examples
{
    public class CustomCollidableExample : MonoBehaviour
    {
        [SerializeField]
        private Volume vdVolume = null;
        
        private void Awake()
        {
            VolumeCollidable volumeCollidable = GetComponent<VolumeCollidable>();
            volumeCollidable.CollisionType = VolumeCollidable.VolumeCollisionType.Solid;
        }
        
        private void OnCollisionEnter(Collision collisionInfo)
        {
            for (int i = 0; i < collisionInfo.contacts.Length; i++)
            {
                if(collisionInfo.contacts[i].otherCollider.gameObject.TryGetComponent<ColliderData>(out ColliderData colliderData))
                {
                    VoxelData voxelData = colliderData.VoxelData;
                    vdVolume.VolumeModifiers.Cutting.RemoveVoxel(voxelData.X,voxelData.Y,voxelData.Z);
                }
            }
        }
    }
}