using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Interface;
using NetBuff;
using NetBuff.Misc;
using NetBuff.UDP;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace SolarBuff.Interface.Menu
{
    public class ServerList : MonoBehaviour
    {
        [Header("SETTINGS")]
        public int udpPort = 7777;

        [Header("REFERENCES")]
        public Transform viewport;
        public GameObject itemPrefab;
        public TMP_InputField inputNickName;
        public TMP_InputField inputAddress;
 
        private int _currentSearchId = 0;
        private readonly Queue<ServerDiscoverer.GameInfo> _servers = new Queue<ServerDiscoverer.GameInfo>();

        private void OnEnable()
        {
            UpdateList();
            inputNickName.text = GenerateName();
        }

        public async void UpdateList()
        {
            try
            {
                var id = ++_currentSearchId;

                //Clear all viewports
                foreach (Transform child in viewport)
                    Destroy(child.gameObject);

                void OnFindServer(ServerDiscoverer.GameInfo gameInfo)
                {
                    _servers.Enqueue(gameInfo);
                }

                void OnSearchFinished()
                {
                    if (id != _currentSearchId)
                        return;
                }

                ServerDiscoverer.FindServers(udpPort, OnFindServer, OnSearchFinished);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        private void Update()
        {
            while (_servers.Count > 0)
            {
                var gameInfo = _servers.Dequeue();
                var go = Instantiate(itemPrefab, viewport);
                go.GetComponent<ServerListItem>().Show(gameInfo);
            }
        }

        public async void HostGame()
        {
            var op = SceneManager.LoadSceneAsync("Scenes/Gameplay");
            while (!op.isDone)
                await Task.Delay(100);
            await Task.Delay(500);
            Chat.LocalPlayerName = inputNickName.text;
            NetworkManager.Instance.transport.Name = inputNickName.text;
            if (NetworkManager.Instance.transport is UDPNetworkTransport udp)
                udp.address = inputAddress.text;
            NetworkManager.Instance.StartHost();
        }

        private static string GenerateName()
        {
            string[] vowels = {"a", "e", "i", "o", "u"};
            string[] others = {"jh", "w", "n", "g", "gn", "b", "t", "th", "r", "l", "s", "sh", "k", "m", "d", "f", "v", "z", "p", "j", "ch"};
            
            var i = Random.Range(0, 2);
            var size = Random.Range(3, 10);
            var name = "";

            for (var j = 0; j < size; j++)
            {
                if(i%2 == 0)
                    name += vowels[Random.Range(0, vowels.Length)];
                else
                    name += others[Random.Range(0, others.Length)];

                i++;
            }
            
            //first letter capital
            return name[0].ToString().ToUpperInvariant() + name[1..];
        }
    }
}
