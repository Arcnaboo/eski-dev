using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Admin
{
    public class SellGoldRequestModel
    {

        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("grams")]
        public decimal Grams { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("onay_link")]
        public string OnayLink { get; set; }

        [JsonProperty("delete_link")]
        public string DeleteLink { get; set; }
    }
}
