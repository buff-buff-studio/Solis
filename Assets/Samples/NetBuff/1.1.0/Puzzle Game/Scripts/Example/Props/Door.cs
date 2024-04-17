using System.IO;
using NetBuff.Interface;
using NetBuff.Misc;
using SolarBuff.Circuit;
using UnityEngine;

namespace ExamplePlatformer.Props
{
    public class Door : CircuitComponent
    {
        public BoolNetworkValue isOpen = new(false);
        public GameObject open;
        public GameObject closed;

        private void OnEnable()
        {
            WithValues(isOpen);
            UpdateVisuals(isOpen.Value, isOpen.Value);
            isOpen.OnValueChanged += UpdateVisuals;
        }

        protected override void OnRefresh()
        {
            if(!HasAuthority) return;
            isOpen.Value = GetPlugValue(CircuitPlug.Type.Input);
        }

        private void UpdateVisuals(bool old, bool now)
        {
            open.SetActive(now);
            closed.SetActive(!now);
        }
    }
}