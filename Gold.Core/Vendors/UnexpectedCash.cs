using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Depreciated unexpected cash class
    /// </summary>
    public class UnexpectedCash
    {
        /// <summary>
        /// id
        /// </summary>
        public Guid UnExpectedId { get; set; }
        /// <summary>
        /// transaction
        /// </summary>
        public Guid TransactionId { get; set; }
        /// <summary>
        /// kt reference
        /// </summary>
        public string KTReference { get; set; }
        /// <summary>
        /// suffix where money came in or not
        /// </summary>
        public string Suffix { get; set; }
        /// <summary>
        /// grams
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// datetime of the event
        /// </summary>
        public DateTime DateTime { get; set; }

        private UnexpectedCash() { }
        /// <summary>
        /// Creates new unexpected cash
        /// </summary>
        /// <param name="transId">transaction</param>
        /// <param name="ktref">referebce</param>
        /// <param name="suffix">suffix related</param>
        /// <param name="amount">grams</param>
        public UnexpectedCash(Guid transId, string ktref, string suffix, decimal amount)
        {
            this.TransactionId = transId;
            KTReference = ktref;
            Suffix = suffix;
            Amount = amount;
            DateTime = DateTime.Now;
        }

        /// <summary>
        /// str repr
        /// </summary>
        /// <returns>str</returns>
        public override string ToString()
        {
            return "Unexpected: " + Amount.ToString();
        }
    }
}
