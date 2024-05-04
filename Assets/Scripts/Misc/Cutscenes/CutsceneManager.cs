using System.Collections.Generic;
using NetBuff;
using NetBuff.Components;
using NetBuff.Interface;
using NetBuff.Misc;
using Solis.Core;
using Solis.Packets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Solis.Misc.Cutscenes
{
    /// <summary>
    /// Used to manage the cutscene state and skip the cutscene when all players have finished it.
    /// </summary>
    public class CutsceneManager : NetworkBehaviour
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public TMP_Text labelDone;
        public Button buttonDone;
        
        [Header("STATE")]
        [ServerOnly]
        public List<int> doneList = new();
        public IntNetworkValue doneCount = new(0);
        public IntNetworkValue playerCount = new(0);
        #endregion

        #region Unity Callbacks
        private void OnEnable()
        {
            WithValues(doneCount, playerCount);
            
            doneCount.OnValueChanged += _OnDoneCountChanged;
            playerCount.OnValueChanged += _OnDoneCountChanged;
            
            _OnDoneCountChanged(0, doneCount.Value);
        }
        
        private void OnDisable()
        {
            doneCount.OnValueChanged -= _OnDoneCountChanged;
            playerCount.OnValueChanged -= _OnDoneCountChanged;
        }
        #endregion
        
        #region Network Callbacks
        public override void OnSpawned(bool isRetroactive)
        {
            if (!HasAuthority)
                return;
            
            playerCount.Value = NetworkManager.Instance.Transport.GetClientCount();
        }

        public override void OnClientConnected(int clientId)
        {
            playerCount.Value = NetworkManager.Instance.Transport.GetClientCount();
        }
        
        public override void OnClientDisconnected(int clientId)
        {
            playerCount.Value = NetworkManager.Instance.Transport.GetClientCount();
            doneList.Remove(clientId);
        }

        public override void OnServerReceivePacket(IOwnedPacket packet, int clientId)
        {
            if (packet is CutsceneStatePacket csp)
            {
                if (csp.FinishedCount == 1)
                {
                    if (!doneList.Contains(clientId))
                    {
                        doneList.Add(clientId);
                        doneCount.Value = doneList.Count;
                    }
                }
                else if(doneList.Contains(clientId))
                {
                    doneList.Remove(clientId);
                    doneCount.Value = doneList.Count;
                }
                
                if(doneList.Count >=  NetworkManager.Instance.Transport.GetClientCount())
                {
                    // All players have finished the cutscene
                    _OnEveryoneFinished();
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Marks the cutscene as finished for the current player.
        /// Called on the client side.
        /// </summary>
        [ClientOnly]
        public void MarkFinished()
        {
            buttonDone.interactable = false;
            var packet = new CutsceneStatePacket
            {
                Id = Id,
                FinishedCount = 1
            };
            ClientSendPacket(packet);
        }
        #endregion

        #region Private Methods
        private void _OnDoneCountChanged(int oldvalue, int newvalue)
        {
            labelDone.text = $"{doneCount.Value} / {playerCount.Value}";
        }
        
        private void _OnEveryoneFinished()
        {
            GameManager.Instance.LoadLevel();
        }
        #endregion
    }
}