using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Depreciated expected cash entity
    /// </summary>
    public class ExpectedCash
    {
        /// <summary>
        /// Db id
        /// </summary>
        public Guid ExpectedId { get; set; }
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
        /// expected suffix
        /// </summary>
        public string ExpectedSuffix { get; set; }
        /// <summary>
        /// expected try amount
        /// </summary>
        public decimal ExpectedTRY { get; set; }
        /// <summary>
        /// exchange rate id
        /// </summary>
        public int RateFxId { get; set; }
        /// <summary>
        /// gram amount
        /// </summary>
        public decimal Amount { get; set; }
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
        /// Datetime this entity created
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private ExpectedCash() { }
        /// <summary>
        /// Creates mew expected chas
        /// </summary>
        /// <param name="vendorId">vendor's id</param>
        /// <param name="transId">transaction id</param>
        /// <param name="referance">vendor ref</param>
        /// <param name="suffix">expected suffix</param>
        /// <param name="fxId">exchange rate id</param>
        /// <param name="tl">try amount</param>
        /// <param name="grams">gram amount</param>
        /// <param name="piy">public price</param>
        /// <param name="kt">kt price</param>
        /// <param name="sat">goldtag price</param>
        public ExpectedCash(Guid vendorId, 
            Guid transId, 
            string referance,
            string suffix,
            int fxId,
            decimal tl,
            decimal grams, 
            decimal piy, 
            decimal kt, 
            decimal sat)
        {
            VendorId = vendorId;
            TransactionId = transId;
            VendorReferenceId = referance;
            ExpectedSuffix = suffix;
            ExpectedTRY = tl;
            RateFxId = fxId;
            Amount = grams;
            PiyasaGramFiyat = piy;
            KTGramFiyat = kt;
            SatisGramFiyat = sat;
            DateTime = DateTime.Now;
        }
        /// <summary>
        /// String representation
        /// </summary>
        /// <returns>a string representation of this class</returns>
        public override string ToString()
        {
            return "ExpectedCash: Id" + ExpectedId.ToString() + "\n" +
                "VEndorId" + VendorId.ToString() + "\n" +
                "Expected Try" + Amount.ToString();
        }
    }
}
