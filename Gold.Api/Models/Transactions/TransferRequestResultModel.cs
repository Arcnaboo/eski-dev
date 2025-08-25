using Gold.Core.Transactions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Transactions
{
    public class TransferRequestResultModel
    {

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("transfer")]
        public TransferRequestModel Transfer { get; set; }
    }

    public class TransferFromEventToUserRequestResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("transaction")]
        public Transaction transaction { get; set; }
    }



    public class UserBankTransferRequestResultModel : TransferRequestResultModel
    {



        [JsonProperty("bank_transfer")]
        public UserBankTransferRequestModel UserBankTransferRequest { get; set; }
    }
}
