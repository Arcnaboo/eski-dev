using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Services;
using Gold.Api.Utilities;
using Gold.Core.Transactions;
using Gold.Domain.Transactions;
using Gold.Domain.Transactions.Interfaces;
using Serilog;

namespace Gold.Api.Robots
{

    /*
     
     money pay altin satiyor
    1 gr 450 lira

    vendor balance -1 gr
    gtag TL suffix > moneypay tl suffix interbank transfer
    gtag's moneypay gold suffix > gtag tl/gold suffix interbank transfer / sell preciuous metal

     
     
     */
    public class ExpectedCashCheckerGoldtag
    {

        private ITransactionsRepository repository;
        private bool running;

        public ExpectedCashCheckerGoldtag(ITransactionsRepository repository)
        {
            this.repository = repository;
            this.running = false;
            Log.Information("ExpectedCashCheckerGoldtag Created");
        }


        private class ResultTuple
        {
            public bool ok { get; set; }
            public GoldtagExpected expected { get; set; }
            public KTTransaction kTTransaction { get; set; }
            public string ktReference { get; set; }
        }
        /*
        public async void Run()
        {
            running = true;
            // runs asynchrounously
            await Task.Run(() => {

                //Log.Information("ExpectedCashChecker FLOW");

              
               
                try
                {


                    var expectedRun = repository.ShouldGoldtagExpectedRobotRun();
                    if (!expectedRun)
                    {
                        Log.Information("Not running goldtag expected cash");
                        return;
                    }

                    if (KuwaitAccessHandler.AccessToken == null)
                    {
                        Log.Information("KUWAIT TURK ACCESS not ready");
                        return;
                    }
                    //var fx = KTApiService.GetCurrentPriceFromKtApi();// current price

                    Log.Debug("Expected cash robot flow start");
                    var expecteds = repository.GetGoldtagExpecteds()
                        .OrderByDescending(x => x.ExpectedDateTime)
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
                    //var rand = new Random();
                    for (var i = 0; i < index; i++)
                    {
                        var data = tasks[i].Result;
                        var ok = data.ok; // if money received
                        var expected = data.expected; // expextedCash
                        var ktref = data.ktReference; // ref
                        
                        Log.Debug("EXpected cash: " + expected.ToString());
                        if (ok)
                        {
                            var user = repository.GetAllUsers().Where(x => x.UserId == expected.UserId).FirstOrDefault();
                            var bankTransfer = repository.GetAllUserBankTransferRequests().Where(x => x.BankTransferId == expected.BankTransferId).FirstOrDefault();
                            var normTransfer = repository.GetAllTransferRequests().Where(x => x.TransferRequestId == bankTransfer.TransferRequestId).FirstOrDefault();
                            var transaction = repository.GetAllTransactions().Where(x => x.TransactionId == normTransfer.TransactionRecord).FirstOrDefault();
                            var silverBalance = repository.GetSilverBalance(user.UserId);

                            var rate = KTApiService.GetCurrentPriceFromKtApi()
                                    .value.FxRates
                                    .Where(x => x.FxId == expected.RateFxId)
                                    .FirstOrDefault();

                            Log.Debug("received rate :" + rate.ToString());
                            var suffFrom = "1";
                            // FINTAG ALTIN = 101
                            // FINTAG GUMUS = 103
                            var suffTo = (expected.RateFxId == 24) ? "101" : "103";

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
                            while (attempts < 5 && success == false)
                            {
                                Log.Debug("Buy attempt: " + attempts.ToString());
                                var metalBuy = KTApiService.PreciousMetalBuy(param);
                                
                                success = metalBuy.Success;
                                if (!success)
                                {
                                    Log.Debug("metalbuy fail: " + metalBuy.RefId);
                                }
                                refId = metalBuy.RefId;
                                attempts++;

                            }
                            var comm = (expected.RateFxId == 24) ? "EFT ile altın alımı" : "EFT ile gümüş alımı";
                            var transType = (expected.RateFxId == 24) ? "GOLD" : "SILVER";

                            if (success == true)
                            {
                                Log.Debug("Success true so handling manipualte balance");
                                var madenTransaction = new Transaction(
                                    transType,
                                    "D94AF0AD-5FE9-4A6C-949E-C66AEF21C907",
                                    "Fintag",
                                    user.UserId.ToString(),
                                    "User",
                                    transaction.GramAmount,
                                    transaction.TransactionId.ToString(),
                                    true,
                                    transaction.GramAmount,
                                    transaction.Amount);
                                madenTransaction.YekunDestination = transaction.Yekun + normTransfer.GramsOfGold;

                                if (expected.RateFxId == 24)
                                {
                                    user.ManipulateBalance(transaction.GramAmount);
                                } 
                                else
                                {
                                    silverBalance.Balance += transaction.GramAmount;
                                }
                                bankTransfer.MoneyReceived = true;
                                normTransfer.CompleteTransfer();
                                transaction.Confirm();
                                repository.AddTransaction(madenTransaction);
                                repository.SaveChanges();
                                madenTransaction = repository.GetAllTransactions().Where(x => x.Comment != null && x.Comment == transaction.TransactionId.ToString()).FirstOrDefault();
                                madenTransaction.Comment = comm;
                                var tl = transaction.Amount;
                                var gram = transaction.GramAmount * (-1);
                                // hmm acaba transaction id olarak TRY transaction i mi koysam yoksa GOLD/SILVEr transaction i mi
                                var finalized = new GoldtagFinalized(user.UserId, transaction.TransactionId, transaction.GramAmount,
                                    transaction.Amount, transType, "Robot created: " + madenTransaction.TransactionId.ToString());

                                repository.AddGoldtagFinalized(finalized);
                                repository.RemoveGoldtagExpected(expected);
                                repository.SaveChanges();

                                //Log.Information("expected ok created finalized removed expected also bought gold from kt - ROB complete");

                            }
                            else
                            {
                                Log.Debug
                                ("unable to buy METAL but money received");

                                var notPos = new GoldtagNotPosition(user.UserId, expected.BankTransferId,
                                    expected.ExpectedSuffix, expected.ExpectedTRY, expected.RateFxId, DateTime.Now,
                                    rate.BuyRate, expected.Amount, expected.PiyasaGramFiyat, expected.KTGramFiyat, expected.SatisGramFiyat,
                                    ktref);
                                repository.AddGoldtagNotPosition(notPos);
                                repository.RemoveGoldtagExpected(expected);
                                repository.SaveChanges();
                            }

                            
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




            });
            
        }
        */

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
        /*private async Task<ResultTuple> SearchExpected(GoldtagExpected expectedCash)
        {
            var result = new ResultTuple
            {
                expected = expectedCash,
                ok = false,
                ktReference = ""
            };
            var queryString = "beginDate=" + createBegin() + "&endDate=" + createEnd();
            var hareketler = KTApiService.GetHesapHareketleri(expectedCash.ExpectedSuffix,
                queryString);

            if (hareketler == null || hareketler.Success == false)
            {
                Log.Information("goldtag expected robot not able to read kt transactions");
                
                return result;
            }
            

            return await Task.Run(() => {
                // create result

                // "description":"319163 - Gönderen:HÜSEYİN BAŞARAN, Alıcı:fintag a.ş, FAST Para Transferi",
                // search transaction in transactions

                var bankTransferRequest = repository.GetAllUserBankTransferRequests().Where(x => x.BankTransferId == expectedCash.BankTransferId).FirstOrDefault();
                
                if (bankTransferRequest == null)
                {
                    Log.Error("Goldtag expecte cash robot unable to find bank transfer request - expectedId: " + expectedCash.ExpectedGoldtagId);
                    result.expected = null;
                    result.ok = false;
                    return result;
                }
                var specialCode = bankTransferRequest.SpecialCode;
                var ktTransaction = hareketler.Value.AccountActivities
                    .Where(x => Utility.GetSpecialCode(x.Description) == specialCode)
                    .FirstOrDefault();
                

                if (ktTransaction != null)
                {
                    // transaction found
                    if (ktTransaction.Amount != expectedCash.ExpectedTRY)
                    {
                        result.expected = null;
                        result.ok = false;
                        Log.Debug(string.Format("kt amount and expected amount not ok {0} {1}", ktTransaction.Amount, expectedCash.ExpectedTRY));


                        var unexpected = new GoldtagUnexpected(expectedCash.UserId, bankTransferRequest.BankTransferId, expectedCash.ExpectedTRY, ktTransaction.Amount);
                        repository.AddGoldtagUnexpected(unexpected);
                        repository.RemoveGoldtagExpected(expectedCash);
                        repository.SaveChanges();
                        return result;
                    }
                    var now = DateTime.Now;
                    var expectedDateTime = expectedCash.ExpectedDateTime;

                    var diff = now - expectedDateTime;
                    var minutes = diff.TotalMinutes;
                    
                    if (minutes > 60)
                    {
                        var rate = KTApiService.GetCurrentPriceFromKtApi().value.FxRates.Where(x => x.FxId == expectedCash.RateFxId).FirstOrDefault().BuyRate;
                        var notPosition = new GoldtagNotPosition(
                                expectedCash.UserId,
                                expectedCash.BankTransferId,
                                expectedCash.ExpectedSuffix,
                                ktTransaction.Amount,
                                expectedCash.RateFxId,
                                now,
                                rate,
                                expectedCash.Amount,
                                expectedCash.PiyasaGramFiyat,
                                expectedCash.KTGramFiyat,
                                expectedCash.SatisGramFiyat,
                                ktTransaction.TransRef,
                                "TIME_OUT"
                            );
                        repository.AddGoldtagNotPosition(notPosition);
                        repository.RemoveGoldtagExpected(expectedCash);
                        repository.SaveChanges();
                        result.expected = null;
                        result.ok = false;
                        Log.Debug("goldtag expected received late mioved into notposition");
                        return result;
                    }

                    result.kTTransaction = ktTransaction;
                    result.ok = true;
                    result.ktReference = ktTransaction.TransRef;
                }
                else
                {
                    var now = DateTime.Now;
                    var expectedDateTime = expectedCash.ExpectedDateTime;

                    var diff = now - expectedDateTime;
                    var minutes = diff.TotalMinutes;
                    if (minutes > 60) // bu dk ya yi GoldPrices dan manipule edilebilir yap
                    {
                        // hareket yok ve 25 dk gecmis
                        var totalSeconds = diff.TotalSeconds;
                        var notReceived = new GoldtagNotReceived(
                            expectedCash.UserId,
                            expectedCash.BankTransferId,
                            expectedCash.ExpectedSuffix,
                            expectedCash.ExpectedTRY,
                            expectedCash.RateFxId,
                            expectedDateTime,
                            now,
                            (int)totalSeconds,
                            expectedCash.Amount,
                            expectedCash.KTGramFiyat,
                            expectedCash.PiyasaGramFiyat,
                            expectedCash.SatisGramFiyat,
                            "TIME_OUT");
                        repository.AddGoldtagNotReceived(notReceived);
                        repository.RemoveGoldtagExpected(expectedCash);
                        repository.SaveChanges();
                        result.ok = false;
                        result.expected = null;

                    }
                }


                return result;
            });
            
        }*/
    }
}
