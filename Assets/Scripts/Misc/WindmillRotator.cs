using UnityEngine;

namespace Solis.Misc
{
    /// <summary>
    /// Class that rotates the windmill blades
    /// </summary>
    public class WindmillRotator : MonoBehaviour
    {
        public float speed = 10f;
        
        void Update()
        {
            transform.Rotate(Vector3.right, speed * Time.deltaTime);
        }
    }
}