using System;
using System.Collections.Generic;
using NetBuff.Components;

namespace SolarBuff.Circuit
{
    public abstract class CircuitComponent : NetworkBehaviour
    {
        public abstract T ReadOutput<T>(CircuitPlug plug);
        
        public abstract IEnumerable<CircuitPlug> GetPlugs();

        public virtual void Refresh()
        {
            
        }

        public void RefreshConnections()
        {
            foreach (var plug in GetPlugs())
            {
                if (plug.Connection == null) 
                    continue;
                
                var other = plug.Connection.a == plug ? plug.Connection.b : plug.Connection.a;
                if (other.Owner != null)
                    other.Owner.Refresh();
            }
        }
    }
}