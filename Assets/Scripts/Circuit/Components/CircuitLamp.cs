using System.Collections.Generic;
using UnityEngine;

namespace Solis.Circuit.Components
{
    /// <summary>
    /// Used to display the power state of a circuit
    /// </summary>
    public class CircuitLamp : CircuitComponent
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        #pragma warning disable 0109
        public new Renderer renderer;
        public new Light light;
        #pragma warning restore 0109
        public CircuitPlug input;
        [Space]
        [ColorUsage(false, true)]
        public Color colorOn = Color.red;

        public bool useColorOff = false;
        [ColorUsage(false, true)]
        public Color colorOff = Color.black;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        #endregion

        private Color offColor => useColorOff ? colorOff : Color.black;

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            return default;
        }

        protected override void OnRefresh()
        {
            renderer.material.SetColor(EmissionColor, input.ReadOutput().power > 0.5f ? colorOn : offColor);
            light.color = input.ReadOutput().power > 0.5f ? colorOn : offColor;
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return input;
        }
        #endregion
    }
}