using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Represetns a transaction where position not closed yet (BUY)
    /// </summary>
    public class VendorNotPosition
    {
        /// <summary>
        /// DB ID
        /// </summary>
        public Guid NotPosId { get; set; }
        /// <summary>
        /// first___last transaction ids
        /// </summary>
        public string RefId { get; set; }
        /// <summary>
        /// total amount of gold or silver or platin
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// goldtag suffix for vendor
        /// </summary>
        public string SuffixFrom { get; set; }
        /// <summary>
        /// goldtag suffix for vendor
        /// </summary>
        public string SuffixTo { get; set; }
        /// <summary>
        /// buy rate of the mineral
        /// </summary>
        public decimal BuyRate { get; set; }
        /// <summary>
        /// transactions count
        /// </summary>
        public int TransactionId { get; set; }
        /// <summary>
        /// related vendor
        /// </summary>
        public Guid VendorId { get; set; }
        /// <summary>
        /// datetime of the transactions
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// useful comments
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// average public price for 1g
        /// </summary>
        public decimal PiyasaGramFiyat { get; set; }
        /// <summary>
        /// kt price for 1g
        /// </summary>
        public decimal KTGramFiyat { get; set; }
        /// <summary>
        /// average goldtag price for 1g
        /// </summary>
        public decimal SatisGramFiyat { get; set; }

        private VendorNotPosition() { }

        /// <summary>
        /// creates new not pos
        /// </summary>
        /// <param name="refId">first_last ids</param>
        /// <param name="amount">total grams</param>
        /// <param name="_from">from suffix</param>
        /// <param name="_to">to suffix</param>
        /// <param name="rate">buy rate</param>
        /// <param name="tId">transactions count</param>
        /// <param name="vid">vendor id</param>
        /// <param name="comments">useful comments</param>
        /// <param name="pgf">public price</param>
        /// <param name="kgf">kt price</param>
        /// <param name="sgf">goldtag price</param>
        public VendorNotPosition(string refId, decimal amount,
            string _from, string _to, decimal rate, int tId, Guid vid, 
            string comments, decimal pgf, decimal kgf, decimal sgf)
        {
            RefId = refId;
            Amount = amount;
            SuffixFrom = _from;
            SuffixTo = _to;
            BuyRate = rate;
            TransactionId = tId;
            VendorId = vid;
            DateTime = DateTime.Now;
            Comments = comments;
            PiyasaGramFiyat = pgf;
            KTGramFiyat = kgf;
            SatisGramFiyat = sgf;
        }

    }
}
