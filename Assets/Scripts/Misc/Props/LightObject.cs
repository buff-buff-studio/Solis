using System;
using NetBuff.Components;
using NetBuff.Interface;
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
        public NetworkRigidbodyTransform networkRigidbodyTransform;

        private PlayerControllerBase playerHolding;
        private Rigidbody rb;
        
        #endregion

        #region Private Fields

        private Vector3 _initialPosition;
        private Collider _collider;
        private ILightObject _lightObjectImplementation;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            _collider = GetComponentInChildren<Collider>();
            _initialPosition = transform.position;
        }

        protected void OnEnable()
        {
            WithValues(isOn);
            
            InvokeRepeating(nameof(_PosCheck), 0, 1f);
            isOn.OnValueChanged += _OnValueChanged;
            PacketListener.GetPacketListener<PlayerInteractPacket>().AddServerListener(_OnPlayerInteract);
            
        }
        protected void OnDisable()
        {
            CancelInvoke(nameof(_PosCheck));
            isOn.OnValueChanged -= _OnValueChanged;
            PacketListener.GetPacketListener<PlayerInteractPacket>().RemoveServerListener(_OnPlayerInteract);
        }

        private void Update()
        {
            if(!isOn.Value && !playerHolding) return;
            
            var ht = playerHolding.handPosition;
            var pos = ht.position;
            var fw = ht.forward;
            var dt = Time.deltaTime * 50f;
            transform.position = Vector3.MoveTowards(transform.position, pos, dt);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(fw), dt);
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag("DeathTrigger"))
            {
                _Reset();
            }
        }

        #endregion

        #region Network Callbacks
        
        public override void OnClientReceivePacket(IOwnedPacket packet)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (packet is LightObjectGrabPacket grabPacket)
            {
                if (HasAuthority)
                    return;

                if (grabPacket.HandId == null)
                    playerHolding = null;
                else
                {
                    NetworkId.TryParse(grabPacket.HandId, out var handId);
                    playerHolding = GetNetworkObject((NetworkId)handId).GetComponent<PlayerControllerBase>();
                }
            }
        }
        
        #endregion
        
        private void _PosCheck()
        {
            if (transform.position.y < -15)
            {
                _Reset();
            }
        }
        
        private void _Reset()
        {
            transform.position = _initialPosition + Vector3.up;
            transform.rotation = Quaternion.identity;
            rb.velocity = Vector3.zero;
            if (playerHolding)
            {
                playerHolding.itemsHeld = 0;
                playerHolding = null;
            }
            isOn.Value = false;
            /*if (isOn.Value)
            {
                playerHolding = null;
                isOn.Value = false;
            }*/
        }

        private void _OnValueChanged(bool old, bool @new)
        {
            _Refresh();
        }

        private void _Refresh()
        {
            var pBody = playerHolding ? playerHolding.body : null;
            _collider.excludeLayers = !isOn.Value
                ? 0
                : ~((playerHolding!.CharacterType == CharacterType.Human
                    ? LayerMask.GetMask("Robot")
                    : LayerMask.GetMask("Human"))+LayerMask.GetMask("Trigger"));
            
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
                playerHolding.itemsHeld = 0;
            }

            playerHolding = isOn.Value ? playerHolding : null;
            networkRigidbodyTransform.enabled = !isOn.Value;
            rb.isKinematic = isOn.Value;
            rb.velocity = Vector3.zero;
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
                if (isOn.Value)
                {
                    if (playerHolding != controller)
                        return false;
                    
                    playerHolding = null;
                    isOn.Value = false;
                    controller.itemsHeld = 0;
                    CheckIfThereIsPlace();
                    ServerBroadcastPacket(new LightObjectGrabPacket
                    {
                        Id = this.Id,
                        HandId = ""
                    });
                    return true;
                }

                if(controller.itemsHeld > 0)
                    return false;
                    
                playerHolding = controller;
                isOn.Value = true;
                rb.isKinematic = false;
                controller.itemsHeld++;
                ServerBroadcastPacket(new LightObjectGrabPacket
                {
                    Id = this.Id,
                    HandId = player.Id.ToString()
                });
                
                return true;
            }

            return false;
        }
        private static readonly Collider[] _objects = new Collider[10];
        private void CheckIfThereIsPlace()
        {
            var count = Physics.OverlapBoxNonAlloc(transform.position, new Vector3(0.35f, 0.35f, 0.35f), _objects,
                Quaternion.identity);
            foreach (var o in _objects)
            {
                if (o.transform.CompareTag("BoxPlace"))
                {
                    transform.position = o.transform.position;
                    transform.rotation = Quaternion.identity;
                    rb.isKinematic = true;
                    return;
                }
            }
        }
        

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}