using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Transactions
{
    /// <summary>
    /// Represents a transaction where money never arrived
    /// </summary>
    public class GoldtagNotReceived
        {/// <summary>
         /// DB ID
         /// </summary>
        public Guid NotReceivedId { get; set; }
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
        public decimal GramAmount { get; set; }
        /// <summary>
        /// When we started to expect
        /// </summary>
        public DateTime ExpectedDateTime { get; set; }
        /// <summary>
        /// Current time when this instance created
        /// </summary>
        public DateTime CurrentDateTime { get; set; }
        /// <summary>
        /// Time difference between current time and expected time
        /// </summary>
        public int TimeDiffSeconds { get; set; }
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
        /// extra notes
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private GoldtagNotReceived() { }
        /// <summary>
        /// Creates new not received
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="bankTransferId">bank id</param>
        /// <param name="suffix">suffix</param>
        /// <param name="expectedTRY">expectedTRY</param>
        /// <param name="fxid">fxid</param>
        /// <param name="expectedDateTime">expectedDateTime</param>
        /// <param name="currentTime">currentTime</param>
        /// <param name="diffSecond">diffSecond</param>
        /// <param name="amount">amount</param>
        /// <param name="ktfiyat">ktfiyat</param>
        /// <param name="piyasaFiyat">piyasaFiyat</param>
        /// <param name="satisFiyat">satisFiyat</param>
        /// <param name="notes">notes</param>
        public GoldtagNotReceived(
            Guid userId,
            Guid bankTransferId,
            string suffix,
            decimal expectedTRY,
            int fxid,
            DateTime expectedDateTime,
            DateTime currentTime,
            int diffSecond,
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
            GramAmount = amount;
            ExpectedDateTime = expectedDateTime;
            CurrentDateTime = currentTime;
            TimeDiffSeconds = diffSecond;
            KTGramFiyat = ktfiyat;
            PiyasaGramFiyat = piyasaFiyat;
            SatisGramFiyat = satisFiyat;
            Notes = notes;
        }
    }
}
