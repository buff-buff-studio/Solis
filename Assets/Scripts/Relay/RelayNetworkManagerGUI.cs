using System;
using TMPro;
using UnityEngine;

namespace NetBuff.Relays
{
    public class RelayNetworkManagerGUI : MonoBehaviour
    {
        public string code = "";
        public TMP_Text text;

        private void FixedUpdate()
        {
            var rnm = (RelayNetworkManager) NetworkManager.Instance;
            if (rnm.Transport.Type != NetworkTransport.EnvironmentType.None)
            {
                text.text = "Room: " + code;
            }
        }

        /*
        private void OnGUI()
        {
            var rnm = (RelayNetworkManager) NetworkManager.Instance;
            if (rnm.Transport.Type == NetworkTransport.EnvironmentType.None)
            {
                code = GUI.TextField(new Rect(10, 10, 200, 20), code);

                if (GUI.Button(new Rect(10, 40, 200, 20), "Join"))
                {
                    Debug.Log("Joining with code: " + code);
                
                    rnm.JoinRelayServer(code, (success) =>
                    {
                        Debug.Log("Success: " + success);
                    });
                }

                if (GUI.Button(new Rect(10, 70, 200, 20), "Create"))
                {
                    Debug.Log("Creating");
                    rnm.StartRelayHost(4, "", (success, c) =>
                    {
                        this.code = c;
                        Debug.Log("Success: " + success + " Code: " + code);
                    });
                }
            }
            else
            {
                GUI.Label(new Rect(10, 10, 200, 20), "Room: " + code);
                if (GUI.Button(new Rect(10, 40, 200, 20), "Close"))
                {
                    rnm.Close();
                }
            }
        }
         */
    }
}