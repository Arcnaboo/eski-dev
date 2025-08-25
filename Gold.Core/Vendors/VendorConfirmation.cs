using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Depreciated confirmation record
    /// </summary>
    public class VendorConfirmation
    {
        /// <summary>
        /// db id
        /// </summary>
        public Guid ConfirmationId { get; set; }
        /// <summary>
        /// trans id
        /// </summary>
        public Guid TransactionId { get; set; }
        /// <summary>
        /// true iff goldtag confirms transaction
        /// </summary>
        public bool ConfirmedByAdmin { get; set; }

        private VendorConfirmation() { }

        /// <summary>
        /// creates new confirmation
        /// </summary>
        /// <param name="transId">related transaction</param>
        public VendorConfirmation(Guid transId)
        {
            TransactionId = transId;
            ConfirmedByAdmin = true;
        }
    }
}
