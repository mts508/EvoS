using System;
using System.Collections.Generic;
using System.Linq;
using CentralServer.LobbyServer.Session;
using EvoS.Framework.Constants.Enums;
using EvoS.Framework.DataAccess;
using EvoS.Framework.Network.NetworkMessages;
using EvoS.Framework.Network.Static;
using static EvoS.Framework.Network.Static.SocialComponent;

namespace CentralServer.LobbyServer.Friend
{
    class FriendManager
    {
        public static FriendStatusNotification GetFriendStatusNotification(long accountId)
        {
            FriendStatusNotification notification = new FriendStatusNotification()
            {
                FriendList = GetFriendList(accountId)
            };

            return notification;
        }

        public static FriendList GetFriendList(long accountId)
        {
            FriendList friendList = new FriendList
            {
                // TODO We are all friends here for now
                Friends = SessionManager.GetOnlinePlayers()
                    .Where(id => id != accountId)
                    .Select(id => DB.Get().AccountDao.GetAccount(id))
                    .Where(acc => {
                        // If somebody blocked the other, they won't see each other in the friend list
                        return !IsBlockedBy(acc.AccountId, accountId) && !IsBlockedBy(accountId, acc.AccountId);
                    })
                    .ToDictionary(acc => acc.AccountId,
                        acc => new FriendInfo()
                        {
                            FriendAccountId = acc.AccountId,
                            FriendHandle = acc.Handle,
                            FriendStatus = FriendStatus.Friend,
                            IsOnline = true,
                            StatusString = GetStatusString(SessionManager.GetClientConnection(acc.AccountId)),
                            // FriendNote = 
                            BannerID = acc.AccountComponent.SelectedBackgroundBannerID,
                            EmblemID = acc.AccountComponent.SelectedForegroundBannerID,
                            TitleID = acc.AccountComponent.SelectedTitleID,
                            TitleLevel = acc.AccountComponent.TitleLevels.GetValueOrDefault(acc.AccountComponent.SelectedTitleID, 0),
                            RibbonID = acc.AccountComponent.SelectedRibbonID,
                        }),
                IsDelta = false
            };

            return friendList;
        }

        private static string GetStatusString(LobbyServerProtocol client)
        {
            if (client == null)
            {
                return "Offline";
            }
            if (client.IsInGame())
            {
                return "In Game";
            }
            if (client.IsInQueue())
            {
                return "Queued";
            }
            if (client.IsInGroup())
            {
                return "GroupChatRoom";  // No localization for "In Group" status so we have to borrow this one
            }
            return string.Empty;
        }

        public static PlayerUpdateStatusResponse OnPlayerUpdateStatusRequest(LobbyServerProtocol client, PlayerUpdateStatusRequest request)
        {
            // TODO: notify this client's friends the status change

            PlayerUpdateStatusResponse response = new PlayerUpdateStatusResponse()
            {
                AccountId = client.AccountId,
                StatusString = request.StatusString,
                ResponseId = request.RequestId
            };

            return response;
        }

        public static void BlockPlayer(LobbyServerProtocol client, long blockedAccountID)
        {
            PersistedAccountData account = DB.Get().AccountDao.GetAccount(client.AccountId);
            account.SocialComponent.UpdateNote(blockedAccountID, "Blocked");
            DB.Get().AccountDao.UpdateAccount(account);
        }

        public static bool IsBlockedBy(long otherId, long accountId)
        {
            PersistedAccountData accountData = DB.Get().AccountDao.GetAccount(accountId);

            if (accountData.SocialComponent == null || accountData.SocialComponent.FriendInfo == null) return false;
            FriendData friendData = null;
            if (accountData.SocialComponent.FriendInfo.TryGetValue(otherId, out friendData))
            {
                return friendData.LastSeenNote == "Blocked";
            }
            return false;
        }
    }
}
