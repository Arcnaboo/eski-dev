using Gold.Core.Banks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Transactions
{
    /// <summary>
    /// Represents a user bank transfer request
    /// </summary>
    public class UserBankTransferRequest
    {
        /// <summary>
        /// DB Id
        /// </summary>
        public Guid BankTransferId { get; set; }
        /// <summary>
        /// Special code for transfer description
        /// </summary>
        public int SpecialCode { get; set; }
        /// <summary>
        /// Bank id of the bank where user needs to transfer cash
        /// </summary>
        public Guid BankId { get; set; }
        /// <summary>
        /// Code generation date time
        /// </summary>
        public DateTime CodeStartDateTime { get; set; }
        /// <summary>
        /// Transfer request related to this bank transfer
        /// </summary>
        public Guid TransferRequestId { get; set; }
        /// <summary>
        /// True iff money is received by goldtag
        /// </summary>
        public bool MoneyReceived { get; set; }
        /// <summary>
        /// User id of the user
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// User of the transaction
        /// </summary>
        public virtual User3 User { get; set; }
        /// <summary>
        /// Related transfer request
        /// </summary>
        public virtual TransferRequest TransferRequest { get; set; }
        /// <summary>
        /// Related bank
        /// </summary>
        public virtual Bank Bank { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private UserBankTransferRequest() { }

        /// <summary>
        /// Creates mew UserBankTransferRequest
        /// </summary>
        /// <param name="code">Special Code</param>
        /// <param name="bankId">Id of the bank</param>
        /// <param name="transferId">Id of the transfer</param>
        /// <param name="userId">Id of the User</param>
        public UserBankTransferRequest(
            int code,
            Guid bankId,
            Guid transferId,
            Guid userId
            )
        {
            SpecialCode = code;
            BankId = bankId;
            TransferRequestId = transferId;
            UserId = userId;
            MoneyReceived = false;
            CodeStartDateTime = DateTime.Now;
        }
    }
}
