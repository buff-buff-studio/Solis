using NetBuff.Components;
using UnityEngine;

namespace SolarBuff.Circuit
{
    public interface ICircuitConnection
    {
        public CircuitPlug PlugA { get; }
        public CircuitPlug PlugB { get; }

        public bool Refresh();
    }
    
    /*
    public abstract class CircuitConnection : NetworkBehaviour
    {
        public CircuitPlug plugA;
        public CircuitPlug plugB;

        private static bool _isQuitting;

        public virtual bool Refresh()
        {
            if (plugA != null && plugB != null)
            {
                if (plugA.Connection != null && plugA.Connection != this)
                {
                    DestroyImmediate(gameObject);
                    return false;
                }

                if (plugB.Connection != null && plugB.Connection != this)
                {
                    DestroyImmediate(gameObject);
                    return false;
                }

                plugA.Connection = this;
                plugB.Connection = this;

                return true;
            }
            
            DestroyImmediate(gameObject);
            return false;
        }
        
        protected virtual void OnDisable()
        {
            if(_isQuitting)
                return;
            
            if(plugA != null)
            {
                plugA.Connection = null;
                if(plugA.Owner != null)
                    plugA.Owner.Refresh();
            }
            
            if(plugB != null)
            {
                plugB.Connection = null;
                if(plugB.Owner != null)
                    plugB.Owner.Refresh();
            }
        }
        
        protected virtual void OnApplicationQuit () 
        {
            _isQuitting = true;
        }
    }
    */
}