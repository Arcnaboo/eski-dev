using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Events
{
    public class GetEventResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("event")]
        public EventModel Event { get; set; }
    }
}
