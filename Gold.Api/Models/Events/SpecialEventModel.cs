using Gold.Core.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Events
{
    public class SpecialEventModel
    {
        [JsonProperty("createdby")]
        public string CreatedBy { get; set; }

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
        public int EventCode { get; set; }

        [JsonProperty("transactions")]
        public List<EventTransactionModel> TransactionModels { get; set; }


        public SpecialEventModel() { }

        public SpecialEventModel(Event evt, List<EventTransactionModel> eventTransactions, decimal money)
        {
            EventDate = evt.EventDate.ToShortDateString();
            EventName = evt.EventName;
            EventId = evt.EventId.ToString();
            BalanceInGold = evt.BalanceInGold;
            Money = money;
            TransCount = eventTransactions.Count;
            EventText = evt.EventText;
            EventCode = evt.EventCode;
            TransactionModels = eventTransactions;
        }
    }
}
