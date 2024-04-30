<<<<<<< HEAD
﻿using UnityEngine;

namespace SolarBuff.Circuit
{
    public class CircuitPlug : MonoBehaviour
    {
        public enum Type
        {
            Output,
            Input
        }

        private void OnEnable()
        {
            Owner.plugs.Add(this);
        }
        
        private void OnDisable()
        {
            Owner.plugs.Remove(this);
        }

        public Type type = Type.Output;
        [SerializeField, HideInInspector]
        private MonoBehaviour connection;
        public ICircuitConnection Connection
        {
            get => connection as ICircuitConnection;
            set => connection = value as MonoBehaviour;
        }

        private CircuitComponent _owner;
=======
﻿using System;
using Solis.Circuit.Interfaces;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

namespace Solis.Circuit
{
    /// <summary>
    /// Used to link two circuit components together via a connection
    /// </summary>
    [ExecuteInEditMode]
    public class CircuitPlug : MonoBehaviour
    {
        #region Inspector Fields
        public CircuitPlugType type;
        public bool acceptMultipleConnections;
        #endregion

        #region Private Fields
        private CircuitComponent _owner;
        private ICircuitConnection[] _connections = Array.Empty<ICircuitConnection>();
        #endregion

        #region Public Properties
        /// <summary>
        /// The connections that this plug is connected to
        /// </summary>
        public ICircuitConnection[] Connections
        {
            get => _connections;
            set
            {
                _connections = value ?? Array.Empty<ICircuitConnection>();

                if (!Application.isPlaying)
                    return;

                if (Owner != null)
                    Owner.Refresh();
            }
        }
        
        /// <summary>
        /// The connection that this plug is connected to. If there are multiple connections, the first one is returned
        /// </summary>
        public ICircuitConnection Connection
        {
            get => Connections.Length > 0 ? Connections[0] : null;
            set => Connections = new[] { value };
        }

        /// <summary>
        /// Returns the owner of this plug, which is the circuit component that this plug is attached to
        /// </summary>
>>>>>>> renaissance
        public CircuitComponent Owner
        {
            get
            {
                if (_owner == null)
                    _owner = GetComponentInParent<CircuitComponent>();
<<<<<<< HEAD
                    
                return _owner;
            }
        }

        public CircuitComponent Other
        {
            get
            {
                if (Connection == null) return null;
                return Connection.PlugA == this ? Connection.PlugB.Owner : Connection.PlugA.Owner;
            }
        }

        public CircuitPlug OtherPlug
        {
            get
            {
                if (Connection == null) return null;
                return Connection.PlugA == this ? Connection.PlugB : Connection.PlugA;
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = type switch
            {
                Type.Input => Color.red,
                Type.Output => Color.green,
                _ => Color.white
            };

            Gizmos.DrawSphere(transform.position, 0.1f);
        }
        
        public T ReadValue<T>()
        {
            if(Connection == null)
                return default;
            
            return type switch
            {
                Type.Input => Other.ReadOutput<T>(OtherPlug),
                Type.Output => Owner.ReadOutput<T>(this),
                _ => default
            };
        }
        
        public bool IsHighVoltage()
        {
            if(Connection == null)
                return false;
            
            return type switch
            {
                Type.Input => Other.IsHighVoltage(OtherPlug),
                Type.Output => Owner.IsHighVoltage(this),
                _ => false
            };
        }
=======

                return _owner;
            }
        }
        #endregion

        #region Unity Callbacks
        #if UNITY_EDITOR
        private async void OnEnable()
        {
            if (Application.isPlaying)
                return;

            await Awaitable.NextFrameAsync();

            var plugs = FindObjectsByType<CircuitPlug>(FindObjectsSortMode.None);

            var connectionList = new List<ICircuitConnection>(Connections);
            foreach (var plug in plugs)
            {
                if (plug == this)
                    continue;

                foreach (var connection in plug.Connections)
                {
                    if (connection.PlugA == this || connection.PlugB == this)
                    {
                        if (!connectionList.Contains(connection))
                            connectionList.Add(connection);
                    }
                }
            }

            Connections = connectionList.ToArray();

            foreach (var connection in Connections)
            {
                if (connection.PlugA == null)
                    connection.PlugA = this;
                else if (connection.PlugB == null)
                    connection.PlugB = this;
            }
        }
        #endif

        private void OnDestroy()
        {
            #if UNITY_EDITOR
            Undo.SetCurrentGroupName("Removed object");
            Undo.RecordObject(gameObject, "Removed object");
            var group = Undo.GetCurrentGroup();
            foreach (var connection in Connections)
            {
                var mb = connection as MonoBehaviour;
                if (mb != null)
                    Undo.DestroyObjectImmediate(mb.gameObject);
            }

            Undo.CollapseUndoOperations(group);
            #else
            foreach (var connection in Connections)
            {
                var mb = connection as MonoBehaviour;
                if (mb != null)
                    Destroy(mb.gameObject);
            }
            #endif
        }
        #endregion

        #region Public Methods
        public CircuitData ReadOutput(int connection = 0)
        {
            switch (type)
            {
                case CircuitPlugType.Output:
                    var read = Owner.ReadOutput(this);
                    return read;

                case CircuitPlugType.Input:
                    var other = GetOtherPlug(connection);

                    if (ReferenceEquals(other, null))
                        return default;
                    return other.Owner.ReadOutput(other);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public CircuitPlug GetOtherPlug(int connection = 0)
        {
            var con = connection < Connections.Length ? Connections[connection] : null;
            if (con == null)
                return null;
            return this == con.PlugA ? con.PlugB : con.PlugA;
        }
        #endregion
>>>>>>> renaissance
    }
}