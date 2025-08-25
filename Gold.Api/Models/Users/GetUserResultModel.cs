using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Users
{
    public class GetUserResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("user")]
        public UserModel User { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }


    }
}
