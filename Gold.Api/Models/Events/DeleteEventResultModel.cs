using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Events
{
    public class DeleteEventResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("event")]
        public EventModel Event { get; set; }
    }

    public class DeleteEventParamModel
    {
        [JsonProperty("event_id")]
        public string Id { get; set; }

        [JsonProperty("claim_gold")]
        public bool Claim { get; set; }
    }
}
