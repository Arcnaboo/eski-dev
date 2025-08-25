using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Services;
using Gold.Api.Services.Interfaces;
using Gold.Api.Utilities;
using Gold.Core.Vendors;
using Gold.Domain.Vendors;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Robots
{
    public class VendorExpectedChecker : IExpectedChecker, IDisposable
    {


        private readonly IVendorsRepository vendorsRepository;
        private readonly IVendorBuySellService vendorBuySellService;
        private bool running;
        private bool disposedValue;

        public VendorExpectedChecker(IVendorsRepository vendorsRepository, IVendorBuySellService vendorBuySellService)
        {
            this.vendorBuySellService = vendorBuySellService;
            this.vendorsRepository = vendorsRepository;
            this.running = false;
            Log.Information("ExpectedCashChecker Created");
        }

        private enum ResultState
        {
            OK,
            UNEXPECTED_AMOUNT,
            UNEXPECTED_TIME,
            SYS_ERR
        }

        private class ResultTuple
        {
            public bool ok { get; set; }
            public VendorExpected expected { get; set; }
            public KTTransaction kTTransaction { get; set; }
            public string ktReference { get; set; }
            public int totalMinutes { get; set; }
            public ResultState state { get; set; }
        }

        public override string ToString()
        {
            return string.Format("robot: " + vendorsRepository.ToString());
        }


        private async Task<int> UnexpectedAmountFlow(ResultTuple data)
        {
            var expectedCash = data.expected;
            var ktTransaction = data.kTTransaction;
            var unexpected = new VendorUnExpected(expectedCash.TransactionId, ktTransaction.TransRef, expectedCash.ExpectedSuffix, expectedCash.Amount, "MONEY_ISSUE", expectedCash.ExpectedTRY, ktTransaction.Amount);

            await vendorsRepository.AddVendorUnExpected(unexpected);
            await vendorsRepository.RemoveVendorExpected(expectedCash);
            await vendorsRepository.SaveChangesAsync();
            Log.Debug("checker created unexpected amount and removed expected");
            return 0;
        }

        private async Task<int> UnexpectedTimeFlow(ResultTuple data)
        {
            var expectedCash = data.expected;
            var minutes = data.totalMinutes;
            var unexpected = new VendorUnExpected(expectedCash.TransactionId,
                        "TIME_OUT",
                        expectedCash.ExpectedSuffix,
                        expectedCash.Amount,
                        "TIME_OUT",
                        expectedCash.ExpectedTRY,
                        null,
                        minutes);
            await vendorsRepository.AddVendorUnExpected(unexpected);
            await vendorsRepository.RemoveVendorExpected(expectedCash);
            await vendorsRepository.SaveChangesAsync();
            Log.Debug("checker created unexpected time and removed expected");
            return 0;
        }

        private async Task<int> DefaultExpectedFlow(ResultTuple data)
        {
            var ok = data.ok; // if money received
            var expected = data.expected; // expextedCash
            var ktref = data.ktReference; // ref
            var vendor = await vendorsRepository.GetVendorAsync(expected.VendorId);
            var transaction = await vendorsRepository.GetVendorTransactionNewAsync(expected.TransactionId);
            var rateThingy = await KTApiService.GetCurrentPriceFromKtApiASync();
            var rate = rateThingy.value.FxRates.Where(x => x.FxId == expected.RateFxId).FirstOrDefault();

            Log.Debug("received current price: " + rate.BuyRate.ToString());

            var vendorData = await vendorsRepository.GetSpecificVendorDataReadOnlyAsync(transaction.Destination.ToString());

            var suffFrom = vendorData.TLSuffix;
            var suffTo = (expected.RateFxId == 24) ? vendorData.GOLDSuffix : vendorData.SLVSuffix;

            Log.Debug(string.Format("the suffixes from {0} to {1}", suffFrom, suffTo));
            var param = new PreciousMetalsBuyParams
            {
                Amount = transaction.GramAmount,
                SuffixFrom = suffFrom,
                SuffixTo = suffTo,
                UserName = KuwaitAccessHandler.FINTAG,
                BuyRate = rate.BuyRate
            };
            Log.Debug("Buying gold from kt");

            var attempts = 0;
            bool success = false;
            string refId = "";
            while (attempts <= 5 && success == false)
            {
                attempts++;
                var metalBuy = await KTApiService.PreciousMetalBuyAsync(param);
                if (metalBuy == null)
                {

                    Log.Debug("metal buy was null at attempt: " + attempts.ToString());
                    continue;
                }
                Log.Debug("metalBuy: " + JsonConvert.SerializeObject(metalBuy));
                success = metalBuy.Success;
                refId = metalBuy.RefId;

            }
            var comm = (expected.RateFxId == 24) ? "GOLD" : "SILVER";
            Log.Debug("Expected type: " + comm);
            if (success == true)
            {

                transaction.CompleteTransaction(comm);
                if (expected.RateFxId == 24)
                {
                    vendor.Balance += transaction.GramAmount;
                }
                else
                {
                    vendor.SilverBalance = (vendor.SilverBalance.HasValue) ? vendor.SilverBalance.Value + transaction.GramAmount : transaction.GramAmount;
                }
                var tl = transaction.TlAmount;
                var gram = transaction.GramAmount * (-1);
                var ff = new VendorFinalized(expected, comm, ktref, "VENDOR_BUY", tl, gram);
                await vendorsRepository.AddVendorFinalized(ff);
                await vendorsRepository.RemoveVendorExpected(expected);
                await vendorsRepository.SaveChangesAsync(); // save things

                Log.Debug("expected ok created finalized removed expected also bought gold from kt - ROBOT completed task");

            }
            else
            {
                var notPos = new VendorNotPosition(refId, transaction.GramAmount,
                    suffFrom, suffTo, rate.BuyRate, transaction.TransactionId,
                    transaction.Destination, comm, 
                    expected.PiyasaGramFiyat, expected.KTGramFiyat, expected.SatisGramFiyat);
                await vendorsRepository.AddVendorNotPosition(notPos);
                await vendorsRepository.RemoveVendorExpected(expected);
                await vendorsRepository.SaveChangesAsync();
                Log.Debug("expected ok but precious metal could not be bought from kt created notPos - " + JsonConvert.SerializeObject(notPos));
            }


            return 0;
        }

        public async void Run()
        {
            
            // runs asynchrounously
            

            Log.Debug("ExpectedCashChecker FLOW: " + ToString());

            try
            {
                if (running)
                {
                    Log.Debug("Expected already running");
                    return;
                }
                running = true;
                var expectedRun = vendorsRepository.GetRobotStatus();

                if (expectedRun == null || !expectedRun.Amount.HasValue || ((int)expectedRun.Amount.Value) == 0)
                {
                    Log.Debug("Not running expected cash");
                    running = false;
                    return;
                }

                if (KuwaitAccessHandler.AccessToken == null)
                {
                    Log.Debug("Expected cash not ready because not connected to KT");
                    running = false;
                    return;
                }
                    
                Log.Debug("Expected cash robot flow start");
                var expecteds = vendorsRepository.GetVendorExpecteds()
                    .OrderByDescending(x => x.DateTime)
                    .ToList();

                Log.Debug("expected cash tasks creating tasks");

                var tasks = new Task<ResultTuple>[expecteds.Count];
                var index = 0;
                foreach (var expectedCash in expecteds)
                {
                    tasks[index++] = SearchExpected(expectedCash); // each task seaches 1 expectedCash
                }
                Log.Debug("expected cash tasks created");
                Task.WaitAll(tasks); // wait all tasks
                Log.Debug("expected cash tasks completed");
                
                for (var i = 0; i < index; i++)
                {
                    var data = tasks[i].Result;
                    
                    var state = data.state;
                    
                    switch (state)
                    {
                        case ResultState.OK:
                            await DefaultExpectedFlow(data);
                            break;
                        case ResultState.UNEXPECTED_AMOUNT:
                            await UnexpectedAmountFlow(data);
                            break;
                        case ResultState.UNEXPECTED_TIME:
                            await UnexpectedTimeFlow(data);
                            break;
                        case ResultState.SYS_ERR:
                            Log.Debug("SYS ERR EXPECTED NOR READY");
                            break;
                    }

                }

            }
            catch (Exception e)
            {
                Log.Error("Expected cash error: " + e.Message);
                Log.Error(e.StackTrace);

                Exception u = e.InnerException;

                while (u != null)
                {
                    Log.Error("Expected cash inner error: " + u.Message);
                    Log.Error(u.StackTrace);
                    u = u.InnerException;
                }

            }
            running = false;

        }


        private string createBegin()
        {

            var begin = DateTime.Now.AddDays(-1);


            return begin.ToString("yyyy-MM-dd");
        }

        private string createEnd()
        {
            var end = DateTime.Now;
            return end.ToString("yyyy-MM-dd");
        }


        /// <summary>
        /// Asynchronously searches expected cash in kt transactions
        /// </summary>
        /// <param name="expectedCash"></param>
        /// <returns></returns>
        private async Task<ResultTuple> SearchExpected(VendorExpected expectedCash)
        {
            var result = new ResultTuple
            {
                expected = expectedCash,
                ok = false,
                ktReference = "",
                state = ResultState.SYS_ERR
            };
            var queryString = "beginDate=" + createBegin() + "&endDate=" + createEnd();
            var hareketler = await KTApiService.GetHesapHareketleriASync(expectedCash.ExpectedSuffix,
                queryString);

            if (hareketler == null || hareketler.Success == false)
            {
                Log.Debug("expected robot not able to read kt transactions");
                result.state = ResultState.SYS_ERR;
                return result;
            }



            // create result

            //"c3ab58ac-ead2-4c2a-8d5d-08d95bfa7988 - Para Transferi, Gönderen: MÜMTAZ TORUNOĞLU  , Alıcı: FİNTAG YAZILIM DANIŞMANLIK ANONİM ŞİRKETİ"
            // search transaction in transactions

            var code = vendorBuySellService.GetCodeOf(expectedCash);
            Log.Debug("expected code: " + code);
            var ktTransaction = hareketler.Value.AccountActivities
                .Where(x => Utility.ExtractVendorHavaleCode(x.Description) ==code)
                .FirstOrDefault();

            if (ktTransaction != null)
            {
                result.ktReference = ktTransaction.TransRef;
                result.kTTransaction = ktTransaction;
                // transaction found
                Log.Debug("expected found KT transaction " + JsonConvert.SerializeObject(ktTransaction));

                if (ktTransaction.Amount != expectedCash.ExpectedTRY)
                {
                    //result.expected = null;
                    result.ok = false;
                    Log.Debug(string.Format("robot notices kt amount and expected amount not ok {0} {1}", ktTransaction.Amount, expectedCash.ExpectedTRY));

                    
                    var unexpected = new VendorUnExpected(expectedCash.TransactionId, ktTransaction.TransRef, expectedCash.ExpectedSuffix, expectedCash.Amount, "MONEY_ISSUE", expectedCash.ExpectedTRY, ktTransaction.Amount);
                   
                    await vendorsRepository.AddVendorUnExpected(unexpected);
                    await vendorsRepository.RemoveVendorExpected(expectedCash);
                    await vendorsRepository.SaveChangesAsync();

                    result.state = ResultState.UNEXPECTED_AMOUNT;

                   // Log.Debug("expected created unexpected: " + JsonConvert.SerializeObject(unexpected));
                    return result;
                }
                
                result.ok = true;
                result.state = ResultState.OK;
                Log.Debug("expected found transaction and all ok for now: " + JsonConvert.SerializeObject(result));
            }
            else
            {

                Log.Debug("kt trans not match for expected");
                var diff = DateTime.Now - expectedCash.DateTime;

                if (diff.TotalMinutes > 30)
                {
                    // never received 
                    var unexpected = new VendorUnExpected(expectedCash.TransactionId,
                        "TIME_OUT",
                        expectedCash.ExpectedSuffix,
                        expectedCash.Amount,
                        "TIME_OUT",
                        expectedCash.ExpectedTRY,
                        null,
                        (int)diff.TotalMinutes);
                    await vendorsRepository.AddVendorUnExpected(unexpected);
                    await vendorsRepository.RemoveVendorExpected(expectedCash);
                    await vendorsRepository.SaveChangesAsync();
                    Log.Debug("expected not ok since 1 min and no money");
                    result.expected = null;
                    result.totalMinutes = (int)diff.TotalMinutes;
                    result.ok = false;
                    result.state = ResultState.UNEXPECTED_TIME;
                }
            }

            return result;
            

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~VendorExpectedChecker()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    
    }
    
}
