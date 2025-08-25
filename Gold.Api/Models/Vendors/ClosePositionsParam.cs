using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Vendors
{
    public class ClosePositionsParam
    {
        [JsonProperty("not_position_ids")]
        public string NotPositionIds { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class ClosePositionsResult
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("results")]
        public List<object> Results { get; set; }

        [JsonProperty("removed_objects")]
        public List<object> Removed { get; set; }
    }
}
