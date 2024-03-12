using System.Collections.Generic;

namespace SolarBuff.Circuit
{
    public class CircuitButton : CircuitComponent
    {
        public CircuitPlug output;
        
        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return output;
        }
    }
}