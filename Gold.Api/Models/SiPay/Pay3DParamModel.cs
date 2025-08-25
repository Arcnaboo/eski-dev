using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gold.Api.Models.Transactions;
using Gold.Core.Transactions;
using Newtonsoft.Json;

namespace Gold.Api.Models.SiPay
{
    public class Pay3DParamModel
    {
        [JsonProperty("cc_holder_name")]
        public string HolderName { get; set; }

        [JsonProperty("cc_digits")]
        public string CreditCard { get; set; }

        [JsonProperty("expiry_month")]
        public string ExpiryMonth { get; set; }

        [JsonProperty("expiry_year")]
        public string ExpiryYear { get; set; }

        [JsonProperty("cvv")]
        public string CVV { get; set; }

        [JsonProperty("user")]
        public User3 User { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        public Transaction Transaction { get; set; }


        public Pay3DParamModel(BuyGoldCCParamModel model, User3 user, Transaction transaction)
        {
            Transaction = transaction;
            User = user;
            HolderName = model.HolderName;
            CreditCard = model.CreditCard;
            CVV = model.CardCode.ToString();
            Amount = model.Amount.ToString();
            ExpiryMonth = model.ExpiryMonth;
            ExpiryYear = model.ExpiryYear;


        }
    }
}
