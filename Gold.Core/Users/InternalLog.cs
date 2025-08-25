using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Users
{
    /// <summary>
    /// Internal Activity Logging
    /// </summary>
    public class InternalLog
    {
        /// <summary>
        /// DB ID
        /// </summary>
        public Guid LogId { get; set; }
        /// <summary>
        /// User's id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Log date time
        /// </summary>
        public DateTime LogDateTime { get; set; }
        /// <summary>
        /// Message related to log
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Any data
        /// </summary>
        public string ArbitraryData { get; set; }
        /// <summary>
        /// User related to log
        /// </summary>
        public virtual User User { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private InternalLog() { }

        /// <summary>
        /// Creates new InternalLog
        /// </summary>
        /// <param name="userid">User id</param>
        /// <param name="message">Log Message</param>
        /// <param name="data">Log Data</param>
        public InternalLog(Guid userid, string message, string data)
        {
            UserId = userid;
            Message = message;
            ArbitraryData = data;
            LogDateTime = DateTime.Now;
        }
    }
}
