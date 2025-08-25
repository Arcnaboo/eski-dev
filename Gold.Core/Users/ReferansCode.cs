using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Users
{
    /// <summary>
    /// Represents a special reference code for a User
    /// </summary>
    public class ReferansCode
    {
        /// <summary>
        /// DB Id
        /// </summary>
        public Guid RefCodeId { get; set; }
        /// <summary>
        /// User's id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Reference Code of the user
        /// </summary>
        public int ReferansKod { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private ReferansCode() { }
        /// <summary>
        /// Creates new ReferanceCode
        /// </summary>
        /// <param name="userId">user's id</param>
        /// <param name="code">reference code</param>
        public ReferansCode(Guid userId, int code)
        {
            UserId = userId;
            ReferansKod = code;
        }

    }
}
