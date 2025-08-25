using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class UserClaimGoldResponseParamModel
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("confirm")]
        public bool Confirm { get; set; }

        [JsonProperty("transactionid")]
        public string TransactionId { get; set; }
    }

    public class UserClaimGoldResponseResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
