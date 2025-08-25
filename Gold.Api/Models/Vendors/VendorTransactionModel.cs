using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Vendors
{
    public class VendorTransactionModel
    {
        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty("vendor_reference")]
        public string Reference { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("destination")]
        public string Destination { get; set; }

        [JsonProperty("vendor_confirmed")]
        public bool ConfirmedByVendor { get; set; }

        [JsonProperty("goldtag_confirmed")]
        public bool ConfirmedByGoldtag { get; set; }

        [JsonProperty("cancelled")]
        public bool Cancelled { get; set; }

        [JsonProperty("finalised")]
        public bool Finalised { get; set; }

        [JsonProperty("gram_amount")]
        public decimal GramAmount { get; set; }

        [JsonProperty("price")]
        public decimal TlAmount { get; set; }

        [JsonProperty("transaction_date_time")]
        public string TransactionDateTime { get; set; }

        [JsonProperty("vendor_confirmed_date_time")]
        public string VendorConfirmedDateTime { get; set; }

        [JsonProperty("goldtag_confirmed_date_time")]
        public string GoldtagConfirmedDateTime { get; set; }

        [JsonProperty("transaction_finalised_date_time")]
        public string TransactionFinalisedDateTime { get; set; }

    }


    public class VendorTransactionAdminModel : VendorTransactionModel
    {
        [JsonProperty("confirmed_by_admin")]
        public bool ConfirmedByAdmin { get; set; }
    }

    public class VendorRequestTransactionsCountParams
    {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("date_from")]
        public string DateFrom { get; set; }

        [JsonProperty("date_to")]
        public string DateTo { get; set; }

        [JsonProperty("only_finalised")]
        public bool? OnlyFinalised { get; set; }

        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        public static bool CheckValid(VendorRequestTransactionsCountParams model)
        {
            if (model == null || model.ApiKey == null || model.DateTo == null || model.DateFrom == null || !model.OnlyFinalised.HasValue)
            {
                return false;
            }
            return true;
        }
    }

    public class VendorRequestTransactionsCountResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class VendorRequestTransactionsParams
    {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("date_from")]
        public string DateFrom { get; set; }

        [JsonProperty("date_to")]
        public string DateTo { get; set; }

        [JsonProperty("limit")]
        public int? Limit { get; set; }

        [JsonProperty("page")]
        public int? Page { get; set; }

        [JsonProperty("only_finalised")]
        public bool? OnlyFinalised { get; set; }

        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        public static bool CheckValid(VendorRequestTransactionsParams model)
        {
            if (model == null || model.ApiKey == null || model.DateFrom == null || model.DateTo == null || !model.Limit.HasValue || !model.Page.HasValue || !model.OnlyFinalised.HasValue)
            {
                return false;
            }
            if (model.Limit.Value < 5 || model.Page.Value < 1)
            {
                return false;
            }
            return true;
        }
    }

    public class VendorRequestTransactionsResult
    {

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("transactions")]
        public List<VendorTransactionModel> TransactionModels { get; set; }

    }
}
