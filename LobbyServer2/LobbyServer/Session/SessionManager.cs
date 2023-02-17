using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CentralServer.LobbyServer.Group;
using EvoS.DirectoryServer.Account;
using EvoS.Framework.Constants.Enums;
using EvoS.Framework.DataAccess;
using EvoS.Framework.Network.NetworkMessages;
using EvoS.Framework.Network.Static;

namespace CentralServer.LobbyServer.Session
{
    public static class SessionManager
    {
        private static ConcurrentDictionary<long, LobbyServerPlayerInfo> ActivePlayers = new ConcurrentDictionary<long, LobbyServerPlayerInfo>();// key: AccountID
        private static ConcurrentDictionary<long, LobbyServerProtocol> ActiveConnections = new ConcurrentDictionary<long, LobbyServerProtocol>();
        private static ConcurrentDictionary<long, LobbySessionInfo> Sessions = new ConcurrentDictionary<long, LobbySessionInfo>();

        public static LobbyServerPlayerInfo OnPlayerConnect(LobbyServerProtocol client, RegisterGameClientRequest registerRequest)
        {
            if (registerRequest == null) return null;

            lock (Sessions)
            {
                if (registerRequest.SessionInfo == null) return null;
                if (registerRequest.SessionInfo.SessionToken == null || registerRequest.SessionInfo.SessionToken == 0) return null;
                
                LobbySessionInfo sessionInfo = GetSessionInfo(registerRequest.SessionInfo.AccountId);

                if (sessionInfo == null) return null; // Session not found
                if (sessionInfo.SessionToken != registerRequest.SessionInfo.SessionToken) return null; // Session token do not match

                long accountId = sessionInfo.AccountId;
            
                PersistedAccountData account = DB.Get().AccountDao.GetAccount(accountId);

                client.AccountId = account.AccountId;
                client.UserName = account.UserName;
                client.SelectedGameType = GameType.PvP;
                client.SelectedSubTypeMask = 0;
                client.SessionToken = sessionInfo.SessionToken;

                LobbyServerPlayerInfo playerInfo = UpdateLobbyServerPlayerInfo(account.AccountId);
                ActiveConnections.TryAdd(client.AccountId, client);

                return playerInfo;
            }
        }

        public static LobbyServerPlayerInfo UpdateLobbyServerPlayerInfo(long accountId)
        {
            PersistedAccountData account = DB.Get().AccountDao.GetAccount(accountId);
            LobbyServerPlayerInfo playerInfo = new LobbyServerPlayerInfo
            {
                AccountId = account.AccountId,
                BannerID = account.AccountComponent.SelectedBackgroundBannerID,
                BotCanTaunt = false,
                CharacterInfo = LobbyCharacterInfo.Of(account.CharacterData[account.AccountComponent.LastCharacter]),
                ControllingPlayerId = 0,
                EffectiveClientAccessLevel = ClientAccessLevel.Full,
                EmblemID = account.AccountComponent.SelectedForegroundBannerID,
                Handle = account.Handle,
                IsGameOwner = true,
                IsLoadTestBot = false,
                IsNPCBot = false,
                PlayerId = 0,
                ReadyState = ReadyState.Unknown,
                ReplacedWithBots = false,
                RibbonID = account.AccountComponent.SelectedRibbonID,
                TitleID = account.AccountComponent.SelectedTitleID,
                TitleLevel = 1
            };
            ActivePlayers.AddOrUpdate(playerInfo.AccountId, playerInfo, (k, v) => playerInfo);
            return playerInfo;
        }

        public static void OnPlayerDisconnect(LobbyServerProtocol client)
        {
            lock (Sessions)
            {
                ActivePlayers.TryRemove(client.AccountId, out _);
                LobbySessionInfo session = null;
                Sessions.TryGetValue(client.AccountId, out _);

                // Sometimes on reconnections we first have the new connection and then we receive the previous disconnection
                // To avoid deleting the new connection, we check if the sessiontoken is the same
                if (session != null && session.SessionToken == client.SessionToken)
                {
                    ActiveConnections.TryRemove(client.AccountId, out _);
                }
            }
        }

        public static LobbyServerPlayerInfo GetPlayerInfo(long accountId)
        {
            LobbyServerPlayerInfo playerInfo = null;
            ActivePlayers.TryGetValue(accountId, out playerInfo);

            return playerInfo;
        }

        public static LobbyServerProtocol GetClientConnection(long accountId)
        {
            LobbyServerProtocol clientConnection = null;
            ActiveConnections.TryGetValue(accountId, out clientConnection);
            return clientConnection;
        }

        public static LobbySessionInfo GetSessionInfo(long accountId)
        {
            LobbySessionInfo sessionInfo = null;
            Sessions.TryGetValue(accountId, out sessionInfo);
            return sessionInfo;
        }

        public static long? GetOnlinePlayerByHandle(string handle)
        {
            return ActivePlayers.Values.FirstOrDefault(lspi => lspi.Handle == handle)?.AccountId;
        }

        public static HashSet<long> GetOnlinePlayers()
        {
            return new HashSet<long>(ActivePlayers.Keys);
        }

        public static LobbySessionInfo CreateSession(long accountId)
        {
            // Remove any previous session from this account
            Sessions.TryRemove(accountId, out _);
            PersistedAccountData account = DB.Get().AccountDao.GetAccount(accountId);

            LobbySessionInfo sessionInfo = new LobbySessionInfo()
            {
                AccountId = accountId,
                Handle = account.Handle,
                UserName = account.Handle,
                ConnectionAddress = "127.0.0.1",
                BuildVersion = "STABLE-122-100",
                LanguageCode = "",
                FakeEntitlements = "",
                ProcessCode = "",
                ProcessType = ProcessType.AtlasReactor,
                SessionToken = GenerateToken(account.Handle),
                ReconnectSessionToken = GenerateToken(account.Handle),
                Region = Region.EU,
            };

            // Add session for account
            Sessions.TryAdd(accountId, sessionInfo);

            return sessionInfo;
        }

        public static void CleanSessionAfterReconnect(long accountId)
        {
            LobbyServerProtocol client = null;
            ActiveConnections.TryGetValue(accountId, out client);
            if (client != null)
            {
                client.WebSocket.Close();
                ActiveConnections.TryRemove(accountId, out client);
            }
        }

        private static long GenerateToken(string a)
        {
            int num = (Guid.NewGuid() + a).GetHashCode();
            if (num < 0)
            {
                num = -num;
            }
            return num;
        }
    }
}
