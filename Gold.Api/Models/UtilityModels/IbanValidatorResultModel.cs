using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.UtilityModels
{
    public class IbanValidatorResultModel
    {
        [JsonProperty("result")]
        public int Result { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        public List<Validation> Validations { get; set; }

        public int Expremental { get; set; }

        public IbanData Data { get; set; }
    }

    public class Validation
    {
        [JsonProperty("result")]
        public int Result { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class IbanData
    {
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
        [JsonProperty("iso_alpha3")]
        public string IsoAlpha3 { get; set; }
        [JsonProperty("country_name")]
        public string CountryName { get; set; }
        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }
        [JsonProperty("sepa_member")]
        public string SepaMember { get; set; }
        [JsonProperty("sepa")]
        public object Sepa { get; set; }
        [JsonProperty("bban")]
        public string BBan { get; set; }
       
    }
}
