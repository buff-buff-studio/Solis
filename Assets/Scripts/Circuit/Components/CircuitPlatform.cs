using System.Collections.Generic;
using System.Linq;
using NetBuff.Misc;
using Solis.Circuit.Interfaces;
using UnityEngine;

namespace Solis.Circuit.Components
{
    /// <summary>
    /// A platform that moves between two points. It can be controlled by a switch and can be blocked by heavy objects.
    /// </summary>
    public class CircuitPlatform : CircuitComponent
    {
        #region Private Static Fields
        private static readonly Collider[] _Results = new Collider[16];
        #endregion

        #region Inspector Fields
        [Header("REFERENCES")]
        public CircuitPlug input;
        
        [Header("STATE")]
        public FloatNetworkValue position = new(0);
        public bool canBeMoving = true;
        public bool alwaysOn = false;
        
        [Header("SETTINGS")]
        public int tickRate = 64;
        public int checkTickRate = 2;
        public float moveSpeed = 2f;
        public Transform from;
        public Transform to;
        public Transform platform;

        [Header("CHECK")]
        public bool canHandleHeavyObjects;
        public Vector3 checkOffset = new(0, 0.1f, 0);
        public Vector3 checkSize = new(0.5f, 0.1f, 0.5f);
        #endregion

        #region Public Properties
        public Vector3 DeltaSinceLastFrame { get; private set; }
        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            WithValues(position);
            
            base.OnEnable();
            InvokeRepeating(nameof(_Tick), 0, 1f / tickRate);
            InvokeRepeating(nameof(_TickCheck), 0, 1f / checkTickRate);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke(nameof(_Tick));
            CancelInvoke(nameof(_TickCheck));
        }
       
        private void FixedUpdate()
        {
            var newPosition = Vector3.Lerp(from.position, to.position, position.Value);
            DeltaSinceLastFrame = newPosition - platform.position;
            platform.position = newPosition;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            var fr = from.position;
            var tr = to.position;
            Gizmos.DrawLine(fr, tr);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(fr, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(tr, 0.1f);
            
            Gizmos.color = Color.blue;
            var mtx = Matrix4x4.TRS(platform.position, platform.rotation, Vector3.one);
            Gizmos.matrix = mtx;
            Gizmos.DrawWireCube(checkOffset, checkSize);
            Gizmos.matrix = Matrix4x4.identity;
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
            yield return input;
        }
        #endregion
        
        #region Network Callbacks
        public override void OnSpawned(bool isRetroactive)
        {
            platform.position = Vector3.Lerp(from.position, to.position, position.Value);
        }
        #endregion

        private float speed;
        bool value;
        #region Private Methods
        private void _Tick()
        {
            if (!HasAuthority)
                return;
            
            if (!canBeMoving)
                return;

           
            if (!alwaysOn)
            {
                value = input.ReadOutput().power > 0.5f;
            }
            else
            {
                if(Mathf.Approximately(position.Value, 0) || Mathf.Approximately(position.Value,1))
                {
                    value = position.Value < 0.1f;
                }
            }
            

            var distance = Vector3.Distance(from.position, to.position);
            speed = moveSpeed / distance / tickRate;

            var newValue = Mathf.Clamp01(position.Value + (value ? speed : -speed));
            position.Value = newValue;
            
           
        }
        
        private void _TickCheck()
        {
            if (!HasAuthority)
                return;

            canBeMoving = _CheckPlatform();
        }

        private bool _CheckPlatform()
        {
            if (canHandleHeavyObjects)
                return true;
            
            var count = Physics.OverlapBoxNonAlloc(platform.position + checkOffset, checkSize / 2, _Results, platform.rotation);

            for (var i = 0; i < count; i++)
            {
                if (_Results[i] == null)
                    continue;
                
                if (_Results[i].TryGetComponent(out IHeavyObject _))
                    return false;
            }
            
            return !_Results.Take(count).Any(col => col.TryGetComponent(out IHeavyObject _));
        }
        #endregion
    }
}