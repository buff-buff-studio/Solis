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
        private MonoBehaviour connection;
        public ICircuitConnection Connection
        {
            get => connection as ICircuitConnection;
            set => connection = value as MonoBehaviour;
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
    }
}