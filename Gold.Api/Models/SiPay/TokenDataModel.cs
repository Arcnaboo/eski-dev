using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.SiPay
{
    public class TokenDataModel
    {
        [JsonProperty("is_3d")]
        public int Is3D { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
