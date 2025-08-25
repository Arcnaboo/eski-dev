using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Transactions
{
    public class TransferFromUserToUserParamModel
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("member_name")]
        public string MemberName { get; set; }

        [JsonProperty("memberid")]
        public int MemberId { get; set; }

        [JsonProperty("type")]
        public string GoldType { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("transfer_message")]
        public string Text { get; set; }

    }
}
