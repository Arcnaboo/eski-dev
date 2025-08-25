using Gold.Api.Models.Vendors;
using Gold.Core.Vendors;
using Gold.Domain.Vendors;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Services.Interfaces
{
    public interface IVendorBuySellService
    {

        void SetAutomatics(Dictionary<Guid, bool> automatics);
        void SetAutomaticSells(Dictionary<Guid, bool> automatics);
        void SetAutomatic(Guid key, bool value);
        void SetAutomaticSell(Guid key, bool value);
        string GetCodeOf(VendorExpected expected);
        Task<FinaliseTransactionResult> TestRoutineBuy(ValidateFinaliseResult validFinalise, IVendorsRepository vendorsRepository, IExpectedCashService expectedCashService);
        Task<FinaliseTransactionResult2> VendorBuysFromFintagRoutine(ValidateFinaliseResult validFinalise, IVendorsRepository vendorsRepository, IExpectedCashService expectedCashService);
        Task<FinaliseTransactionResult2> VendorSellsToFintagRoutine(ValidateFinaliseResult validFinalise, IVendorsRepository vendorsRepository, ICashSenderService cashSenderService);
        
    }
}
