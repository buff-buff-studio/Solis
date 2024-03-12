using System;
using UnityEngine;

namespace SolarBuff.Circuit
{
    public class CircuitConnection : MonoBehaviour
    {
        public CircuitPlug a;
        public CircuitPlug b;

        private void OnEnable()
        {
            if (a != null && b != null)
            {
                a.Connection = this;
                b.Connection = this;
            }
        }

        private void OnDrawGizmos()
        {
            if (a != null && b != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(a.transform.position, b.transform.position);
            }
        }
    }
}