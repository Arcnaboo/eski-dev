using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Users
{
    /// <summary>
    /// Represents a forgot password entry
    /// </summary>
    public class ForgotPassword
    {
        /// <summary>
        /// DB id
        /// </summary>
        public Guid ForgotId { get; set; }
        /// <summary>
        /// User id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Datetime of the request
        /// </summary>
        public DateTime ForgotRequestDateTime { get; set; }
        /// <summary>
        /// Special generated code
        /// </summary>
        public int GeneratedCode { get; set; }
        /// <summary>
        /// User
        /// </summary>
        public virtual User User { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private ForgotPassword() { }
        /// <summary>
        /// Creates new forgot password entry
        /// </summary>
        /// <param name="userId">for the user</param>
        /// <param name="code">code that is generated</param>
        public ForgotPassword(Guid userId, int code)
        {
            UserId = userId;
            GeneratedCode = code;
            ForgotRequestDateTime = DateTime.Now;
        }
    }
}
