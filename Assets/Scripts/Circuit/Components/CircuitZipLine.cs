using System.Collections.Generic;
using System.Linq;
using NetBuff.Components;
using NetBuff.Interface;
using NetBuff.Misc;
using Solis.Circuit.Interfaces;
using Solis.Packets;
using UnityEngine;

namespace Solis.Circuit.Components
{
    /// <summary>
    /// A magnetic zipline that can be used to transport objects between two points.
    /// Only works with objects that implement the IMagneticObject interface.
    /// </summary>
    public class CircuitZipline : CircuitComponent
    {
        #region Private Static Fields
        private static readonly Collider[] _Results = new Collider[16];
        #endregion

        #region Inspector Fields
        [Header("REFERENCES")]
        public Transform from;
        public Transform to;
        public Transform claw;
        public Transform anchor;
        public CircuitPlug input;
        
        [Header("STATE")]
        public FloatNetworkValue position = new(0);

        [Header("SETTINGS")]
        public int tickRate = 64;
        public float moveSpeed = 2f;
        public float clawRadius = 2f;
        #endregion

        #region Private Fields
        private bool _wasMoving;
        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            WithValues(position);
            
            base.OnEnable();
            InvokeRepeating(nameof(_Tick), 0, 1f / tickRate);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke(nameof(_Tick));
        }
        
        private void Update()
        {
            claw.position = Vector3.Lerp(from.position, to.position, position.Value);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(from.position, to.position);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(anchor.position, clawRadius);
        }
        #endregion

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            return default;
        }

        protected override void OnRefresh()
        {
            
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            return new[] {input};
        }
        #endregion

        #region Network Callbacks
        public override void OnClientReceivePacket(IOwnedPacket packet)
        {
            if (packet is PacketMagnetizedStateChange stateChange)
            {
                var target = GetNetworkObject(stateChange.Object);
                if (target == null)
                    return;
                
                var magnetic = target.GetComponent<IMagneticObject>();
                if (magnetic == null)
                    return;
                
                if (stateChange.Magnetized)
                    magnetic.Magnetize(claw.gameObject, anchor);
                else
                    magnetic.Demagnetize(claw.gameObject, anchor);
            }
        }

        public override void OnSpawned(bool isRetroactive)
        {
            base.OnSpawned(isRetroactive);
            claw.position = Vector3.Lerp(from.position, to.position, position.Value);
        }
        #endregion

        #region Private Methods
        private void _Tick()
        {
            if (!HasAuthority)
                return;

            var value = input.ReadOutput().power > 0.5f;
            
            var distance = Vector3.Distance(from.position, to.position);
            var speed = moveSpeed / distance / tickRate;
            
            var newValue = Mathf.Clamp01(position.Value + (value ? speed : -speed));
            var isMoving = Mathf.Abs(newValue - position.Value) > 0.001f;
            position.Value = newValue;

            if (isMoving != _wasMoving)
            {
                if (isMoving)
                {
                    foreach (var target in _GetTargetsByCollider())
                    {
                        var go = target.GetGameObject();
                        var identity = go.GetComponent<NetworkIdentity>();
                        if (identity == null)
                            continue;
                        
                        SendPacket(new PacketMagnetizedStateChange
                        {
                            Id = Id,
                            Object = identity.Id,
                            Magnetized = true
                        });
                    }
                }
                else
                {
                    foreach (var target in _GetAllMagnetized())
                    {
                        var go = target.GetGameObject();
                        var identity = go.GetComponent<NetworkIdentity>();
                        if (identity == null)
                            continue;
                        
                        SendPacket(new PacketMagnetizedStateChange
                        {
                            Id = Id,
                            Object = identity.Id,
                            Magnetized = false
                        });
                    }
                }
            }
            
            _wasMoving = isMoving;
        }
        
        private IEnumerable<IMagneticObject> _GetTargetsByCollider()
        {
            var count = Physics.OverlapSphereNonAlloc(anchor.position, clawRadius, _Results);
            return _Results.Take(count).Select(c => c.GetComponent<IMagneticObject>()).Where(c => c != null).ToArray();
        }
        
        private IEnumerable<IMagneticObject> _GetAllMagnetized()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IMagneticObject>().Where(c => c.GetCurrentAnchor() == anchor).ToArray();
        }
        #endregion
    }
}