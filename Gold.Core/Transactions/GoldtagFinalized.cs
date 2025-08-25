using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Transactions
{
    /// <summary>
    /// Represetns a finalized transaction for goldtag app
    /// </summary>
    public class GoldtagFinalized
    {
        /// <summary>
        /// DB ID
        /// </summary>
        public Guid FinalizedId { get; set; }
        /// <summary>
        /// User ID
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Transaction ID
        /// </summary>
        public Guid TransactionId { get; set; }
        /// <summary>
        /// Finalized date time
        /// </summary>
        public DateTime FinalizedDateTime { get; set; }
        /// <summary>
        /// Gram amount
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// TRY amount
        /// </summary>
        public decimal TLAmount { get; set; }
        /// <summary>
        /// Mineral type
        /// </summary>
        public string Maden { get; set; }
        /// <summary>
        /// Comments for the transaction
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private GoldtagFinalized() { }
        /// <summary>
        /// Creates nwe goldtagfinalized
        /// </summary>
        /// <param name="userId">id of the user</param>
        /// <param name="transId">id of the transaction</param>
        /// <param name="amount">grams amount</param>
        /// <param name="tlAmount">try amount</param>
        /// <param name="maden">mineral type</param>
        /// <param name="comments">arbitrary comments</param>
        public GoldtagFinalized(Guid userId,
            Guid transId,
            decimal amount,
            decimal tlAmount,
            string maden,
            string comments=null)
        {
            UserId = userId;
            TransactionId = transId;
            FinalizedDateTime = DateTime.Now;
            Amount = amount;
            TLAmount = tlAmount;
            Maden = maden;
            Comments = comments;
        }
    }
}
