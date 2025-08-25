using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Represents a situation where either expected cash timed out
    /// or expected cash did not completed as intended
    /// </summary>
    public class VendorUnExpected
    {
        /// <summary>
        /// db id
        /// </summary>
        public Guid UnExpectedId { get; set; }
        /// <summary>
        /// transactions count
        /// </summary>
        public int TransactionId { get; set; }
        /// <summary>
        /// last kt execution id
        /// </summary>
        public string KTReference { get; set; }
        /// <summary>
        /// suffix where money received or was expected to receive
        /// </summary>
        public string Suffix { get; set; }
        /// <summary>
        /// total transactions grams 
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// date time when this entity created
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// useful comments
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// expected lira amount
        /// </summary>
        public decimal ExpectedTRY { get; set; }
        /// <summary>
        /// received lira amount if any
        /// </summary>
        public decimal? ReceivedTRY { get; set; }
        /// <summary>
        /// time difference if timed out
        /// </summary>
        public int? DifferenceSEconds { get; set; }

        private VendorUnExpected() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transId">transId</param>
        /// <param name="ktref">kt ref</param>
        /// <param name="suffix">expected suffix</param>
        /// <param name="amount">total grams</param>
        /// <param name="comm">useful comments</param>
        /// <param name="expectedTry">expected lira</param>
        /// <param name="received">received lira if any</param>
        /// <param name="diff">time difference if any</param>
        public VendorUnExpected(int transId, string ktref, string suffix, decimal amount, string comm,
            decimal expectedTry, decimal? received = null, int? diff = null)
        {
            this.TransactionId = transId;
            KTReference = ktref;
            Suffix = suffix;
            Amount = amount;
            DateTime = DateTime.Now;
            Comment = comm;
            ExpectedTRY = expectedTry;
            ReceivedTRY = received;
            DifferenceSEconds = diff;
        }

        /// <summary>
        /// string representaion
        /// </summary>
        /// <returns>string rep</returns>
        public override string ToString()
        {
            return string.Format("VendorUnexpected: {0} to {1} the trans = {2}", ExpectedTRY, Suffix, TransactionId);
        }
    }
}
