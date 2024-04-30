<<<<<<< HEAD
﻿using System;
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
=======
﻿using System.Collections.Generic;
using NetBuff.Misc;
using Solis.Data;
using Solis.Packets;
using Solis.Player;
using UnityEngine;

namespace Solis.Circuit.Components
{
    /// <summary>
    /// A lever that can be toggled on and off by players.
    /// </summary>
    public class CircuitLever : CircuitComponent
    {
        #region Inspector Fields
        [Header("SETTINGS")]
        public float radius = 3f;
        public CharacterTypeFilter playerTypeFilter = CharacterTypeFilter.Both;

        [Header("REFERENCES")]
        public BoolNetworkValue isOn = new(false);
        public CircuitPlug output;
        public Transform handle;
        
        [Header("SETTINGS")]
        public float handleAngle = 60;
        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            base.OnEnable();
            WithValues(isOn);

            isOn.OnValueChanged += _OnValueChanged;
            PacketListener.GetPacketListener<PlayerInteractPacket>().AddServerListener(_OnPlayerInteract);

            handle.localEulerAngles = new Vector3(isOn.Value ? handleAngle : 0, -90f, 0);
        }

        protected void Update()
        {
            handle.localEulerAngles =
                new Vector3(Mathf.Lerp(handle.localEulerAngles.x, isOn.Value ? handleAngle : 0, Time.deltaTime * 10),
                    -90f, 0);
>>>>>>> renaissance
        }

        protected override void OnDisable()
        {
            base.OnDisable();
<<<<<<< HEAD
            
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
=======
            isOn.OnValueChanged -= _OnValueChanged;
            PacketListener.GetPacketListener<PlayerInteractPacket>().RemoveServerListener(_OnPlayerInteract);
        }
        #endregion

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            return new CircuitData(isOn.Value);
        }

        protected override void OnRefresh()
        {

        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return output;
        }
        #endregion

        #region Private Methods
        private bool _OnPlayerInteract(PlayerInteractPacket arg1, int arg2)
        {
            var player = GetNetworkObject(arg1.Id);
            var dist = Vector3.Distance(player.transform.position, transform.position);

            if (dist > radius)
                return false;

            var controller = player.GetComponent<PlayerControllerBase>();
            if (controller == null)
                return false;

            if (playerTypeFilter.Filter(controller.CharacterType))
            {
                isOn.Value = !isOn.Value;
                return true;
            }

            return false;
        }

        private void _OnValueChanged(bool old, bool @new)
        {
            Refresh();
        }
        #endregion
>>>>>>> renaissance
    }
}