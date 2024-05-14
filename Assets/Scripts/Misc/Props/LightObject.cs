using NetBuff.Components;
using NetBuff.Misc;
using Solis.Circuit;
using Solis.Circuit.Interfaces;
using Solis.Data;
using Solis.Packets;
using Solis.Player;
using UnityEngine;

namespace Solis.Misc.Props
{
    public class LightObject : NetworkBehaviour, ILightObject
    {
        
        #region Inspector Fields
        [Header("SETTINGS")]
        public float radius = 3f;
        public CharacterTypeFilter playerTypeFilter = CharacterTypeFilter.Both;

        [Header("REFERENCES")]
        public BoolNetworkValue isOn = new(false);

        private PlayerControllerBase playerHolding;
        
        #endregion

        #region Unity Callbacks
        protected void OnEnable()
        {
            WithValues(isOn);

            isOn.OnValueChanged += _OnValueChanged;
            PacketListener.GetPacketListener<PlayerInteractPacket>().AddServerListener(_OnPlayerInteract);
            
        }
        protected void OnDisable()
        {
            isOn.OnValueChanged -= _OnValueChanged;
            PacketListener.GetPacketListener<PlayerInteractPacket>().RemoveServerListener(_OnPlayerInteract);
        }
        #endregion

        private void _OnValueChanged(bool old, bool @new)
        {
            Refresh();
        }

        private void Refresh()
        {
            playerHolding = isOn.Value ? playerHolding : null;
            transform.parent = playerHolding ? playerHolding.handPosition : null;
            
            if (isOn.Value)
            {
                if(playerHolding)
                    transform.position = playerHolding.handPosition.position;
            }
                
        }
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
                playerHolding = controller;
                isOn.Value = !isOn.Value;
                return true;
            }

            return false;
        }
        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}