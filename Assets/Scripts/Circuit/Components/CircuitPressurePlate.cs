using System.Collections.Generic;
using NetBuff.Misc;
using UnityEngine;

namespace Solis.Circuit.Components
{
    /// <summary>
    /// A pressure plate that sends a signal when an object is on top of it.
    /// </summary>
    public class CircuitPressurePlate : CircuitComponent
    {
        #region Private Static Fields
        private static readonly Collider[] _Results = new Collider[16];
        #endregion

        #region Inspector Fields
        [Header("REFERENCES")]
        public Transform knob;
        public CircuitPlug output;
        
        [Header("STATE")]
        public BoolNetworkValue isOn = new(false);

        [Header("SETTINGS")]
        public int tickRate = 20;
        public Vector3 offset = new(0, 0.75f, 0);
        public float radius = 0.4f;
        public Vector3 knobLocalPositionOn = new(0, 0, 0);
        public Vector3 knobLocalPositionOff = new(0, 0.05f, 0);
        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            base.OnEnable();
            WithValues(isOn);
            isOn.OnValueChanged += _OnValueChanged;
            
            InvokeRepeating(nameof(_Tick), 0, 1f / tickRate);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            isOn.OnValueChanged -= _OnValueChanged;
            
            CancelInvoke(nameof(_Tick));
        }
        
        private void Update()
        {
            knob.localPosition = Vector3.Lerp(knob.localPosition, isOn.Value ? knobLocalPositionOn : knobLocalPositionOff, Time.deltaTime * 10);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            var t = transform;
            var off = t.TransformDirection(offset);
            Gizmos.DrawWireSphere(t.position + off, radius);
        }
        #endregion

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            return new CircuitData(isOn.Value);
        }
        
        protected override void OnRefresh()
        {
            
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return output;
        }
        #endregion

        #region Private Methods
        private void _Tick()
        {
            if (!HasAuthority)
                return;

            var t = transform;
            var off = t.TransformDirection(offset);
            var size = Physics.OverlapSphereNonAlloc(t.position + off, radius, _Results);
            isOn.Value = size > 0;
        }
        
        private void _OnValueChanged(bool old, bool @new)
        {
            Refresh();
        }
        #endregion
    }
}