using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.KuwaitTurk
{
    public class TransactionReceiptParams
    {

        [JsonProperty("transactionReference")]
        public string TransactionReference { get; set; }
    }
}
