using System.Collections.Generic;
using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitLamp : CircuitComponent
    {
        public CircuitPlug input;

        protected override void OnRefresh()
        {
            GetComponent<Renderer>().sharedMaterial.color = input.ReadValue<float>() > 0.5f ? Color.green : Color.red;
        }
    }
}