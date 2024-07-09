using System.Collections;
using System.Collections.Generic;
using NetBuff.Misc;
using Solis.Misc;
using UnityEngine;

namespace Solis.Circuit.Components
{
    /// <summary>
    /// Used to provide a constant power source to the circuit
    /// </summary>
    public class CircuitEolicTurbine : CircuitComponent
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public BoolNetworkValue isOn = new(false);
        public WindmillRotator windmillRotator;
        public CircuitPlug output;
        public CircuitPlug input;

        private float Power => windmillRotator.Power;
        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            base.OnEnable();
            WithValues(isOn);

            isOn.OnValueChanged += _OnValueChanged;

            if(!windmillRotator) windmillRotator = GetComponentInChildren<WindmillRotator>();
            windmillRotator.ChangeState(isOn.Value, true);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            isOn.OnValueChanged -= _OnValueChanged;
        }
        #endregion

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            return new CircuitData(Power);
        }

        protected override void OnRefresh()
        {
            if(isOn.AttachedTo != null && HasAuthority)
                isOn.Value = input.ReadOutput().power > 0;
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return output;
            yield return input;
        }
        #endregion

        public void ChangeState(bool state)
        {
            if(!IsServer) return;

            isOn.Value = state;
        }

        public void ChangeState()
        {
            if(!IsServer) return;

            isOn.Value = !isOn.Value;
        }

        #region Private Methods

        private void _OnValueChanged(bool old, bool @new)
        {
            Refresh();
            windmillRotator.ChangeState(@new);
            onToggleComponent?.Invoke();
        }
        #endregion
    }
}
