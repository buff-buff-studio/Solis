using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Solis.Circuit.Gates
{
    /// <summary>
    /// Basic gate component that can be used to create simple logic circuits.
    /// </summary>
    public class CircuitBasicGate : CircuitComponent
    {
        #region Types
        /// <summary>
        /// The mode of the gate.
        /// </summary>
        public enum Mode
        {
            And,
            Or,
            Not,
            Nand,
            Nor,
            Xor,
            Number
        }
        #endregion

        #region Inspector Fields
        [Header("REFERENCES")]
        public CircuitPlug input;
        public CircuitPlug output;
        public TMP_Text label;
        
        [Header("SETTINGS")]
        public Mode mode = Mode.And;
        public int number;
        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            base.OnEnable();
            _UpdateLabel();
        }
        
        private void OnValidate()
        {
            _UpdateLabel();
        }
        #endregion

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            var count = input.Connections.Length;
            var result = 0;
            for(var i = 0; i < count; i++)
            {
                if(input.ReadOutput(i).power > 0)
                    result++;
            }
            
            switch (mode)
            {
                case Mode.And:
                    return new CircuitData(result == count ? 1 : 0);
                case Mode.Or:
                    return new CircuitData(result > 0 ? 1 : 0);
                case Mode.Not:
                    return new CircuitData(result == 0 ? 1 : 0);
                case Mode.Nand:
                    return new CircuitData(result == count ? 0 : 1);
                case Mode.Nor:
                    return new CircuitData(result > 0 ? 0 : 1);
                case Mode.Xor:
                    return new CircuitData(result == 1 ? 1 : 0);
                case Mode.Number:
                    return new CircuitData(result == number ? 1 : 0);
                default:
                    return new CircuitData(0);
            }
        }
        
        protected override void OnRefresh()
        {
            
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return input;
            yield return output;
        }
        #endregion

        #region Private Methods
        private void _UpdateLabel()
        {
            label.text = mode switch
            {
                Mode.And => "AND",
                Mode.Or => "OR",
                Mode.Not => "NOT",
                Mode.Nand => "NAND",
                Mode.Nor => "NOR",
                Mode.Xor => "XOR",
                Mode.Number => number.ToString(),
                _ => "?"
            };
        }
        #endregion
    }
}