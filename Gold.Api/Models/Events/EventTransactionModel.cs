using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Events
{
    public class EventTransactionModel
    {
        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("photo")]
        public string Photo { get; set; }

        [JsonProperty("grams")]
        public decimal Grams { get; set; }

        [JsonProperty("money")]
        public decimal Money { get; set; }


        
    }
}
