using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Transactions
{
    public class UserModel3
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("memberid")]
        public int MemberId { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }
    }
}
