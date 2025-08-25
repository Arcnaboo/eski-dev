using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gold.Core.Banks;
using Gold.Core.GoldPrices;
using Gold.Core.Transactions;
using Gold.Core.Vendors;
using Gold.Domain.Transactions.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Gold.Domain.Transactions.Repositories
{
    public class TransactionsRepository : ITransactionsRepository
    {

        private readonly TransactionDbContext Context;

        public TransactionsRepository()
        {
            Context = new TransactionDbContext();
        }

        Wedding ITransactionsRepository.GetWedding(string weddingId)
        {
            var wedding = Context
                .Weddings
                .Where(x => x.WeddingId.ToString() == weddingId)
                .Include(x => x.User)
                .FirstOrDefault();

            return wedding;
        }

        void ITransactionsRepository.AddNotification(Notification2 notification)
        {
            Context.Notifications.Add(notification);
        }

        void ITransactionsRepository.AddTransaction(Transaction transaction)
        {
            Context.Transactions.Add(transaction);
        }

        IQueryable<Transaction> ITransactionsRepository.GetAllTransactions()
        {
            return Context.Transactions;
        }

        IQueryable<User3> ITransactionsRepository.GetAllUsers()
        {
            return Context.Users;
        }

        IQueryable<Transaction> ITransactionsRepository.GetGoldDayTransactions(Guid goldDayId)
        {
            //var query = Context.Transactions.Where(x => x.TransactionType == "8F2A9FCA-6292-41A1-8D84-81EF5450132C" && x.Destination == goldDayId);
            throw new NotImplementedException("Gold day system revised");
            //return query;
        }

        IQueryable<Transaction> ITransactionsRepository.GetSpecialEventTransactions(Guid eventId)
        {
            var query = Context.Transactions.Where(x => x.DestinationType.ToString() == "Event" && x.Destination == eventId.ToString());

            return query;
        }

        IQueryable<Transaction> ITransactionsRepository.GetUserTransactions(Guid userId)
        {
            var query = Context.Transactions.Where(x => x.Confirmed && x.TransactionType == "GOLD" && (x.Destination == userId.ToString() || x.Source == userId.ToString()));

            return query; ;
        }

        IQueryable<Transaction> ITransactionsRepository.GetWeddingTransactions(Guid weedingId)
        {
            var query = Context.Transactions.Where(x => x.DestinationType.ToString() == "Wedding" && x.Destination == weedingId.ToString());

            return query;
        }

        void ITransactionsRepository.SaveChanges()
        {
            Context.SaveChanges();
        }

        void ITransactionsRepository.AddTransferRequest(TransferRequest transferRequest)
        {
            Context.TransferRequests.Add(transferRequest);
        }

        IQueryable<TransferRequest> ITransactionsRepository.GetAllTransferRequests()
        {
            return Context.TransferRequests;
        }

        Event2 ITransactionsRepository.GetEvent(string eventId)
        {
            var even = Context
                .Events
                .Where(x => x.EventId.ToString() == eventId)
                .Include(x => x.User)
                .FirstOrDefault();

            return even;
        }

        GoldPrice ITransactionsRepository.AccessBuyPrice()
        {
            return Context.GoldPrices.Where(c => c.Name == "Buy").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessSellPrice()
        {
            return Context.GoldPrices.Where(c => c.Name == "Sell").FirstOrDefault();
        }

        IQueryable<Bank> ITransactionsRepository.GetAllBanks()
        {
            return Context.Banks;
        }

        IQueryable<UserBankTransferRequest> ITransactionsRepository.GetAllUserBankTransferRequests()
        {
            return Context.UserBankTransferRequests;
        }

        void ITransactionsRepository.AddUserBankTansferRequest(UserBankTransferRequest userBankTransfer)
        {
            Context.UserBankTransferRequests.Add(userBankTransfer);
        }

        IQueryable<Event2> ITransactionsRepository.GetAllEvents()
        {
            return Context.Events;
        }

        IQueryable<Wedding> ITransactionsRepository.GetAllWeddings()
        {
            return Context.Weddings;
        }

        void ITransactionsRepository.AddLog(InternalLog log)
        {
            Context.Logs.Add(log);
        }

        GoldPrice ITransactionsRepository.AccessKar()
        {
            return Context.GoldPrices.Where(c => c.Name == "Kar").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessVposCommission()
        {
            return Context.GoldPrices.Where(c => c.Name == "SanalPosKomisyon").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessBankTax()
        {
            return Context.GoldPrices.Where(c => c.Name == "BankaMuaveleVergisi").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessSatisKomisyon()
        {
            return Context.GoldPrices.Where(c => c.Name == "SatisKomisyon").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessKtBuy()
        {
            return Context.GoldPrices.Where(c => c.Name == "KT_BUY").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessKtSell()
        {
            return Context.GoldPrices.Where(c => c.Name == "KT_SELL").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessBuyPercentage()
        {
            return Context.GoldPrices.Where(c => c.Name == "BUY_PERCENTAGE").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessSellPercentage()
        {
            return Context.GoldPrices.Where(c => c.Name == "SELL_PERCENTAGE").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessAutomatic()
        {
            return Context.GoldPrices.Where(c => c.Name == "automatic").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessSilverBuy()
        {
            return Context.GoldPrices.Where(c => c.Name == "SilverBuy").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessSilverSell()
        {
            return Context.GoldPrices.Where(c => c.Name == "SilverSell").FirstOrDefault();
        }

        void ITransactionsRepository.RemoveUserBankTransferRequest(UserBankTransferRequest userBankTransferRequest)
        {
            Context.UserBankTransferRequests.Remove(userBankTransferRequest);
        }

        void ITransactionsRepository.RemoveTransferRequest(TransferRequest request)
        {
            Context.TransferRequests.Remove(request);
        }

        void ITransactionsRepository.RemoveTransaction(Transaction transaction)
        {
            Context.Transactions.Remove(transaction);
        }

        void ITransactionsRepository.AddVendor(Vendor vendor)
        {
            Context.Vendors.Add(vendor);
        }

        IQueryable<Vendor> ITransactionsRepository.GetAllVendors()
        {
            return Context.Vendors;
        }

        void ITransactionsRepository.AddGCode(Gcode gcode)
        {
            Context.Gcodes.Add(gcode);
        }

        IQueryable<Gcode> ITransactionsRepository.GetAllGCodes()
        {
            return Context.Gcodes;
        }

        void ITransactionsRepository.AddSilverBalance(SilverBalance silverBalance)
        {
            Context.SilverBalances.Add(silverBalance);
        }

        SilverBalance ITransactionsRepository.GetSilverBalance(Guid userId)
        {
            return Context.SilverBalances.Where(x => x.UserId == userId).FirstOrDefault();
        }

        UserLevel ITransactionsRepository.GetUserLevel(Guid userId)
        {
            return Context.UserLevels.Where(x => x.UserId == userId).FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.GetRobotStatus()
        {
            return Context.GoldPrices.Where(c => c.Name == "expected_run").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessSellPercentageSilver()
        {
            return Context.GoldPrices.Where(x => x.Name == "SELL_PERCENTAGE_SILVER").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessBuyPercentageSilver()
        {
            return Context.GoldPrices.Where(x => x.Name == "BUY_PERCENTAGE_SILVER").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessKtSilverSell()
        {
            return Context.GoldPrices.Where(x => x.Name == "kt_sell_silver").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessKtSilverBuy()
        {
            return Context.GoldPrices.Where(x => x.Name == "kt_buy_silver").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessPlatinBuy()
        {
            return Context.GoldPrices.Where(x => x.Name == "plt_buy").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessPlatinSell()
        {
            return Context.GoldPrices.Where(x => x.Name == "plt_sell").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessKtPlatinBuy()
        {
            return Context.GoldPrices.Where(x => x.Name == "kt_plt_buy").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessKtPlatinSell()
        {
            return Context.GoldPrices.Where(x => x.Name == "kt_plt_sell").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessSellPercentagePlatin()
        {
            return Context.GoldPrices.Where(x => x.Name == "SELL_PERCENTAGE_PLATIN").FirstOrDefault();
        }

        GoldPrice ITransactionsRepository.AccessBuyPercentagePlatin()
        {
            return Context.GoldPrices.Where(x => x.Name == "BUY_PERCENTAGE_PLATIN").FirstOrDefault();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessBuyPriceAsync()
        {
            return await Context.GoldPrices.Where(x => x.Name == "Buy").FirstOrDefaultAsync();
            
        }

        async Task<GoldPrice> ITransactionsRepository.AccessSellPriceAsync()
        {
            return await Context.GoldPrices.Where(x => x.Name == "Sell").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessKarAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "Kar").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessVposCommissionAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "SanalPosKomisyon").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessBankTaxAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "BankaMuaveleVergisi").FirstOrDefaultAsync();
            
        }

        async Task<GoldPrice> ITransactionsRepository.AccessSatisKomisyonAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "SatisKomisyon").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessSilverBuyAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "SilverBuy").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessSilverSellAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "SilverSell").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessPlatinBuyAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "plt_buy").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessPlatinSellAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "plt_sell").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessKtBuyAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "KT_BUY").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessKtSellAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "KT_SELL").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessKtSilverBuyAsync()
        {
            return await Context.GoldPrices.Where(x => x.Name == "kt_buy_silver").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessKtSilverSellASync()
        {
            return await Context.GoldPrices.Where(x => x.Name == "kt_sell_silver").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessKtPlatinBuyAsync()
        {
            return await Context.GoldPrices.Where(x => x.Name == "kt_plt_buy").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessKtPlatinSellAsync()
        {
            return await Context.GoldPrices.Where(x => x.Name == "kt_plt_sell").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessBuyPercentageAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "BUY_PERCENTAGE").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessSellPercentageAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "SELL_PERCENTAGE").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessSellPercentageSilverAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "SELL_PERCENTAGE_SILVER").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessBuyPercentageSilverAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "BUY_PERCENTAGE_SILVER").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessSellPercentagePlatinAsync()
        {
            return await Context.GoldPrices.Where(x => x.Name == "SELL_PERCENTAGE_PLATIN").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessBuyPercentagePlatinAsync()
        {
            return await Context.GoldPrices.Where(x => x.Name == "BUY_PERCENTAGE_PLATIN").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> ITransactionsRepository.AccessAutomaticAsync()
        {
            return await Context.GoldPrices.Where(c => c.Name == "automatic").FirstOrDefaultAsync();
        }

        async Task<int> ITransactionsRepository.SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

        /*
        void ITransactionsRepository.AddGoldtagExpected(GoldtagExpected goldtagExpected)
        {
            Context.GoldtagExpecteds.Add(goldtagExpected);
        }

        void ITransactionsRepository.AddGoldtagUnexpected(GoldtagUnexpected goldtagUnexpected)
        {
            Context.GoldtagUnexpecteds.Add(goldtagUnexpected);
        }

        void ITransactionsRepository.AddGoldtagFinalized(GoldtagFinalized goldtagFinalized)
        {
            Context.GoldtagFinalizeds.Add(goldtagFinalized);
        }

        void ITransactionsRepository.AddGoldtagNotPosition(GoldtagNotPosition goldtagNotPosition) 
        {
            Context.GoldtagNotPositions.Add(goldtagNotPosition);
        }

        void ITransactionsRepository.AddGoldtagNotReceived(GoldtagNotReceived goldtagNotReceived)
        {
            Context.GoldtagNotReceiveds.Add(goldtagNotReceived);
        }

        IQueryable<GoldtagExpected> ITransactionsRepository.GetGoldtagExpecteds()
        {
            return Context.GoldtagExpecteds;
        }

        IQueryable<GoldtagUnexpected> ITransactionsRepository.GetGoldtagUnexpecteds()
        {
            return Context.GoldtagUnexpecteds;
        }

        IQueryable<GoldtagFinalized> ITransactionsRepository.GetGoldtagFinalizeds()
        {
            return Context.GoldtagFinalizeds;
        }

        IQueryable<GoldtagNotPosition> ITransactionsRepository.GetGoldtagNotPositions()
        {
            return Context.GoldtagNotPositions;
        }

        IQueryable<GoldtagNotReceived> ITransactionsRepository.GetGoldtagNotReceiveds()
        {
            return Context.GoldtagNotReceiveds;
        }

        void ITransactionsRepository.RemoveGoldtagExpected(GoldtagExpected goldtagExpected)
        {
            Context.GoldtagExpecteds.Remove(goldtagExpected);
        }

        bool ITransactionsRepository.ShouldGoldtagExpectedRobotRun()
        {
            var expectedRun = Context.GoldPrices.Where(x => x.Name == "goldtag_expected_run").FirstOrDefault();

            if (expectedRun == null)
            {
                return false;
            }

            return expectedRun.Amount.HasValue && expectedRun.Amount == 1.00m;
        }*/
    }
}
