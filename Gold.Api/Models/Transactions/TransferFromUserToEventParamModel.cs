using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class TransferFromUserToEventParamModel
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("eventid")]
        public string EventId { get; set; }

        [JsonProperty("type")]
        public string GoldType { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("transfer_message")]
        public string TransferMessage { get; set; }

    }
}
