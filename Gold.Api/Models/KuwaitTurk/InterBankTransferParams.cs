using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.KuwaitTurk
{
    public class InterBankTransferParams
    {
        [JsonProperty("senderAccountSuffix")]
        public string SenderSuffix { get; set; }

        [JsonProperty("receiverAccountNumber")]
        public string ReceiverAccount { get; set; }

        [JsonProperty("receiverAccountSuffix")]
        public string ReceiverSuffix { get; set; }

        [JsonProperty("transferType")]
        public int TransferType { get; set; } // 1 --> Aidat, 2 --> Diğer Kiralar, 3 --> Diğer Ödeme Türleri, 4 --> Eğitim, 5 --> İşyeri Kirası, 6 --> Konut Kirası, 7 --> Kredi Kartı Borcu, 8 --> Personel Ödemeleri, 9 --> E-Ticaret Ödemeleri,

        [JsonProperty("moneyTransferAmount")]
        public decimal Amount { get; set; }

        [JsonProperty("moneyTransferDescription")]
        public string Description { get; set; }
        [JsonProperty("corporateWebUserName")]
        public string UserName { get; set; }

    }


    public class InterBanTransferResult
    {
        /*
         "value": {
        "moneyTransferTransactionId": 97327178,
        "executionReferenceId": "DLaRpuYx5W4gzJaScJ9EwmgQDREzo6E/QDuR61lh9XM="
    },
    "executionReferenceId": "DLaRpuYx5W4gzJaScJ9EwmgQDREzo6E/QDuR61lh9XM=",
    "results": [],
    "success": true
         
         */

        [JsonProperty("value")]
        public IBankTransValue BankTransferValue { get; set; }

        [JsonProperty("executionReferenceId")]
        public string RefId { get; set; }

        [JsonProperty("results")]
        public List<object> EmptyResults { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public class IBankTransValue
    {
        [JsonProperty("moneyTransferTransactionId")]
        public int TransactionId { get; set; }

        [JsonProperty("executionReferenceId")]
        public string RefId { get; set; }
    }
}
