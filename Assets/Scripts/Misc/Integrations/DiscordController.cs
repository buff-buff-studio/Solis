using System;
using Discord;
using Solis.Data;
using UnityEngine;

namespace Solis.Misc.Integrations
{
    /// <summary>
    /// Used to handle Discord integration.
    /// </summary>
    public class DiscordController : MonoBehaviour
    {
        private static readonly long CLIENT_ID = 1287743540322897920;

        public static DiscordController Instance;
        public static long LobbyStartTimestamp;
        public static bool IsConnected;
        public static string Username;

        public string user, id, discriminator, avatar;

        public Discord.Discord Discord;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }else Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Debug.Log("Starting Discord Rich Presence");

            try
            {
                Discord = new Discord.Discord(CLIENT_ID, (UInt64)CreateFlags.NoRequireDiscord);
                if (Discord == null)
                {
                    Debug.LogError("Failed to initialize Discord Rich Presence");
                    IsConnected = false;
                    this.enabled = false;
                    return;
                }

                var activityManager = Discord.GetActivityManager();
                var activity = new Activity
                {
                    Details = "Playing Solis",
                    State = "In Menu",
                    Assets =
                    {
                        LargeImage = "solis_logo",
                        LargeText = "*uebeti*"
                    }
                };

                activityManager.UpdateActivity(activity, result =>
                {
                    if (result == Result.Ok)
                    {
                        Debug.Log("Discord Rich Presence updated successfully");
                        IsConnected = true;
                        Discord.GetUserManager().OnCurrentUserUpdate += () =>
                        {
                            var user = Discord.GetUserManager().GetCurrentUser();
                            Debug.Log("Discord Rich Presence connected as: " + user.Username);
                            this.user = user.Username;
                            avatar = user.Avatar;
                            Username = user.Username;
                        };
                    }
                    else
                    {
                        Debug.LogError("Failed to update Discord Rich Presence");
                        IsConnected = false;
                        this.enabled = false;
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to initialize Discord Rich Presence: " + e);
                IsConnected = false;
                this.enabled = false;
                throw;
            }

        }

        private void Update()
        {
            Discord.RunCallbacks();
        }

        private void OnApplicationQuit()
        {
            if(!IsConnected) return;
            Discord.Dispose();
        }

        public void SetGameActivity(CharacterType characterType, bool inLobby = true)
        {
            if(!IsConnected) return;

            var activityManager = Discord.GetActivityManager();
            var activity = new Activity
            {
                ApplicationId = CLIENT_ID,
                Name = "Solis",
                Details = "Playing Solis",
                State = inLobby ? "In Lobby" : "In Game",
                Assets =
                {
                    LargeImage = "solis_logo",
                    LargeText = "*uebeti*",
                    SmallImage = characterType == CharacterType.Human ? "nina_icon" : "ram_icon",
                    SmallText = characterType == CharacterType.Human ? "Nina" : "RAM"
                },
                Timestamps =
                {
                    Start = LobbyStartTimestamp
                }
            };


            activityManager.UpdateActivity(activity, result =>
            {
                if (result == Result.Ok)
                    Debug.Log("Discord Rich Presence updated successfully");
                else
                    Debug.LogError("Failed to update Discord Rich Presence");
            });
        }

        public void SetMenuActivity()
        {
            if(!IsConnected) return;

            var activityManager = Discord.GetActivityManager();
            var activity = new Activity
            {
                ApplicationId = CLIENT_ID,
                Name = "Solis",
                Details = "Playing Solis",
                State = "In Menu",
                Assets =
                {
                    LargeImage = "solis_logo",
                    LargeText = "*uebeti*"
                }
            };

            activityManager.UpdateActivity(activity, result =>
            {
                if (result == Result.Ok)
                    Debug.Log("Discord Rich Presence updated successfully");
                else
                    Debug.LogError("Failed to update Discord Rich Presence");
            });
        }
    }
}