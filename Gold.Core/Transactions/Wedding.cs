using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Transactions
{
    /// <summary>
    /// Wedding class represetns a wedding
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
        public virtual User3 User { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private Wedding() { }

        /// <summary>
        /// Claims the gold/silver
        /// </summary>
        /// <param name="message">Output message</param>
        /// <returns>Amount of gold/silver claimed or 0</returns>

        public decimal ClaimGold(out string message)
        {
            if (GoldClaimed)
            {
                message = "Altın daha önce çekilmiş.";
                return 0;
            }
            if (DateTime.Now < WeddingDate)
            {
                message = "Düğün günü gelmemiş.";
                return 0;
            }
            if (BalanceInGold == 0)
            {
                message = "Düğünde altın bulunmamaktadır.";
                return 0;
            }
            var result = BalanceInGold;
            message = BalanceInGold.ToString() + "gr. altın " + User.FirstName + " " + User.FamilyName + " tarafından çekildi.";
            GoldClaimed = true;
            BalanceInGold = 0;
            return result;
        }

        /// <summary>
        /// Adds grams to the event
        /// </summary>
        /// <param name="grams">Grams to be added</param>
        public void AddGold(decimal grams)
        {
            BalanceInGold += grams;
        }
    }
}
