using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.KuwaitTurk
{
    public class AccountStatusResult
    {
        [JsonProperty("value")]
        public AccountValue Value { get; set; }

        [JsonProperty("results")]
        public List<object> Results { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
        
        [JsonProperty("executionReferenceId")]
        public string Reference { get; set; }
    }

    public class AccountValue
    {
        [JsonProperty("accountList")]
        public List<Account> AccountList { get; set; }
    }

    public class Account
    {
        /*
         "name": "",
                "suffix": 1,
                "balance": 2006.18,
                "availableBalance": 2006.18,
                "fxId": 0,
                "iban": "TR980020500009619334500001",
                "productType": "Cari Hesap",
                "openDate": "2020-01-21T00:00:00",
                "branchName": "Balgat Şubesi",
                "branchId": 64,
                "withHoldingAmount": 0.00,
                "customerName": "FİNTAG YAZILIM DANIŞMANLIK ANONİM ŞİRKETİ",
                "isActive": 1
        */
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("suffix")]
        public int Suffix { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        [JsonProperty("availableBalance")]
        public decimal ABalance { get; set; }

        [JsonProperty("fxId")]
        public int FxId { get; set; }

        [JsonProperty("iban")]
        public string IBAN { get; set; }

        [JsonProperty("productType")]
        public string Type { get; set; }

        [JsonProperty("openDate")]
        public string OpenDate { get; set; }

        [JsonProperty("branchName")]
        public string Branch { get; set; }

        [JsonProperty("branchId")]
        public int BranchId { get; set; }

        [JsonProperty("withHoldingAmount")]
        public decimal Withhold { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }
    }
}
