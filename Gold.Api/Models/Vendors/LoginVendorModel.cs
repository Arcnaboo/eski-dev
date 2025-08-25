using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Vendors
{
    public class LoginVendorModel
    {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }

    }

    public class LoginVendorResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("token")]
        public string AuthToken { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

}
