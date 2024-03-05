using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AYellowpaper.SerializedCollections;
using NetBuff;
using NetBuff.Components;
using NetBuff.Interface;
using NetBuff.Misc;
using TMPro;
using UnityEngine;

namespace Interface
{
    
    public class Chat : NetworkBehaviour
    {
        public static string LocalPlayerName { get; set; }
        
        
        [Header("REFERENCES")]
        public TMP_Text chatContents;
        public TMP_InputField chatInput;
        
        [Header("STATE"), ServerOnly, SerializeField, HideInInspector]
        private SerializedDictionary<int, string> clientNames = new SerializedDictionary<int, string>();
        public List<string> chatHistory = new List<string>();

        private void OnEnable()
        {
            var pnp = NetworkManager.Instance.GetPacketListener<PlayerNamePacket>();
            pnp.OnServerReceive += OnServerReceivePlayerNamePacket;
            pnp.OnClientReceive += OnClientReceivePlayerNamePacket;

            var cmp = NetworkManager.Instance.GetPacketListener<ChatMessagePacket>();
            cmp.OnServerReceive += OnServerReceiveChatMessagePacket;
            cmp.OnClientReceive += OnClientReceiveChatMessagePacket;
            
            chatInput.onSubmit.AddListener(OnChatInput);
        }

        private void OnChatInput(string message)
        {
            ClientSendPacket(new ChatMessagePacket()
            {
                Message = message
            });
            
            chatInput.text = "";
        }

        private void OnDisable()
        {
            var pnp = NetworkManager.Instance.GetPacketListener<PlayerNamePacket>();
            pnp.OnServerReceive -= OnServerReceivePlayerNamePacket;
            pnp.OnClientReceive -= OnClientReceivePlayerNamePacket;

            var cmp = NetworkManager.Instance.GetPacketListener<ChatMessagePacket>();
            cmp.OnServerReceive -= OnServerReceiveChatMessagePacket;
            cmp.OnClientReceive -= OnClientReceiveChatMessagePacket;
        }

        private void OnServerReceiveChatMessagePacket(ChatMessagePacket packet, int client)
        {
            if (!clientNames.ContainsKey(client)) 
                return;
            
            var nickName = clientNames[client];
            ServerBroadcastPacket(new ChatMessagePacket
            {
                Message = $"{nickName}: {packet.Message}"
            });
        }

        private void OnClientReceiveChatMessagePacket(ChatMessagePacket packet)
        {
            chatHistory.Add(packet.Message);
            UpdateChatDisplay();
            
            if (chatHistory.Count > 100)
                chatHistory.RemoveAt(0);
        }

        private void OnClientReceivePlayerNamePacket(PlayerNamePacket packet)
        {
            
        }

        private void OnServerReceivePlayerNamePacket(PlayerNamePacket packet, int client)
        {
            clientNames[client] = packet.Name;
            ServerBroadcastPacket(new ChatMessagePacket()
            {
                Message = $"<color=yellow>{packet.Name} joined the game</color>"
            });
        }

        public override void OnClientDisconnected(int clientId)
        {
            if (!clientNames.ContainsKey(clientId)) 
                return;
            
            ServerBroadcastPacket(new ChatMessagePacket()
            {
                Message = $"<color=yellow>{clientNames[clientId]} left the game</color>"
            });
            
            clientNames.Remove(clientId);
        }

        public override void OnSpawned(bool isRetroactive)
        {
            var packet = new PlayerNamePacket
            {
                Name = LocalPlayerName,
                ClientId = -1
            };
            
            ClientSendPacket(packet);
        }
        
        public void UpdateChatDisplay()
        {
            chatContents.text = chatHistory.Skip(Math.Max(0, chatHistory.Count - 5)).Take(5).Aggregate("", (current, message) => current + message + "\n");
        }
    }

    public class PlayerNamePacket : IPacket
    {
        public string Name { get; set; }
        public int ClientId { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(ClientId);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            Name = reader.ReadString();
            ClientId = reader.ReadInt32();
        }
    }
    
    public class ChatMessagePacket : IPacket
    {
        public string Message { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Message);
        }

        public void Deserialize(BinaryReader reader)
        {
            Message = reader.ReadString();   
        }
    }
}