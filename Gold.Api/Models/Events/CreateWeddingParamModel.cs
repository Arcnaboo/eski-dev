using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Events
{
    public class CreateWeddingParamModel
    {
        [JsonProperty("wedding_date")]
        public string WeddingDate { get; set; }

        [JsonProperty("wedding_name")]
        public string WeddingName { get; set; }
		
        [JsonProperty("wedding_text")]
        public string WeddingText { get; set; }
		
        [JsonProperty("userid")]
        public string UserId { get; set; }
    }
}
