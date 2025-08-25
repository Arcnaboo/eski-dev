using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Version2
{
    /// <summary>
    /// represents a mineral type (gold / silver ...)
    /// </summary>
    public class Maden
    {
        /// <summary>
        /// db id
        /// </summary>
        public Guid MadenId { get; set; }
        /// <summary>
        /// common name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// kt buy price
        /// </summary>
        public decimal KtBuyRate { get; set; }
        /// <summary>
        /// kt sell price
        /// </summary>
        public decimal KtSellRate { get; set; }
        /// <summary>
        /// goldtag buy price manually entered
        /// </summary>
        public decimal ManuelBuyRate { get; set; }
        /// <summary>
        /// goldtag sell price
        /// </summary>
        public decimal ManuelSellRate { get; set; }
        /// <summary>
        /// kt exchange rate fx id
        /// </summary>
        public int KtFxId { get; set; }
        /// <summary>
        /// if true then system should use kt price
        /// </summary>
        public bool UseKtPrice { get; set; }

        private Maden() { }

        /// <summary>
        /// creates new maden
        /// </summary>
        /// <param name="name">name of maden</param>
        /// <param name="fdxId">kt fx id</param>
        public Maden(string name, int fdxId)
        {
            Name = name;
            KtFxId = fdxId;
            UseKtPrice = true;
            KtBuyRate = -1;
            KtSellRate = -1;
        }
    }
}
