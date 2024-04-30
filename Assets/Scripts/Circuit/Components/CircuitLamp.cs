<<<<<<< HEAD
﻿using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitLamp : CircuitComponent
    {
        public CircuitPlug input;

        protected override void OnRefresh()
        {
            GetComponent<Renderer>().material.color = input.ReadValue<float>() > 0.5f ? Color.red : Color.black;
        }
        
=======
﻿using System.Collections.Generic;
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
        #pragma warning restore 0109
        public CircuitPlug input;
        #endregion

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            return default;
        }

        protected override void OnRefresh()
        {
            renderer.materials[1].color = input.ReadOutput().power > 0.5f ? Color.red : Color.black;
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return input;
        }
        #endregion
>>>>>>> renaissance
    }
}