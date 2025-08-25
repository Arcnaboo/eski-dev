using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Events
{
    public class GoldDayModel
    {
        [JsonProperty("gold_day_id")]
        public string GoldDayId { get; set; }

        [JsonProperty("date_start")]
        public string StartDate { get; set; }

        [JsonProperty("interval")]
        public string Interval { get; set; }

        [JsonProperty("grams")]
        public decimal GramAmount { get; set; }

    }
}
