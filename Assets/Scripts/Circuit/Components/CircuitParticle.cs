using System;
using System.Collections;
using System.Collections.Generic;
using NetBuff.Misc;
using UnityEngine;

namespace Solis.Circuit.Components
{
    public class CircuitParticle : CircuitComponent
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public CircuitPlug input;
        public ParticleSystem particles;

        [Header("STATE")]
        public bool invert;
        #endregion

        private void Start()
        {
            Refresh();
        }

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            return default;
        }

        protected override void OnRefresh()
        {
            if (input.ReadOutput().power > 0.5f ^ invert)
            {
                particles.Play();
            }
            else
            {
                particles.Stop();
            }
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return input;
        }
        #endregion
    }
}