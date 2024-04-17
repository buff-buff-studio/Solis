using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitLamp : CircuitComponent
    {
        public CircuitPlug input;

        protected override void OnRefresh()
        {
            GetComponent<Renderer>().material.color = input.ReadValue<float>() > 0.5f ? Color.red : Color.black;
        }
        
    }
}