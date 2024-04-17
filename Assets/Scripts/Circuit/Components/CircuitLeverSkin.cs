using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitLeverSkin : CircuitLever
    {
        protected void Update()
        {
            handle.localEulerAngles = new Vector3(Mathf.Lerp(handle.localEulerAngles.x, isOn.Value ? angle : 0, Time.deltaTime * 10), -90f, 0);
        }
    }
}