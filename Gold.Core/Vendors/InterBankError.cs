using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Depreciated interbank error
    /// </summary>
    public class InterBankError
    {
        /// <summary>
        /// Db id
        /// </summary>
        public Guid IBanErrId { get; set; }
        /// <summary>
        /// Amount of grams
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// REceiver
        /// </summary>
        public string ReceiverAccount { get; set; }
        /// <summary>
        /// Receiver suffix
        /// </summary>
        public string ReceiverSuffix { get; set; }
        /// <summary>
        /// Tramsfer type, should be 3
        /// </summary>
        public int TransferType { get; set; }
        /// <summary>
        /// KT ref id
        /// </summary>
        public string RefId { get; set; }
        /// <summary>
        /// DateTime of error
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// Comments for admin panel
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// Related transactionm
        /// </summary>
        public Guid TransactionId { get; set; }
        /// <summary>
        /// id of the vendor
        /// </summary>
        public Guid VendorId { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private InterBankError()
        {

        }
        /// <summary>
        /// Creates new inter bank transfer error record
        /// </summary>
        /// <param name="transId">Transaction id</param>
        /// <param name="vendorId">vendor id</param>
        /// <param name="amount">amount of transaction</param>
        /// <param name="recAcc">receiver account number</param>
        /// <param name="suff">receiver suffix</param>
        /// <param name="type">type of transaction</param>
        /// <param name="refId">reference id of KT</param>
        /// <param name="comments">comments for admin panel</param>
        public InterBankError(Guid transId, Guid vendorId, 
            decimal amount, string recAcc, string suff,
            int type, string refId, string comments)
        {
            TransactionId = transId;
            VendorId = vendorId;
            Amount = amount;
            ReceiverAccount = recAcc;
            ReceiverSuffix = suff;
            TransferType = type;
            RefId = refId;
            Comments = comments;
            DateTime = DateTime.Now;
        }
    }
}
