using UnityEngine;

namespace ExamplePlatformer
{
    public class OrbitCamera : MonoBehaviour
    {
        public GameObject target;
        public float distance = 10.0f;
        public Vector3 offset = new Vector3(0, 1f, 0);
        public float radius = 0.5f;
        public float rotationX;

        public LayerMask occlusion = 1 << 0;
        
        void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            
            if (target == null)
                return;
            
            rotationX -= Input.GetAxis("Mouse Y") * 3f;
            rotationX = Mathf.Clamp(rotationX, -20, 90);

            Transform t = transform;
            t.eulerAngles = new Vector3(rotationX, t.eulerAngles.y + Input.GetAxis("Mouse X") * 3f, 0);

            var ds = distance;

            if(Physics.Raycast(target.transform.position, -t.forward, out var hit, distance, occlusion))
                ds = (hit.distance - radius) + 1;

            t.position = target.transform.position - (t.forward * ds) + offset;
        }
    }
}