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

        private void FixedUpdate()
        {
            _HandlePlatform();
        }

        private void _HandlePlatform()
        {
            var ray = new Ray(transform.position, Vector3.down);
            if (Physics.Raycast(ray, out var hit, 0.71f))
            {
                var platform = hit.collider.GetComponentInParent<CircuitPlatform>(); 
                if (platform != null)
                {
                    if(rb.isKinematic) return;
                    Debug.Log("Moving");
                    Debug.Log(platform.DeltaSinceLastFrame);
                    rb.MovePosition(transform.position + platform.DeltaSinceLastFrame);
               
                }
            }
        }
    }
}