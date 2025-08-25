using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Transactions
{
    public class BuyGoldEftParamModel
    {
        /// <summary>
        ///  Bank Id si
        /// </summary>
        [JsonProperty("bankid")]
        public string BankId { get; set; }

        /// <summary>
        ///  UserId si
        /// </summary>
        [JsonProperty("userid")]
        public string UserId { get; set; }


        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        public override string ToString()
        {
            return string.Format("BuyGoldEftParamModel:{0}:{1}:{2}:{3}", BankId, UserId, Amount, Type);
        }
    }

    public class BuyGoldEftResponseParamModel
    {
        [JsonProperty("confirmed")]
        public bool Confirmed { get; set; }

        [JsonProperty("bank_transfer_id")]
        public string BankTransferId { get; set; }
    }

    public class BuyGoldEftResponseResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
