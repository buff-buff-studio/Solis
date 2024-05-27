using NetBuff.Components;
using Solis.Circuit.Interfaces;
using UnityEngine;

namespace Solis.Misc.Props
{
    [RequireComponent(typeof(NetworkRigidbodyTransform))]
    public class MagneticProp : MonoBehaviour, IMagneticObject
    {
        private Rigidbody _rigidbody;
        
        private void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
        
        public void Magnetize(GameObject magnet, Transform anchor)
        {
            transform.SetParent(anchor);
            _rigidbody.isKinematic = true;
        }

        public void Demagnetize(GameObject magnet, Transform anchor)
        {
            transform.SetParent(null);
            _rigidbody.isKinematic = false;
        }

        public Transform GetCurrentAnchor()
        {
            return transform.parent;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}