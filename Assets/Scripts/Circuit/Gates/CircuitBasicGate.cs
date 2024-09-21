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
        public enum Mode : int
        {
            And = 0,
            Or = 1,
            Not = 2,
            Nand = 3,
            Nor = 4,
            Xor = 5,
            NumberEqual = 10,
            NumberNotEqual = 11,
            NumberLess = 12,
            NumberGreater = 13
        }
        #endregion

        #region Inspector Fields
        [Header("REFERENCES")]
        public CircuitPlug input;
        public CircuitPlug output;
        public TMP_Text label;

        [Header("SETTINGS")]
        public bool invisibleOnPlay = false;
        public Mode mode = Mode.And;

        [Header("SETTINGS - Number Mode")]
        public int number;
        [Range(0, 1)]
        public float minPower = 0;
        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            base.OnEnable();
            if(mode < (Mode)10) minPower = 0;
            _UpdateLabel();
            if(invisibleOnPlay)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                label.gameObject.SetActive(false);
            }
        }
        
        private void OnValidate()
        {
            if(mode < (Mode)10) minPower = 0;
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
                if(input.ReadOutput(i).power > minPower)
                    result++;
            }

            if(mode >= (Mode)10) _UpdateLabel(result.ToString());

            return mode switch
            {
                Mode.And => new CircuitData(result == count ? 1 : 0),
                Mode.Or => new CircuitData(result > 0 ? 1 : 0),
                Mode.Not => new CircuitData(result == 0 ? 1 : 0),
                Mode.Nand => new CircuitData(result == count ? 0 : 1),
                Mode.Nor => new CircuitData(result > 0 ? 0 : 1),
                Mode.Xor => new CircuitData(result == 1 ? 1 : 0),
                Mode.NumberEqual => new CircuitData(result == number ? 1 : 0),
                Mode.NumberNotEqual => new CircuitData(result != number ? 1 : 0),
                Mode.NumberLess => new CircuitData(result < number ? 1 : 0),
                Mode.NumberGreater => new CircuitData(result > number ? 1 : 0),
                _ => new CircuitData(0)
            };
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
        private void _UpdateLabel(string numberX = "X")
        {
            label!.text = mode switch
            {
                Mode.And => "AND",
                Mode.Or => "OR",
                Mode.Not => "NOT",
                Mode.Nand => "NAND",
                Mode.Nor => "NOR",
                Mode.Xor => "XOR",
                Mode.NumberEqual => $"{numberX} = {number}",
                Mode.NumberNotEqual => $"{numberX} != {number}",
                Mode.NumberGreater => $"{numberX} > {number}",
                Mode.NumberLess => $"{numberX} < {number}",
                _ => "?"
            };
        }
        #endregion
    }
}