<<<<<<< HEAD
﻿using NetBuff.Misc;

namespace SolarBuff.Circuit.Components
{
    
    public class CircuitBattery : CircuitComponent
    {
        public BoolNetworkValue highVoltage = new(false);
        public float level = 1;
        public CircuitPlug[] outputs;

        protected override void OnEnable()
        {
            base.OnEnable();
            WithValues(highVoltage);
            highVoltage.OnValueChanged += OnChangedType;
        }

        public override T ReadOutput<T>(CircuitPlug plug)
        {
            return SafeOutput<T>(level);
        }

        private void OnChangedType(bool old, bool current)
        {
            Refresh();
        }

        public override bool IsHighVoltage(CircuitPlug plug)
        {
            return highVoltage.Value;
        }
=======
﻿using System.Collections.Generic;
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
>>>>>>> renaissance
    }
}