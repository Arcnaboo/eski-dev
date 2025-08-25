using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    public class TransactionModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("key")]
        public int Key { get; set; }

        [JsonProperty("idhangisi")]
        public string IdKim { get; set; }

        [JsonProperty("date")]
        public string DateTime { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("source")]
        public UserModel3 Source { get; set; }

        [JsonProperty("dest")]
        public UserModel3 DestinationUser { get; set; }

        [JsonProperty("destination")]
        public string Destination { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("gelis_gidis")]
        public string GelisGidis { get; set; }

        [JsonProperty("photoid")]
        public int PhotoId { get; set; }



    }
}
