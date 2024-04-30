using System.Collections.Generic;
using UnityEngine;

namespace Solis.Circuit.Components
{
    /// <summary>
    /// Used to provide a constant power source to the circuit
    /// </summary>
    public class CircuitBattery : CircuitComponent
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public CircuitPlug output;
        #endregion

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            return new CircuitData(1);
        }

        protected override void OnRefresh()
        {
            
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return output;
        }
        #endregion
    }
}