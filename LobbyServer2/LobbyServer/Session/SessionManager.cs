﻿using CentralServer.LobbyServer.Character;
using EvoS.Framework.Constants.Enums;
using EvoS.Framework.Logging;
using EvoS.Framework.Network.NetworkMessages;
using EvoS.Framework.Network.Static;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CentralServer.LobbyServer.Session
{
    public static class SessionManager
    {
        private static ConcurrentDictionary<long, LobbyServerPlayerInfo> ActivePlayers = new ConcurrentDictionary<long, LobbyServerPlayerInfo>();// key: AccountID
        private static ConcurrentDictionary<long, long> SessionTokenAccountIDCache = new ConcurrentDictionary<long, long>(); // key: SessionToken, value: AccountID
        private static ConcurrentDictionary<long, LobbyServerProtocol> ActiveConnections = new ConcurrentDictionary<long, LobbyServerProtocol>();
        private static ConcurrentDictionary<long, LobbySessionInfo> ActiveSessions = new ConcurrentDictionary<long, LobbySessionInfo>();
        private static long GeneratedSessionToken = 0;


        public static LobbyServerPlayerInfo OnPlayerConnect(LobbyServerProtocol client, RegisterGameClientRequest clientRequest)
        {
            long sessionToken = Interlocked.Increment(ref GeneratedSessionToken);
            Database.Account user = Database.Account.GetByUserName(clientRequest.AuthInfo.Handle);
            user.AccountId = clientRequest.AuthInfo.AccountId;

            client.AccountId = user.AccountId;
            client.SessionToken = sessionToken;
            client.UserName = user.UserName;
            client.SelectedGameType = user.LastSelectedGameType;
            client.SelectedSubTypeMask = 0;
            clientRequest.SessionInfo.SessionToken = sessionToken;

            LobbyServerPlayerInfo playerInfo = new LobbyServerPlayerInfo
            {
                AccountId = user.AccountId,
                BannerID = user.BannerID,
                BotCanTaunt = false,
                // BotsMasqueradeAsHumans = false,
                CharacterInfo = CharacterManager.GetCharacterInfo(user.AccountId, user.LastCharacter),
                ControllingPlayerId = 0,
                EffectiveClientAccessLevel = ClientAccessLevel.Full,
                EmblemID = user.EmblemID,
                Handle = user.UserName,
                IsGameOwner = true,
                IsLoadTestBot = false,
                IsNPCBot = false,
                PlayerId = 0,
                ReadyState = ReadyState.Unknown,
                ReplacedWithBots = false,
                RibbonID = user.RibbonID,
                TitleID = user.TitleID,
                TitleLevel = 1
            };

            ActivePlayers.TryAdd(user.AccountId, playerInfo);
            SessionTokenAccountIDCache.TryAdd(sessionToken, playerInfo.AccountId);
            ActiveConnections.TryAdd(user.AccountId, client);
            ActiveSessions.TryAdd(user.AccountId, clientRequest.SessionInfo);

            return playerInfo;
        }

        public static void OnPlayerDisconnect(LobbyServerProtocol client)
        {
            ActivePlayers.TryRemove(client.AccountId, out _);
            SessionTokenAccountIDCache.TryRemove(client.SessionToken, out _);
            ActiveConnections.TryRemove(client.AccountId, out _);
            ActiveSessions.TryRemove(client.AccountId, out _);
        }

        public static LobbyServerPlayerInfo GetPlayerInfo(long accountId)
        {
            LobbyServerPlayerInfo playerInfo = null;
            ActivePlayers.TryGetValue(accountId, out playerInfo);

            return playerInfo;
        }

        public static long GetAccountIdOf(long sessionToken)
        {
            if (SessionTokenAccountIDCache.TryGetValue(sessionToken, out long accountId))
            {
                return accountId;
            }

            return 0;
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
            ActiveSessions.TryGetValue(accountId, out sessionInfo);
            return sessionInfo;
        }
    }
}
