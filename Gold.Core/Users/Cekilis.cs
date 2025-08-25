using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Users
{
    /// <summary>
    /// Represents a sweepstakes
    /// </summary>
    public class Cekilis
    {
        /// <summary>
        /// DB id
        /// </summary>
        public Guid CekilisId { get; set; }
        /// <summary>
        /// Name of the sweepstakes
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Prize of the event
        /// </summary>
        public string Prize { get; set; }
        /// <summary>
        /// Final entry DateTime
        /// </summary>
        public DateTime SonKatilimTarihi { get; set; }
        /// <summary>
        /// Planned event time
        /// </summary>
        public DateTime PlanlananCekilisTarihi { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private Cekilis() { }
        /// <summary>
        /// Creates new Cekilis
        /// </summary>
        /// <param name="name">Name of the cekilis</param>
        /// <param name="prize">PRize of the cekilis</param>
        /// <param name="skt">Final entry datetime</param>
        /// <param name="pct">planned cekilis datetime</param>
        public Cekilis(string name, string prize, DateTime skt, DateTime pct)
        {
            Name = name;
            Prize = prize;
            SonKatilimTarihi = skt;
            PlanlananCekilisTarihi = pct;
        }

    }
}
