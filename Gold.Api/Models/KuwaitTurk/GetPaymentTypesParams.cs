using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.KuwaitTurk
{
    public class GetPaymentTypesParams
    {

        [JsonProperty("senderAccountSuffix")]
        public int SenderSuffix { get; set; }
        
        [JsonProperty("receiverIBAN")]
        public string ReceiverIBAN { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}
