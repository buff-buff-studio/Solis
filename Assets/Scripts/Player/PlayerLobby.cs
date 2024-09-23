using System;
using NetBuff.Components;
using NetBuff.Interface;
using NetBuff.Misc;
using Solis.Core;
using UnityEngine;
using Solis.Data;
using Solis.Misc.Integrations;
using Solis.Packets;
using Random = UnityEngine.Random;

namespace Solis.Player
{
    /// <summary>
    /// Player Lobby Controller. Used to control the player in the lobby
    /// </summary>
    public class PlayerLobby : NetworkBehaviour
    {
        #region Inspector Fields
        [Header("SETTINGS")]
        [SerializeField]
        private CharacterType characterType;
        
        [Header("STATE")]
        public bool mouseIsGrabbed;
        public float rotationSpeed = 0f;
        #endregion

        private float _timeToReset = 0.5f;

        #region Unity Callbacks
        private void Update()
        {
            if (!HasAuthority || !IsOwnedByClient) return;
            
            //check if mouse is being dragged over the player, to rotate it
            if (!mouseIsGrabbed)
            {
                _timeToReset -= Time.deltaTime;
                if (Input.GetMouseButton(0))
                {
                    var ray = Camera.allCameras[1].ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit))
                        if (hit.collider.gameObject == gameObject)
                        {
                            mouseIsGrabbed = true;
                            _timeToReset = Random.Range(2, 6);
                        }
                }
            }
            else
            {
                if(Input.GetMouseButtonUp(0)) mouseIsGrabbed = false;
                rotationSpeed -= Input.GetAxis("Mouse X") * 50f;
            }

            if(_timeToReset>0)
                transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
            else
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime);

            rotationSpeed = Mathf.Lerp(rotationSpeed, 0, Time.deltaTime * 5f);
            
            if (!HasAuthority)
                return;

            if (Input.GetKeyDown(KeyCode.C))
                ChangeCharacterType();
        }
        #endregion
        
        #region Network Callbacks

        public override void OnSpawned(bool isRetroactive)
        {
            base.OnSpawned(isRetroactive);
            DiscordController.LobbyStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            DiscordController.Instance!.SetGameActivity(characterType);
        }

        public override void OnServerReceivePacket(IOwnedPacket packet, int clientId)
        {
            if (packet is LobbyPlayerActionPacket actionPacket)
            {
                switch (actionPacket.Action)
                {
                    case LobbyPlayerAction.ChangeCharacter:
                        GameManager.Instance.SetCharacterType(clientId,
                            characterType == CharacterType.Human
                                ? CharacterType.Robot
                                : CharacterType.Human);

                        DiscordController.Instance!.SetGameActivity(characterType);
                        break;
                }
            }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Changes the character type of the player
        /// </summary>
        [RequiresAuthority]
        public void ChangeCharacterType()
        {
            if (!HasAuthority)
                return;

            var packet = new LobbyPlayerActionPacket
            {
                Id = Id,
                Action = LobbyPlayerAction.ChangeCharacter,
            };

            SendPacket(packet);
        }
        #endregion
    }
}