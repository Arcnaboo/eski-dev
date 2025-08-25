using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Transactions
{
    /// <summary>
    /// Represents a Transaction between two party
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Types of the transactions
        /// </summary>
        public static readonly string[] _TransactionTypes =  {"GOLD", "TRY", "SILVER", "TRY_FOR_SILVER" };
        /// <summary>
        /// Possible destination types
        /// </summary>
        public static readonly string[] _DestinationTypes = { "User", "VirtualPos", "VirtualPosEtkinlik", "Wedding", "Event", "GoldDayUser", "Charity", "IBAN"};
        /// <summary>
        /// Possible source types
        /// </summary>
        public static readonly string[] _SourceTypes = { "User", "Fintag", "Wedding", "Event", "UnregisteredUser" };
        /// <summary>
        /// DB Id
        /// </summary>
        public Guid TransactionId { get; set; }
        /// <summary>
        /// Transaction type
        /// </summary>
        public string TransactionType { get; set; }
        /// <summary>
        /// Source of the transaction, who sends gold or cash or silver..
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Type of the source, can be user or fintag or ...
        /// </summary>
        public string SourceType { get; set; }
        /// <summary>
        /// Type of the destination
        /// </summary>
        public string DestinationType { get; set; }
        /// <summary>
        /// Destination of the transaction
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// Any comment goes here
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// date and time of the transaction
        /// </summary>
        public DateTime TransactionDateTime { get; set; }
        /// <summary>
        ///  Amount of the transaction generallt represents grams
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Its true of tje transaction confirmed
        /// </summary>
        public bool Confirmed { get; set; }
        /// <summary>
        /// its true iff transaction is cancelled
        /// </summary>
        public bool Cancelled { get; set; }
        /// <summary>
        /// Exact gram amount of the transaction
        /// </summary>
        public decimal GramAmount { get; set; }
        /// <summary>
        /// Exact final TL amount of the transaction
        /// </summary>
        public decimal TlAmount { get; set; }
        /// <summary>
        /// Yekun value of soure
        /// </summary>
        public decimal Yekun { get; set; }
        /// <summary>
        /// Yekun value of destination
        /// </summary>
        public decimal? YekunDestination { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private Transaction() { }

        /// <summary>
        /// Creates new transaction
        /// </summary>
        /// <param name="transactionType">Type of the transaction</param>
        /// <param name="source">source id</param>
        /// <param name="sourceType">source type</param>
        /// <param name="destination">destination id</param>
        /// <param name="destType">destination type</param>
        /// <param name="amount">amount of grams or try</param>
        /// <param name="comment">comment of transaction</param>
        /// <param name="confirmed">is it confirmed</param>
        /// <param name="gramAmount">gram amount</param>
        /// <param name="tlAmount">try amount</param>
        public Transaction(string transactionType, string source, 
            string sourceType, string destination, string destType,
            decimal amount, string comment, bool confirmed, decimal gramAmount, decimal tlAmount)
        {
            if (!Utilities.ArrayContains(_TransactionTypes, transactionType))
            {
                throw new ArgumentException("Hata TransactionType: " + transactionType);
            }
            if (!Utilities.ArrayContains(_SourceTypes, sourceType))
            {
                throw new ArgumentException("Unknown SourceType: " + sourceType);
            }
            if (!Utilities.ArrayContains(_DestinationTypes, destType))
            {
                throw new ArgumentException("Unknown DestinationType: " + destType);
            }
            TransactionType = transactionType;
            Source = source;
            SourceType = sourceType;
            DestinationType = destType;
            Destination = destination;
            Amount = amount;
            Cancelled = false;
            Confirmed = confirmed;
            TransactionDateTime = DateTime.Now;
            Comment = comment;
            GramAmount = gramAmount;
            TlAmount = tlAmount;
            Yekun = 0;
            YekunDestination = 0;
        }

        /// <summary>
        /// Returns what would be the html file name of this transactio
        /// Simply adds .html at the end of transactionId
        /// </summary>
        /// <returns>html file name</returns>
        public string TransactionHtml()
        {
          
            return TransactionId.ToString() + ".html";
          
        }

        /// <summary>
        /// Confirms transaction
        /// </summary>
        public void Confirm()
        {
            if (!Confirmed)
                Confirmed = true;
        }

        /// <summary>
        /// Cancels transaction
        /// </summary>
        /// <param name="comment">Reason for cancellation</param>
        public void Cancel(string comment)
        {
            Cancelled = true;
            Comment = (Comment != null) ? Comment += ":" + comment : comment;
        }
   
    }
}
