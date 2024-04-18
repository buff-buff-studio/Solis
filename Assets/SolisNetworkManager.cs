using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NetBuff;
using NetBuff.Components;
using NetBuff.Misc;
using NetBuff.Packets;
using UnityEngine;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

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
    
#if UNITY_EDITOR
    [CustomEditor(typeof(SolisNetworkManager))]
    public class NetworkManagerEditor : Editor
    {
        private static readonly FieldInfo _IDField = typeof(NetworkIdentity).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Regenerate Ids"))
            {
                foreach (var identity in FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Include,
                             FindObjectsSortMode.None))
                {
                    _IDField.SetValue(identity, NetworkId.New());
                    EditorUtility.SetDirty(identity);
                }
            }

            if (GUILayout.Button("Dump Ids"))
            {
                string path = EditorUtility.SaveFilePanel("Save Ids", "", "Ids", "txt");
                if (path.Length != 0)
                {
                    var ids = NetworkManager.Instance.GetNetworkObjects();

                    System.IO.File.WriteAllText(path, string.Join("\n", ids.Select(x => $"{x.gameObject.name}: {x.Id}")));
                    System.Diagnostics.Process.Start(path);
                }
            }

        }
    }
#endif
}
