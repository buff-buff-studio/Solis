using System;
using Solis.Circuit.Components;
using UnityEngine;

namespace Solis.Misc.Props
{
    [RequireComponent(typeof(Rigidbody))]
    public class DraggedByPlataformObject : MonoBehaviour
    {
        private Rigidbody rb;
   

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            _HandlePlatform();
        }

        private void _HandlePlatform()
        {
            var ray = new Ray(transform.position, Vector3.down);
            if (Physics.Raycast(ray, out var hit, 1.1f))
            {
                var platform = hit.collider.GetComponentInParent<CircuitPlatform>();
                if (platform != null)
                {
                    Debug.Log("Draggin");
                    rb.velocity += platform.DeltaSinceLastFrame;
                    Debug.Log(platform.DeltaSinceLastFrame);
                }
            }
        }
    }
}