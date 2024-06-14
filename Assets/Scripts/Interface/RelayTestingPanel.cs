using System;
using Solis.Core;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interface
{
    public class RelayTestingPanel : MonoBehaviour
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public TMP_InputField inputFieldUsername;
        public TMP_InputField inputRelayCode;
        #endregion

        private async void OnEnable()
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public void Create()
        {
            SolisNetworkManager.username = inputFieldUsername.text;
            SolisNetworkManager.isJoining = false;
            SolisNetworkManager.usingRelay = true;

            SceneManager.LoadScene("Core");
        }

        public void Join()
        {
            SolisNetworkManager.username = inputFieldUsername.text;
            SolisNetworkManager.isJoining = true;
            SolisNetworkManager.usingRelay = true;
            SolisNetworkManager.relayCode = inputRelayCode.text;

            SceneManager.LoadScene("Core");
        }
    }
}