using System;
using Unity.VisualScripting;
using UnityEngine;

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
        private CircuitConnection connection;
        public CircuitConnection Connection
        {
            get => connection;
            set => connection = value;
        }

        private CircuitComponent _owner;
        public CircuitComponent Owner
        {
            get
            {
                if (_owner == null)
                    _owner = GetComponentInParent<CircuitComponent>();
                    
                return _owner;
            }
        }

        public CircuitComponent Other
        {
            get
            {
                if (connection == null) return null;
                return connection.a == this ? connection.b.Owner : connection.a.Owner;
            }
        }
        
        public CircuitPlug OtherPlug
        {
            get
            {
                if (connection == null) return null;
                return connection.a == this ? connection.b : connection.a;
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
    }
}