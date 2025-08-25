using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Users
{
    public class LoginUserModel
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("app_version")]
        public string AppVersion { get; set; }
    }

    public class LogoutUserModel
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("loginid")]
        public string LoginId { get; set; }
    }
}
