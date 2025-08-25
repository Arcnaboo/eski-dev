using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Transactions
{
    /// <summary>
    /// Event class represents a special event
    /// </summary>
    public class Event2
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
        public decimal BalanceInGold { get; set; }

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
        public virtual User3 User { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private Event2() { }


        /// <summary>
        /// Ends the event
        /// </summary>
        /// <param name="message">If any error, error message displayed here</param>
        /// <returns></returns>
        public decimal ClaimGold(out string message)
        {
            if (GoldClaimed)
            {
                message = "Altın daha önce çekilmiş.";
                return 0;
            }
            if (DateTime.Now < EventDate)
            {
                message = "Etkinlik günü gelmemiş.";
                return 0;
            }
            if (BalanceInGold == 0)
            {
                message = "Etkinlikte altın bulunmamaktadır.";
                return 0;
            }
            var result = BalanceInGold;
            message = BalanceInGold.ToString() + "gr. altın " + User.FirstName + " " + User.FamilyName + " tarafından çekildi.";
            GoldClaimed = true;
            BalanceInGold = 0;
            return result;
        }

        /// <summary>
        /// Adds balance to event
        /// </summary>
        /// <param name="grams"></param>
        public void AddGold(decimal grams)
        {
            BalanceInGold += grams;
        }
    }
}
