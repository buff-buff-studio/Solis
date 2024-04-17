using System.Threading.Tasks;
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
        [Header("REFERENCES")]
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
        
        public void Join()
        {
            var sList = GetComponentInParent<ServerList>();
            TempData.PlayerName = sList.inputNickName.text;
            sList.windowManager.ShowWindow(4);

            var udpTransport = (NetworkManager.Instance.transport as UDPNetworkTransport)!;
            udpTransport.address = GetComponentInParent<ServerList>().inputAddressJoin.text;
            udpTransport.StartClient();
        }
    }
}