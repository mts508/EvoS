using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CentralServer.LobbyServer;
using CentralServer.LobbyServer.Session;
using EvoS.Framework.Constants.Enums;
using EvoS.Framework.Logging;
using EvoS.Framework.Misc;
using EvoS.Framework.Network.NetworkMessages;
using EvoS.Framework.Network.Static;
using EvoS.Framework.Network.Unity;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace CentralServer.BridgeServer
{
    public class BridgeServerProtocol : WebSocketBehavior
    {
        public string Address;
        public int Port;
        private LobbyGameInfo GameInfo;
        private LobbyServerTeamInfo TeamInfo;
        public List<LobbyServerProtocolBase> clients = new List<LobbyServerProtocolBase>();
        public string URI => "ws://" + Address + ":" + Port;
        public GameStatus GameStatus { get; private set; } = GameStatus.Stopped;
        public string ProcessCode { get; } = "Artemis" + DateTime.Now.Ticks;

        public static readonly List<Type> BridgeMessageTypes = new List<Type>
        {
            typeof(RegisterGameServerRequest),
            typeof(RegisterGameServerResponse),
            typeof(LaunchGameRequest),
            typeof(JoinGameServerRequest),
            null, // typeof(JoinGameAsObserverRequest),
            null, // typeof(ShutdownGameRequest),
            null, // typeof(DisconnectPlayerRequest),
            null, // typeof(ReconnectPlayerRequest),
            null, // typeof(MonitorHeartbeatResponse),
            typeof(ServerGameSummaryNotification),
            null, // typeof(PlayerDisconnectedNotification),
            null, // typeof(ServerGameMetricsNotification),
            typeof(ServerGameStatusNotification),
            null, // typeof(MonitorHeartbeatNotification),
            null, // typeof(LaunchGameResponse),
            null, // typeof(JoinGameServerResponse),
            null, // typeof(JoinGameAsObserverResponse)
        };

        protected List<Type> GetMessageTypes()
        {
            return BridgeMessageTypes;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            NetworkReader networkReader = new NetworkReader(e.RawData);
            short messageType = networkReader.ReadInt16();
            int callbackId = networkReader.ReadInt32();
            List<Type> messageTypes = GetMessageTypes();
            if (messageType >= messageTypes.Count)
            {
                Log.Print(LogType.Error, $"Unknown bridge message type {messageType}");
                return;
            }

            Type type = messageTypes[messageType];

            if (type == typeof(RegisterGameServerRequest))
            {
                RegisterGameServerRequest request = Deserialize<RegisterGameServerRequest>(networkReader);
                string data = request.SessionInfo.ConnectionAddress;
                Address = data.Split(":")[0];
                Port = Convert.ToInt32(data.Split(":")[1]);
                ServerManager.AddServer(this);

                Send(new RegisterGameServerResponse
                    {
                        Success = true
                    },
                    callbackId);
            }
            else if (type == typeof(ServerGameSummaryNotification))
            {
                ServerGameSummaryNotification request = Deserialize<ServerGameSummaryNotification>(networkReader);
                Log.Print(LogType.Game, $"Game {GameInfo.Name} at {request.GameSummary.GameServerAddress} finished " +
                                        $"({request.GameSummary.NumOfTurns} turns), " +
                                        $"{request.GameSummary.GameResult} {request.GameSummary.TeamAPoints}-{request.GameSummary.TeamBPoints}");
                foreach (LobbyServerProtocolBase client in clients)
                {
                    MatchResultsNotification response = new MatchResultsNotification
                    {
                        // TODO
                        BadgeAndParticipantsInfo = request.GameSummary.BadgeAndParticipantsInfo
                    };
                    client.Send(response);
                }
            }
            else if (type == typeof(ServerGameStatusNotification))
            {
                ServerGameStatusNotification request = Deserialize<ServerGameStatusNotification>(networkReader);
                Log.Print(LogType.Game, $"Game {GameInfo.Name} {request.GameStatus}");
                GameStatus = request.GameStatus;
            }
            else
            {
                Log.Print(LogType.Game, $"Received unhandled bridge message type {(type != null ? type.Name : "id_" + messageType)}");
            }
        }

        private T Deserialize<T>(NetworkReader reader) where T : AllianceMessageBase
        {
            ConstructorInfo constructor = typeof(T).GetConstructor(Type.EmptyTypes);
            T o = (T)(AllianceMessageBase)constructor.Invoke(Array.Empty<object>());
            o.Deserialize(reader);
            return o;
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            ServerManager.RemoveServer(this.ID);
        }

        public bool IsAvailable()
        {
            return GameStatus == GameStatus.Stopped;
        }

        public void StartGame(LobbyGameInfo gameInfo, LobbyServerTeamInfo teamInfo)
        {
            GameInfo = gameInfo;
            TeamInfo = teamInfo;
            GameStatus = GameStatus.Assembling;
            Dictionary<int, LobbySessionInfo> SessionInfo = teamInfo.TeamPlayerInfo
                .ToDictionary(
                    playerInfo => playerInfo.PlayerId,
                    playerInfo => SessionManager.GetSessionInfo(playerInfo.AccountId) ?? new LobbySessionInfo());  // fallback for bots TODO something smarter

            foreach (LobbyServerPlayerInfo playerInfo in teamInfo.TeamPlayerInfo)
            {
                LobbySessionInfo sessionInfo = SessionInfo[playerInfo.PlayerId];
                JoinGameServerRequest request = new JoinGameServerRequest
                {
                    OrigRequestId = 0,
                    GameServerProcessCode = GameInfo.GameServerProcessCode,
                    PlayerInfo = playerInfo,
                    SessionInfo = sessionInfo
                };
                Send(request);
            }
            
            Send(new LaunchGameRequest()
            {
                GameInfo = gameInfo,
                TeamInfo = teamInfo,
                SessionInfo = SessionInfo,
                GameplayOverrides = new LobbyGameplayOverrides()
            });
        }

        public bool Send(AllianceMessageBase msg, int originalCallbackId = 0)
        {
            short messageType = GetMessageType(msg);
            if (messageType >= 0)
            {
                Send(messageType, msg, originalCallbackId);
                return true;
            }

            return false;
        }

        private bool Send(short msgType, AllianceMessageBase msg, int originalCallbackId = 0)
        {
            NetworkWriter networkWriter = new NetworkWriter();
            networkWriter.Write(msgType);
            networkWriter.Write(originalCallbackId);
            msg.Serialize(networkWriter);
            Send(networkWriter.ToArray());
            return true;
        }

        public short GetMessageType(AllianceMessageBase msg)
        {
            short num = (short)GetMessageTypes().IndexOf(msg.GetType());
            if (num < 0)
            {
                Log.Print(LogType.Error, $"Message type {msg.GetType().Name} is not in the MonitorGameServerInsightMessages MessageTypes list and doesnt have a type");
            }

            return num;
        }
    }
}