﻿using System;
using System.Collections.Generic;
using NetBuff;
using NetBuff.Components;
using NetBuff.Interface;
using Solis.Core;
using Solis.Data;
using Solis.Data.Emotes;
using Solis.Packets;
using UnityEngine;
using UnityEngine.UI;

namespace Solis.Player
{
    /// <summary>
    /// Used to control the player emotes. Can be attached to any player prefab
    /// </summary>
    public class PlayerEmoteController : NetworkBehaviour
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public Image emoteImage;
        public GameObject emoteObject;
        
        [Header("SETTINGS")]
        public float timeToHide = 2;
        public List<Emote> emotes = new();
        [SerializeField, HideInInspector]
        private float timer;
        #endregion

        #region Unity Callbacks
        private void Update()
        {
            var quat = Quaternion.LookRotation(transform.position - Camera.main!.transform.position);
            emoteObject.transform.rotation = quat;
            
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                    emoteObject.SetActive(false);
            }
        }
        #endregion

        #region Network Callbacks
        public override void OnServerReceivePacket(IOwnedPacket packet, int clientId)
        {
            if (packet is PlayerEmotePacket emotePacket)
                ServerBroadcastPacket(emotePacket);
        }

        public override void OnClientReceivePacket(IOwnedPacket packet)
        {
            if (packet is PlayerEmotePacket emotePacket)
            {
                var emote = emotes.Find(e => e.unlocalizedName == emotePacket.EmoteName);
                if (emote != null && emote.canUseInGameplay || GameManager.Instance.IsOnLobby)
                {
                    if (emote is EmoteBalloon emoteBalloon)
                    {
                        switch (emotePacket.CharacterType)
                        {
                            case CharacterType.Human:
                                emoteImage.sprite = emoteBalloon.humanSprite;
                                break;
                            case CharacterType.Robot:
                                emoteImage.sprite = emoteBalloon.robotSprite;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        
                        if(timer <= 0)
                            emoteObject.SetActive(true);
                        
                        timer = timeToHide;
                    }
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Display the emote registered in the slot
        /// </summary>
        /// <param name="slot"></param>
        public void ShowEmote(int slot)
        {
            if(timer > 0)
                timer = 0;

            var data = NetworkManager.Instance.GetLocalSessionData<SolisSessionData>();
            var emoteName = data.Emotes[slot];

            var emote = emotes.Find(e => e.unlocalizedName == emoteName);
            if (emote != null && emote.canUseInGameplay || GameManager.Instance.IsOnLobby)
            {
                SendPacket(new PlayerEmotePacket()
                {
                    EmoteName = emoteName,
                    Id = Id,
                    CharacterType = data.PlayerCharacterType
                });
            }
        }
        #endregion
    }
}