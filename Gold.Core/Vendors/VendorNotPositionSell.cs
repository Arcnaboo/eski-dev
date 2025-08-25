using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Represetns a transaction where position not closed yet (SELL)
    /// </summary>
    public class VendorNotPositionSell
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
        /// total try to be sent
        /// </summary>
        public decimal TlAmount { get; set; }
        /// <summary>
        /// total grams
        /// </summary>
        public decimal GramAmount { get; set; }
        /// <summary>
        /// accoutn suffix
        /// </summary>
        public string Suffix { get; set; }
        /// <summary>
        /// account number
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// kt sell rate
        /// </summary>
        public decimal SellRate { get; set; }
        /// <summary>
        /// transactions count
        /// </summary>
        public int TransactionId { get; set; }
        /// <summary>
        /// related vendor's id
        /// </summary>
        public Guid VendorId { get; set; }
        /// <summary>
        /// datetime of the transaction
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// useful comments
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// avg public price for 1g
        /// </summary>
        public decimal PiyasaGramFiyat { get; set; }
        /// <summary>
        /// kt price for 1g
        /// </summary>
        public decimal KTGramFiyat { get; set; }
        /// <summary>
        /// avg fintag price for 1g
        /// </summary>
        public decimal SatisGramFiyat { get; set; }

        private VendorNotPositionSell() { }

        /// <summary>
        /// creates new not pos
        /// </summary>
        /// <param name="refId">first last id</param>
        /// <param name="tlamount">total try</param>
        /// <param name="gamount">grams</param>
        /// <param name="suff">vendor's desired suffix</param>
        /// <param name="acc">vendor kt account number</param>
        /// <param name="rate">sell rate</param>
        /// <param name="tId">transactions count</param>
        /// <param name="vid">vendor id</param>
        /// <param name="comments">useful comments</param>
        /// <param name="pgf">public price</param>
        /// <param name="kgf">kt price</param>
        /// <param name="sgf">goldtag price</param>
        public VendorNotPositionSell(string refId, decimal tlamount, decimal gamount,
            string suff, string acc, decimal rate, int tId, Guid vid, 
            string comments, decimal pgf, decimal kgf, decimal sgf)
        {
            RefId = refId;
            TlAmount = tlamount;
            GramAmount = gamount;
            Account = acc;
            Suffix = suff;
            SellRate = rate;
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
