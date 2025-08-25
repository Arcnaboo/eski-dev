using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class PriceCheckParamModel
    {
        [JsonProperty("type")]
        public string Type { get; set; } // gram, ceyrek, yarim, Tam, Cumhuriyet

        [JsonProperty("amount")]
        public decimal Amount { get; set; } // if gr then gr else amount of ceyrek or tam or cumhu..
    }
}