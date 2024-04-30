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
        [SerializeField]
        private CharacterType characterType;
        #endregion

        #region Unity Callbacks
        private void Update()
        {
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