using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Transactions
{
    /// <summary>
    /// Represents a transaction where outcome was unexpected
    /// </summary>
    public class GoldtagUnexpected
    {   /// <summary>
        /// DB ID
        /// </summary>
        public Guid UnexpectedId { get; set; }
        /// <summary>
        /// User's id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// bank transfer id
        /// </summary>
        public Guid BankTransferId { get; set; }
        /// <summary>
        ///  expected try amount
        /// </summary>
        public decimal ExpectedTL { get; set; }
        /// <summary>
        ///  received try amount
        /// </summary>
        public decimal ReceivedTL { get; set; }
        /// <summary>
        /// Time of the creation of this instance
        /// </summary>
        public DateTime UnexpectedDateTime { get; set; }
        /// <summary>
        /// extra notes
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private GoldtagUnexpected() { }
        /// <summary>
        /// Creates new GoldtagUnexpected
        /// </summary>
        /// <param name="userId">userId</param>
        /// <param name="bankTransId">bankTransId</param>
        /// <param name="expectedTL">expectedTL</param>
        /// <param name="receivedTL">receivedTL</param>
        /// <param name="notes">notes</param>
        public GoldtagUnexpected(Guid userId,
            Guid bankTransId,
            decimal expectedTL,
            decimal receivedTL,
            string notes = null)
        {
            UserId = UserId;
            BankTransferId = bankTransId;
            ExpectedTL = expectedTL;
            ReceivedTL = receivedTL;
            UnexpectedDateTime = DateTime.Now;
            Notes = notes;
        }
    }
}
