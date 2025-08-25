using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Gold.Api.Models.Admin
{
    public class UserBankTransferRequestModel2
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        [JsonProperty("grams")]
        public decimal Grams { get; set; }

        [JsonProperty("final_price")]
        public decimal FinalPrice { get; set; }

        [JsonProperty("kur")]
        public decimal Kur { get; set; }

        [JsonProperty("special_code")]
        public int Code { get; set; }

        [JsonProperty("code_start")]
        public string CodeStart { get; set; }

        [JsonProperty("code_end")]
        public string CodeEnd { get; set; }

        [JsonProperty("onay_link")]
        public string OnayLink { get; set; }

        [JsonProperty("delete_link")]
        public string DeleteLink { get; set; }
    }
}
