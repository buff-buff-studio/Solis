using UnityEngine;

namespace Solis.Misc
{
    /// <summary>
    /// Class that rotates the windmill blades
    /// </summary>
    public class WindmillRotator : MonoBehaviour
    {
        [SerializeField]
        private Vector3 rotationAxis = Vector3.right;
        public float speed = 10f;
        
        void Update()
        {
            transform.Rotate(rotationAxis, speed * Time.deltaTime);
        }
    }
}