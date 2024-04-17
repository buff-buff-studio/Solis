using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NetBuff;
using NetBuff.Misc;
using NetBuff.Packets;
using UnityEngine;
using UnityEngine.Assertions;

[Icon("Assets/CG/Sprites/SolisNetScript_Ico.png")]
public class SolisNetworkManager : NetworkManager
{
    public GameObject robotPrefab;
    public Transform robotSpawnPoint;
    public Transform humanSpawnPoint;

    private void Awake()
    {
        if (Instance == this || Instance == null) DontDestroyOnLoad(this.gameObject);
        else Destroy(this.gameObject);
    }

    protected override void SpawnPlayer(int clientId)
    {
#if UNITY_EDITOR
        if (!isClientReloaded)
        {
            if (spawnsPlayer)
            {
                var prefab = clientId == 0 ? playerPrefab : robotPrefab;
                var t = clientId == 0 ? humanSpawnPoint : robotSpawnPoint;
                Assert.IsTrue(prefabRegistry.IsPrefabValid(prefab), "Player prefab is not valid");
                SpawnNetworkObjectForClients(prefabRegistry.GetPrefabId(prefab), t.position, t.rotation, t.localScale, clientId);
            }
        }
#else
            if (spawnsPlayer)
            {
                var prefab = clientId == 0 ? playerPrefab : robotPrefab;
                var t = clientId == 0 ? humanSpawnPoint : robotSpawnPoint;
                Assert.IsTrue(prefabRegistry.IsPrefabValid(prefab), "Player prefab is not valid");
                SpawnNetworkObjectForClients(prefabRegistry.GetPrefabId(prefab), t.position, t.rotation, t.localScale, clientId);
            }
#endif
    }
}
