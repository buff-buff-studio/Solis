using NetBuff.Misc;

namespace SolarBuff.Circuit.Components
{
    
    public class CircuitBattery : CircuitComponent
    {
        public BoolNetworkValue highVoltage = new(false);
        public float level = 1;
        public CircuitPlug[] outputs;

        protected override void OnEnable()
        {
            base.OnEnable();
            WithValues(highVoltage);
            highVoltage.OnValueChanged += OnChangedType;
        }

        public override T ReadOutput<T>(CircuitPlug plug)
        {
            return SafeOutput<T>(level);
        }

        private void OnChangedType(bool old, bool current)
        {
            Refresh();
        }

        public override bool IsHighVoltage(CircuitPlug plug)
        {
            return highVoltage.Value;
        }
    }
}