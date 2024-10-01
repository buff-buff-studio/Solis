using System;
using NetBuff.Components;
using NetBuff.Misc;
using Solis.Data;
using Solis.Packets;
using Solis.Player;
using UnityEngine;

namespace Misc.Props
{
    public class InteractiveObject : NetworkBehaviour
    {
        [Header("SETTINGS")]
        public float radius = 3f;
        public CharacterTypeFilter playerTypeFilter = CharacterTypeFilter.Both;

        private LayerMask _layerMask;

        protected virtual void OnEnable()
        {
            PacketListener.GetPacketListener<PlayerInteractPacket>().AddServerListener(OnPlayerInteract);

            _layerMask = ~(playerTypeFilter != CharacterTypeFilter.Both
                ? LayerMask.GetMask("Box", playerTypeFilter == CharacterTypeFilter.Human ? "Human" : "Robot")
                : LayerMask.GetMask("Box", "Human", "Robot"));
        }

        protected virtual void OnDisable()
        {
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

            //Check if have a wall between player and object
            var originalLayer = gameObject.layer;
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            Physics.Linecast(networkObject.transform.position, transform.position, out var hit, _layerMask);
            gameObject.layer = originalLayer;
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
    }
}
