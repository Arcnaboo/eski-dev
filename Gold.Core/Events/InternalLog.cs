using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Events
{
    /// <summary>
    /// InternalLog class represents a log record
    /// </summary>
    public class InternalLog
    {
        /// <summary>
        /// DB id
        /// </summary>
        public Guid LogId { get; set; }
        /// <summary>
        /// user involved
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// DateTime of the log record
        /// </summary>
        public DateTime LogDateTime { get; set; }
        /// <summary>
        /// Message of the log
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Any string data for admin use
        /// </summary>
        public string ArbitraryData { get; set; }

        /// <summary>
        /// User involved
        /// </summary>
        public virtual User2 User { get; set; }

        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private InternalLog() { }

        /// <summary>
        /// Creates new internal log
        /// </summary>
        /// <param name="userid">for this user</param>
        /// <param name="message">with this message</param>
        /// <param name="data">extra data</param>
        public InternalLog(Guid userid, string message, string data)
        {
            UserId = userid;
            Message = message;
            ArbitraryData = data;
            LogDateTime = DateTime.Now;
        }
    }
}
