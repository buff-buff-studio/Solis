using TMPro;

namespace SolarBuff.Circuit.Components.Testing
{
    public class CircuitCharacterDisplay : CircuitComponent
    {
        public TMP_Text label;
        public CircuitPlug input;
        public int sum = 0;

        protected override void OnRefresh()
        {
            var value = input.ReadValue<float>();
            var c = value == 0 ? '-' : (char) (sum + value);
            label.text = $"{c}";
        }
    }
}