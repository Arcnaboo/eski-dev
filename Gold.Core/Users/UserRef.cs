using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Users
{
    /// <summary>
    /// Represents an action where user referenced another user during registration
    /// </summary>
    public class UserRef
    {
        /// <summary>
        /// DB id
        /// </summary>
        public Guid UserRefId { get; set;}
        /// <summary>
        /// User who used the reference code
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Reference code that is used
        /// </summary>
        public int UsedRefKod { get; set; }

        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private UserRef() { }

        /// <summary>
        /// Creates new user ref
        /// </summary>
        /// <param name="userId">User who is using a ref code</param>
        /// <param name="usedCode">Ref code of another user</param>
        public UserRef(Guid userId, int usedCode)
        {
            UserId = userId;
            UsedRefKod = usedCode;
        }
    }
}
