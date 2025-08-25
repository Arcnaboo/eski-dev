using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.PanelUsers
{
    /// <summary>
    /// PaNel user class currently not in use
    /// </summary>
    public class PanelUser
    {

        /// <summary>
        /// DB ID
        /// </summary>
        public Guid PanelUserId { get; set; }
        /// <summary>
        /// Name of the user
        /// </summary>
        public string NameSurname { get; set; }
        /// <summary>
        /// User role
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// Email of the user
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Password of the user
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Phone number of the user
        /// </summary>
        public string MobileNumber { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private PanelUser() { }
        /// <summary>
        /// Creates new panel user
        /// </summary>
        /// <param name="role">Role of the user</param>
        /// <param name="nameSurname">Name of the user</param>
        /// <param name="email">email of the user</param>
        /// <param name="password">user's password</param>
        /// <param name="mobile">mobile number of the user</param>
        public PanelUser(string role, string nameSurname, string email, string password, string mobile)
        {
            Role = role;
            NameSurname = nameSurname;
            Email = email;
            Password = password;
            MobileNumber = mobile;
        }
    }
}
