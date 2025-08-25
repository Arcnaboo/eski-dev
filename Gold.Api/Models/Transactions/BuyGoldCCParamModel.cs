using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Transactions
{
    public class BuyGoldCCParamModel
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }
        
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        
        [JsonProperty("cc")]
        public string CreditCard { get; set; }

        [JsonProperty("expiry_month")]
        public string ExpiryMonth { get; set; }

        [JsonProperty("expiry_year")]
        public string ExpiryYear { get; set; }

        [JsonProperty("cardcode")]
        public int CardCode { get; set; }

        [JsonProperty("cc_holder_name")]
        public string HolderName { get; set; }
    }
    public class BuyCCEventparams
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("eventid")]
        public string EventId { get; set; }

        [JsonProperty("etype")]
        public string EventType { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("cc")]
        public string CreditCard { get; set; }

        [JsonProperty("expiry_month")]
        public string ExpiryMonth { get; set; }

        [JsonProperty("expiry_year")]
        public string ExpiryYear { get; set; }

        [JsonProperty("cardcode")]
        public string CardCode { get; set; }

        [JsonProperty("cc_holder_name")]
        public string HolderName { get; set; }
    }

    public class BuyCCparams
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("cc")]
        public string CreditCard { get; set; }

        [JsonProperty("expiry_month")]
        public string ExpiryMonth { get; set; }

        [JsonProperty("expiry_year")]
        public string ExpiryYear { get; set; }

        [JsonProperty("cardcode")]
        public string CardCode { get; set; }

        [JsonProperty("cc_holder_name")]
        public string HolderName { get; set; }
    }
}
