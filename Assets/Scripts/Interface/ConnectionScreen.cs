using System;
using NetBuff;
using SolarBuff.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Menu
{
    public class ConnectionScreen : MonoBehaviour
    {
        public NetworkManager manager;

        private void OnEnable()
        {
            manager.transport.OnConnect += OnConnect;
            manager.transport.OnDisconnect += OnDisconnect;
        }

        private void OnDisconnect()
        {
            SceneManager.LoadScene("Scenes/Menu");
            SaveManager.Instance.currentProfile = null;
        }

        private void OnConnect()
        {
            gameObject.SetActive(false);
        }
    }
}