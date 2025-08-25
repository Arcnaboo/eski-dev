using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Banks
{
    /// <summary>
    /// Bank Information
    /// </summary>
    public class Bank
    {
        /// <summary>
        /// GUID of a Bank
        /// </summary>
        public Guid BankId { get; set; }

        /// <summary>
        /// name of a Bank
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// Fintag IBAN at this Bank
        /// </summary>
        public string FintagIBAN { get; set; }

        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private Bank() { }

        /// <summary>
        /// Creates new Bank
        /// </summary>
        /// <param name="name">Name of the Bank</param>
        /// <param name="iban">Iban of Fintag</param>
        public Bank(string name, string iban)
        {
            BankName = name;
            FintagIBAN = iban;
        }

    }
}
