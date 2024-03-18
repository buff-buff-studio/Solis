namespace SolarBuff.Circuit.Components
{
    public class CircuitXor : CircuitComponent
    {
        public CircuitPlug inputA;
        public CircuitPlug inputB;

        public CircuitPlug output;

        public override T ReadOutput<T>(CircuitPlug plug)
        {
            var b = inputA.ReadValue<bool>() ^ inputB.ReadValue<bool>();
            
            return SafeOutput<T>(b);
        }
    }
}