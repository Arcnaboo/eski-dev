using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// A task for expected service
    /// where automatic buy process occurs
    /// </summary>
    public class VendorExpected
    {
        /// <summary>
        /// DB id if venexpected added to db during server shutdown
        /// </summary>
        public Guid ExpectedId { get; set; }
        /// <summary>
        /// vendor's id
        /// </summary>
        public Guid VendorId { get; set; }
        /// <summary>
        /// tranasction id
        /// </summary>
        public int TransactionId { get; set; }
        /// <summary>
        /// vendor's reference
        /// </summary>
        public string VendorReferenceId { get; set; }
        /// <summary>
        /// suffix whre money is expected
        /// </summary>
        public string ExpectedSuffix { get; set; }
        /// <summary>
        ///  expected lira amount
        /// </summary>
        public decimal ExpectedTRY { get; set; }
        /// <summary>
        /// exchange rate id
        /// </summary>
        public int RateFxId { get; set; }
        /// <summary>
        /// grams amount
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// public price for 1g
        /// </summary>
        public decimal PiyasaGramFiyat { get; set; }
        /// <summary>
        /// kt price for 1g
        /// </summary>
        public decimal KTGramFiyat { get; set; }
        /// <summary>
        /// goldtag price for 1g
        /// </summary>
        public decimal SatisGramFiyat { get; set; }
        /// <summary>
        /// datetime of the entity creation
        /// </summary>
        public DateTime DateTime { get; set; }

        private VendorExpected() { }

        /// <summary>
        /// Generates Hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int result = 1;
            int prime = 13;
            result += prime * VendorId.ToByteArray().GetHashCode();
            result += prime * VendorReferenceId.GetHashCode();
            result += prime * TransactionId;
            return result % 10000;
        }

        /// <summary>
        /// Creates new VendorExpected
        /// </summary>
        /// <param name="vendorId">Vendor id</param>
        /// <param name="transId">Transaction id</param>
        /// <param name="referance">vendor reference id</param>
        /// <param name="suffix">expected suffix</param>
        /// <param name="fxId">exchange rate id</param>
        /// <param name="tl">expected try</param>
        /// <param name="grams">grams of transaction</param>
        /// <param name="piy">public price for 1g</param>
        /// <param name="kt">lt price for 1g</param>
        /// <param name="sat">fintag price for 1g</param>
        public VendorExpected(Guid vendorId, 
            int transId, 
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
        /// String representation of this class
        /// </summary>
        /// <returns>a string</returns>
        public override string ToString()
        {
            return "ExpectedCash: Grams " + Amount.ToString() + " VEndorId" + VendorId.ToString() + " Expected Try " + ExpectedTRY.ToString();
        }
    }
}
