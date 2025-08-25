using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class TransferRequestResponseParamModel
    {
        [JsonProperty("confirmed")]
        public bool Confirmed { get; set; }

        [JsonProperty("transfer_request_id")]
        public string TransferRequestId { get; set; }
    }
}
