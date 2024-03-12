using System.Collections.Generic;
using System.Linq;
using NetBuff.Components;

namespace SolarBuff.Circuit.Gates
{
    public class CircuitAndGate : CircuitComponent
    {
        public CircuitPlug[] inputs;
        public CircuitPlug output;
        
        public override T ReadOutput<T>(CircuitPlug plug)
        {
            if (plug == output)
            {
                var result = inputs.Aggregate(true, (current, input) => current & input.ReadValue<bool>());
                return SafeOutput<T>(result);
            }
            return default;
        }
    }
}