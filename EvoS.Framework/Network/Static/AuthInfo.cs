using EvoS.Framework.Constants.Enums;
using System;
using System.IO;
using Newtonsoft.Json;

namespace EvoS.Framework.Network.Static
{
    [Serializable]
    [EvosMessage(781, typeof(AuthInfo))]
    public class AuthInfo
    {
        public string AccountCurrency { get; set; }
        public long AccountId { get; set; }
        public string AccountStatus { get; set; }
        public string Handle { get; set; }
        public string Password { internal get; set; }  // internal so that it is not serialized
        public long SteamId { get; set; }
        public string TicketData { get; set; }
        public AuthType Type { get; set; }
        public string UserName { get; set; }

        [JsonIgnore] public string _Password => Password;
    }
}
