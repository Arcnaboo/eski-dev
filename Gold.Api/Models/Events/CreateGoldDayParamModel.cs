using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Events
{
    public class CreateGoldDayParamModel
    {
        [JsonProperty("type")]
        public string IntervalType { get; set; }

        [JsonProperty("day_name")]
        public string GoldDayName { get; set; }

        [JsonProperty("grams")]
        public decimal GramAmount { get; set; }

        [JsonProperty("user_amount")]
        public int UserAmount { get; set; }

        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("start_date")]
        public string StartDateTime { get; set; }
    }
}
