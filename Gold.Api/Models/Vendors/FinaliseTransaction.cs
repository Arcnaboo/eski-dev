using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Vendors
{
    public class FinaliseTransactionParams
    {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty("vendor_reference")]
        public string Reference { get; set; }

        [JsonProperty("confirm")]
        public bool? Confirmed { get; set; }


        public async static Task<bool> CheckValidAsync(FinaliseTransactionParams model)
        {
            return await Task.Run(() =>
            {
                if (model == null || model.ApiKey == null || model.Reference == null || !model.Confirmed.HasValue)
                {
                    return false;
                }
                return true;
            });
    
        }

        public override string ToString()
        {
            return string.Format("FinaliseTransactionParams: AK={0} | tid={1} : ref={2} | confirm={3}", ApiKey, TransactionId, Reference, Confirmed);
        }

    }



    public class FinaliseTransactionResult
    {

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("result_code")]
        public string ResultCode { get; set; }

        [JsonProperty("havale_description")]
        public string HavaleCode { get; set; }
    }

    public class FinaliseTransactionResult2
    {

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("result_code")]
        public string ResultCode { get; set; }

        /*[JsonProperty("havale_description")]
        public string HavaleCode { get; set; }*/
    }
}
