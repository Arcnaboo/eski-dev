using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Vendors
{
    public class VendorSellGoldParams
    {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("vendor_reference")]
        public string Reference { get; set; }

        [JsonProperty("gram_amount")]
        public decimal? GramAmount { get; set; }


        public static bool CheckValid(VendorSellGoldParams model)
        {
            if (model == null || model.ApiKey == null || model.Reference == null || !model.GramAmount.HasValue)
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckValidAsync(VendorSellGoldParams model)
        {
            return await Task.Run(() =>
            {
              if (model == null || model.ApiKey == null || model.Reference == null || !model.GramAmount.HasValue)
              {
                  return false;
              }
              return true;
            });
            
        }
    }

    public class VendorSellGoldResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("vendor_reference")]
        public string Reference { get; set; }
    }
}
