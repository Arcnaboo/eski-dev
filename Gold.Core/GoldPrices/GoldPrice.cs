using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.GoldPrices
{   
    /// <summary>
    /// Gold price class, represents a valuable data in the rutime
    /// </summary>
    public class GoldPrice
    {
        /// <summary>
        /// DB id
        /// </summary>
        public Guid GoldPriceId { get; set; }
        /// <summary>
        /// Name of the variable
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// If true there is amount 
        /// </summary>
        public decimal? Amount { get; set; }
        /// <summary>
        /// if true there is percentagec
        /// </summary>
        public decimal? Percentage { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private GoldPrice() { }
    }
}
