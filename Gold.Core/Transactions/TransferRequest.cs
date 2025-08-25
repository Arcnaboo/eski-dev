using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Transactions
{
    /// <summary>
    /// Represents a Transfer Request from User to User or Event or Fintag
    /// </summary>
    public class TransferRequest
    {
        /// <summary>
        /// DB Id
        /// </summary>
        public Guid TransferRequestId { get; set; }
        /// <summary>
        /// Total grams to be transferred
        /// </summary>
        public decimal GramsOfGold { get; set; }
        /// <summary>
        /// User who wants to transfer
        /// </summary>
        public Guid SourceUserId { get; set; }
        /// <summary>
        /// Destination type
        /// </summary>
        public string DestinationType { get; set; }
        /// <summary>
        /// Destination id
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// DateTime of the request
        /// </summary>
        public DateTime RequestDateTime { get; set; }
        /// <summary>
        /// True iff user confirmed
        /// </summary>
        public bool RequestConfirmed { get; set; }
        /// <summary>
        /// DateTIme of the confirmation
        /// </summary>
        public DateTime? ConfirmationDateTime { get; set; }
        /// <summary>
        /// Transfer comments
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// True iff transfer completed
        /// </summary>
        public bool RequestCompleted { get; set; }
        /// <summary>
        /// Transaction related to this transfer request
        /// </summary>
        public Guid TransactionRecord { get; set; }
        /// <summary>
        /// Source user
        /// </summary>
        public virtual User3 SourceUser { get; set; }
        /// <summary>
        /// Transaction record
        /// </summary>
        public virtual Transaction Transaction { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private TransferRequest() { }
        /// <summary>
        /// Creates new transfer request
        /// </summary>
        /// <param name="sourceUserId">Source user id</param>
        /// <param name="gramsOfGold">Total grams to be transferred</param>
        /// <param name="destType">Destination type</param>
        /// <param name="destination">Destination ID</param>
        /// <param name="transactionId">Transaction ID</param>
        /// <param name="comments">Comments for transaction</param>
        public TransferRequest(
            Guid sourceUserId,
            decimal gramsOfGold,
            string destType,
            string destination,
            Guid transactionId,
            string comments = null
            )
        {
            TransactionRecord = transactionId;
            SourceUserId = sourceUserId;
            GramsOfGold = gramsOfGold;
            Destination = destination;
            DestinationType = destType;
            Comments = comments;
            RequestCompleted = RequestConfirmed = false;
            ConfirmationDateTime = null;
            RequestDateTime = DateTime.Now;
        }
        /// <summary>
        /// Completes transfer
        /// </summary>
        public void CompleteTransfer()
        {
            if (RequestCompleted || RequestConfirmed)
                return;


            RequestConfirmed = true;
            ConfirmationDateTime = DateTime.Now;
            RequestCompleted = true;
        }
        /// <summary>
        /// Updates comments
        /// </summary>
        /// <param name="comment">Comment to be added</param>
        public void UpdateComment(string comment)
        {
            Comments = comment;
        }
    }
}
