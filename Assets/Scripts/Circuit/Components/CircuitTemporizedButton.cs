using System.Threading.Tasks;
using ExamplePlatformer;
using NetBuff.Misc;
using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitTemporizedButton : CircuitComponent
    {
        public BoolNetworkValue isOn = new(false);
        public float radius = 2f;
        public int timeOn = 4;
        public Transform handle;
        public CircuitPlug output;
        private float buttonScale = 0.01f;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            WithValues(isOn);
            handle.localScale = new Vector3(handle.localScale.x, isOn.Value ? buttonScale : 0.3f,handle.localScale.z);
            isOn.OnValueChanged += (_, _) => Refresh();
            
            GetPacketListener<PlayerPunchActionPacket>().OnServerReceive += OnPlayerPunch;
        }
        
        private async void OnPlayerPunch(PlayerPunchActionPacket obj, int client)
        {
            var o = GetNetworkObject(obj.Id);
            var dist = Vector3.Distance(o.transform.position, transform.position);

            if (dist > radius)
                return;
            
            if(isOn.Value) return;

            isOn.Value = true;
            await TurnOffAsync();
        }

        private async Task TurnOffAsync()
        {
            await SomeAsyncOperation();
            async Task SomeAsyncOperation()
            {
                await Task.Delay(timeOn * 1000); 
            }
            
            isOn.Value = false;
        }
        
        private void Update()
        {
            handle.localScale = new Vector3(handle.localScale.x, Mathf.Lerp(handle.localScale.y,isOn.Value ? buttonScale : 0.3f, Time.deltaTime * 10),handle.localScale.z); 
        }
        
        public override T ReadOutput<T>(CircuitPlug plug)
        {
            return SafeOutput<T>(isOn.Value);
        }
    }
}