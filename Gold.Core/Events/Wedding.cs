using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Events
{
    /// <summary>
    /// Wedding class represents a special event
    /// </summary>
    public class Wedding
    {
        /// <summary>
        /// Database id
        /// </summary>
        public Guid WeddingId { get; set; }
        /// <summary>
        /// Event name
        /// </summary>
        public string WeddingName { get; set; }
        /// <summary>
        /// Event creation datetime
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// User id who created this event
        /// </summary>
        public Guid CreatedBy { get; set; }
        /// <summary>
        /// Event date time
        /// </summary>
        public DateTime WeddingDate { get; set; }
        /// <summary>
        /// How much gold collected in the event
        /// </summary>
        public decimal  BalanceInGold { get; set; }
        /// <summary>
        /// Its true if event is complete
        /// </summary>
        public bool GoldClaimed { get; set; }
        /// <summary>
        /// Event descriptive text
        /// </summary>
        public string WeddingText { get; set; }
        /// <summary>
        /// Public event code
        /// </summary>
        public int WeddingCode { get; set; }
        /// <summary>
        /// User who created this event
        /// </summary>
        public virtual User2 User { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private Wedding() { }
        /// <summary>
        /// Creates a new Wedding
        /// </summary>
        /// <param name="weddingName">Event name</param>
        /// <param name="weddingText">Event text</param>
        /// <param name="weddingDate">Event date</param>
        /// <param name="userId">Created by this user</param>
        /// <param name="weddingCode">Event public code</param>
        public Wedding(string weddingName, string weddingText, DateTime weddingDate, Guid userId, int weddingCode)
        {
            
            WeddingName = weddingName;
            WeddingText = weddingText;
            CreatedBy = userId;
            WeddingDate = weddingDate;
            BalanceInGold = 0.00M;
            GoldClaimed = false;
            WeddingCode = weddingCode;
            DateCreated = DateTime.Now;


        }

        /// <summary>
        /// Updates wedding text
        /// </summary>
        /// <param name="text">Text to be displayed</param>
        public void UpdateWeddingText(string text)
        {
            WeddingText = text;
        }


    }
}
