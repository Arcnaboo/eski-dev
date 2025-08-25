using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Transactions
{
    public class GetBankRequestResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("name")]
        public string BankName { get; set; }
        [JsonProperty("iban")]
        public string IBAN { get; set; }
        [JsonProperty("code")]
        public int SpecialCode { get; set; }
        [JsonProperty("price")]
        public decimal Price { get; set; }
        [JsonProperty("amount")]
        public decimal Grams { get; set; }
    }
}
