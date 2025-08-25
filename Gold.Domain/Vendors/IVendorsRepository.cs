using Gold.Core.GoldPrices;
using Gold.Core.Vendors;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gold.Domain.Vendors
{
    public interface IVendorsRepository
    {
        void SaveChanges();
        /*Normal add remove */

        void AddVendor(Vendor vendor);
        void AddVendorData(VendorData vendorData);
        void AddVendorPlatinBalance(VendorPlatinBalance vendorPlatinBalance);
        void AddVendorTransaction(VendorTransaction vendorTransaction);
        void AddBannedIp(BannedIp2 bannedIp);
        void AddGcode(Gcode gcode);
        void AddConfirmation(VendorConfirmation confirmation);
        void AddExpectedCash(ExpectedCash expectedCash);
        void AddNotPosGoldSell(NotPosGoldSell notPosGoldSell);
        void AddIBankError(InterBankError bankError);
        void AddUnexpectedCash(UnexpectedCash unexpectedCash);
        void AddFinalizedGold(FinalizedGold finalizedGold);
        void RemoveExpected(ExpectedCash expectedCash);

        /*async add remove */
        void AddVendorAsync(Vendor vendor);
        Task<EntityEntry<VendorExpected>> AddVendorExpected(VendorExpected vendorExpected);
        Task<EntityEntry<VendorUnExpected>> AddVendorUnExpected(VendorUnExpected vendorUnExpected);
        Task<EntityEntry<NotPosGold>> AddNotPosGold(NotPosGold notPosGold);
        Task<VendorExpected> GetVendorExpected(string vendorRef);
        Task<EntityEntry<VendorData>> AddVendorDataAsync(VendorData vendorData);
        Task<EntityEntry<VendorNotPosition>> AddVendorNotPosition(VendorNotPosition notPosition);
        Task<EntityEntry<VendorNotPositionSell>> AddVendorNotPositionSell(VendorNotPositionSell vendorNotPositionSell);
        Task<EntityEntry<VendorFinalized>> AddVendorFinalized(VendorFinalized vendorFinalized);
        Task<EntityEntry<VendorTransaction>> AddVendorTransactionAsync(VendorTransaction vendorTransaction);
        Task<EntityEntry<VendorTransactionNew>> AddVendorTransactionNewAsync(VendorTransactionNew vendorTransaction);
        Task<EntityEntry<VenTransaction>> AddVenTransactionAsync(VenTransaction vendorTransaction);
        Task<EntityEntry<VendorExpected>> RemoveVendorExpected(VendorExpected vendorExpected);
        Task RemoveVendorExpecteds(List<VendorExpected> expecteds);
        Task<EntityEntry<UsedFirstLast>> AddFirstLast(UsedFirstLast usedFirstLast);
        Task<EntityEntry<VendorCashPayment>> RemoveCashPayment(VendorCashPayment cashPayment);
        Task<EntityEntry<VendorCashPayment>> AddVendorCashPayment(VendorCashPayment cashPayment);
        void AddVendorPlatinBalanceAsync(VendorPlatinBalance vendorPlatinBalance);
        void AddBannedIpAsync(BannedIp2 bannedIp);
        void AddGcodeAsync(Gcode gcode);
        void AddConfirmationAsync(VendorConfirmation confirmation);
        void AddExpectedCashAsync(ExpectedCash expectedCash);
        void AddNotPosGoldAsync(NotPosGold notPosGold);
        void AddNotPosGoldSellAsync(NotPosGoldSell notPosGoldSell);
        void AddIBankErrorAsync(InterBankError bankError);
        void AddUnexpectedCashAsync(UnexpectedCash unexpectedCash);
        void AddFinalizedGoldAsync(FinalizedGold finalizedGold);
        void RemoveExpectedAsync(ExpectedCash expectedCash);
        

        /*Normal get methods */

        IQueryable<InterBankError> GetInterBankErrors();
        IQueryable<NotPosGoldSell> GetNotPosGoldSells();
        IQueryable<NotPosGold> GetNotPosGolds();
        IQueryable<UnexpectedCash> GetUnexpectedCashes();
        IQueryable<VendorExpected> GetVendorExpecteds();
        IQueryable<VendorCashPayment> GetVendorCashPayments();
        IQueryable<VendorNotPosition> GetVendorNotPositions();
        IQueryable<VendorNotPositionSell> GetVendorNotPositionSells();
        IQueryable<ExpectedCash> GetExpectedCashes();
        IQueryable<FinalizedGold> GetFinalizedGolds();
        IQueryable<VendorFinalized> GetVendorFinalizeds();
        IQueryable<VendorConfirmation> GetVendorConfirmations();
        IQueryable<VendorConfirmationNew> GetVendorConfirmationsNew();
        IQueryable<Gcode> GetGcodes();
        IQueryable<Vendor> GetVendors();
        IQueryable<VendorData> GetVendorDatas();
        IQueryable<BannedIp2> GetBannedIps();
        IQueryable<VendorTransaction> GetVendorTransactions();
        IQueryable<VendorTransactionNew> GetVendorTransactionsNew();
        IQueryable<VenTransaction> GetVenTransactions();
        IAsyncEnumerable<VenTransaction> GetVenTransactions(int first, int last);
        IQueryable<VendorUnExpected> GetVendorUnExpecteds();
        Task<IAsyncEnumerable<VendorTransactionNew>> GetVendorTransactionsNewAsync();
        IQueryable<VendorPlatinBalance> GetVendorPlatinBalances();
        VendorPlatinBalance GetVendorPlatinBalance(Guid vendorId);
        Task<VendorPlatinBalance> GetVendorPlatinBalanceStringAsync(string vendorId);
        GoldPrice GetRobotStatus();

        bool ExpectedServiceRun();
        Task<bool> ExpectedServiceRunAsync();
        Task<bool> IsFinalized(int transactionId);
        bool PaymentServiceRun();
        Task<bool> PaymentServiceRunAsync();
        Task<bool> ShouldSenderRun();
        Task<int> GetPaycellMinutesBuy();
        Task SetPaycellMinutesBuy(int minutes);
        Task<int> GetPaycellMinutesSell();
        Task SetPaycellMinutesSell(int minutes);
        Task<int> GetExpectedTimeoutMinutes();
        Task SetExpectedTimeoutMinutes(int minutes);

        VendorTransaction GetVendorTransaction(Guid transId);
        VendorTransaction GetVendorTransaction(string refId);

        VendorTransactionNew GetVendorTransactionNew(int transId);
        VendorTransactionNew GetVendorTransactionNew(string refId);

        VenTransaction GetVenTransaction(int transId);
        VenTransaction GetVenTransaction(string refId);

        /*async get mothods*/
        Task<List<VendorExpected>> GetVendorExpectedsAsync();
        IAsyncEnumerable<VendorCashPayment> GetVendorCashPaymentsAsyncEnumerable();
        IAsyncEnumerable<VendorNotPosition> GetVendorNotPositionsAsyncEnum();
        IAsyncEnumerable<VendorNotPosition> GetVendorNotPositionsAsyncEnum(List<string> ids);
        IAsyncEnumerable<VendorNotPositionSell> GetVendorNotPositionSellsAsyncEnum();
        IAsyncEnumerable<VendorNotPositionSell> GetVendorNotPositionSellsAsyncEnum(List<string> ids);
        Task<IQueryable<InterBankError>> GetInterBankErrorsAsync();
        Task<IQueryable<NotPosGoldSell>> GetNotPosGoldSellsAsync();
        Task<IQueryable<NotPosGold>> GetNotPosGoldsAsync();
        Task<IQueryable<UnexpectedCash>> GetUnexpectedCashesAsync();
        Task<IQueryable<ExpectedCash>> GetExpectedCashesAsync();
        Task<IQueryable<FinalizedGold>> GetFinalizedGoldsAsync();
        Task<IQueryable<VendorConfirmation>> GetVendorConfirmationsAsync();
        Task<IQueryable<Gcode>> GetGcodesAsync();
        Task<IQueryable<VendorFinalized>> GetVendorFinalizedsAsync();
        Task<IQueryable<Vendor>> GetVendorsAsync();
        Task<IQueryable<VendorData>> GetVendorDatasAsync();
        Task<VendorData> GetSpecificVendorDataAsync(string vendorId);
        Task<VendorData> GetSpecificVendorDataReadOnlyAsync(string vendorId);
        Task<VendorData> GetVendorDataAsync(Guid vendorId);
        Task<IQueryable<BannedIp2>> GetBannedIpsAsync();
        Task<BannedIp2> GetBannedIpAsync(string ip);
        Task<List<UsedFirstLast>> GetUsedFirstLasts();
        Task<IQueryable<VendorTransaction>> GetVendorTransactionsAsync();
        Task<IQueryable<VendorPlatinBalance>> GetVendorPlatinBalancesAsync();
        Task<VendorPlatinBalance> GetVendorPlatinBalanceAsync(Guid vendorId);
        Task<GoldPrice> GetRobotStatusAsync();
        Task<GoldPrice> GetBuyServiceStatusAsync();
        Task<GoldPrice> GetSellServiceStatusAsync();
        Task<VendorTransaction> GetVendorTransactionAsync(Guid transId);
        Task<VendorTransactionNew> GetVendorTransactionNewAsync(int transId, string refId);
        Task<VendorTransactionNew> GetVendorTransactionNewAsync(int transId);
        Task<VendorTransaction> GetVendorTransactionAsync(string refId);
        Task<VendorTransaction> GetVendorTransactionReadOnlyAsync(string refId);
        Task<List<EntityEntry<VendorNotPosition>>> RemoveNotPositions(List<VendorNotPosition> vendorNotPositions);
        Task<List<EntityEntry<VendorNotPositionSell>>> RemoveNotPositionSells(List<VendorNotPositionSell> vendorNotPositions);
        Task<VenTransaction> GetVenTransactionAsync(int transId);
        Task<VenTransaction> GetVenTransactionAsync(int transId, string refId);
        Task<VenTransaction> GetVenTransactionReadOnly(int transId);

        Task<bool> IsTherePreviousTransaction(string redId);
        Task<VendorTransaction> GetVendorTransactionWithComments(string comment);
        Task<VendorTransaction> GetVendorTransactionWithCommentsReadOnlyAsync(string comment);
        Task<Vendor> GetVendorAsync(Guid vendorId);
        Task<Vendor> GetVendorAsync(string apiKey);
        Task<Vendor> GetVendorReadOnlyAsync(Guid vendorId);
        Task<Vendor> GetVendorReadOnlyAsync(string apiKey);
        Task<Vendor> GetVendorAsync(Guid vendorId, string apiKey);
        Task<int> SaveChangesAsync();
    }
}
