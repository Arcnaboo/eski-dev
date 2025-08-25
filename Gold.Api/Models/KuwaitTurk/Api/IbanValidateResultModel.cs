using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.KuwaitTurk.Api
{
    public class IbanValidateResultModel
    {
        /// {"value":{"IsValid":true,"ValidationMessage":""},"success":true,"results":[]}
        [JsonProperty("value")]
        public IbanValidValue Value { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("results")]
        public object [] Results { get; set; }
    }
}
