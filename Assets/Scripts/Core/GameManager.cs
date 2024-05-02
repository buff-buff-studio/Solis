using System.Linq;
using NetBuff;
using NetBuff.Components;
using NetBuff.Misc;
using Solis.Player;
using Solis.Data;
using Solis.Interface.Lobby;
using Solis.Misc;
using UnityEngine;

namespace Solis.Core
{
    /// <summary>
    /// Main game manager class that handles game logic and player spawning.
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        #region Public Static Properties
        /// <summary>
        /// Returns the instance of the GameManager.
        /// </summary>
        public static GameManager Instance { get; private set; }
        #endregion
        
        #region Inspector Fields
        [Header("PREFABS")]
        public GameObject playerHumanLobbyPrefab;
        public GameObject playerRobotLobbyPrefab;
        public GameObject playerHumanGamePrefab;
        public GameObject playerRobotGamePrefab;
        #endregion

        #region Private Fields
        [SerializeField]
        private Save save = new();
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns true if the game is currently in the lobby.
        /// </summary>
        public bool IsOnLobby => NetworkManager.Instance.IsSceneLoaded("Lobby");

        /// <summary>
        /// Returns the current level of the game.
        /// </summary>
        public int CurrentLevel
        {
            get
            {
                var level = NetworkManager.Instance.LoadedScenes.FirstOrDefault(x => x.StartsWith("Level"));
                if (level == null)
                    return 0;
                
                return int.TryParse(level.Replace("Level ", ""), out var result) ? result : 0;
            }
        }

        /// <summary>
        /// Returns the save instance.
        /// </summary>
        public Save Save => save;

        /// <summary>
        /// Returns current save data.
        /// </summary>
        public SaveData SaveData
        {
            get => save.data;
            set => save.data = value;
        }
        #endregion

        #region Unity Callbacks
        private void OnEnable()
        {
            Instance = this;
        }

        private void Update()
        {
            if (!save.IsSaved)
                return;

            save.playTime += Time.deltaTime;
        }

        private void OnDisable()
        {
            Instance = null;
        }
        
        private void OnApplicationQuit()
        {
            if (save.IsSaved)
                save.SaveData(null);
        }
        #endregion

        #region Network Callbacks
        public override void OnClientConnected(int clientId)
        {
            if (!HasAuthority)
                return;

            _RespawnPlayerForClient(clientId, IsOnLobby);
        }

        public override void OnClientDisconnected(int clientId)
        {
            var spawnPoints = FindObjectsByType<LobbySpawnPoint>(FindObjectsSortMode.InstanceID)
                .Where(x => x.occupiedBy == clientId).ToArray();
            foreach (var spawnPoint in spawnPoints)
                spawnPoint.occupiedBy = -1;
        }
        #endregion

        #region Public Methods - Game
        /// <summary>
        /// Starts the game.
        /// Called only on the server.
        /// </summary>
        [ServerOnly]
        public void StartGame()
        {
            LoadLevel(SaveData.currentLevel);
        }
        
        /// <summary>
        /// Returns to the lobby.
        /// Called only on the server.
        /// </summary>
        [ServerOnly]
        public void ReturnToLobby()
        {
            var currentLevel = CurrentLevel;
            var manager = NetworkManager.Instance!;
            
            if (manager.IsSceneLoaded($"Level {currentLevel}"))
                manager.UnloadScene($"Level {currentLevel}");
            
            if (!manager.IsSceneLoaded("Lobby"))
                manager.LoadScene("Lobby").Then((_) =>
                {
                    foreach (var clientId in manager.GetConnectedClients())
                        _RespawnPlayerForClient(clientId, true);
                });
        }

        /// <summary>
        /// Loads the specified level.
        /// Can be used to restart the current level.
        /// Called only on the server.
        /// </summary>
        /// <param name="level"></param>
        [ServerOnly]
        public void LoadLevel(int level)
        {
            var currentLevel = CurrentLevel;
            SaveData.currentLevel = level;
            
            var manager = NetworkManager.Instance!;

            if (IsOnLobby)
            {
                manager.UnloadScene("Lobby").Then((_) =>
                {
                    //load level scene
                    manager.LoadScene($"Level {level}").Then((_) =>
                    {
                        foreach (var clientId in manager.GetConnectedClients())
                            _RespawnPlayerForClient(clientId, false);
                    });
                });
            }
            else
            {
                if (manager.IsSceneLoaded($"Level {currentLevel}"))
                {
                    manager.UnloadScene($"Level {currentLevel}").Then((_) =>
                    {
                        //load level scene
                        manager.LoadScene($"Level {level}").Then((_) =>
                        {
                            foreach (var clientId in manager.GetConnectedClients())
                                _RespawnPlayerForClient(clientId, false);
                        });
                    });
                }
                else
                {
                    //load level scene
                    manager.LoadScene($"Level {level}").Then((_) =>
                    {
                        foreach (var clientId in manager.GetConnectedClients())
                            _RespawnPlayerForClient(clientId, false);
                    });
                }
            }
        }
        
        /// <summary>
        /// Called when the player exits the game.
        /// </summary>
        public void OnExitGame()
        {
            if (save.IsSaved)
                save.SaveData(null);
        }
        #endregion

        #region Public Methods - Player
        /// <summary>
        /// Changes the character type of the player.
        /// Called only on the server.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="type"></param>
        [ServerOnly]
        public void SetCharacterType(int client, CharacterType type)
        {
            if (NetworkManager.Instance.TryGetSessionData<SolisSessionData>(client, out var data))
            {
                data.PlayerCharacterType = type;
                _RespawnPlayerForClient(client, IsOnLobby);
                if (LobbyScreen.Instance != null)
                    LobbyScreen.Instance.UpdateRoom();
            }
        }
        
        /// <summary>
        /// Respawns the player.
        /// Called only on the server.
        /// </summary>
        /// <param name="client"></param>
        [ServerOnly]
        public void RespawnPlayer(int client)
        {
            _RespawnPlayerForClient(client, IsOnLobby); 
        }
        #endregion

        #region Public Methods - Save
        /// <summary>
        /// Prepares the save data, creates a new save if it doesn't exist.
        /// Called only on the server.
        /// </summary>
        [ServerOnly]
        public void PrepareSaveData()
        {
            if (!save.IsSaved)
                save.New();
        }
        #endregion

        #region Private Methods
        [ServerOnly]
        private void _RespawnPlayerForClient(int clientId, bool isLobby)
        {
            if (NetworkManager.Instance.TryGetSessionData<SolisSessionData>(clientId, out var data))
            {
                #region Remove Existing
                var existingPlayer = FindObjectsByType<PlayerLobby>(FindObjectsSortMode.None)
                    .FirstOrDefault(x => x.OwnerId == clientId);
                
                if (existingPlayer != null)
                {
                    existingPlayer.ForceSetOwner(-1);
                    existingPlayer.Identity.Despawn();
                }
                
                var existingPlayerController = FindObjectsByType<PlayerControllerBase>(FindObjectsSortMode.None)
                    .FirstOrDefault(x => x.OwnerId == clientId);

                if (existingPlayerController != null)
                    return;
                
                #endregion

                #region Spawn New
                var spawnPos = Vector3.zero;
                if (isLobby)
                {
                    var spawnPoint = FindObjectsByType<LobbySpawnPoint>(FindObjectsSortMode.InstanceID)
                        .FirstOrDefault(x => x.occupiedBy == -1 || x.occupiedBy == clientId);

                    spawnPoint!.occupiedBy = clientId;
                    spawnPos = spawnPoint.transform.position;
                }
                else
                {
                    var spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.InstanceID).Where(x => x.characterType == data.PlayerCharacterType).ToArray();
                    if(spawnPoints.Length > 0)
                        spawnPos = spawnPoints[clientId % spawnPoints.Length].transform.position;
                }

                var prefab = data.PlayerCharacterType == CharacterType.Human
                    ? (isLobby ? playerHumanLobbyPrefab : playerHumanGamePrefab)
                    : (isLobby ? playerRobotLobbyPrefab : playerRobotGamePrefab);
                
                Spawn(prefab, spawnPos, Quaternion.identity, Vector3.one, true, clientId);
                #endregion
            }
        }
        #endregion
    }
}