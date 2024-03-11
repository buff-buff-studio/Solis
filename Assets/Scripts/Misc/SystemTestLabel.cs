using TMPro;

namespace SolarBuff.Misc
{
    public class SystemTestLabel : SystemOutput
    {
        public TMP_Text label;
        
        public override SystemType GetSystemOutputType()
        {
            return SystemType.Any;
        }
        
        public override void SetSystemOutput(object output)
        {
            label.text = output.ToString();
        }
    }
}