using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Depreciated
    /// Represetns a transaction where position not closed yet
    /// </summary>
    public class NotPosGold
    {
        /// <summary>
        /// DB ID
        /// </summary>
        public Guid NotPosId { get; set; }
        /// <summary>
        /// KT ref id
        /// </summary>
        public string RefId { get; set; }
        /// <summary>
        /// Amount of grams
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// From which suffix
        /// </summary>
        public string SuffixFrom { get; set; }
        /// <summary>
        /// To which suffix
        /// </summary>
        public string SuffixTo { get; set; }
        /// <summary>
        /// BuyRate tried during buy process
        /// </summary>
        public decimal BuyRate { get; set; }
        /// <summary>
        /// Related transactiomn id
        /// </summary>
        public Guid TransactionId { get; set; }
        /// <summary>
        /// Related Vendor id
        /// </summary>
        public Guid VendorId { get; set; }
        /// <summary>
        /// DateTime of not position
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// Arbitrary comments
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private NotPosGold() { }
        /// <summary>
        /// Constructs new NotPosGold
        /// </summary>
        /// <param name="refId">kt ref</param>
        /// <param name="amount">gram amount</param>
        /// <param name="_from">From suffix</param>
        /// <param name="_to">to suffix</param>
        /// <param name="rate">buy rate</param>
        /// <param name="tId">transaction id</param>
        /// <param name="vid">vendor id</param>
        /// <param name="comments">any useful comments</param>
        public NotPosGold(string refId, decimal amount,
            string _from, string _to, decimal rate, Guid tId, Guid vid, 
            string comments)
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
        }

    }
}
