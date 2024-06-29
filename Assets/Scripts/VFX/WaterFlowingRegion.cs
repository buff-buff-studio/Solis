using UnityEngine;

namespace VFX
{
    public class WaterFlowingRegion : MonoBehaviour
    {
        public float radius = 1f;
        public float transitionRadius = 0f;
        public float speed = 1f;
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);
            //Draw forward vector
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * speed);
        }
    }
}