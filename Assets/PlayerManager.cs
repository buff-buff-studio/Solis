using System;
using System.Collections;
using System.Collections.Generic;
using ExamplePlatformer;
using NetBuff.Components;
using NetBuff.Misc;
using SolarBuff;
using SolarBuff.Player;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    private static readonly List<PlayerManager> Players = new List<PlayerManager>();
    
    public GameObject humanPrefab;
    public GameObject robotPrefab;
    public OrbitCamera orbitCamera;
    
    public StringNetworkValue playerName = new StringNetworkValue("");

    private void OnEnable()
    {
        Players.Add(this);
        WithValues(playerName);
        playerName.OnValueChanged += (oldValue, newValue) =>
        {
            RoomManager.Instance.roomList.text = string.Join("\n", Players.ConvertAll(p => p.playerName.Value));
        };
    }

    public override void OnSpawned(bool isRetroactive)
    {
        if (HasAuthority)
        {
            var type = (int)(Players.Count % 2 != 0 ? PlayerControllerCore.PlayerType.Human : PlayerControllerCore.PlayerType.Robot);
            Spawn(type == 0 ? humanPrefab : robotPrefab, Vector3.zero, Quaternion.identity, OwnerId);
        }
    }
}
