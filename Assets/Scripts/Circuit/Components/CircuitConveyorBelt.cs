using System;
using System.Collections.Generic;
using NetBuff.Misc;
using UnityEngine;

namespace Solis.Circuit.Components
{
    public class CircuitConveyorBelt : CircuitComponent
    {
        private Rigidbody _rigidbody;
        public float speed = 3f;
        
        public BoolNetworkValue isOnValue = new BoolNetworkValue();
        public FloatNetworkValue speedValue = new FloatNetworkValue();
        
        protected override void OnEnable()
        {
            WithValues(isOnValue, speedValue);
            base.OnEnable();
            _rigidbody = gameObject.AddComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
            
            if (HasAuthority)
                speedValue.Value = isOnValue.Value ? speed : 0f;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Destroy(_rigidbody);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            
            var t = transform;
            var pos = t.position;
            var forward = t.forward;
            var tip = pos + forward * speed;
            var right = t.right;

            var sign = -Mathf.Sign(speed);
            
            Gizmos.DrawLine(pos, tip);
            Gizmos.DrawLine(tip, tip + (sign * forward + right).normalized * 0.1f);
            Gizmos.DrawLine(tip, tip + (sign * forward - right).normalized * 0.1f);
        }

        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            return new CircuitData();
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield break;
        }

        protected override void OnRefresh()
        {
            
        }

        private void FixedUpdate()
        {
            if (!HasAuthority)
                return;

            speedValue.Value = Mathf.Lerp(speedValue.Value,  isOnValue.Value ? speed : 0f, Time.fixedDeltaTime * 5f);

            var delta = transform.forward * (speed * Time.fixedDeltaTime);
            _rigidbody.position -= delta;
            // ReSharper disable once Unity.InefficientPropertyAccess
            _rigidbody.MovePosition(_rigidbody.position + delta);
        }
    }
}