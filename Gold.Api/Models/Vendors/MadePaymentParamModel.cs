using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Vendors
{
    public class MadePaymentParamModel
    {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("first_id")]
        public string FirstId { get; set; }

        [JsonProperty("last_id")]
        public string LastId { get; set; }
    }

    public class PaymentCompleteParamModel
    {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("result_id")]
        public string CheckId { get; set; }
    }
}
