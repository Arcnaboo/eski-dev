using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Class in order to store used FirstLast keys 
    /// for vendor automatic transactions
    /// </summary>
    public class UsedFirstLast
    {
        /// <summary>
        /// Db id
        /// </summary>
        public Guid FirstLastId { get; set; }
        /// <summary>
        /// First last key
        /// </summary>
        public string FirstLast { get; set; }

        private UsedFirstLast() { }

        /// <summary>
        /// Creates new first last entry
        /// </summary>
        /// <param name="firstLast"></param>
        public UsedFirstLast(string firstLast)
        {
            FirstLast = firstLast;
        }
    }
}
