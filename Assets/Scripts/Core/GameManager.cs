using System;
using System.Linq;
using NetBuff;
using NetBuff.Components;
using NetBuff.Misc;
using Solis.Player;
using Solis.Data;
using Solis.Data.Saves;
using Solis.Interface.Lobby;
using Solis.Misc.Props;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("sceneRegistry")]
        [Header("REFERENCES")]
        public GameRegistry registry;
        
        [Header("PREFABS")]
        public GameObject playerHumanLobbyPrefab;
        public GameObject playerRobotLobbyPrefab;
        public GameObject playerHumanGamePrefab;
        public GameObject playerRobotGamePrefab;

        [Header("SETTINGS")]
        public string[] persistentScenes = { "Core" };
        #endregion

        #region Private Fields
        [SerializeField]
        private Save save = new();

        [SerializeField]
        private bool playedCutscene;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns true if the game is currently in the lobby.
        /// </summary>
        public bool IsOnLobby => SceneManager.GetSceneByName(registry.sceneLobby.Name).isLoaded;
        
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
        
        /// <summary>
        /// Returns the current level info.
        /// </summary>
        public LevelInfo CurrentLevel => save.data.currentLevel < 0 ? null : registry.levels[save.data.currentLevel];
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
            
            //If it's the cutscene scene, don't spawn the player
            if (NetworkManager.Instance.LoadedScenes.Contains(registry.sceneCutscene.Name))
                return;
            
            _RespawnPlayerForClient(clientId);
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
            save.SaveData(null);
            LoadLevel();
        }
        
        /// <summary>
        /// Returns to the lobby.
        /// Called only on the server.
        /// </summary>
        [ServerOnly]
        public void ReturnToLobby()
        {
            var manager = NetworkManager.Instance!;
            
            foreach (var s in manager.LoadedScenes)
            {
                if (Array.IndexOf(persistentScenes, s) == -1 && s != name)
                    manager.UnloadScene(s);
            }
            
            if (!manager.IsSceneLoaded(registry.sceneLobby.Name))
                manager.LoadScene(registry.sceneLobby.Name).Then((_) =>
                {
                    foreach (var clientId in manager.GetConnectedClients())
                        _RespawnPlayerForClient(clientId);
                });
        }

        /// <summary>
        /// Loads the specified level.
        /// Can be used to restart the current level.
        /// Called only on the server.
        /// </summary>
        [ServerOnly]
        public void LoadLevel()
        {
            var manager = NetworkManager.Instance!;
            var levelInfo = registry.levels[save.data.currentLevel];

            var scene = levelInfo.scene.Name;
            
            //Unload other scenes
            foreach (var s in manager.LoadedScenes)
            {
                if (Array.IndexOf(persistentScenes, s) == -1  && s != name)
                    manager.UnloadScene(s);
            }
            
            if (levelInfo.hasCutscene && !playedCutscene)
            {
                if (IsOnLobby)
                {
                    manager.UnloadScene(registry.sceneLobby.Name).Then((_) =>
                    {
                        _LoadSceneSafely(registry.sceneCutscene.Name);
                    });
                }
                else
                {
                    _LoadSceneSafely(registry.sceneCutscene.Name);
                }

                playedCutscene = true;
                return;
            }
            
            playedCutscene = false;

            if (IsOnLobby)
            {
                manager.UnloadScene(registry.sceneLobby.Name).Then((_) =>
                {
                    _LoadSceneSafely(scene, (_) =>
                    {
                        foreach (var clientId in manager.GetConnectedClients())
                            _RespawnPlayerForClient(clientId);
                    });
                });
            }
            else
            {
                _LoadSceneSafely(scene, (_) =>
                {
                    foreach (var clientId in manager.GetConnectedClients())
                        _RespawnPlayerForClient(clientId);
                });
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
                _RespawnPlayerForClient(client);
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
            _RespawnPlayerForClient(client); 
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
        private void _RespawnPlayerForClient(int clientId)
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
                if (IsOnLobby)
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
                    ? (IsOnLobby ? playerHumanLobbyPrefab : playerHumanGamePrefab)
                    : (IsOnLobby ? playerRobotLobbyPrefab : playerRobotGamePrefab);
                
                Spawn(prefab, spawnPos, Quaternion.identity, Vector3.one, true, clientId);
                #endregion
            }
        }

        private void _LoadSceneSafely(string scene, Action<int> then = null)
        {
            var manager = NetworkManager.Instance!;
            if(!manager.IsSceneLoaded(scene))
                manager.LoadScene(scene).Then(then);
            else manager.UnloadScene(scene).Then((_) =>
            {
                manager.LoadScene(scene).Then(then);
            });
        }
        #endregion
    }
}