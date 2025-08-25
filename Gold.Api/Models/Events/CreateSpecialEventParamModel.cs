using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Events
{
    public class CreateSpecialEventParamModel
    {
        [JsonProperty("event_date")]
        public string EventDate { get; set; }

        [JsonProperty("event_name")]
        public string EventName { get; set; }

        [JsonProperty("event_text")]
        public string EventText { get; set; }

        [JsonProperty("userid")]
        public string UserId { get; set; }
    }

    public class CreateDonationEventParamModel
    {
        [JsonProperty("event_date")]
        public string EventDate { get; set; }

        [JsonProperty("event_name")]
        public string EventName { get; set; }

        [JsonProperty("event_text")]
        public string EventText { get; set; }

        [JsonProperty("donator_id")]
        public string DonatorId { get; set; }
    }
}
