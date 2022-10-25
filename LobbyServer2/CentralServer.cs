using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using EvoS.Framework;
using EvoS.Framework.Logging;

namespace CentralServer
{
    public class CentralServer
    {
        public static void Main(string[] args)
        {
            int port = EvosConfiguration.GetLobbyServerPort();
            WebSocketServer server = new WebSocketServer(port);
            server.AddWebSocketService<LobbyServer.LobbyServerProtocol>("/LobbyGameClientSessionManager");
            server.AddWebSocketService<BridgeServer.BridgeServerProtocol>("/BridgeServer");
            server.Log.Level = LogLevel.Debug;


            server.Start();
            Log.Print(LogType.Lobby, $"Started lobby server on port {port}");
            if (EvosConfiguration.GetGameServerExecutable().IsNullOrEmpty())
            {
                Log.Print(LogType.Warning, "GameServerExecutable not set in settings.yaml");
                Log.Print(LogType.Warning, "Automatic game server launch is disabled. Game servers can still connect to this lobby");
            }
            
            
            
            //server.Stop();
        }
    }
}
