using NetBuff;
using UnityEngine;

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
            /*
            SceneManager.LoadScene("Scenes/Menu");

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.CurrentProfile.Save();
            }
            */
        }

        private void OnConnect()
        {
            gameObject.SetActive(false);
        }
    }
}