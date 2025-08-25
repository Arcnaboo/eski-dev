using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Users
{
    public class UserStatus
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("events")]
        public bool Events { get; set; }

        [JsonProperty("transactions")]
        public bool Transactions { get; set; }

        [JsonProperty("notifications")]
        public bool Notifications { get; set; }

        [JsonProperty("balance")]
        public bool Balance { get; set; }

        public override string ToString()
        {
            return "UserStatus: events " + Events + " tra " + Transactions + " n " + Notifications + " b " + Balance;
        }
    }
}
