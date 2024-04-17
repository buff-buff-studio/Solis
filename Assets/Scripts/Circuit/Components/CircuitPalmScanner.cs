using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitPalmScanner : CircuitLever
    {
        private float buttonScale = 0.01f;
        protected void Update()
        {
            handle.localScale = new Vector3(handle.localScale.x, Mathf.Lerp(handle.localScale.y,isOn.Value ? buttonScale : 0.3f, Time.deltaTime * 10),handle.localScale.z); 
        }
    }
}