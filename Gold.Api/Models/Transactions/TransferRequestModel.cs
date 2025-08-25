using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gold.Api.Models.Events;
using Newtonsoft.Json;

namespace Gold.Api.Models.Transactions
{
    /// <summary>
    /// 
    /// </summary>
    public class TransferRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("trasfer_request_id")]
        public string TransferRequestId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("amount")]
        public decimal GramsOfGold { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("baz_fiyat")]
        public decimal BazFiyat { get; set; }

        [JsonProperty("kar_vergi")]
        public decimal KarVergi { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("source")]
        public UserModel3 Source { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("source_event")]
        public EventModel SourceEvent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("dest_user")]
        public UserModel3 DestinationUser { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("dest_event")]
        public EventModel DestinationEvent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("date")]
        public string RequestDateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("comments")]
        public string Comments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("photo")]
        public string Photo { get; set; }

        
    }

    /// <summary>
    /// 
    /// </summary>
    public class UserBankTransferRequestModel
    {
        [JsonProperty("special_code")]
        public int SpecialCode { get; set; }
        
        [JsonProperty("bank_transfer_id")]
        public string BankTransferId { get; set; }

        [JsonProperty("kar_vergi")]
        public decimal KarVergi { get; set; }

        [JsonProperty("baz_fiyat")]
        public decimal BazFiyat { get; set; }
    }


    public class GetTransferRequestResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("transfer")]
        public TransferRequestModel Transfer { get; set; }
    }
}
