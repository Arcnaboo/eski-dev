using Gold.Api.Models.KuwaitTurk;
using Gold.Core.Vendors;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Services.Interfaces
{
    public interface IExpectedCashService
    {
        void SetTimeoutMinutes(int minutes);
        void SetBuyMinutes(int minutes);
        void SetExpectedRun(bool status);
        void AddExpectedCash(VendorExpected expected);
        Task<MetalBuyResult> ClosePosition(VendorNotPosition notPosition);
    }
}
