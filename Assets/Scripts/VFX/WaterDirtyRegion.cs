using UnityEngine;

namespace VFX
{
    public class WaterDirtyRegion : MonoBehaviour
    {
        public float radius = 1f;
        public float transitionRadius = 0f;
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}