using System;
using Solis.Core;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interface
{
    public class RelayScreen : MonoBehaviour
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public TMP_InputField inputFieldUsername;
        public TMP_Text textLoggedIn;
        public TMP_InputField inputRelayCode;
        #endregion

        private async void OnEnable()
        {
#if PLATFORM_STANDALONE_WIN
            inputFieldUsername.text = System.Environment.UserName;
#else
            inputFieldUsername.text = SystemInfo.deviceName;
#endif
            if(string.IsNullOrEmpty(inputFieldUsername.text) || string.IsNullOrWhiteSpace(inputFieldUsername.text) || inputFieldUsername.text == "<unknown>")
                inputFieldUsername.text = $"Player {UnityEngine.Random.Range(0, 1000)}";

            //resize the input field to fit the text
            ResizeUsername();
            //resize the logged in text to fit the text
            textLoggedIn.ForceMeshUpdate();
            textLoggedIn.rectTransform.sizeDelta = new Vector2(textLoggedIn.preferredWidth, textLoggedIn.rectTransform.sizeDelta.y);

            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public void ResizeUsername()
        {
            inputFieldUsername.ForceLabelUpdate();
            inputFieldUsername.GetComponent<RectTransform>().sizeDelta =
                new Vector2(inputFieldUsername.preferredWidth, inputFieldUsername.GetComponent<RectTransform>().sizeDelta.y);
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