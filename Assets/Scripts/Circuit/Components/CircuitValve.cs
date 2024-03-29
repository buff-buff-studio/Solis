using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitValve : CircuitComponent
    {
        public CircuitPlug input;
        [SerializeField]
        private GameObject gas;

        protected override void OnRefresh()
        {
            gas.SetActive(!input.ReadValue<bool>());
        }

    }
}