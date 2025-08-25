using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class RequestGoldParamModel
    {

        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("memberid")]
        public string MemberId { get; set; }

        [JsonProperty("name_sirname")]
        public string MemberName { get; set; }

        [JsonProperty("request_name")]
        public string RequestName { get; set; }

        [JsonProperty("type")]
        public string GoldType { get; set; }

        [JsonProperty("amount")]
        public decimal GoldAmount { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }

    public class RequestGoldResultModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
