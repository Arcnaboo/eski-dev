using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gold.Core.Events;
using Newtonsoft.Json;

namespace Gold.Api.Models.Events
{
    public class WeddingModel
    {

        [JsonProperty("createdby")]
        public string CreatedBy { get; set; }

        [JsonProperty("wedding_date")]
        public string WeddingDate { get; set; }

        [JsonProperty("wedding_name")]
        public string WeddingName { get; set; }

        [JsonProperty("weddingid")]
        public string WeddingId { get; set; }

        [JsonProperty("balance")]
        public decimal BalanceInGold { get; set; }

        [JsonProperty("money")]
        public decimal Money { get; set; }

        [JsonProperty("trans_count")]
        public int TransCount { get; set; }

        [JsonProperty("wedding_text")]
        public string WeddingText { get; set; }

        [JsonProperty("code")]
        public int WeddingCode { get; set; }

        [JsonProperty("transactions")]
        public List<EventTransactionModel> TransactionModels { get; set; }

        public WeddingModel() { }

        public WeddingModel(Wedding evt, List<EventTransactionModel> eventTransactions, decimal money)
        {
            WeddingDate = evt.WeddingDate.ToShortDateString();
            WeddingName = evt.WeddingName;
            WeddingId = evt.WeddingId.ToString();
            BalanceInGold = evt.BalanceInGold;
            Money = money;
            TransCount = eventTransactions.Count;
            WeddingText = evt.WeddingText;
            WeddingCode = evt.WeddingCode;
            TransactionModels = eventTransactions;
        }

    }
}
