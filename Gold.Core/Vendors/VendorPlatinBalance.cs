using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Stores vendor's platin balance
    /// </summary>
    public class VendorPlatinBalance
    {
        /// <summary>
        /// vendor id
        /// </summary>
        public Guid VendorId { get; set;}
        /// <summary>
        /// balance
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// vendorplayinbalance constructor
        /// </summary>
        private VendorPlatinBalance() { }

        /// <summary>
        /// creates new platin balance
        /// </summary>
        /// <param name="vendorId">which vendor</param>
        public VendorPlatinBalance(Guid vendorId)
        {
            VendorId = vendorId;
            Balance = 0;
        }
    }
}
