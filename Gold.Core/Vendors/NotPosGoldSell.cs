using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Depreciated not pos sell class
    /// </summary>
    public class NotPosGoldSell
    {
        /// <summary>
        /// databse id
        /// </summary>
        public Guid NotPosId { get; set; }
        /// <summary>
        /// kt ref id
        /// </summary>
        public string RefId { get; set; }
        /// <summary>
        /// gram amount
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// sell precious metal suffix from 
        /// </summary>
        public string SuffixFrom { get; set; }
        /// <summary>
        /// sell previous metal seuffix to
        /// </summary>
        public string SuffixTo { get; set; }
        /// <summary>
        /// Sell rate
        /// </summary>
        public decimal SellRate { get; set; }
        /// <summary>
        /// Transaction ID
        /// </summary>
        public Guid TransactionId { get; set; }
        /// <summary>
        /// Vendor ID
        /// </summary>
        public Guid VendorId { get; set; }
        /// <summary>
        /// Datetime of non pos sell
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// Arbitrary comments
        /// </summary>
        public string Comments { get; set; }

        private NotPosGoldSell() { }

        /// <summary>
        /// Creates new NotPosGoldSell
        /// </summary>
        /// <param name="refId">reference</param>
        /// <param name="amount">grams</param>
        /// <param name="_from">kt from suffiix</param>
        /// <param name="_to">kt to suffix</param>
        /// <param name="rate">sell rate</param>
        /// <param name="tId">transaction</param>
        /// <param name="vid">vendor</param>
        /// <param name="comments">comments</param>
        public NotPosGoldSell(string refId, decimal amount,
            string _from, string _to, decimal rate, Guid tId, Guid vid, 
            string comments)
        {
            RefId = refId;
            Amount = amount;
            SuffixFrom = _from;
            SuffixTo = _to;
            SellRate = rate;
            TransactionId = tId;
            VendorId = vid;
            DateTime = DateTime.Now;
            Comments = comments;
        }

    }
}
