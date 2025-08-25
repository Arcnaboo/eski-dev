using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Users
{
    /// <summary>
    /// Represents a Succesful login to the system
    /// </summary>
    public class Login
    {
        /// <summary>
        /// Db id
        /// </summary>
        public Guid LoginId { get; set; }
        /// <summary>
        /// User's id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Datetime of the login
        /// </summary>
        public DateTime LoginDateTime { get; set; }
        /// <summary>
        /// Datetime of the logout if the user logged out
        /// </summary>
        public DateTime? LogoutDateTime { get; set; }
        /// <summary>
        /// IP address used while login attempt into the system
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// Random string
        /// </summary>
        public string Random { get; set; }
        /// <summary>
        /// User related to login
        /// </summary>
        public virtual User User { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private Login() { }
        /// <summary>
        /// Creates new login record
        /// </summary>
        /// <param name="userid">user's id</param>
        /// <param name="ip">user's ip address</param>
        /// <param name="dateTime">datetime of the attempt</param>
        /// <param name="rand">random string</param>
        public Login(Guid userid, string ip, DateTime dateTime, string rand)
        {
            UserId = userid;
            IP = ip;
            LoginDateTime = dateTime;
            Random = rand;
        }

        /// <summary>
        /// Logs out the user
        /// </summary>
        public void LogOut()
        {
            LogoutDateTime = DateTime.Now;
        }
    }
}
