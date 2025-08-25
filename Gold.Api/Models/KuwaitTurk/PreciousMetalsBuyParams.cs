using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.KuwaitTurk
{
    public class PreciousMetalsBuyParams
    {
        /*
         AccountSuffixFrom: Kıymetli madeni alacağınız hesabın ek numarası,
 AccountSuffixTo: Alınan kıymetli madenin atılacağı hesabın ek numarasıdır.,
 CorporateWebUserName: Kıymetli maden alacağınız hesabın web kullanıcı adıdır.,
 BuyRate: Kıymetli Madenin alınacağı kur bilgisidir. Kur apisinden gelen alış değerdir.,
 ExchangeAmount: Kaç gr kıymetli maden satılacağının belirtildiği bölümdür.

         */

        [JsonProperty("AccountSuffixFrom")]
        public string SuffixFrom { get; set; }

        [JsonProperty("AccountSuffixTo")]
        public string SuffixTo { get; set; }

        [JsonProperty("CorporateWebUserName")]
        public string UserName { get; set; }

        [JsonProperty("BuyRate")]
        public decimal BuyRate { get; set; }

        [JsonProperty("ExchangeAmount")]
        public decimal Amount { get; set; }
    }

    public class PreciousMetalsSellParams
    {
        /*
         AccountSuffixFrom: Kıymetli madeni alacağınız hesabın ek numarası,
 AccountSuffixTo: Alınan kıymetli madenin atılacağı hesabın ek numarasıdır.,
 CorporateWebUserName: Kıymetli maden alacağınız hesabın web kullanıcı adıdır.,
 BuyRate: Kıymetli Madenin alınacağı kur bilgisidir. Kur apisinden gelen alış değerdir.,
 ExchangeAmount: Kaç gr kıymetli maden satılacağının belirtildiği bölümdür.

         */

        [JsonProperty("AccountSuffixFrom")]
        public string SuffixFrom { get; set; }

        [JsonProperty("AccountSuffixTo")]
        public string SuffixTo { get; set; }

        [JsonProperty("CorporateWebUserName")]
        public string UserName { get; set; }

        [JsonProperty("SellRate")]
        public decimal SellRate { get; set; }

        [JsonProperty("ExchangeAmount")]
        public decimal Amount { get; set; }
    }

    /*
     {
    "value": {
        "fromFec": "TL",
        "toFec": "ALT (gr)",
        "transactionAmount": 48.45,
        "currencyAmount": 0.1,
        "fxRate": 484.49414,
        "taxFecCode": "TL",
        "taxAmount": 0.10
    },
    "results": [],
    "success": true,
    "executionReferenceId": "w3M3D83BwnaFxD7PdzJv1SiwwsCDgbdgsYANubJQzg8="
     
     */

    public class MetalTaxResult : MetalResult
    {
        [JsonProperty("taxFecCode")]
        public string TaxCode { get; set; }

        [JsonProperty("taxAmount")]
        public decimal TaxAmount { get; set; }

    }



    /*{
     * selll
    "value": {
        "fromFec": "ALT (gr)",
        "toFec": "TL",
        "transactionAmount": 48.23,
        "currencyAmount": 0.1,
        "fxRate": 482.25204
    },
    "results": [],
    "success": true,
    "executionReferenceId": "ryJnzn9XO/IGhG6F1GY0dGE31HS0aVUPwaOAgV73Zqw="
}*/


    public class MetalSellResult
    {
        [JsonProperty("value")]
        public MetalResult Result { get; set; }

        [JsonProperty("results")]
        public List<object> Objects { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("executionReferenceId")]
        public string RefId { get; set; }

    }

    public class MetalBuyResult
    {
        [JsonProperty("value")]
        public MetalTaxResult Result { get; set; }

        [JsonProperty("results")]
        public List<object> Objects { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("executionReferenceId")]
        public string RefId { get; set; }


        public override string ToString()
        {
            return "MetalBuyResult: Success: " + Success.ToString() + " value: " + Result.ToString();
        }

    }

    public class MetalResult
    {
        [JsonProperty("fromFec")]
        public string Neyden { get; set; }

        [JsonProperty("toFec")]
        public string Neye { get; set; }

        [JsonProperty("transactionAmount")]
        public decimal TransAmount { get; set; }

        [JsonProperty("currencyAmount")]
        public decimal GramAmount { get; set; }

        [JsonProperty("fxRate")]
        public decimal Kur { get; set; }


        public override string ToString()
        {
            return "MetalResult: " + "\n" + "from: " + Neyden + " - Neye: " + Neye + " - Amount: " + TransAmount.ToString() + " - curAmount: " + GramAmount.ToString() + " - fxRate: " + Kur.ToString();
        }

    }
}
