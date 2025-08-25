using Gold.Core.Banks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Transactions
{
    public class BankModel
    {
        [JsonProperty("bankid")]
        public string BankId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("IBAN")]
        public string IBAN { get; set; }

        
        public static BankModel ParseBankAsModel(Bank bank)
        {
            var res = new BankModel { BankId = bank.BankId.ToString(), IBAN = bank.FintagIBAN, Name = bank.BankName };
            return res;
        }
    }
}
