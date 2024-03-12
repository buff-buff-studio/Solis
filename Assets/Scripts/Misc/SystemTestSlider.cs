using System;
using UnityEngine.UI;

namespace SolarBuff.Misc
{
    public class SystemTestSlider : SystemInput
    {
        public Slider slider;

        private void OnEnable()
        {
            slider.onValueChanged.AddListener((_) => onValueChanged.Invoke());
        }

        public override SystemType GetSystemInputType()
        {
            return SystemType.Number;
        }
        
        public override object GetSystemInput()
        {
            return slider.value;
        }
    }
}