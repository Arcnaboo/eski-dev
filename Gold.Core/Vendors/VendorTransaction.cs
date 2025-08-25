using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Depreciated vendor transaction class
    /// </summary>
    public class VendorTransaction
    {
        /// <summary>
        /// db id
        /// </summary>
        public Guid TransactionId { get; set; }
        /// <summary>
        /// given by vendor 
        /// </summary>
        public string VendorReferenceId { get; set; }
        /// <summary>
        /// source of transaction (who is selling gold or silver or..)
        /// </summary>
        public Guid Source { get; set; }
        /// <summary>
        /// destination of transaction (who is buying gold or silver or ..)
        /// </summary>
        public Guid Destination { get; set; }
        /// <summary>
        /// true iff vendor confirms
        /// </summary>
        public bool ConfirmedByVendor { get; set; }
        /// <summary>
        /// true if vendor confirms and no error
        /// </summary>
        public bool ConfirmedByGoldtag { get; set; }
        /// <summary>
        /// date time of transaction
        /// </summary>
        public DateTime TransactionDateTime { get; set; }
        /// <summary>
        /// datetime when vendor confirmed
        /// </summary>
        public DateTime? VendorConfirmedDateTime { get; set; }
        /// <summary>
        /// datetime when goldtag confirmed
        /// </summary>
        public DateTime? GoldtagConfirmedDateTime { get; set; }
        /// <summary>
        /// datetime when transaction finalized
        /// </summary>
        public DateTime? TransactionFinalisedDateTime { get; set; }
        /// <summary>
        /// true iff transaciton cancelled
        /// </summary>
        public bool Cancelled { get; set; }
        /// <summary>
        /// true iff transaction success
        /// </summary>
        public bool Succesful { get; set; }
        /// <summary>
        /// grams to be exchanged
        /// </summary>
        public decimal GramAmount { get; set; }
        /// <summary>
        /// tl price of the grams exchanged
        /// </summary>
        public decimal TlAmount { get; set; }
        /// <summary>
        /// useful comments
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// depreciated
        /// </summary>
        public static int? IdNumber { get; set; }

        /// <summary>
        /// for random seed
        /// </summary>
        private static readonly DateTime ArcDate = DateTime.ParseExact("1983-09-14", "yyyy-MM-dd", CultureInfo.InvariantCulture);

        /// <summary>
        /// depreciated
        /// </summary>
        public static void InitiateTheDayOrFirstStart()
        {
            if (!IdNumber.HasValue)
            {
                IdNumber = 1;
            }

        }

        /// <summary>
        /// depreciated
        /// creates unique id
        /// </summary>
        /// <returns>returns guid</returns>
        public static Guid GetId()
        {
            var timeDiff = DateTime.Now - ArcDate;

            var subid = timeDiff.TotalSeconds * IdNumber.Value * timeDiff.TotalMilliseconds;
            var val = IdNumber.Value;

            return Guid.NewGuid();
        }

        private VendorTransaction() { }

        /// <summary>
        /// depreciated
        /// creates new transaction
        /// </summary>
        /// <param name="refId">reference</param>
        /// <param name="source">source</param>
        /// <param name="dest">dest</param>
        /// <param name="grams">grams</param>
        /// <param name="price">price</param>
        /// <param name="comment">comment</param>
        public VendorTransaction(string refId, Guid source, Guid dest, decimal grams, decimal price, string comment)
        {
            VendorReferenceId = refId;
            Source = source;
            Destination = dest;
            ConfirmedByVendor = false;
            ConfirmedByGoldtag = false;
            TransactionDateTime = DateTime.Now;
            Cancelled = false;
            Succesful = false;
            GramAmount = grams;
            TlAmount = price;
            Comment = comment;
        }

        /// <summary>
        /// vendor confirms
        /// </summary>
        public void VendorConfirmed()
        {
            ConfirmedByVendor = true;
            VendorConfirmedDateTime = DateTime.Now;
        }

        /// <summary>
        /// goldtag confirms
        /// </summary>
        public void GTagConfirmed()
        {
            ConfirmedByGoldtag = true;
            GoldtagConfirmedDateTime = DateTime.Now;
        }

        /// <summary>
        /// cancel the transaction
        /// </summary>
        /// <param name="reason">what is the reason</param>
        public void CancelTransaction(string reason)
        {
            Cancelled = true;
            if (reason != null)Comment += ":" + reason;
            TransactionFinalisedDateTime = DateTime.Now;
        }

        /// <summary>
        /// completes transaction
        /// </summary>
        /// <param name="message">useful comment</param>
        public void CompleteTransaction(string message)
        {
            Succesful = true;
            TransactionFinalisedDateTime = DateTime.Now;
            if (message != null) Comment += ":" + message;
        }


    }
}
