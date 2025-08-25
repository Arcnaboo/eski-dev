using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gold.Core.Transactions;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class TransactionResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("transaction")]
        public TransactionModel Transaction { get; set; }
    }
}
