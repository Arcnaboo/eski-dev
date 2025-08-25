using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Events
{
    /// <summary>
    /// Event class represents a special event
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Database id
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Event name
        /// </summary>
        public string EventName { get; set; }

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
        public DateTime EventDate { get; set; }

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
        public string EventText { get; set; }

        /// <summary>
        /// Public event code
        /// </summary>
        public int EventCode { get; set; }

        /// <summary>
        /// User who created this event
        /// </summary>
        public virtual User2 User { get; set; }

        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private Event() { }

        /// <summary>
        /// Creates a new Event
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="eventText">Event text</param>
        /// <param name="eventDate">Event date</param>
        /// <param name="userId">Created by this user</param>
        /// <param name="eventCode">Event public code</param>
        public Event(string eventName, string eventText, DateTime eventDate, Guid userId, int eventCode)
        {
            EventName = eventName;
            EventText = eventText;
            CreatedBy = userId;
            EventDate = eventDate;
            BalanceInGold = 0.00M;
            GoldClaimed = false;
            EventCode = eventCode;
            DateCreated = DateTime.Now;
        }

    }
}
