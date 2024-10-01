using System;
using System.Collections.Generic;
using System.Linq;
using NetBuff.Misc;
using Solis.Data;
using Solis.Packets;
using Solis.Player;
using UnityEngine;

namespace Solis.Circuit
{
    public abstract class CircuitInteractive : CircuitComponent
    {
        [Header("SETTINGS")]
        public float radius = 3f;
        public CharacterTypeFilter playerTypeFilter = CharacterTypeFilter.Both;

        private List<Collider> _colliders = new List<Collider>();
        private LayerMask _layerMask;

#if UNITY_EDITOR
        private Transform _playerTransform;
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            PacketListener.GetPacketListener<PlayerInteractPacket>().AddServerListener(OnPlayerInteract);
            _layerMask = ~(playerTypeFilter != CharacterTypeFilter.Both
                ? LayerMask.GetMask("Interactive", playerTypeFilter == CharacterTypeFilter.Human ? "Human" : "Robot")
                : LayerMask.GetMask("Interactive", "Human", "Robot"));
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PacketListener.GetPacketListener<PlayerInteractPacket>().RemoveServerListener(OnPlayerInteract);
        }

        protected virtual bool OnPlayerInteract(PlayerInteractPacket arg1, int arg2)
        {
            return PlayerChecker(arg1, out var player);
        }

        protected bool PlayerChecker(PlayerInteractPacket arg1, out PlayerControllerBase player)
        {
            player = null;
            // Check if player is within radius
            var networkObject = GetNetworkObject(arg1.Id);
            var dist = Vector3.Distance(networkObject.transform.position, transform.position);
            if (dist > radius) return false;

            // Check if game object has a player controller
            if(!networkObject.TryGetComponent(out player))
                return false;

            // Check if player is allowed to interact with object
            if (!playerTypeFilter.Filter(player.CharacterType))
                return false;

#if UNITY_EDITOR
            _playerTransform = player.body;
#endif

            //Check if have a wall between player and object
            Physics.Linecast(networkObject.transform.position, transform.position, out var hit, _layerMask);
            if (hit.collider != null)
            {
                Debug.Log($"{hit.transform.name} is between the {player.CharacterType} and {this.name}", hit.collider.gameObject);
                return false;
            }

            // Check if player is facing the object
            var directionToTarget = transform.position - player.body.position;
            var dot = Vector3.Dot(player.body.forward, directionToTarget.normalized);
            if (dot < 0)
            {
                Debug.Log("Player is not facing the object, dot: " + dot);
                return false;
            }

            return true;
        }

        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnRefresh()
        {
            throw new System.NotImplementedException();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_playerTransform == null) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_playerTransform.position, _playerTransform.forward * radius);
        }

        protected virtual void OnValidate()
        {
            if(Application.isPlaying) return;
            _colliders.Clear();
            _colliders.AddRange(GetComponentsInChildren<Collider>());
            if (_colliders.Count == 0)
            {
                Debug.LogError("No colliders found in children", this);
            }

        }
#endif
    }
}
