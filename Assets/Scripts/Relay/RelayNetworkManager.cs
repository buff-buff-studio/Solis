using System;

namespace NetBuff.Relays
{
    public class RelayNetworkManager : NetworkManager
    {
        public void StartRelayHost(int maxPlayers, string regionId, Action<bool, string> callback)
        {
            var tp = (RelayNetworkTransport) Transport;
            
            tp.AllocateRelayServer(maxPlayers, regionId,
                (joinCode) =>
                {
                    StartHost();
                    callback?.Invoke(true, joinCode);
                },
                () =>
                {
                    callback?.Invoke(false, "");
                });
        }
        
        public void StartRelayServer(int maxPlayers, string regionId, Action<bool, string> callback)
        {
            var tp = (RelayNetworkTransport) Transport;
            
            tp.AllocateRelayServer(maxPlayers, regionId,
                (joinCode) =>
                {
                    StartServer();
                    callback?.Invoke(true, joinCode);
                },
                () =>
                {
                    callback?.Invoke(false, "");
                });
        }
        
        public void JoinRelayServer(string code, Action<bool> callback)
        {
            var tp = (RelayNetworkTransport) Transport;
            
            tp.GetAllocationFromJoinCode(code,
                () =>
                {
                    StartClient();
                    callback?.Invoke(true);
                },
                () =>
                {
                    callback?.Invoke(false);
                });
        }
    }
}