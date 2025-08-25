using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class TransferFromUserToWeddingParamModel
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("weddingid")]
        public string WeddingId { get; set; }

        [JsonProperty("type")]
        public string GoldType { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("transfer_message")]
        public string TransferMessage { get; set; }

        [JsonProperty("payment_type")]
        public string PaymentType { get; set; }

        [JsonProperty("cc_data")]
        public BuyGoldCCParamModel CCData { get; set; }

        [JsonProperty("eft_data")]
        public BuyGoldEftParamModel EFTData { get; set; }



    }
}
