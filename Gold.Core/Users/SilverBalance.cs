using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Users
{
    /// <summary>
    /// Stores user's silver balance
    /// </summary>
    public class SilverBalance
    {
        /// <summary>
        /// Database unique id
        /// </summary>
        public Guid SilverId { get; set; }
        /// <summary>
        /// User id of the user
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Silver balance
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// Blocked gra,s
        /// </summary>
        public decimal? BlockedGrams { get; set; }
        /// <summary>
        /// Arbitrary comments
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private SilverBalance() { }
        /// <summary>
        /// Creates new silverbalance for given user
        /// </summary>
        /// <param name="userId"></param>
        public SilverBalance(Guid userId)
        {
            UserId = userId;
            Balance = 0;
        }
    }
}
