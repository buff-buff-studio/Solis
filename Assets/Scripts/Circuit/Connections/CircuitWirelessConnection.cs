using System;
using Solis.Circuit.Interfaces;
using UnityEngine;

namespace Solis.Circuit.Connections
{
    /// <summary>
    /// A wireless connection between two plugs.
    /// Does not render a wire, but draw a line in the scene view.
    /// </summary>
    [ExecuteInEditMode]
    public class CircuitWirelessConnection : MonoBehaviour, ICircuitConnection
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        [SerializeField]
        private CircuitPlug plugA;

        [SerializeField]
        private CircuitPlug plugB;

        [Header("SETTINGS")]
        public Color color = Color.black;
        #endregion

        #region Private Fields
        private CircuitPlug _currentPlugA;
        private CircuitPlug _currentPlugB;
        private bool _isValid;
        #endregion

        #region ICircuitConnection Properties
        public CircuitPlug PlugA
        {
            get => plugA;
            set
            {
                plugA = value;
                OnValidate();
            }
        }

        public CircuitPlug PlugB
        {
            get => plugB;
            set
            {
                plugB = value;
                OnValidate();
            }
        }
        
        public bool IsValid => _isValid && plugA != null && plugB != null && plugA.type != plugB.type;
        #endregion

        #region Unity Callbacks
        protected virtual void OnEnable()
        {
            OnValidate();
        }

        protected virtual void OnDisable()
        {
            _currentPlugA = null;
            _currentPlugB = null;
            _isValid = false;

            if (plugA != null)
                _UnsetFrom(plugA);

            if (plugB != null)
                _UnsetFrom(plugB);
        }

        protected virtual void OnValidate()
        {
            if (_currentPlugA != null && _currentPlugA != plugA)
                _UnsetFrom(_currentPlugA);

            if (_currentPlugB != null && _currentPlugB != plugB)
                _UnsetFrom(_currentPlugB);

            if (plugA != null && _currentPlugA != plugA)
                _SetTo(plugA);

            if (plugB != null && _currentPlugB != plugB)
                _SetTo(plugB);

            _currentPlugA = plugA;
            _currentPlugB = plugB;
            _isValid = plugA != null && plugB != null;

            if (IsValid)
            {
                transform.position = (plugA.transform.position + plugB.transform.position) / 2;
            }
        }

        protected virtual void OnDrawGizmos()
        {
            if (!IsValid)
                return;

            Gizmos.color = color;
            Gizmos.DrawLine(plugA.transform.position, plugB.transform.position);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!IsValid)
                return;

            //Draw outlined
            Gizmos.color = new Color(1 - color.r, 1 - color.g, 1 - color.b, color.a);
            Gizmos.DrawLine(plugA.transform.position, plugB.transform.position);
        }
        #endregion

        #region Private Methods
        private void _UnsetFrom(CircuitPlug plug)
        {
            plug.Connections = Array.FindAll(plug.Connections, c => !ReferenceEquals(c, this));
        }

        private void _SetTo(CircuitPlug plug)
        {
            if (plug.acceptMultipleConnections)
            {
                if (Array.FindIndex(plug.Connections, c => ReferenceEquals(c, this)) == -1)
                {
                    var connections = new ICircuitConnection[plug.Connections.Length + 1];
                    Array.Copy(plug.Connections, connections, plug.Connections.Length);
                    connections[^1] = this;
                    plug.Connections = connections;
                }
            }
            else
            {
                if (plug.Connection != null && !ReferenceEquals(plug.Connection, this))
                    plug.Connection.Detach(plug);

                plug.Connection = this;
            }
        }
        #endregion

        #region ICircuitConnection Methods
        public void Detach(CircuitPlug plug)
        {
            if (plug == plugA)
                plugA = null;
            else if (plug == plugB)
                plugB = null;

            OnValidate();
        }
        #endregion
    }
}