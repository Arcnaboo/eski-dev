using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// confirmation record
    /// </summary>
    public class VendorConfirmationNew
    {
        /// <summary>
        /// db id
        /// </summary>
        public Guid ConfirmationId { get; set; }
        /// <summary>
        /// trans id
        /// </summary>
        public int TransactionId { get; set; }
        /// <summary>
        /// true iff goldtag confirms transaction
        /// </summary>
        public bool ConfirmedByAdmin { get; set; }

        private VendorConfirmationNew() { }
        
        /// <summary>
        /// creates new confirmation
        /// </summary>
        /// <param name="transId">related transaction</param>
        public VendorConfirmationNew(int transId)
        {
            TransactionId = transId;
            ConfirmedByAdmin = true;
        }
    }
}
