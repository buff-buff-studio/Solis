using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitValve : CircuitComponent
    {
        [SerializeField]
        private GameObject gas;

        protected override void OnRefresh()
        {
            gas.SetActive(!GetPlugValue(CircuitPlug.Type.Input));
        }
    }
}