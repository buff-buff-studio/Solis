using System;
using System.Linq;
using System.Threading.Tasks;
using NetBuff;
using NetBuff.UDP;
using SolarBuff.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SolarBuff.Interface.Menu
{
    public class SaveList : MonoBehaviour
    {
        [Header("REFERENCES")]
        public Transform viewport;
        public GameObject itemPrefab;
        public ServerList serverList;
        
        private void OnEnable()
        {
            UpdateList();
        }

        public async void UpdateList()
        {
            foreach (Transform child in viewport)
                Destroy(child.gameObject);

            var list = SaveManager.Instance.GetSaveProfiles().ToList();
            list.Sort((x, y) => y.modifiedTime.CompareTo(x.modifiedTime));
            
            foreach (var save in list)
            {
                var item = Instantiate(itemPrefab, viewport);
                item.GetComponent<SaveListItem>().Show(save);
            }
        }
        
        public void CreateSave()
        {
            var sm = SaveManager.Instance;
            sm.currentProfile = sm.CreateNewSave();
            sm.currentProfile.Save();
            StartServer();
        }

        public static async void StartServer()
        {
            var op = SceneManager.LoadSceneAsync("Scenes/Gameplay");
            while (!op.isDone)
                await Task.Delay(100);
            await Task.Delay(500);
            
            NetworkManager.Instance.transport.Name = TempData.PlayerName;
            if (NetworkManager.Instance.transport is UDPNetworkTransport udp)
            {
                udp.address = TempData.ServerAddress;
                udp.port = TempData.ServerPort;
            }
            NetworkManager.Instance.StartHost();
        }

        public void Return()
        {
            gameObject.SetActive(false);
            serverList.gameObject.SetActive(true);
        }
    }
}