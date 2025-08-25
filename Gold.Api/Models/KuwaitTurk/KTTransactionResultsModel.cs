using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.KuwaitTurk
{
    public class KTTransactionResultsModel
    {
        //{"value":{"accountActivities":[]},"results":[],"success":true,"executionReferenceId":"x4bW1oORKDAo/PdmsYWiog8s14B3FbzIshYpy2ojJ+Q="}

        [JsonProperty("value")]
        public KTTransactionResultValue Value { get; set; }

        [JsonProperty("results")]
        public List<KTTransaction> Results { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("executionReferenceId")]
        public string Reference { get; set; }
    }

    public class KTTransactionResultValue
    {
        [JsonProperty("accountActivities")]
        public List<KTTransaction> AccountActivities { get; set; }
    }

    public class KTTransaction
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("fxCode")]
        public string FxCode { get; set; }

        [JsonProperty("senderIdentityNumber")]
        public string SenderId { get; set; }

        [JsonProperty("suffix")]
        public string Suffix { get; set; }

        [JsonProperty("transactionCode")]
        public string TransCode { get; set; }

        [JsonProperty("transactionReference")]
        public string TransRef { get; set; }
    }
}
