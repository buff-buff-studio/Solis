using NetBuff.Components;
using NetBuff.Interface;
using NetBuff.Misc;
using Solis.Core;
using UnityEngine;
using Solis.Data;
using Solis.Packets;

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
        public float rotationSpeed = 0f;
        #endregion

        #region Unity Callbacks
        private void Update()
        {
            //check if mouse is being dragged over the player, to rotate it
            if (Input.GetMouseButton(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        rotationSpeed -= Input.GetAxis("Mouse X") * 50f;
                    }
                }
            }
            
            transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
            rotationSpeed = Mathf.Lerp(rotationSpeed, 0, Time.deltaTime * 5f);
            
            if (!HasAuthority)
                return;

            if (Input.GetKeyDown(KeyCode.C))
                ChangeCharacterType();
        }
        #endregion
        
        #region Network Callbacks
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