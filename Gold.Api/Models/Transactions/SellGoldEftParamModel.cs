using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class SellGoldEftParamModel
    {
        // Ad soyad iban miktar cins

        [JsonProperty("userid")]
        public string Userid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lastname")]
        public string FamilyName { get; set; }

        [JsonProperty("IBAN")]
        public string IBAN { get; set; }

        [JsonProperty("type")]
        public string GoldType { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }


    public class SellGoldEftResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("transfer")]
        public TransferRequestModel Transfer { get; set; }
    }

    public class SellGoldEftResponseParamModel
    {
        [JsonProperty("userid")]
        public string Userid { get; set; }

        [JsonProperty("transfer_request_id")]
        public string TransferRequestId { get; set; }

        [JsonProperty("confirmed")]
        public bool Confirmed { get; set; }

    }

}
