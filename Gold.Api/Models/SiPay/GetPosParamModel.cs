using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.SiPay
{
    public class GetPosParamModel
    {
        [JsonProperty("credit_card")]
        public decimal CreditCard { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }

        [JsonProperty("merchant_key")]
        public string MerchantKey { get; set; }
    }
}
