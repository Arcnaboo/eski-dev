using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Users
{
    public class UploadPhotoResultModel
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("src")]
        public string PhotoSource { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
