using System.Collections.Generic;

namespace SolarBuff.Circuit
{
    public class CircuitLamp : CircuitComponent
    {
        public CircuitPlug input;
        
        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return input;
        }
    }
}