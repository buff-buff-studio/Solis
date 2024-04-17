using System;
using ExamplePlatformer;
using NetBuff;
using NetBuff.Misc;
using SolarBuff.Player;
using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitLever : CircuitComponent
    {
        public BoolNetworkValue isOn = new(false);
        public float radius = 2f;
        
        public Transform handle;
        public float angle = 60;
        [SerializeField] private PlayerControllerCore.PlayerType playerTypeFilter;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            WithValues(isOn);
            handle.localEulerAngles = new Vector3(isOn.Value ? angle : 0, -90f, 0);
            isOn.OnValueChanged += (_, _) => Refresh();
            
            GetPacketListener<PlayerPunchActionPacket>().OnServerReceive += OnPlayerPunch;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            GetPacketListener<PlayerPunchActionPacket>().OnServerReceive -= OnPlayerPunch;
        }
        private void OnPlayerPunch(PlayerPunchActionPacket obj, int client)
        {
            var o = GetNetworkObject(obj.Id);
            var dist = Vector3.Distance(o.transform.position, transform.position);
            if (dist > radius)
                return;
                
            if (o.transform.GetComponent<PlayerControllerCore>().type == playerTypeFilter ||
                    playerTypeFilter == PlayerControllerCore.PlayerType.Both)
            {
                isOn.Value = !isOn.Value;
            }
            
        }
        
        public override T ReadOutput<T>(CircuitPlug plug)
        {
            return SafeOutput<T>(isOn.Value);
        }
    }
}