using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.KuwaitTurk.Api
{
    public class IbanValidateParametersModel
    {
        [JsonProperty("iban")]
        public string IBAN { get; set; }
        
    }
}
