﻿using System.Collections.Generic;
using NetBuff.Components;
using NetBuff.Misc;
using UnityEngine;

namespace Solis.Circuit.Components
{
    [RequireComponent(typeof(NetworkAnimator))]
    public class CircuitAnimator : CircuitComponent
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public CircuitPlug input;
        public Animator animator;
        
        [Header("STATE")]
        public BoolNetworkValue isOpen = new(false);

        private static readonly int IsOn = Animator.StringToHash("IsOn");

        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            WithValues(isOpen);
            base.OnEnable();

            var state = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(state.fullPathHash, 0, 1);
            _OnValueChanged(isOpen.Value, isOpen.Value);
            isOpen.OnValueChanged += _OnValueChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            isOpen.OnValueChanged -= _OnValueChanged;
        }
        #endregion

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            return new CircuitData();
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return input;
        }

        protected override void OnRefresh()
        {
            if(isOpen.AttachedTo != null && HasAuthority)
                isOpen.Value = input.ReadOutput().power > 0;
        }
        #endregion

        #region Private Methods
        private void _OnValueChanged(bool old, bool now)
        {
            animator.SetBool(IsOn, now);
            if (now) onToggleComponent.Invoke();
        }
        #endregion
    }
}