using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Gold.Core.Transactions
{    
    /// <summary>
    /// Represents a User
    /// </summary>
    public class User3
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
        /// Phone of the user
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// User Gold balance
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// IBAN of the user
        /// </summary>
        public string IBAN { get; set; }
        /// <summary>
        /// User given member id for app
        /// </summary>
        public int MemberId { get; set; }
        /// <summary>
        /// True iff user is verified to lvl 1
        /// </summary>
        public bool Verified { get; set; }
        /// <summary>
        /// Date created
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// Blocked grams
        /// </summary>
        public decimal? BlockedGrams { get; set; }

        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private User3() { }

        
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
