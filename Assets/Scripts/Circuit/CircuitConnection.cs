using System;
using UnityEngine;

namespace SolarBuff.Circuit
{
    [ExecuteInEditMode]
    public class CircuitConnection : MonoBehaviour
    {
        public float currentDisplayValue = 0;
        
        public CircuitPlug a;
        public CircuitPlug b;

        private void OnEnable()
        {
            if (a != null && b != null)
            {
                a.Connection = this;
                b.Connection = this;
            }
            
            Refresh();
        }

        private void OnValidate()
        {
            if (a != null && b != null)
            {
                transform.position = (a.transform.position + b.transform.position) / 2;
            }
        }

        private void OnDisable()
        {
            if (a != null)
            {
                a.Connection = null;
                a.Owner.Refresh();
            }
            
            if (b != null)
            {
                b.Connection = null;
                b.Owner.Refresh();
            }
        }

        private void OnDrawGizmos()
        {
            if (a != null && b != null)
            {
                Gizmos.color = Color.Lerp(Color.black, Color.red, currentDisplayValue);
                Gizmos.DrawLine(a.transform.position, b.transform.position);
            }
        }

        public void Refresh()
        {
            if (a.type == CircuitPlug.Type.Input)
            {
                UpdateDisplay(b.ReadValue<float>());
                a.Owner.Refresh();
            }
            else
            {
                UpdateDisplay(a.ReadValue<float>());
                b.Owner.Refresh();
            }
        }

        private void UpdateDisplay(float value)
        {
            currentDisplayValue = value;
        }
    }
}