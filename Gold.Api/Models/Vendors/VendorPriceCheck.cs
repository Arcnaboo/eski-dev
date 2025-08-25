using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Vendors
{
    public class VendorPriceCheck
    {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("gram_amount")]
        public decimal? GramAmount { get; set; }

        public static bool CheckValid(VendorPriceCheck model)
        {
            if (model == null || model.ApiKey == null || !model.GramAmount.HasValue)
            {
                return false;
            }
            return true;
        }
    }

    public class VendorPriceCheckResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("buy")]
        public decimal Buy { get; set; }

        [JsonProperty("sell")]
        public decimal Sell { get; set; }

        [JsonProperty("buy_silver")]
        public decimal BuySilver { get; set; }

        [JsonProperty("sell_silver")]
        public decimal SellSilver { get; set; }

        [JsonProperty("buy_platin")]
        public decimal BuyPlatin { get; set; }

        [JsonProperty("sell_platin")]
        public decimal SellPlatin { get; set; }
    }
}
