using Gold.Api.Models.KuwaitTurk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Services.Interfaces
{
    public interface IKtApiService
    {

        Task<AccountStatusResult> GetAccStatus();
        Task<MetalBuyResult> MetalBuy(PreciousMetalsBuyParams model);
        Task<MetalSellResult> MetalSell(PreciousMetalsSellParams model);
        Task<InterBanTransferResult> KTHaveleTransfer(InterBankTransferParams model);
        Task<KTTransactionResultsModel> KTHesapHareketleri(string suffix, string queryString);
        Task<FxResultModel> KTCurrentPrices();
    }
}
