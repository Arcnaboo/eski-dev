using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Users
{
    /// <summary>
    /// Represents a single or many entry for a specific cekilis
    /// </summary>
    public class CekilisHak
    {
        /// <summary>
        /// DB id
        /// </summary>
        public Guid HakId { get; set; }
        /// <summary>
        /// Cekilis id
        /// </summary>
        public Guid CekilisId { get; set; }
        /// <summary>
        /// User id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Amount of entries
        /// </summary>
        public int CekilisHakki { get; set; }
        /// <summary>
        /// Last date time when user received new entry
        /// </summary>
        public DateTime SonHakAlisTarihi { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private CekilisHak() { }
        /// <summary>
        /// creates new CEkilis hak for a given cekilis and user
        /// </summary>
        /// <param name="userId">User to be added for entry</param>
        /// <param name="cekId">Cekilis to be entered</param>
        public CekilisHak(Guid userId, Guid cekId)
        {
            CekilisId = cekId;
            UserId = userId;
            CekilisHakki = 0;
            SonHakAlisTarihi = DateTime.Now.AddDays(-2);
        }
    }
}
