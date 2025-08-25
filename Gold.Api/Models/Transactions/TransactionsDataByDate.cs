using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class TransactionsDataByDate
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("transactions")]
        public List<TransactionModel> Transactions { get; set; }

    }
}
