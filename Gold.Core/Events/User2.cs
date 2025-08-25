using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Gold.Core.Events
{
    /// <summary>
    /// Represents a User
    /// </summary>
    public class User2
    {
        /// <summary>
        /// Id of the user
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Role of the user
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// First name of the user
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Family name of the user
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Email of the user
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Photo link of the user
        /// </summary>
        public string Photo { get; set; }

        /// <summary>
        /// Gold balance of the user
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Iban of the user
        /// </summary>
        public string IBAN { get; set; }

        /// <summary>
        /// User member id to be displyed in the app
        /// </summary>
        public int MemberId { get; set; }


        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private User2() { }
        
       

        /// <summary>
        /// Changes the Balance of the user
        /// </summary>
        /// <param name="value">Can be either negtive or positive value</param>
        public void ManipulateBalance(decimal value)
        {
            Balance += value;
        }
       
    }
}
