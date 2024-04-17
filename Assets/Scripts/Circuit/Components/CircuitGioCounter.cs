using ExamplePlatformer;
using NetBuff.Misc;
using SolarBuff.Player;
using TMPro;
using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitGioCounter : CircuitComponent
    {
        public IntNetworkValue state = new(0);

        public CircuitPlug output;

        public TMP_Text display;

        public float radius = 1f;

        protected override void OnEnable()
        {
            base.OnEnable();
            WithValues(state);

            state.OnValueChanged += (_, _) => Refresh(); 

            GetPacketListener<PlayerPunchActionPacket>().OnServerReceive += OnPlayerPunch;
        }

        private void OnPlayerPunch(PlayerPunchActionPacket packet, int client)
        {
            var o = GetNetworkObject(packet.Id);
            var dist = Vector3.Distance(o.transform.position, transform.position);

            if (dist > radius)
                return;

            state.Value++;
        }

        protected override void OnRefresh()
        {
            display.text = state.Value.ToString();
        }

        public override T ReadOutput<T>(CircuitPlug plug)
        {
            return SafeOutput<T>(state.Value);
        }
    }
}