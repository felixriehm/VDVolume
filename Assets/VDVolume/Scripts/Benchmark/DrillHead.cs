using UnityEngine;
using VDVolume.UnityComponents;

namespace Benchmark
{
    public abstract class DrillHead : MonoBehaviour
    {
        [SerializeField]
        private protected Volume vdVolume = null;
        private protected int _deletedVoxels = 0;
        
        public abstract void SetSize(float size);
        
        public void ResetDeletedVoxelsCount()
        {
            _deletedVoxels = 0;
        }
        
        public int GetDeletedVoxelsCount()
        {
            return _deletedVoxels;
        }
    }
}