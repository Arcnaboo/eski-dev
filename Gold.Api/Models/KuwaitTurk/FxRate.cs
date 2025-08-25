using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.KuwaitTurk
{
    /// <summary>
    /// "name":"Amerikan Doları",
   /*     "fxCode":"USD",
        "fxId":1,
        "buyRate":5.92262,
        "sellRate":5.9572,
        "parityBuyRate":1.0,
        "paritySellRate":1.0*/
    /// </summary>
    public class FxRate
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("fxCode")]
        public string FxCode { get; set; }

        [JsonProperty("fxId")]
        public decimal FxId { get; set; }

        [JsonProperty("buyRate")]
        public decimal BuyRate { get; set; }

        [JsonProperty("sellRate")]
        public decimal SellRate { get; set; }

        [JsonProperty("parityBuyRate")]
        public decimal ParityBuyRate { get; set; }

        [JsonProperty("paritySellRate")]
        public decimal ParitySellRate { get; set; }
    }

    public class FxRateKiymetliMaden
    {
        [JsonProperty("fxName")]
        public string Name { get; set; }

        [JsonProperty("fxCode")]
        public string FxCode { get; set; }

        [JsonProperty("fxId")]
        public decimal FxId { get; set; }

        [JsonProperty("buyRate")]
        public decimal BuyRate { get; set; }

        [JsonProperty("sellRate")]
        public decimal SellRate { get; set; }
        [JsonProperty("isSpreadApplied")]
        public string IsSpreadApplied { get; set; }


        public override string ToString()
        {
            return Name + "-" + FxCode + "-" + FxId.ToString() + "-" + BuyRate.ToString() + "-" + SellRate.ToString();
        }
    }
}
