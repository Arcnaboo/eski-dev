using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Transactions
{   
    /// <summary>
    /// Represetns a transaction where position not closed yet
    /// </summary>
    public class GoldtagNotPosition
    {
        /// <summary>
        /// DB ID
        /// </summary>
        public Guid NotPosId { get; set; }
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
        /// amount of received try
        /// </summary>
        public decimal ReceivedTRY { get; set; }
        /// <summary>
        /// fx id for the mineral
        /// </summary>
        public int RateFxId { get; set; }
        /// <summary>
        /// money receive time
        /// </summary>
        public DateTime MoneyReceivedDateTime { get; set; }
        /// <summary>
        /// fintag rate at the time of money receive
        /// </summary>
        public decimal ThatMomentBuyRate { get; set; }
        /// <summary>
        /// transaction gram amount
        /// </summary>
        public decimal GramAmount { get; set; }
        /// <summary>
        /// general price at the moment of creation
        /// </summary>
        public decimal PiyasaGramFiyat { get; set; }
        /// <summary>
        /// KT gram price at the moment of creation
        /// </summary>
        public decimal KTGramFiyat { get; set; }
        /// <summary>
        /// goldtag price at the moment of creation
        /// </summary>
        public decimal SatisGramFiyat { get; set; }
        /// <summary>
        /// KT reference
        /// </summary>
        public string KTHareketReferans { get; set; }
        /// <summary>
        /// Extra comments
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private GoldtagNotPosition() { }
        /// <summary>
        /// Creates new not position
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="bankTransId">bank trans id</param>
        /// <param name="suffix">expected suffix</param>
        /// <param name="received">received try</param>
        /// <param name="fxid">fx id of mineral</param>
        /// <param name="thatMoment">current time</param>
        /// <param name="momentBuyRate">current buy rate</param>
        /// <param name="gramAmount">gram amount of trans</param>
        /// <param name="pFiyat">general price</param>
        /// <param name="ktFiyat">kt price</param>
        /// <param name="satisFiyat">goldtag price</param>
        /// <param name="ktReferans">kt execution reference</param>
        /// <param name="notes">extra notes</param>
        public GoldtagNotPosition(Guid userId, Guid bankTransId,
            string suffix, decimal received, int fxid, 
            DateTime thatMoment, decimal momentBuyRate,
            decimal gramAmount, decimal pFiyat, decimal ktFiyat,
            decimal satisFiyat, string ktReferans, string notes=null)
        {
            UserId = userId;
            BankTransferId = bankTransId;
            ExpectedSuffix = suffix;
            ReceivedTRY = received;
            RateFxId = fxid;
            MoneyReceivedDateTime = thatMoment;
            ThatMomentBuyRate = momentBuyRate;
            GramAmount = gramAmount;
            PiyasaGramFiyat = pFiyat;
            KTGramFiyat = ktFiyat;
            SatisGramFiyat = satisFiyat;
            KTHareketReferans = ktReferans;
            Notes = notes;
        }

        /// <summary>
        /// String representation
        /// </summary>
        /// <returns>a string representation of this class</returns>
        public override string ToString()
        {
            return "NotposGoldtag: userid: " + NotPosId.ToString() + " - " + UserId + " - ";
        }
    }
}
