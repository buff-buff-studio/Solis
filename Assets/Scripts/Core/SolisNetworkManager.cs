using System.Linq;
using NetBuff;
using NetBuff.Packets;
using NetBuff.Session;
using NetBuff.UDP;
using Solis.Data;
using Solis.Packets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Solis.Core
{
    /// <summary>
    /// Solis Network Manager. Used to customize the network manager for Solis project.
    /// </summary>
    [Icon("Assets/Art/Sprites/Editor/SolisNetworkManager_ico.png")]
    public class SolisNetworkManager : NetworkManager
    {
        #region Public Static Fields
        public const int SOLIS_MAGIC_NUMBER = 10_000;
        public const int SOLIS_NETWORK_PORT = 7777;
        
        public static string networkAddress;
        public static string username;
        #if UNITY_EDITOR
        public static string sceneToLoad;
        #endif
        public static bool isJoining;
        public static CharacterType defaultType = CharacterType.Human;
        #endregion

        #region Unity Callbacks
        private void Start()
        {
            if (EnvironmentType is NetworkTransport.EnvironmentType.None)
            {
                #if UNITY_EDITOR
                var scene = sceneToLoad ?? "Lobby";
                VersionMagicNumber = SOLIS_MAGIC_NUMBER;

                if (string.IsNullOrEmpty(username))
                    username = "test_" + Random.Range(0, 1000);

                Name = username;

                var udp = (Transport as UDPNetworkTransport);
                if (udp != null)
                {
                    udp.Port = SOLIS_NETWORK_PORT;
                    if (!string.IsNullOrEmpty(networkAddress))
                        udp.Address = networkAddress;
                }

                if (!isJoining)
                {
                    StartServer();
                    LoadScene(scene);
                }

                StartClient();

                isJoining = false;
                sceneToLoad = null;
                networkAddress = null;
                username = null;
                #else
                VersionMagicNumber = SOLIS_MAGIC_NUMBER;

                if (string.IsNullOrEmpty(username))
                    username = "test_" + Random.Range(0, 1000);

                Name = username;

                var udp = (Transport as UDPNetworkTransport);
                if (udp != null)
                {
                    udp.Port = SOLIS_NETWORK_PORT;
                    if (!string.IsNullOrEmpty(networkAddress))
                        udp.Address = networkAddress;
                }

                if (!isJoining)
                {
                    StartServer();
                    LoadScene("Lobby");
                }

                StartClient();
                
                networkAddress = null;
                username = null;
                #endif
            }
        }
        #endregion

        #region Network Callbacks - Main
        protected override void OnServerStart()
        {
            base.OnServerStart();
            GameManager.Instance.PrepareSaveData();
        }

        protected override void OnClearEnvironment(NetworkTransport.ConnectionEndMode mode, string cause)
        {
            GameManager.Instance.OnExitGame();
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            SceneManager.LoadScene("Menu");
        }
        #endregion

        #region Network Callbacks - Session Establishing Request
        protected override NetworkSessionEstablishRequestPacket OnCreateSessionEstablishRequest()
        {
            return new SolisNetworkSessionEstablishRequestPacket()
            {
                Username = NetworkManager.Instance.Name
            };
        }

        protected override SessionEstablishingResponse OnSessionEstablishingRequest(
            NetworkSessionEstablishRequestPacket requestPacket)
        {
            var request = (requestPacket as SolisNetworkSessionEstablishRequestPacket)!;

            //Check if name is already in use
            foreach (var session in GetAllSessionData<SolisSessionData>())
            {
                if (session.Username == request.Username)
                    return new SessionEstablishingResponse()
                    {
                        Type = SessionEstablishingResponse.SessionEstablishingResponseType.Reject,
                        Reason = "name_in_use"
                    };
            }

            return new SessionEstablishingResponse()
            {
                Type = SessionEstablishingResponse.SessionEstablishingResponseType.Accept
            };
        }
        #endregion

        #region Network Callbacks - Session Data
        protected override SessionData OnCreateEmptySessionData()
        {
            return new SolisSessionData();
        }

        protected override SessionData OnCreateNewSessionData(int clientId,
            NetworkSessionEstablishRequestPacket requestPacket)
        {
            var sessions = NetworkManager.Instance.GetAllSessionData<SolisSessionData>();
            // ReSharper disable once PossibleMultipleEnumeration
            var humanCount = sessions.Count(s => s.PlayerCharacterType == CharacterType.Human);
            // ReSharper disable once PossibleMultipleEnumeration
            var robotCount = sessions.Count(s => s.PlayerCharacterType == CharacterType.Robot);

            var type = humanCount <= robotCount ? CharacterType.Human : CharacterType.Robot;
            
            if(humanCount + robotCount == 0)
                type = defaultType;
            
            var request = (requestPacket as SolisNetworkSessionEstablishRequestPacket)!;
            return new SolisSessionData()
            {
                Username = request.Username,
                PlayerCharacterType = type
            };
        }

        protected override SessionData OnRestoreSessionData(int clientId,
            NetworkSessionEstablishRequestPacket requestPacket)
        {
            var request = (requestPacket as SolisNetworkSessionEstablishRequestPacket)!;
            return GetAllDisconnectedSessionData<SolisSessionData>()
                .FirstOrDefault((x) => x.Username == request.Username);
        }
        #endregion
    }
}