using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Cash payment request for Gold/Silver sales
    /// TO be handled by service automatically
    /// </summary>
    public class VendorCashPayment
    {
        /// <summary>
        /// DB id
        /// </summary>
        public Guid PaymentId { get; set; }
        /// <summary>
        /// Vendor id
        /// </summary>
        public Guid VendorId { get; set; }
        /// <summary>
        /// Transaction id
        /// </summary>
        public int TransactionId { get; set; }
        /// <summary>
        /// Vendor's reference 
        /// </summary>
        public string VendorReferenceId { get; set; }
        /// <summary>
        /// Exchange rate fxcode 
        /// </summary>
        public int RateFxId { get; set; }
        /// <summary>
        /// Grams
        /// </summary>
        public decimal GramAmount { get; set; }
        /// <summary>
        ///  Lira amount
        /// </summary>
        public decimal TLAmount { get; set; }
        /// <summary>
        /// public price 
        /// </summary>
        public decimal PiyasaGramFiyat { get; set; }
        /// <summary>
        /// kt price
        /// </summary>
        public decimal KTGramFiyat { get; set; }
        /// <summary>
        /// goldtag price
        /// </summary>
        public decimal SatisGramFiyat { get; set; }
        /// <summary>
        /// DateTime of the request
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// Explanatory comments
        /// </summary>
        public string Comments { get; set; }

        private VendorCashPayment() { }

        /// <summary>
        /// Generates hashcode
        /// </summary>
        /// <returns>hashcode as int</returns>
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
        /// Creates new VendorCashPayment
        /// </summary>
        /// <param name="vendorId">the id of vendor</param>
        /// <param name="transId">the transaction id</param>
        /// <param name="referance">vendor ref id</param>
        /// <param name="fxId">exchange rate id</param>
        /// <param name="tl">total try</param>
        /// <param name="grams">total grams</param>
        /// <param name="piy">public price</param>
        /// <param name="kt">kt price</param>
        /// <param name="sat">sale price</param>
        /// <param name="comments">useful comments</param>
        public VendorCashPayment(
            Guid vendorId, 
            int transId, 
            string referance,
            int fxId,
            decimal tl,
            decimal grams, 
            decimal piy, 
            decimal kt, 
            decimal sat,string comments = null)
        {
            VendorId = vendorId;
            TransactionId = transId;
            VendorReferenceId = referance;
            TLAmount = tl;
            RateFxId = fxId;
            GramAmount = grams;
            PiyasaGramFiyat = piy;
            KTGramFiyat = kt;
            SatisGramFiyat = sat;
            DateTime = DateTime.Now;
            Comments = comments;
        }

        /// <summary>
        /// returns string representation
        /// </summary>
        /// <returns>string representation of CashPayment</returns>
        public override string ToString()
        {
            return "VendorCashPayment: Id" + PaymentId.ToString() + "\n" +
                "VEndorId" + VendorId.ToString() + "\n" +
                "Try" + TLAmount.ToString();
        }
    }
}
