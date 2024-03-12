using System;
using UnityEngine.UI;

namespace SolarBuff.Misc
{
    public class SystemTestToggle : SystemInput
    {
        public Toggle toggle;

        private void OnEnable()
        {
            toggle.onValueChanged.AddListener((_) => onValueChanged.Invoke());
        }

        public override SystemType GetSystemInputType()
        {
            return SystemType.Boolean;
        }
        
        public override object GetSystemInput()
        {
            return toggle.isOn;
        }
    }
}