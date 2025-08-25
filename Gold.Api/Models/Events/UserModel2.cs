using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Events
{
    public class UserModel2
    {
        [JsonProperty("memberid")]
        public int MemberId { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("is_complete")]
        public bool IsComplete { get; set; }

        [JsonProperty("is_next")]
        public bool IsNext { get; set; }

        [JsonProperty("pay_day")]
        public string PayDay { get; set; }
    }
}
