using Gold.Core.GoldPrices;
using Gold.Core.Vendors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gold.Domain.Vendors
{
    public class VendorsRepository : IVendorsRepository, IDisposable
    {
        private readonly VendorsDbContext Context;

        public VendorsRepository(VendorsDbContext vendorsDbContext)
        {
            this.Context = vendorsDbContext;
        }

        void IVendorsRepository.AddBannedIp(BannedIp2 bannedIp)
        {
            Context.BannedIps.Add(bannedIp);
        }

        void IVendorsRepository.AddConfirmation(VendorConfirmation confirmation)
        {
            Context.VendorConfirmations.Add(confirmation);
        }

        void IVendorsRepository.AddExpectedCash(ExpectedCash expectedCash)
        {
            Context.ExpectedCashes.Add(expectedCash);
        }

        

        void IVendorsRepository.AddUnexpectedCash(UnexpectedCash unexpectedCash)
        {
            Context.UnexpectedCashes.Add(unexpectedCash);
        }

        void IVendorsRepository.AddFinalizedGold(FinalizedGold finalizedGold)
        {
            Context.FinalizedGolds.Add(finalizedGold);
        }

        void IVendorsRepository.AddGcode(Gcode gcode)
        {
            Context.Gcodes.Add(gcode);
        }

        void IVendorsRepository.AddVendor(Vendor vendor)
        {
            Context.Vendors.Add(vendor);
        }

        void IVendorsRepository.AddVendorData(VendorData vendorData)
        {
            Context.VendorDatas.Add(vendorData);
        }

        void IVendorsRepository.AddVendorTransaction(VendorTransaction vendorTransaction)
        {
            Context.VendorTransactions.Add(vendorTransaction);
        }

        IQueryable<BannedIp2> IVendorsRepository.GetBannedIps()
        {
            return Context.BannedIps;
        }

        IQueryable<UnexpectedCash> IVendorsRepository.GetUnexpectedCashes()
        {
            return Context.UnexpectedCashes;
        }

        IQueryable<ExpectedCash> IVendorsRepository.GetExpectedCashes()
        {
            return Context.ExpectedCashes;
        }

       
        IQueryable<FinalizedGold> IVendorsRepository.GetFinalizedGolds()
        {
            return Context.FinalizedGolds;
        }

        IQueryable<Gcode> IVendorsRepository.GetGcodes()
        {
            return Context.Gcodes;
        }

        IQueryable<VendorConfirmation> IVendorsRepository.GetVendorConfirmations()
        {
            return Context.VendorConfirmations;
        }
        IQueryable<VendorConfirmationNew> IVendorsRepository.GetVendorConfirmationsNew()
        {
            return Context.VendorConfirmationsNew;
        }
        IQueryable<VendorData> IVendorsRepository.GetVendorDatas()
        {
            return Context.VendorDatas;
        }

        IQueryable<Vendor> IVendorsRepository.GetVendors()
        {
            return Context.Vendors;
        }

        VendorTransaction IVendorsRepository.GetVendorTransaction(Guid transId)
        {
            return Context.VendorTransactions.Where(x => x.TransactionId == transId).FirstOrDefault();
        }

        VendorTransaction IVendorsRepository.GetVendorTransaction(string refId)
        {
            return Context.VendorTransactions.Where(x => x.VendorReferenceId == refId).FirstOrDefault();
        }

        IQueryable<VendorTransaction> IVendorsRepository.GetVendorTransactions()
        {
            return Context.VendorTransactions;
          
        }

        IQueryable<VendorTransactionNew> IVendorsRepository.GetVendorTransactionsNew()
        {
            return Context.VendorTransactionsNew;

        }

        void IVendorsRepository.RemoveExpected(ExpectedCash expectedCash)
        {
            Context.ExpectedCashes.Remove(expectedCash);
        }


        void IVendorsRepository.SaveChanges()
        {
            Context.SaveChanges();
        }

        async Task<EntityEntry<NotPosGold>> IVendorsRepository.AddNotPosGold(NotPosGold notPosGold)
        {
            return await Context.NotPosGolds.AddAsync(notPosGold);
        }

        IQueryable<NotPosGold> IVendorsRepository.GetNotPosGolds()
        {
            return Context.NotPosGolds;
        }

        void IVendorsRepository.AddNotPosGoldSell(NotPosGoldSell notPosGoldsell)
        {
            Context.NotPosGoldSells.Add(notPosGoldsell);
        }

        IQueryable<NotPosGoldSell> IVendorsRepository.GetNotPosGoldSells()
        {
            return Context.NotPosGoldSells;
        }

        void IVendorsRepository.AddIBankError(InterBankError bankError)
        {
            Context.InterBankErrors.Add(bankError);
        }

        IQueryable<InterBankError> IVendorsRepository.GetInterBankErrors()
        {
            return Context.InterBankErrors;
        }

        GoldPrice IVendorsRepository.GetRobotStatus()
        {
            return Context.GoldPrices.Where(x => x.Name == "expected_run").FirstOrDefault();
        }

        async Task<GoldPrice> IVendorsRepository.GetRobotStatusAsync()
        {
            return await Context.GoldPrices.Where(x => x.Name == "expected_run").AsNoTracking().FirstOrDefaultAsync();
        }

        void IVendorsRepository.AddVendorPlatinBalance(VendorPlatinBalance vendorPlatinBalance)
        {
            Context.VendorPlatinBalances.Add(vendorPlatinBalance);
        }

        IQueryable<VendorPlatinBalance> IVendorsRepository.GetVendorPlatinBalances()
        {
            return Context.VendorPlatinBalances;
        }

        VendorPlatinBalance IVendorsRepository.GetVendorPlatinBalance(Guid vendorId)
        {
            return Context.VendorPlatinBalances.Where(x => x.VendorId == vendorId).FirstOrDefault();
        }

        async void IVendorsRepository.AddVendorAsync(Vendor vendor)
        {
            await Context.Vendors.AddAsync(vendor);
        }

        async Task<EntityEntry<VendorData>> IVendorsRepository.AddVendorDataAsync(VendorData vendorData)
        {
            return await Context.VendorDatas.AddAsync(vendorData);
        }

        
        async void IVendorsRepository.AddVendorPlatinBalanceAsync(VendorPlatinBalance vendorPlatinBalance)
        {
            await Context.VendorPlatinBalances.AddAsync(vendorPlatinBalance);
        }

        async Task<EntityEntry<VendorTransaction>> IVendorsRepository.AddVendorTransactionAsync(VendorTransaction vendorTransaction)
        {
            return await Context.VendorTransactions.AddAsync(vendorTransaction);
        }

        async Task<EntityEntry<VendorTransactionNew>> IVendorsRepository.AddVendorTransactionNewAsync(VendorTransactionNew vendorTransaction)
        {
            return await Context.VendorTransactionsNew.AddAsync(vendorTransaction);
        }

        async void IVendorsRepository.AddBannedIpAsync(BannedIp2 bannedIp)
        {
            await Context.BannedIps.AddAsync(bannedIp);
        }

        async void IVendorsRepository.AddGcodeAsync(Gcode gcode)
        {
            await Context.Gcodes.AddAsync(gcode);
        }

        async void IVendorsRepository.AddConfirmationAsync(VendorConfirmation confirmation)
        {
            await Context.VendorConfirmations.AddAsync(confirmation);
        }

        async void IVendorsRepository.AddExpectedCashAsync(ExpectedCash expectedCash)
        {
            await Context.ExpectedCashes.AddAsync(expectedCash);
        }

        async void IVendorsRepository.AddNotPosGoldAsync(NotPosGold notPosGold)
        {
            await Context.NotPosGolds.AddAsync(notPosGold);
        }

        async void IVendorsRepository.AddNotPosGoldSellAsync(NotPosGoldSell notPosGoldSell)
        {
            await Context.NotPosGoldSells.AddAsync(notPosGoldSell);
        }

        async void IVendorsRepository.AddIBankErrorAsync(InterBankError bankError)
        {
            await Context.InterBankErrors.AddAsync(bankError);
        }

        async void IVendorsRepository.AddUnexpectedCashAsync(UnexpectedCash unexpectedCash)
        {
            await Context.UnexpectedCashes.AddAsync(unexpectedCash);
        }

        async void IVendorsRepository.AddFinalizedGoldAsync(FinalizedGold finalizedGold)
        {
            await Context.FinalizedGolds.AddAsync(finalizedGold);
        }

        async void IVendorsRepository.RemoveExpectedAsync(ExpectedCash expectedCash)
        {
            await Task.Run(() => {


                Context.ExpectedCashes.Remove(expectedCash);
            });
        }


        async Task<IQueryable<InterBankError>> IVendorsRepository.GetInterBankErrorsAsync()
        {

            return await Task.Run(() => {
                return Context.InterBankErrors;
            }); 

        }

        async Task<IQueryable<NotPosGoldSell>> IVendorsRepository.GetNotPosGoldSellsAsync()
        {
            return await Task.Run(() => {
                return Context.NotPosGoldSells;
            });
        }

        async Task<IQueryable<NotPosGold>> IVendorsRepository.GetNotPosGoldsAsync()
        {
            return await Task.Run(() => {
                return Context.NotPosGolds;
            });
        }

        async Task<IQueryable<UnexpectedCash>> IVendorsRepository.GetUnexpectedCashesAsync()
        {
            return await Task.Run(() => {
                return Context.UnexpectedCashes;
            });
        }

        async Task<IQueryable<ExpectedCash>> IVendorsRepository.GetExpectedCashesAsync()
        {
            return await Task.Run(() => {
                return Context.ExpectedCashes;
            });
        }

        async Task<IQueryable<FinalizedGold>> IVendorsRepository.GetFinalizedGoldsAsync()
        {
            return await Task.Run(() => {
                return Context.FinalizedGolds;
            });
        }

        async Task<IQueryable<VendorConfirmation>> IVendorsRepository.GetVendorConfirmationsAsync()
        {
            return await Task.Run(() => {
                return Context.VendorConfirmations;
            });
        }

        async Task<IQueryable<Gcode>> IVendorsRepository.GetGcodesAsync()
        {
            return await Task.Run(() => {
                return Context.Gcodes;
            });
        }

        async Task<IQueryable<Vendor>> IVendorsRepository.GetVendorsAsync()
        {
            return await Task.Run(() => {
                return Context.Vendors;
            });
        }

        async Task<IQueryable<VendorData>> IVendorsRepository.GetVendorDatasAsync()
        {
            return await Task.Run(() => {
                return Context.VendorDatas;
            });
        }

        async Task<IQueryable<BannedIp2>> IVendorsRepository.GetBannedIpsAsync()
        {
            return await Task.Run(() => {
                return Context.BannedIps;
            });
        }

        async Task<IQueryable<VendorTransaction>> IVendorsRepository.GetVendorTransactionsAsync()
        {
            return await Task.Run(() => {
                return Context.VendorTransactions;
            });
        }

        async Task<IQueryable<VendorPlatinBalance>> IVendorsRepository.GetVendorPlatinBalancesAsync()
        {
            return await Task.Run(() => {
                return Context.VendorPlatinBalances;
            });
        }

        async Task<VendorPlatinBalance> IVendorsRepository.GetVendorPlatinBalanceAsync(Guid vendorId)
        {
             return await Context.VendorPlatinBalances.Where(x => x.VendorId == vendorId).FirstOrDefaultAsync();   
        }

        async Task<VendorTransaction> IVendorsRepository.GetVendorTransactionAsync(Guid transId)
        {
            
            return await Context.VendorTransactions.Where(x => x.TransactionId == transId).FirstOrDefaultAsync();
            
        }

        async Task<VendorTransactionNew> IVendorsRepository.GetVendorTransactionNewAsync(int transId, string refId)
        {

            return await Context.VendorTransactionsNew.Where(x => x.TransactionId == transId && x.VendorReferenceId == refId).FirstOrDefaultAsync();

        }
        async Task<VendorTransactionNew> IVendorsRepository.GetVendorTransactionNewAsync(int transId)
        {

            return await Context.VendorTransactionsNew.Where(x => x.TransactionId == transId).FirstOrDefaultAsync();

        }

        async Task<VendorTransaction> IVendorsRepository.GetVendorTransactionAsync(string refId)
        {
            
            return await Context.VendorTransactions.Where(x => x.VendorReferenceId == refId).FirstOrDefaultAsync();
            
        }

        async Task<VendorTransaction> IVendorsRepository.GetVendorTransactionReadOnlyAsync(string refId)
        {

            return await Context.VendorTransactions.AsNoTracking().Where(x => x.VendorReferenceId == refId).FirstOrDefaultAsync();

        }

        async Task<bool> IVendorsRepository.IsTherePreviousTransaction(string refId)
        {
            /*return await (from first in Context.VendorTransactions
                       from second in Context.VendorTransactionsNew
                       select new
                       {
                           a = first.VendorReferenceId,
                           b = second.VendorReferenceId
                       }).AsNoTracking().Where(x => x.a == refId || x.b == refId).AnyAsync();*/

            //return await Context.VendorTransactionsNew.Where(x => x.VendorReferenceId == refId).AnyAsync();
            return await Context.VenTransactions.Where(x => x.VendorReferenceId == refId).AnyAsync();
        }

        async Task<VendorTransaction> IVendorsRepository.GetVendorTransactionWithComments(string comment)
        {
            
            return await Context.VendorTransactions.Where(x => x.Comment == comment).FirstOrDefaultAsync();
            
        }

      
        async Task<VendorTransaction> IVendorsRepository.GetVendorTransactionWithCommentsReadOnlyAsync(string comment)
        {

            return await Context.VendorTransactions.AsNoTracking().Where(x => x.Comment == comment).FirstOrDefaultAsync();

        }

        async Task<int> IVendorsRepository.SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

        async Task<Vendor> IVendorsRepository.GetVendorAsync(Guid vendorId)
        {
            
            
            return await Context.Vendors
                .Where(x => x.VendorId == vendorId)
                .FirstOrDefaultAsync();
            
        }

        async Task<Vendor> IVendorsRepository.GetVendorAsync(string apiKey)
        {
            
            
             return await Context.Vendors.Where(x => x.ApiKey == apiKey).FirstOrDefaultAsync();
            
        }

        async Task<Vendor> IVendorsRepository.GetVendorReadOnlyAsync(Guid vendorId)
        {
            
            
            return await Context.Vendors.AsNoTracking()
                .Where(x => x.VendorId == vendorId)
                .FirstOrDefaultAsync();
        }

        async Task<Vendor> IVendorsRepository.GetVendorAsync(Guid vendorId, string apiKey)
        {
            return await Context.Vendors.Where(x => x.VendorId == vendorId && x.ApiKey == apiKey).FirstOrDefaultAsync();
        }

        async Task<Vendor> IVendorsRepository.GetVendorReadOnlyAsync(string apiKey)
        {
            
            
            return await Context.Vendors.AsNoTracking().Where(x => x.ApiKey == apiKey).FirstOrDefaultAsync();
            
        }

        async Task<VendorData> IVendorsRepository.GetSpecificVendorDataAsync(string vendorId)
        {
            
            
            return await Context.VendorDatas.Where(x => x.RelatedId == vendorId).FirstOrDefaultAsync();
            
        }

        async Task<VendorData> IVendorsRepository.GetSpecificVendorDataReadOnlyAsync(string vendorId)
        {


            return await Context.VendorDatas.AsNoTracking().Where(x => x.RelatedId == vendorId).FirstOrDefaultAsync();

        }

        async Task<VendorData> IVendorsRepository.GetVendorDataAsync(Guid vendorId)
        {
            
            
            var vendorid = vendorId.ToString();
            return await Context.VendorDatas.Where(x => x.RelatedId == vendorid).FirstOrDefaultAsync();
            
        }

        async Task<BannedIp2> IVendorsRepository.GetBannedIpAsync(string ip)
        {
            
            return await Context.BannedIps.AsNoTracking().Where(x => x.IP == ip).FirstOrDefaultAsync();
            
        }

        void IDisposable.Dispose()
        {
            Context.DisposeAsync();
        }
        async Task<bool> IVendorsRepository.ShouldSenderRun()
        {
            var status = await Context.GoldPrices.Where(x => x.Name == "sender_run").FirstOrDefaultAsync();

            return status != null && status.Amount.HasValue && status.Amount.Value == 1.00m;
        }
        async Task<IAsyncEnumerable<VendorTransactionNew>> IVendorsRepository.GetVendorTransactionsNewAsync()
        {
            return Context.VendorTransactionsNew.AsAsyncEnumerable();
        }
        
        async Task<List<VendorExpected>> IVendorsRepository.GetVendorExpectedsAsync()
        {

            return await Task.Run(() =>
            {
                return Context.VendorExpecteds.AsQueryable().ToList();
            });
        }

        IQueryable<VendorCashPayment> IVendorsRepository.GetVendorCashPayments()
        {
            return Context.VendorCashPayments;
        }

        IAsyncEnumerable<VendorCashPayment> IVendorsRepository.GetVendorCashPaymentsAsyncEnumerable()
        {
            return Context.VendorCashPayments.AsAsyncEnumerable();
        }

        IAsyncEnumerable<VendorNotPosition> IVendorsRepository.GetVendorNotPositionsAsyncEnum()
        {
            return Context.VendorNotPositions.AsAsyncEnumerable();
        }

        IAsyncEnumerable<VendorNotPosition> IVendorsRepository.GetVendorNotPositionsAsyncEnum(List<string> ids)
        {
            var guids = new List<Guid>(ids.Count + 1);
            foreach (var id in ids)
            {
                guids.Add(Guid.Parse(id));
            }
            return Context.VendorNotPositions.Where(x => guids.Contains(x.NotPosId)).AsAsyncEnumerable();
        }

        IAsyncEnumerable<VendorNotPositionSell> IVendorsRepository.GetVendorNotPositionSellsAsyncEnum()
        {
            return Context.VendorNotPositionSells.AsAsyncEnumerable();
        }

        IAsyncEnumerable<VendorNotPositionSell> IVendorsRepository.GetVendorNotPositionSellsAsyncEnum(List<string> ids)
        {
            var guids = new List<Guid>(ids.Count + 1);
            foreach (var id in ids)
            {
                guids.Add(Guid.Parse(id));
            }
            return Context.VendorNotPositionSells.Where(x => guids.Contains(x.NotPosId)).AsAsyncEnumerable();
        }

        async Task<VendorExpected> IVendorsRepository.GetVendorExpected(string vendorRef)
        {

            return await Context.VendorExpecteds.Where(x => x.VendorReferenceId == vendorRef).FirstOrDefaultAsync();

        }

        async Task<EntityEntry<VendorExpected>> IVendorsRepository.RemoveVendorExpected(VendorExpected vendorExpected)
        {


            return await Task.Run(() => {
                return Context.VendorExpecteds.Remove(vendorExpected);
            });


        }
        IQueryable<VendorExpected> IVendorsRepository.GetVendorExpecteds()
        {
            return Context.VendorExpecteds;
        }
        async Task<EntityEntry<VendorExpected>> IVendorsRepository.AddVendorExpected(VendorExpected expectedCash)
        {
            return await Context.VendorExpecteds.AddAsync(expectedCash);
        }

        async Task<EntityEntry<VendorUnExpected>> IVendorsRepository.AddVendorUnExpected(VendorUnExpected vendorUnExpected)
        {
            return await Context.VendorUnExpecteds.AddAsync(vendorUnExpected);
        }
        async Task<EntityEntry<VendorNotPosition>> IVendorsRepository.AddVendorNotPosition(VendorNotPosition notPos)
        {
            return await Context.VendorNotPositions.AddAsync(notPos);
        }

        async Task<EntityEntry<VendorNotPositionSell>> IVendorsRepository.AddVendorNotPositionSell(VendorNotPositionSell notPos)
        {
            return await Context.VendorNotPositionSells.AddAsync(notPos);
        }

        async Task<EntityEntry<VendorFinalized>> IVendorsRepository.AddVendorFinalized(VendorFinalized vendorFinalized)
        {
            return await Context.VendorFinalizeds.AddAsync(vendorFinalized);
        }


        async Task<EntityEntry<VendorCashPayment>> IVendorsRepository.RemoveCashPayment(VendorCashPayment cashPayment)
        {
            return await Task.Run(() => { return Context.VendorCashPayments.Remove(cashPayment); });
        }

        IQueryable<VendorFinalized> IVendorsRepository.GetVendorFinalizeds()
        {
            return Context.VendorFinalizeds;
        }

        async Task<IQueryable<VendorFinalized>> IVendorsRepository.GetVendorFinalizedsAsync()
        {
            return await Task.Run(() =>
            {
                return Context.VendorFinalizeds;
            });
        }

        async Task<EntityEntry<VendorCashPayment>> IVendorsRepository.AddVendorCashPayment(VendorCashPayment cashPayment)
        {
            return await Context.VendorCashPayments.AddAsync(cashPayment);
        }

        IQueryable<VendorUnExpected> IVendorsRepository.GetVendorUnExpecteds()
        {
            return Context.VendorUnExpecteds;
        }

        IQueryable<VendorNotPosition> IVendorsRepository.GetVendorNotPositions()
        {
            return Context.VendorNotPositions;
        }

        IQueryable<VendorNotPositionSell> IVendorsRepository.GetVendorNotPositionSells()
        {
            return Context.VendorNotPositionSells;
        }

        VendorTransactionNew IVendorsRepository.GetVendorTransactionNew(int transId)
        {
            return Context.VendorTransactionsNew.Where(x => x.TransactionId == transId).FirstOrDefault();
        }

        VendorTransactionNew IVendorsRepository.GetVendorTransactionNew(string refId)
        {
            return Context.VendorTransactionsNew.Where(x => x.VendorReferenceId == refId).FirstOrDefault();
        }

        bool IVendorsRepository.ExpectedServiceRun()
        {
            var ee = Context.GoldPrices.Where(x => x.Name == "five_m_service").FirstOrDefault();

            return ee != null && ee.Amount.HasValue && ee.Amount.Value == 1.00m;
        }

        async Task<bool> IVendorsRepository.ExpectedServiceRunAsync()
        {
            var ee = await Context.GoldPrices.Where(x => x.Name == "five_m_service").FirstOrDefaultAsync();

            return ee != null && ee.Amount.HasValue && ee.Amount.Value == 1.00m;
        }

        bool IVendorsRepository.PaymentServiceRun()
        {
            var ee = Context.GoldPrices.Where(x => x.Name == "five_m_service_2").FirstOrDefault();

            return ee != null && ee.Amount.HasValue && ee.Amount.Value == 1.00m;
        }

        async Task<bool> IVendorsRepository.PaymentServiceRunAsync()
        {
            var ee = await Context.GoldPrices.Where(x => x.Name == "five_m_service_2").FirstOrDefaultAsync();

            return ee != null && ee.Amount.HasValue && ee.Amount.Value == 1.00m;
        }

        async Task<EntityEntry<VenTransaction>> IVendorsRepository.AddVenTransactionAsync(VenTransaction vendorTransaction)
        {
            return await Context.VenTransactions.AddAsync(vendorTransaction);
        }

        async Task<bool> IVendorsRepository.IsFinalized(int transactionId)
        {
            return await Context.VendorFinalizeds.Where(x => x.TransactionId == transactionId).AnyAsync();
        }


        IQueryable<VenTransaction> IVendorsRepository.GetVenTransactions()
        {
            return Context.VenTransactions;
        }

        IAsyncEnumerable<VenTransaction> IVendorsRepository.GetVenTransactions(int first, int last)
        {
            return Context.VenTransactions.Where(x => x.TransactionId >= first && x.TransactionId <= last).AsAsyncEnumerable();
        }

        VenTransaction IVendorsRepository.GetVenTransaction(int transId)
        {
            return Context.VenTransactions.Where(x => x.TransactionId == transId).FirstOrDefault();
        }

        VenTransaction IVendorsRepository.GetVenTransaction(string refId)
        {
            throw new NotImplementedException();
        }

        async Task<VenTransaction> IVendorsRepository.GetVenTransactionAsync(int transId, string refId)
        {
            return await Context.VenTransactions.Where(x => x.TransactionId == transId && x.VendorReferenceId == refId).FirstOrDefaultAsync();
        }

        async Task<VenTransaction> IVendorsRepository.GetVenTransactionAsync(int transId)
        {
            return await Context.VenTransactions.Where(x => x.TransactionId == transId).FirstOrDefaultAsync();
        }

        async Task<VenTransaction> IVendorsRepository.GetVenTransactionReadOnly(int transId)
        {
            return await Context.VenTransactions.AsNoTracking().Where(x => x.TransactionId == transId).FirstOrDefaultAsync();
        }

        async Task<List<EntityEntry<VendorNotPosition>>> IVendorsRepository.RemoveNotPositions(List<VendorNotPosition> vendorNotPositions)
        {

            return await Task.Run(() =>
            {
                var result = new List<EntityEntry<VendorNotPosition>>();
                foreach (var np in vendorNotPositions)
                {

                    result.Add(Context.VendorNotPositions.Remove(np));

                }
                return result;
            });
            
        }

        async Task<List<EntityEntry<VendorNotPositionSell>>> IVendorsRepository.RemoveNotPositionSells(List<VendorNotPositionSell> vendorNotPositions)
        {

            return await Task.Run(() =>
            {
                var result = new List<EntityEntry<VendorNotPositionSell>>();
                foreach (var np in vendorNotPositions)
                {

                    result.Add(Context.VendorNotPositionSells.Remove(np));

                }
                return result;
            });
            
        }

        async Task<int> IVendorsRepository.GetPaycellMinutesBuy()
        {
            Context.ChangeTracker.DetectChanges();
            var data = await Context
                .GoldPrices
                .Where(x => x.Name == "paycell_minutes_buy")
                .FirstOrDefaultAsync();

            if (data == null || data.Name != "paycell_minutes_buy" || !data.Amount.HasValue)
            {
                return 15;
            }
            else
            {
                return (int) data.Amount.Value;
            }
            
        }

        async Task IVendorsRepository.SetPaycellMinutesBuy(int minutes)
        {
            var data = await Context.GoldPrices.Where(x => x.Name == "paycell_minutes_buy").FirstOrDefaultAsync();
            
            if (data != null && data.Name == "paycell_minutes_buy")
                data.Amount = minutes;
        }

        async Task<int> IVendorsRepository.GetPaycellMinutesSell()
        {
            Context.ChangeTracker.DetectChanges();
            var data = await Context.GoldPrices.Where(x => x.Name == "paycell_minutes_sell").FirstOrDefaultAsync();

            if (data == null || data.Name != "paycell_minutes_sell" || !data.Amount.HasValue)
            {
                return 15;
            }
            else
            {
                return (int)data.Amount.Value;
            }

        }

        async Task IVendorsRepository.SetPaycellMinutesSell(int minutes)
        {
            var data = await Context.GoldPrices.Where(x => x.Name == "paycell_minutes_sell").FirstOrDefaultAsync();

            if (data != null && data.Name == "paycell_minutes_sell")
                data.Amount = minutes;
        }

        async Task<int> IVendorsRepository.GetExpectedTimeoutMinutes()
        {// expected_timeout_minutes


            Context.ChangeTracker.DetectChanges();
            var data = await Context.GoldPrices.Where(x => x.Name == "expected_timeout_minutes").FirstOrDefaultAsync();

            if (data == null || data.Name != "expected_timeout_minutes" || !data.Amount.HasValue)
            {
                return 10;
            }
            else
            {
                return (int)data.Amount.Value;
            }
        }

        async Task IVendorsRepository.SetExpectedTimeoutMinutes(int minutes)
        {
            var data = await Context.GoldPrices.Where(x => x.Name == "expected_timeout_minutes").FirstOrDefaultAsync();

            if (data != null && data.Name == "expected_timeout_minutes")
                data.Amount = minutes;
        }

        async Task IVendorsRepository.RemoveVendorExpecteds(List<VendorExpected> expecteds)
        {
            Context.VendorExpecteds.RemoveRange(expecteds);
            await Context.SaveChangesAsync();
        }

        async Task<EntityEntry<UsedFirstLast>> IVendorsRepository.AddFirstLast(UsedFirstLast usedFirstLast)
        {
            return await Context.UsedFirstLasts.AddAsync(usedFirstLast);
        }

        async Task<List<UsedFirstLast>> IVendorsRepository.GetUsedFirstLasts()
        {
            return await Context.UsedFirstLasts.AsQueryable().ToListAsync();
        }

        async Task<GoldPrice> IVendorsRepository.GetBuyServiceStatusAsync()
        {
            return await Context.GoldPrices.Where(x => x.Name == "five_m_service").FirstOrDefaultAsync();
        }

        async Task<GoldPrice> IVendorsRepository.GetSellServiceStatusAsync()
        {
            return await Context.GoldPrices.Where(x => x.Name == "five_m_service_2").FirstOrDefaultAsync();
        }

        async Task<VendorPlatinBalance> IVendorsRepository.GetVendorPlatinBalanceStringAsync(string vendorId)
        {
            Guid id = Guid.Parse(vendorId);

            return await Context.VendorPlatinBalances.Where(x => x.VendorId == id).FirstOrDefaultAsync();
        }

      
    }
}
