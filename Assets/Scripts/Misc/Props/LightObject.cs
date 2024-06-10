using System;
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
    [RequireComponent(typeof(Rigidbody))]
    public class LightObject : NetworkBehaviour, ILightObject
    {
        
        #region Inspector Fields
        [Header("SETTINGS")]
        public float radius = 3f;
        public CharacterTypeFilter playerTypeFilter = CharacterTypeFilter.Both;

        [Header("REFERENCES")]
        public BoolNetworkValue isOn = new(false);

        private PlayerControllerBase playerHolding;
        private Rigidbody rb;
        
        #endregion

        #region Private Fields

        private Vector3 _initialPosition;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            _initialPosition = transform.position;
        }

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

        private void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag("DeathTrigger"))
            {
                transform.position = _initialPosition + Vector3.up;
                transform.rotation = Quaternion.identity;
            }
        }

        #endregion

        private void _OnValueChanged(bool old, bool @new)
        {
            Refresh();
        }

        private void Refresh()
        {
            var pBody = playerHolding ? playerHolding.body : null;
            playerHolding = isOn.Value ? playerHolding : null;
            transform.parent = playerHolding ? playerHolding.handPosition : null;
            rb.isKinematic = isOn.Value;

            if (isOn.Value)
            {
                if (playerHolding)
                {
                    transform.position = playerHolding.handPosition.position;
                    transform.rotation = playerHolding.handPosition.rotation;
                }
            }
            else if(pBody)
            {
                var pPos = pBody.position;
                var newPos = new Vector3(pPos.x, transform.position.y, pPos.z);
                transform.position = newPos + (pBody.forward*1.25f);
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