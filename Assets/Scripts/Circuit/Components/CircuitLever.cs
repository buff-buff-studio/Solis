using System.Collections.Generic;
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

            handle.localEulerAngles = new Vector3(isOn.Value ? handleAngle : 0, 0, 0);
        }

        protected void Update()
        {
            handle.localRotation = Quaternion.Lerp(handle.localRotation, Quaternion.Euler(isOn.Value ? handleAngle : -handleAngle, 0, 0), Time.deltaTime * 10);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
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

        public void ChangeState(bool state)
        {
            isOn.Value = state;
        }

        public void ChangeState()
        {
            isOn.Value = !isOn.Value;
            onToggleComponent?.Invoke();
        }

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
                onToggleComponent?.Invoke();
                return true;
            }

            return false;
        }

        private void _OnValueChanged(bool old, bool @new)
        {
            Refresh();
        }
        #endregion
    }
}