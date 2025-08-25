using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.UtilityModels
{
    public class DashBoardInfoResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("dashboard")]
        public DashBoardInfo DashBoard { get; set; }
    }

    public class DashBoardInfoResultModelSilver
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("dashboard")]
        public DashBoardInfoSilver DashBoard { get; set; }
    }


    public class DashBoardInfo
    {
        [JsonProperty("gold_sold")]
        public decimal TotalGoldSold { get; set; }

        [JsonProperty("gold_sold_today")]
        public decimal TotalGoldSoldToday { get; set; }

        [JsonProperty("gold_sold_cc")]
        public decimal TotalGoldSoldCC { get; set; }

        [JsonProperty("gold_sold_today_cc")]
        public decimal TotalGoldSoldTodayCC { get; set; }

        [JsonProperty("gold_sold_eft")]
        public decimal TotalGoldSoldEFT { get; set; }

        [JsonProperty("gold_sold_today_eft")]
        public decimal TotalGoldSoldTodayEFT { get; set; }

        [JsonProperty("gold_transfer")]
        public decimal TotalGoldTransfer{ get; set; }

        [JsonProperty("gold_transfer_today")]
        public decimal TotalGoldTransferToday { get; set; }

        [JsonProperty("gold_bozdurma")]
        public decimal TotalGoldBozdurma { get; set; }

        [JsonProperty("gold_bozdurma_today")]
        public decimal TotalGoldBozdurmaToday { get; set; }

        [JsonProperty("member_count")]
        public int MemberCount { get; set; }

        [JsonProperty("new_members")]
        public int NewMembers { get; set; }

        [JsonProperty("wedding_count")]
        public int WeddingCount { get; set; }

        [JsonProperty("wedding_count_today")]
        public int WeddingCountToday { get; set; }

        [JsonProperty("event_count")]
        public int EventCount { get; set; }

        [JsonProperty("event_count_today")]
        public int EventCountToday { get; set; }
    }

    public class DashBoardInfoSilver
    {
        [JsonProperty("silver_sold")]
        public decimal TotalsilverSold { get; set; }

        [JsonProperty("silver_sold_today")]
        public decimal TotalsilverSoldToday { get; set; }

        [JsonProperty("silver_sold_cc")]
        public decimal TotalsilverSoldCC { get; set; }

        [JsonProperty("silver_sold_today_cc")]
        public decimal TotalsilverSoldTodayCC { get; set; }

        [JsonProperty("silver_sold_eft")]
        public decimal TotalsilverSoldEFT { get; set; }

        [JsonProperty("silver_sold_today_eft")]
        public decimal TotalsilverSoldTodayEFT { get; set; }

        [JsonProperty("silver_transfer")]
        public decimal TotalsilverTransfer { get; set; }

        [JsonProperty("silver_transfer_today")]
        public decimal TotalsilverTransferToday { get; set; }

        [JsonProperty("silver_bozdurma")]
        public decimal TotalsilverBozdurma { get; set; }

        [JsonProperty("silver_bozdurma_today")]
        public decimal TotalsilverBozdurmaToday { get; set; }

    }
}
