using Gold.Core.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Users
{
    public class CreateUserResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
