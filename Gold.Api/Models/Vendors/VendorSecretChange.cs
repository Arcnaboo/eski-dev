using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Vendors
{
    public class VendorSecretChange
    {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("old_secret")]
        public string OldSecret { get; set; }

        [JsonProperty("new_secret")]
        public string NewSecret { get; set; }

        [JsonProperty("gcode")]
        public string Gcode { get; set; }

        public static bool CheckValid(VendorSecretChange model)
        {

            if (model == null || model.ApiKey == null || model.OldSecret == null || model.NewSecret == null || model.Gcode == null)
            {
                return false;
            }
            return true;
        }

    }

    public class ChangeSecretResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class RequestGcodeResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
