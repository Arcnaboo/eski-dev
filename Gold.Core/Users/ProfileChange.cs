using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Users
{
    /// <summary>
    /// Records a profile change activity
    /// </summary>
    public class ProfileChange
    {
        /// <summary>
        /// DB id
        /// </summary>
        public Guid ChangeId { get; set; }
        /// <summary>
        /// User's id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Type of change
        /// </summary>
        public string ChangeType { get; set; }
        /// <summary>
        /// Former value (example old password)
        /// </summary>
        public string OldValue { get; set; }
        /// <summary>
        /// Newly obtained value (example new password)
        /// </summary>
        public string NewValue { get; set; }
        /// <summary>
        /// Datetime of the change
        /// </summary>
        public DateTime ChangeDateTime { get; set; }
        /// <summary>
        /// IP address during the change
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// User who made the change
        /// </summary>
        public virtual User User { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private ProfileChange() { }
        /// <summary>
        /// Creates a new profile change record
        /// </summary>
        /// <param name="userid">User's id</param>
        /// <param name="type">Type of change</param>
        /// <param name="old">Former value</param>
        /// <param name="nw">New Value</param>
        /// <param name="ip">IP Address</param>
        public ProfileChange(Guid userid,
            string type,
            string old,
            string nw,
            string ip)
        {
            UserId = userid;
            ChangeType = type;
            OldValue = old;
            NewValue = nw;
            IP = ip;
            ChangeDateTime = DateTime.Now;
        }
    }
}
