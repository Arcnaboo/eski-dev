using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Transactions
{
    public class PriceCheckResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("price_satis")]
        public decimal SalePrice { get; set; }

        [JsonProperty("silver_buy")]
        public decimal SilverBuy { get; set; }

        [JsonProperty("silver_sell")]
        public decimal SilverSell { get; set; }


        [JsonProperty("alis")]
        public decimal Alis { get; set; }

        [JsonProperty("satis")]
        public decimal Satis { get; set; }

        [JsonProperty("kar")]
        public decimal Kar { get; set; }

        [JsonProperty("vergi")]
        public decimal Vergi { get; set; }

        [JsonProperty("komisyon")]
        public decimal Commission { get; set; }

        [JsonProperty("satis_komisyon")]
        public decimal SaleCommission { get; set; }

        [JsonProperty("vpos_price")]
        public decimal VposPrice { get; set; }

        [JsonProperty("eft_price")]
        public decimal EftPrice { get; set; }

        [JsonProperty("kt_buy")]
        public decimal KtBuy { get; set; }

        [JsonProperty("kt_sell")]
        public decimal KtSell { get; set; }

        [JsonProperty("buy_percentage")]
        public decimal BuyPercentage { get; set; }

        [JsonProperty("sell_percentage")]
        public decimal SellPercentage { get; set; }

        [JsonProperty("buy_percentage_silver")]
        public decimal BuyPercentageSilver { get; set; }

        [JsonProperty("sell_percentage_silver")]
        public decimal SellPercentageSilver { get; set; }

        [JsonProperty("automatic")]
        public decimal Automatic { get; set; }

        [JsonProperty("expected_run")]
        public decimal ExpectedRun { get; set; }
    }
}
