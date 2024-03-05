using System.Threading.Tasks;
using Interface;
using NetBuff;
using NetBuff.Misc;
using NetBuff.UDP;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SolarBuff.Interface.Menu
{
    public class ServerListItem : MonoBehaviour
    {
        public TMP_Text labelRoomName;
        public TMP_Text labelPlayerCount;
        public TMP_Text labelPlatform;
        public GameObject iconHasPassword;
        public Button buttonJoin;
        
        private ServerDiscoverer.GameInfo _gameInfo;

        public void Show(ServerDiscoverer.GameInfo gameInfo)
        {
            buttonJoin.interactable = gameInfo.Players < gameInfo.MaxPlayers;
            _gameInfo = gameInfo;

            labelRoomName.text = gameInfo.Name;
            labelPlayerCount.text = gameInfo.Players + "/" + gameInfo.MaxPlayers;
            labelPlatform.text = gameInfo.Platform.ToString();
            iconHasPassword.SetActive(gameInfo.HasPassword);
            
            buttonJoin.onClick.RemoveAllListeners();
            buttonJoin.onClick.AddListener(Join);
        }
        
        public async void Join()
        {
            Chat.LocalPlayerName = GetComponentInParent<ServerList>().inputNickName.text;

            var op = SceneManager.LoadSceneAsync("Scenes/Gameplay");
            while (!op.isDone)
                await Task.Delay(100);
            await Task.Delay(500);
            
            if (_gameInfo is ServerDiscoverer.EthernetGameInfo ethernetGameInfo)
            {
                var udpTransport = (NetworkManager.Instance.transport as UDPNetworkTransport)!;
                udpTransport.address = ethernetGameInfo.Address.ToString();
                udpTransport.StartClient();
            }
        }
    }
}