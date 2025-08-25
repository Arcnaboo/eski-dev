using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class ClaimGoldParamModel
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("id")]
        public string EventOrWeddingId { get; set; }
    }
}
