using UnityEngine;

namespace VFX
{
    public class WaterPipeExit : MonoBehaviour
    {
        public float radius;
        public float strength = 2f;
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}