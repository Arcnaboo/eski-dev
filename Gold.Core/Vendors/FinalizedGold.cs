using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Depreciated finalized gold entity
    /// </summary>
    public class FinalizedGold
    {
        /// <summary>
        /// DB id
        /// </summary>
        public Guid FinalizedId { get; set; }
        /// <summary>
        /// Vendor's id
        /// </summary>
        public Guid VendorId { get; set; }
        /// <summary>
        /// Transaction id
        /// </summary>
        public Guid TransactionId { get; set; }
        /// <summary>
        /// Vendor's reference id
        /// </summary>
        public string VendorReferenceId { get; set; }
        /// <summary>
        /// Datetime this entity created
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// TRY Amount
        /// </summary>
        public decimal TLAmount { get; set; }
        /// <summary>
        /// Gram amount
        /// </summary>
        public decimal GoldAmount { get; set; }
        /// <summary>
        /// public price
        /// </summary>
        public decimal PiyasaGramFiyat { get; set; }
        /// <summary>
        /// KT price
        /// </summary>
        public decimal KTGramFiyat { get; set; }
        /// <summary>
        /// Goldtag price
        /// </summary>
        public decimal SatisGramFiyat { get; set; }
        /// <summary>
        /// True iff vendor bought mineral
        /// </summary>
        public bool AltinVerildi { get; set; }
        /// <summary>
        /// KT execution reference
        /// </summary>
        public string KTReferansId { get; set; }
        /// <summary>
        /// Comments for admin panel
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// Value for accounting TRY
        /// </summary>
        public decimal MuhasebeTL { get; set; }
        /// <summary>
        /// Value for accounting mineral gram
        /// </summary>
        public decimal MuhasebeGram { get; set; }

        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private FinalizedGold()
        {

        }
        /// <summary>
        /// Creates new finalized transaction
        /// </summary>
        /// <param name="expectedCash">Expected cash</param>
        /// <param name="altinVerildi">true or false</param>
        /// <param name="ktReferans">kt ref id</param>
        /// <param name="comments">comments for panel</param>
        /// <param name="tl">total try</param>
        /// <param name="gram">total grams</param>
        public FinalizedGold(ExpectedCash expectedCash, bool altinVerildi, string ktReferans, string comments, decimal tl, decimal gram)
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
            AltinVerildi = altinVerildi;
            KTReferansId = ktReferans;
            Comments = comments;
            MuhasebeTL = tl;
            MuhasebeGram = gram;
        }


        /// <summary>
        /// Creates new finalized transaction
        /// </summary>
        /// <param name="vendorId">vendor's id</param>
        /// <param name="transId">transaction id</param>
        /// <param name="refe">referans</param>
        /// <param name="tlAmount">total try</param>
        /// <param name="goldAmount">total grams</param>
        /// <param name="pg">public price</param>
        /// <param name="ktg">kt price</param>
        /// <param name="stg">goldtag price</param>
        /// <param name="ktRef">execution reference by kt</param>
        /// <param name="comments">admin panel comments</param>
        /// <param name="tl">accounting try</param>
        /// <param name="gram">accoounting grams</param>
        public FinalizedGold(Guid vendorId, Guid transId, string refe, decimal tlAmount, decimal goldAmount,
            decimal pg, decimal ktg, decimal stg, string ktRef, string comments, decimal tl, decimal gram)
        {
            VendorId = vendorId;
            TransactionId = transId;
            VendorReferenceId = refe;
            DateTime = DateTime.Now;
            TLAmount = tlAmount;
            GoldAmount = goldAmount;
            PiyasaGramFiyat = pg;
            KTGramFiyat = ktg;
            SatisGramFiyat = stg;
            AltinVerildi = false;
            KTReferansId = ktRef;
            Comments = comments;
            MuhasebeTL = tl;
            MuhasebeGram = gram;
        }

    }
}
