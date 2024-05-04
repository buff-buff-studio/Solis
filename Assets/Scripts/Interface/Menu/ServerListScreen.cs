using System;
using System.Collections.Generic;
using Interface;
using NetBuff.Discover;
using NetBuff.UDP;
using Solis.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Solis.Interface.Menu
{
    /// <summary>
    /// Represents a screen that shows a list of servers.
    /// </summary>
    public class ServerListScreen : BaseListScreen<ServerListEntry>
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public TMP_InputField inputFieldUsername;
        public TMP_InputField inputFieldAddress;
        public Button refreshButton;

        [Header("SETTINGS")]
        public float timeout = 3f;
        #endregion

        #region Internal Fields
        private readonly List<ServerDiscoverer> _discoverers = new();
        private readonly List<ServerDiscoverer.ServerInfo> _servers = new();
        private readonly Queue<Action> _mainThreadActions = new();
        #endregion

        #region Unity Callbacks
        public void Update()
        {
            while (_mainThreadActions.Count > 0)
                _mainThreadActions.Dequeue().Invoke();
        }
        #endregion

        #region Abstract Methods Implementation
        protected override void OnRefreshList()
        {
            foreach (var sd in _discoverers)
                sd.Cancel();

            _discoverers.Clear();
            _discoverers.Add(new UDPServerDiscoverer(SolisNetworkManager.SOLIS_MAGIC_NUMBER,
                SolisNetworkManager.SOLIS_NETWORK_PORT));
            _servers.Clear();

            refreshButton.interactable = false;
   
            foreach (var sd in _discoverers)
                sd.Search(_OnFindServer, _OnSearchFinished);
            
            Invoke(nameof(_OnSearchFinished), timeout);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Joins a server at the specified index.
        /// </summary>
        /// <param name="index"></param>
        public void JoinServer(int index)
        {
            if (index < 0 || index >= _servers.Count)
                return;

            var server = _servers[index];

            if (server is UDPServerDiscoverer.UDPServerInfo udp)
                SolisNetworkManager.networkAddress = udp.Address.ToString();

            SolisNetworkManager.username = inputFieldUsername.text;
            SolisNetworkManager.isJoining = true;

            SceneManager.LoadScene("Core");
        }

        /// <summary>
        /// Hosts a server, then joins it.
        /// </summary>
        public void HostServer()
        {
            SolisNetworkManager.username = inputFieldUsername.text;
            SolisNetworkManager.networkAddress = inputFieldAddress.text;

            SceneManager.LoadScene("Core");
        }
        #endregion

        #region Private Methods
        private void _OnFindServer(ServerDiscoverer.ServerInfo info)
        {
            _mainThreadActions.Enqueue(() =>
            {
                var sle = CreateEntry();
                sle.textName.text = info.Name;
                sle.textPlayerCount.text = $"{info.Players} / {info.MaxPlayers}";
                sle.textPlatform.text = info.Platform.ToString();
                sle.iconHasPassword.gameObject.SetActive(info.HasPassword);

                var index = _servers.Count;
                sle.onClick = () => JoinServer(index);

                _servers.Add(info);
            });
        }

        private void _OnSearchFinished()
        {
            CancelInvoke(nameof(_OnSearchFinished));
            
            foreach (var sd in _discoverers)
                sd.Cancel();
            
            if (refreshButton != null)
                refreshButton.interactable = true;

            if (_servers.Count == 0)
                ShowEmpty();
        }
        #endregion
    }
}