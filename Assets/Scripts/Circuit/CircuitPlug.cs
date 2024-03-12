using System;
using UnityEngine;

namespace SolarBuff.Circuit
{
    public class CircuitPlug : MonoBehaviour
    {
        public enum Type
        {
            None,
            Output,
            Input
        }
        
        public Type type = Type.None;
        [SerializeField, HideInInspector]
        private CircuitConnection connection;

        public CircuitComponent Owner
        {
            get
            {
                if (_owner == null)
                    _owner = GetComponentInParent<CircuitComponent>();
                return _owner;
            }
        }
        
        public CircuitConnection Connection
        {
            get => connection;
            set => connection = value;
        }

        private CircuitComponent _owner;

        private void OnDrawGizmos()
        {
            switch (type)
            {
                case Type.Input:
                    Gizmos.color = Color.red;
                    break;
                case Type.Output:
                    Gizmos.color = Color.green;
                    break;
                default:
                    Gizmos.color = Color.white;
                    break;
            }
            
            Gizmos.DrawSphere(transform.position, 0.1f);
        }
        
        public T ReadInput<T>()
        {
            if (type != Type.Input)
                throw new InvalidOperationException("Cannot read input from non-input plug");
            if (connection == null)
                return default;
            return connection.a == this
                ? connection.b.Owner.ReadOutput<T>(connection.b)
                : connection.a.Owner.ReadOutput<T>(connection.a);
        }
    }
}