using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.SiPay
{
    public class GetPosResponseModel
    {
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }

        [JsonProperty("status_description")]
        public string StatusDesc { get; set; }

        [JsonProperty("data")]
        public List<GetPosDataModel> Data { get; set; }


    }
}
