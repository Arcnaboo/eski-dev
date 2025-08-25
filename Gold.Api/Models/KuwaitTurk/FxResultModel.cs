using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.KuwaitTurk
{
    public class FxResultModel
    {
        [JsonProperty("value")]
        public Value value { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("results")]
        public List<object> Results { get; set; }

        [JsonProperty("executionReferenceId")]
        public string ExecRefId { get; set; }
    
    
    }

    public class Value
    {
        [JsonProperty("rateList")]
        public List<FxRateKiymetliMaden> FxRates { get; set; }
    }
}
