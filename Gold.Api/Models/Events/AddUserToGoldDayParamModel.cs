using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Events
{
    public class AddUserToGoldDayParamModel
    {

        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("gold_day_id")]
        public string GoldDayId { get; set; }

        [JsonProperty("memberid")]
        public string MemberId { get; set; }

    }
}
