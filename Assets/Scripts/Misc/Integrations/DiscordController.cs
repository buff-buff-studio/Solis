using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;
using Solis.Core;
using Solis.Data;

namespace Solis.Misc.Integrations
{
    /// <summary>
    /// Used to handle Discord integration.
    /// </summary>
    public class DiscordController : MonoBehaviour
    {
        private static readonly long CLIENT_ID = 1287743540322897920;

        public static DiscordController Instance { get; private set; }
        public static long LobbyStartTimestamp;
        public static CharacterType CharacterType;
        public static string RelayCode;

        private Discord.Discord _discord;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            //_discord = new Discord.Discord(CLIENT_ID, (ulong) CreateFlags.NoRequireDiscord);
            SetMenuActivity();
        }

        private void Update()
        {
            //_discord.RunCallbacks();
        }

        private void OnApplicationQuit()
        {
            //_discord.Dispose();
        }

        public void UpdateActivity(Activity activity)
        {
            var activityManager = _discord.GetActivityManager();
            activityManager.UpdateActivity(activity, result =>
            {
                if (result != Result.Ok)
                {
                    Debug.LogError("Failed to update Discord activity: " + result);
                }else
                {
                    Debug.Log("Discord activity updated successfully.");
                }
            });
        }

        public void SetMenuActivity()
        {
            return;
            var activity = new Activity
            {
                Details = "Playing Solis",
                State = "In Menu",
                Assets =
                {
                    LargeImage = "solis_logo",
                    LargeText = "Solis"
                }
            };

            UpdateActivity(activity);
        }

        public void SetLobbyActivity(int playersCount)
        {
            return;
            var activity = new Activity
            {
                ApplicationId = CLIENT_ID,
                Name = "Solis",
                Details = "Playing Solis",
                State = "In Lobby",
                Assets =
                {
                    LargeImage = "solis_logo",
                    LargeText = "*uebeti*",
                    SmallImage = CharacterType == CharacterType.Human ? "nina_icon" : "ram_icon",
                    SmallText = CharacterType == CharacterType.Human ? "Nina" : "RAM"
                },
                Timestamps =
                {
                    Start = LobbyStartTimestamp
                },
                Party =
                {
                    Size =
                    {
                        CurrentSize = playersCount,
                        MaxSize = 2
                    },
                    Id = "SolisNetworkManager.Instance.LocalClientIds[0].ToString()"
                },
                Secrets =
                {
                    Join = RelayCode
                }
            };

            UpdateActivity(activity);
        }
    }
}