using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Transactions
{   
    /// <summary>
    /// Represents an expected cash for goldtag app
    /// </summary>
    public class GoldtagExpected
    {
        /// <summary>
        /// DB ID
        /// </summary>
        public Guid ExpectedGoldtagId { get; set; }
        /// <summary>
        /// User's id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// bank transfer id
        /// </summary>
        public Guid BankTransferId { get; set; }
        /// <summary>
        /// expected suffix in kt
        /// </summary>
        public string ExpectedSuffix { get; set; }
        /// <summary>
        ///  expected try amount
        /// </summary>
        public decimal ExpectedTRY { get; set; }
        /// <summary>
        /// Rate fx id should be 24 for gold 26 for silver ...
        /// </summary>
        public int RateFxId { get; set; }
        /// <summary>
        /// Grams
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// When we started to expect
        /// </summary>
        public DateTime ExpectedDateTime { get; set; }
        /// <summary>
        /// KT gram price at the moment of creation
        /// </summary>
        public decimal KTGramFiyat { get; set; }

        /// <summary>
        /// general price at the moment of creation
        /// </summary>
        public decimal PiyasaGramFiyat { get; set; }

        /// <summary>
        /// goldtag price at the moment of creation
        /// </summary>
        public decimal SatisGramFiyat { get; set; }

        /// <summary>
        /// Extra comments
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private GoldtagExpected() { }
        /// <summary>
        /// Creates new expected cash
        /// </summary>
        /// <param name="userId">users id</param>
        /// <param name="bankTransferId">bank transfer id</param>
        /// <param name="suffix">expected suffix</param>
        /// <param name="expectedTRY">expected try amount</param>
        /// <param name="fxid">rate fx id for checking rate</param>
        /// <param name="amount">amount of mineral</param>
        /// <param name="ktfiyat">1 gr price of kt</param>
        /// <param name="piyasaFiyat">1 gr price of general public</param>
        /// <param name="satisFiyat">1 gr price of goldtag</param>
        /// <param name="notes">any extra comment</param>
        public GoldtagExpected(
            Guid userId,
            Guid bankTransferId,
            string suffix,
            decimal expectedTRY,
            int fxid,
            decimal amount,
            decimal ktfiyat,
            decimal piyasaFiyat,
            decimal satisFiyat,
            string notes = null)
        {
            UserId = userId;
            BankTransferId = bankTransferId;
            ExpectedSuffix = suffix;
            ExpectedTRY = expectedTRY;
            RateFxId = fxid;
            Amount = amount;
            ExpectedDateTime = DateTime.Now;
            KTGramFiyat = ktfiyat;
            PiyasaGramFiyat = piyasaFiyat;
            SatisGramFiyat = satisFiyat;
            Notes = notes;
        }
        /// <summary>
        /// String representation
        /// </summary>
        /// <returns>a string representation of this class</returns>
        public override string ToString()
        {
            return "ExpectedCash: Id" + ExpectedGoldtagId.ToString() + "\n" +
                "Userid" + UserId.ToString() + "\n" +
                "Expected Try" + ExpectedTRY.ToString();
        }
    }
}
