using Gold.Core.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Services.Interfaces
{
    public interface ICashSenderService
    {
        void SetSellMinutes(int minutes);
        void SetSenderRun(bool status);
        void AddCashPayment(VendorCashPayment payment);
        Task<object> DoWorkSilver(List<VendorCashPayment> payments);
        Task<object> DoWorkGold(List<VendorCashPayment> payments);
    }
}
