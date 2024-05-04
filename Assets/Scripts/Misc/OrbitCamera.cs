using UnityEngine;

namespace Solis.Misc
{
    /// <summary>
    /// A simple orbit camera.
    /// Orbit around a target object with the mouse.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class OrbitCamera : MonoBehaviour
    {
        #region Inspector Fields
        [Header("REFERENCE")]
        public GameObject target;
        
        [Header("SETTINGS")]
        public float distance = 5f;
        public Vector3 offset = new(0, 1f, 0);
        public float radius = 0.5f;
        public LayerMask occlusion = 1 << 0;
        
        [Header("STATE")]
        public float rotationX;
        #endregion

        #region Unity Callbacks
        private void LateUpdate()
        {
            if (target == null)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                var tr = this.transform;
                tr.position = new Vector3(0, 1, -10);
                tr.rotation = Quaternion.identity;
                return;
            }
            
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
            }
            
            rotationX -= Input.GetAxis("Mouse Y") * 3f;
            rotationX = Mathf.Clamp(rotationX, -20, 90);

            Transform t = transform;
            t.eulerAngles = new Vector3(rotationX, t.eulerAngles.y + Input.GetAxis("Mouse X") * 3f, 0);

            var ds = distance;

            if(Physics.Raycast(target.transform.position, -t.forward, out var hit, distance, occlusion))
                ds = (hit.distance - radius) + 1;

            t.position = target.transform.position - (t.forward * ds) + offset;
        }
        #endregion
    }
}