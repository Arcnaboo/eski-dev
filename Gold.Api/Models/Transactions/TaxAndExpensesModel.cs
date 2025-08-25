using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Transactions
{
    public class TaxAndExpensesModel
    {
        public decimal Kar { get; set; }
        public decimal VposCommission{ get; set; }
        public decimal BankaMukaveleTax { get; set; }
        public decimal SatisKomisyon { get; set; }

    }
}
