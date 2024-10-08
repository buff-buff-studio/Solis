using NetBuff.Components;
using NetBuff.Misc;
using Solis.Circuit.Interfaces;
using UnityEngine;

namespace Solis.Misc.Props
{
    [RequireComponent(typeof(NetworkRigidbodyTransform))]
    public class MagneticProp : NetworkBehaviour, IMagneticObject
    {
        private Rigidbody _rigidbody;
        public BoolNetworkValue cantBeMagnetized;
        
        private void OnEnable()
        {
            WithValues(cantBeMagnetized);
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

        public bool CanBeMagnetized()
        {
            return !cantBeMagnetized.Value;
        }
    }
}