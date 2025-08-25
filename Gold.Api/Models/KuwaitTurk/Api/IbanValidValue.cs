using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.KuwaitTurk.Api
{
    public class IbanValidValue
    {
        [JsonProperty("IsValid")]
        public bool IsValid { get; set; }

        [JsonProperty("ValidationMessage")]
        public string ValidationMessage { get; set; }
    }
}
