using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitGioDisplay : CircuitComponent
    {
        public CircuitPlug inputA;
        public CircuitPlug inputB;
        
        public TMP_Text display;

        protected override void OnRefresh()
        {
            var a = inputA.ReadValue<float>();
            var b = inputB.ReadValue<float>();

            display.text = (a - b).ToString("0.00");
        }
    }
}