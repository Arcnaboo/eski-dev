using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class WeddingModel
    {

        [JsonProperty("wedding_date")]
        public string WeddingDate { get; set; }

        [JsonProperty("wedding_name")]
        public string WeddingName { get; set; }

        [JsonProperty("weddingid")]
        public string WeddingId { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("balance")]
        public decimal BalanceInGold { get; set; }

        [JsonProperty("money")]
        public decimal Money { get; set; }

        [JsonProperty("trans_count")]
        public int TransCount { get; set; }

        [JsonProperty("wedding_text")]
        public string WeddingText { get; set; }

        [JsonProperty("transactions")]
        public List<EventTransactionModel> TransactionModels { get; set; }

    }
}
