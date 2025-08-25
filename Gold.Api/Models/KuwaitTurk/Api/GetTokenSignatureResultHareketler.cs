using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.KuwaitTurk.Api
{
    public class GetTokenSignatureResultHareketler
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}
