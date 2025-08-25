using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Users
{
    public class LoginUserResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("user")]
        public UserModel User { get; set; }

        [JsonProperty("authToken")]
        public string AuthToken { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("sms_verify")]
        public bool Sms { get; set; }
        [JsonProperty("sms_userid")]
        public string UserId { get; set; }

    }

    public class RequestCodeModel
    {

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
