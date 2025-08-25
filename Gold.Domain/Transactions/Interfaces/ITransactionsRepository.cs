using System;
using System.Collections.Generic;
using System.Text;
using Gold.Core.Transactions;
using System.Linq;
using Gold.Core.GoldPrices;
using Gold.Core.Banks;
using Gold.Core.Vendors;
using System.Threading.Tasks;

namespace Gold.Domain.Transactions.Interfaces
{
    /// <summary>
    /// Donation
    /// CustomEvent
    /// QRWithdrawal
    /// Wedding
    /// VirtualPost
    /// GoldDay
    /// BankWithdrawal
    /// Internal
    /// </summary>
    public interface ITransactionsRepository
    {
        void AddSilverBalance(SilverBalance silverBalance);
        SilverBalance GetSilverBalance(Guid userId);
        void AddVendor(Vendor vendor);
        IQueryable<Vendor> GetAllVendors();
        void AddGCode(Gcode gcode);
        IQueryable<Gcode> GetAllGCodes();
        void AddLog(InternalLog log);
        IQueryable<Bank> GetAllBanks();
        IQueryable<UserBankTransferRequest> GetAllUserBankTransferRequests();
        void RemoveUserBankTransferRequest(UserBankTransferRequest userBankTransferRequest);
        void RemoveTransferRequest(TransferRequest request);
        void RemoveTransaction(Transaction transaction);
        GoldPrice AccessKtBuy();
        GoldPrice AccessKtSell();
        GoldPrice AccessBuyPrice();
        GoldPrice AccessSellPrice();
        GoldPrice AccessBuyPercentage();
        GoldPrice AccessSellPercentage();
        GoldPrice AccessSellPercentageSilver();
        GoldPrice AccessBuyPercentageSilver();
        GoldPrice AccessSellPercentagePlatin();
        GoldPrice AccessBuyPercentagePlatin();
        Task<GoldPrice> AccessKtBuyAsync();
        Task<GoldPrice> AccessKtSellAsync();
        Task<GoldPrice> AccessBuyPriceAsync();
        Task<GoldPrice> AccessSellPriceAsync();
        Task<GoldPrice> AccessBuyPercentageAsync();
        Task<GoldPrice> AccessSellPercentageAsync();
        Task<GoldPrice> AccessSellPercentageSilverAsync();
        Task<GoldPrice> AccessBuyPercentageSilverAsync();
        Task<GoldPrice> AccessSellPercentagePlatinAsync();
        Task<GoldPrice> AccessBuyPercentagePlatinAsync();
        Task<GoldPrice> AccessAutomaticAsync();
        Task<int> SaveChangesAsync();


        GoldPrice AccessAutomatic();
        GoldPrice AccessKar();
        GoldPrice AccessVposCommission();
        GoldPrice AccessBankTax();
        GoldPrice AccessSatisKomisyon();

        Task<GoldPrice> AccessKarAsync();
        Task<GoldPrice> AccessVposCommissionAsync();
        Task<GoldPrice> AccessBankTaxAsync();
        Task<GoldPrice> AccessSatisKomisyonAsync();

        GoldPrice AccessSilverBuy();
        Task<GoldPrice> AccessSilverBuyAsync();
        GoldPrice AccessSilverSell();
        Task<GoldPrice> AccessSilverSellAsync();
        GoldPrice AccessKtSilverBuy();
        GoldPrice AccessKtSilverSell();
        Task<GoldPrice> AccessKtSilverBuyAsync();
        Task<GoldPrice> AccessKtSilverSellASync();
        //
        GoldPrice AccessPlatinBuy();
        Task<GoldPrice> AccessPlatinBuyAsync();
        GoldPrice AccessPlatinSell();
        Task<GoldPrice> AccessPlatinSellAsync();
        GoldPrice AccessKtPlatinBuy();
        GoldPrice AccessKtPlatinSell();
        Task<GoldPrice> AccessKtPlatinBuyAsync();
        Task<GoldPrice> AccessKtPlatinSellAsync();
        GoldPrice GetRobotStatus();



        // buy sell active disable enable
        //bool IsBuySystemActive();
        //bool IsSellSystemActive();

        

        void AddTransferRequest(TransferRequest transferRequest);
        void AddUserBankTansferRequest(UserBankTransferRequest userBankTransfer);
        IQueryable<TransferRequest> GetAllTransferRequests();
        void AddTransaction(Transaction transaction);
        void AddNotification(Notification2 notification);
        Wedding GetWedding(string weddingId);
        Event2 GetEvent(string eventId);
        IQueryable<Transaction> GetAllTransactions();
        IQueryable<User3> GetAllUsers();
        IQueryable<Event2> GetAllEvents();
        IQueryable<Wedding> GetAllWeddings();
        void SaveChanges();
        IQueryable<Transaction> GetUserTransactions(Guid userId);
        IQueryable<Transaction> GetWeddingTransactions(Guid weedingId);
        IQueryable<Transaction> GetGoldDayTransactions(Guid goldDayId);
        IQueryable<Transaction> GetSpecialEventTransactions(Guid eventId);
        UserLevel GetUserLevel(Guid userId);

        // for robot auto system
       /* void AddGoldtagExpected(GoldtagExpected goldtagExpected);
        void AddGoldtagUnexpected(GoldtagUnexpected goldtagUnexpected);
        void AddGoldtagFinalized(GoldtagFinalized goldtagFinalized);
        void AddGoldtagNotPosition(GoldtagNotPosition goldtagNotPosition);
        void AddGoldtagNotReceived(GoldtagNotReceived goldtagNotReceived);
        IQueryable<GoldtagExpected> GetGoldtagExpecteds();
        IQueryable<GoldtagUnexpected> GetGoldtagUnexpecteds();
        IQueryable<GoldtagFinalized> GetGoldtagFinalizeds();
        IQueryable<GoldtagNotPosition> GetGoldtagNotPositions();
        IQueryable<GoldtagNotReceived> GetGoldtagNotReceiveds();

        void RemoveGoldtagExpected(GoldtagExpected goldtagExpected);

        bool ShouldGoldtagExpectedRobotRun();*/
    }
}
