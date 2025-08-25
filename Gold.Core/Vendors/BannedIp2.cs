using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Represents a banned Ip
    /// </summary>
    public class BannedIp2
    {
        /// <summary>
        /// DB Id
        /// </summary>
        public Guid BannedIpId { get; set; }
        /// <summary>
        /// IP address that is banned
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// Date time of the ban
        /// </summary>
        public DateTime BannedDateTime { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private BannedIp2() { }
        /// <summary>
        /// Creates new BannedIp record
        /// </summary>
        /// <param name="ip">IP to be banned</param>
        public BannedIp2(string ip)
        {
            IP = ip;
            BannedDateTime = DateTime.Now;
        }
    }
}
