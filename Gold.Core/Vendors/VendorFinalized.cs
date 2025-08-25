using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Stores information related to finalized transactions
    /// where gold is transferred physically and money is exchanged
    /// </summary>
    public class VendorFinalized
    {
        /// <summary>
        /// DB id
        /// </summary>
        public Guid FinalizedId { get; set; }
        /// <summary>
        /// vendor's id
        /// </summary>
        public Guid VendorId { get; set; }
        /// <summary>
        /// tranasction id // changed to count
        /// </summary>
        public int TransactionId { get; set; }
        /// <summary>
        /// vendor's reference
        /// </summary>
        public string VendorReferenceId { get; set; }
        /// <summary>
        /// finalization datetime
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// total lira amount exchanged
        /// </summary>
        public decimal TLAmount { get; set; }
        /// <summary>
        /// total gold or silver or platin amount
        /// </summary>
        public decimal GoldAmount { get; set; }
        /// <summary>
        /// average price for one or many expected/ payment
        /// </summary>
        public decimal PiyasaGramFiyat { get; set; }
        /// <summary>
        /// kt price for 1g
        /// </summary>
        public decimal KTGramFiyat { get; set; }
        /// <summary>
        /// average goldtag price for one or many expected/payment
        /// </summary>
        public decimal SatisGramFiyat { get; set; }
        /// <summary>
        /// Gold silver or platin
        /// </summary>
        public string MadenType { get; set; }
        /// <summary>
        /// KT execution reference
        /// </summary>
        public string KTReferansId { get; set; }
        /// <summary>
        /// Useful comments
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// TL income or expense 
        /// </summary>
        public decimal MuhasebeTL { get; set; }
        /// <summary>
        /// Grams income or expense
        /// </summary>
        public decimal MuhasebeGram { get; set; }
        /// <summary>
        /// Final profit from the transactions
        /// </summary>
        public decimal? FinalKar { get; set; }

        private VendorFinalized()
        {

        }

        /// <summary>
        /// Creates new finalized transaction
        /// </summary>
        /// <param name="expectedCash">Expected cash object</param>
        /// <param name="madenType">type of transaction </param>
        /// <param name="ktReferans">kt reference</param>
        /// <param name="comments">useful comments</param>
        /// <param name="tl">total try of transactions</param>
        /// <param name="gram">total grams of transactions</param>
        public VendorFinalized(VendorExpected expectedCash, string madenType, string ktReferans, string comments, decimal tl, decimal gram)
        {
            VendorId = expectedCash.VendorId;
            TransactionId = expectedCash.TransactionId;
            VendorReferenceId = expectedCash.VendorReferenceId;
            DateTime = DateTime.Now;
            TLAmount = expectedCash.ExpectedTRY;
            GoldAmount = expectedCash.Amount;
            PiyasaGramFiyat = expectedCash.PiyasaGramFiyat;
            KTGramFiyat = expectedCash.KTGramFiyat;
            SatisGramFiyat = expectedCash.SatisGramFiyat;
            MadenType = madenType;
            KTReferansId = ktReferans;
            Comments = comments;
            MuhasebeTL = tl;
            MuhasebeGram = gram;
        }

        /// <summary>
        /// Creates new finalized transaction 
        /// </summary>
        /// <param name="vendorId">vendor id</param>
        /// <param name="transId">transactions count</param>
        /// <param name="vendorRefId">first last transaction id</param>
        /// <param name="tlAmount">total try</param>
        /// <param name="gramAmount">total grams</param>
        /// <param name="pgf">public price for 1g</param>
        /// <param name="kgf">lt price for 1g</param>
        /// <param name="sgf">fintag price for 1g</param>
        /// <param name="madenType">type of mineral</param>
        /// <param name="ktRef">kt execution id</param>
        /// <param name="comm">useful comments</param>
        /// <param name="mTl">for accounting minus or plus try</param>
        /// <param name="mGr">for accounting minus or plus grams</param>
        /// <param name="finalKar">final profit if any</param>
        public VendorFinalized(Guid vendorId,
            int transId,
            string vendorRefId,
            decimal tlAmount,
            decimal gramAmount,
            decimal pgf,
            decimal kgf,
            decimal sgf,
            string madenType,
            string ktRef,
            string comm,
            decimal mTl,
            decimal mGr,
            decimal finalKar)
        {
            VendorId = vendorId;
            TransactionId = transId;
            VendorReferenceId = vendorRefId;
            DateTime = DateTime.Now;
            TLAmount = tlAmount;
            GoldAmount = gramAmount;
            PiyasaGramFiyat = pgf;
            KTGramFiyat = kgf;
            SatisGramFiyat = sgf;
            MadenType = madenType;
            KTReferansId = ktRef;
            Comments = comm;
            MuhasebeTL = mTl;
            MuhasebeGram = mGr;
            FinalKar = finalKar;
        }



    }
}
