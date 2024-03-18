using System;
using NetBuff.Misc;
using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitButton : CircuitComponent
    {
        public CircuitPlug output;
        public BoolNetworkValue isOn = new(false);
        public Transform knob;
        
        private readonly Collider[] _results = new Collider[16];

        protected override void OnEnable()
        {
            WithValues(isOn);
            isOn.OnValueChanged += (_, _) => Refresh();
        }

        public override T ReadOutput<T>(CircuitPlug plug)
        {
            return Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.Boolean => (T)(object)isOn.Value,
                TypeCode.Single => (T)(object)(isOn.Value ? 1f : 0f),
                _ => default
            };
        }
        
        private void FixedUpdate()
        {
            if (!HasAuthority)
                return;

            var t = transform;
            var size = Physics.OverlapSphereNonAlloc(t.position + t.up * 0.75f, 0.4f, _results);
            isOn.Value = size > 0;
        }
        
        private void Update()
        {
            var o = knob.transform.localPosition.y;
            var f = Mathf.Lerp(o, isOn.Value ? 0 : 0.05f, Time.deltaTime * 10);
            knob.transform.localPosition = new Vector3(0, f, 0);
        }
    }
}