using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.SiPay
{
    public class GetPosDataModel
    {

        [JsonProperty("pos_id")]
        public int PosId { get; set; }

        [JsonProperty("campaign_id")]
        public int CampaignId { get; set; }

        [JsonProperty("allocation_id")]
        public int AllocationId { get; set; }

        [JsonProperty("installments_number")]
        public int InstallmentsNumber { get; set; }

        [JsonProperty("cot_percentage")]
        public int CotPercentage { get; set; }

        [JsonProperty("cot_fixed")]
        public int CotFixed { get; set; }

        [JsonProperty("amount_to_be_paid")]
        public decimal AmountToBePaid { get; set; }

        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }

        [JsonProperty("currency_id")]
        public int CurrencyId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
