using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class SpecialEventModel
    {

        [JsonProperty("event_date")]
        public string EventDate { get; set; }

        [JsonProperty("event_name")]
        public string EventName { get; set; }

        [JsonProperty("eventid")]
        public string EventId { get; set; }

        [JsonProperty("balance")]
        public decimal BalanceInGold { get; set; }

        [JsonProperty("money")]
        public decimal Money { get; set; }

        [JsonProperty("trans_count")]
        public int TransCount { get; set; }

        [JsonProperty("event_text")]
        public string EventText { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("transactions")]
        public List<EventTransactionModel> TransactionModels { get; set; }

    }
}
