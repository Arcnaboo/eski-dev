using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Services;
using Gold.Api.Utilities;
using Gold.Core.Vendors;
using Gold.Domain.Vendors;
using Newtonsoft.Json;
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
    public class ExpectedCashChecker
    {

        private IVendorsRepository vendorsRepository;
        private bool running;

        public ExpectedCashChecker(IVendorsRepository vendorsRepository)
        {
            this.vendorsRepository = vendorsRepository;
            this.running = false;
            Log.Information("ExpectedCashChecker Created");
        }


        private class ResultTuple
        {
            public bool ok { get; set; }
            public ExpectedCash expected { get; set; }
            public KTTransaction kTTransaction { get; set; }
            public string ktReference { get; set; }
        }

        public async void Run()
        {
            running = true;
            // runs asynchrounously
            await Task.Run(() => {

                Log.Debug("ExpectedCashChecker FLOW");
               
                try
                {
                    if (running)
                    {
                        Log.Debug("Expected already running");
                        return;
                    }

                    var expectedRun = vendorsRepository.GetRobotStatus();
                    if (expectedRun == null || !expectedRun.Amount.HasValue || ((int)expectedRun.Amount.Value) == 0)
                    {
                        Log.Debug("Not running expected cash");
                        return;
                    }

                    if (KuwaitAccessHandler.AccessToken == null)
                    {
                        Log.Debug("Expected cash not ready because not connected to KT");
                        return;
                    }
                    //var fx = KTApiService.GetCurrentPriceFromKtApi();// current price
                    Log.Debug("Expected cash robot flow start");
                    var expecteds = vendorsRepository.GetExpectedCashes()
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
                        var ok = data.ok; // if money received
                        var expected = data.expected; // expextedCash
                        var ktref = data.ktReference; // ref
                        var vendorTask = vendorsRepository.GetVendorAsync(expected.VendorId);
                        vendorTask.Wait();
                        var vendor = vendorTask.Result;
                        Log.Debug("Current expected: " + expected.ToString());
                        if (ok)
                        {
                            var transaction = vendorsRepository
                                    .GetVendorTransaction(expected.TransactionId);// related transaction

                            var rate = KTApiService.GetCurrentPriceFromKtApi()
                                    .value.FxRates
                                    .Where(x => x.FxId == expected.RateFxId)
                                    .FirstOrDefault();
                           
                            Log.Debug("received current price: " + rate.BuyRate.ToString());
                            var vendorData = vendorsRepository.GetVendorDatas()
                                .Where(x => x.RelatedId == transaction.Destination.ToString())
                                .FirstOrDefault();

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
                            while (attempts < 5 && success == false)
                            {
                                var metalBuy = KTApiService.PreciousMetalBuy(param);
                                Log.Debug("metalBuy: " + JsonConvert.SerializeObject(metalBuy));
                                success = metalBuy.Success;
                                refId = metalBuy.RefId;
                                attempts++;

                            }
                            var comm = (expected.RateFxId == 24) ? "ROBOT-GOLD" : "ROBOT-SILVER";
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
                                var finalized = new FinalizedGold(expected, true, ktref, comm, tl, gram);
                                var diff = DateTime.Now - transaction.TransactionDateTime;
                                finalized.PiyasaGramFiyat = rate.BuyRate;
                                Log.Debug("expected time DIFF in seconds - " + diff.TotalSeconds.ToString());
                                /*if (diff.TotalSeconds >= 300)
                                {
                                   
                                    finalized.Comments += "-GECIKMELI ODEME - " + DateTime.Now.ToLongTimeString() + " -- ";
                                }*/


                                vendorsRepository.AddFinalizedGold(finalized);
                                vendorsRepository.RemoveExpected(expected); // remove expected
                                vendorsRepository.SaveChanges(); // save things

                                Log.Debug("expected ok created finalized removed expected also bought gold from kt - ROBOT completed task");

                            }
                            else
                            {
                                var notPos = new NotPosGold(refId, transaction.GramAmount,
                                    suffFrom, suffTo, rate.BuyRate, transaction.TransactionId,
                                    transaction.Destination, comm);
                                vendorsRepository.AddNotPosGold(notPos);
                                vendorsRepository.RemoveExpected(expected);
                                vendorsRepository.SaveChanges();
                                Log.Debug("expected ok but precious metal could not be bought from kt created notPos - " + JsonConvert.SerializeObject(notPos));
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
        private async Task<ResultTuple> SearchExpected(ExpectedCash expectedCash)
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
                Log.Debug("expected robot not able to read kt transactions");
                
                return result;
            }
            

            return await Task.Run(() => {
                // create result

                //"c3ab58ac-ead2-4c2a-8d5d-08d95bfa7988 - Para Transferi, Gönderen: MÜMTAZ TORUNOĞLU  , Alıcı: FİNTAG YAZILIM DANIŞMANLIK ANONİM ŞİRKETİ"
                // search transaction in transactions
                var ktTransaction = hareketler.Value.AccountActivities
                    .Where(x => Utility.GetGuid(x.Description).ToString() == expectedCash.TransactionId.ToString())
                    .FirstOrDefault();
                

                if (ktTransaction != null)
                {
                    // transaction found
                    Log.Debug("expected found KT transaction " + JsonConvert.SerializeObject(ktTransaction));

                    if (ktTransaction.Amount != expectedCash.ExpectedTRY)
                    {
                        result.expected = null;
                        result.ok = false;
                        Log.Debug(string.Format("robot notices kt amount and expected amount not ok {0} {1}", ktTransaction.Amount, expectedCash.ExpectedTRY));


                        var unexpected = new UnexpectedCash(expectedCash.TransactionId, ktTransaction.TransRef, expectedCash.ExpectedSuffix, ktTransaction.Amount);
                        vendorsRepository.AddUnexpectedCash(unexpected);
                        vendorsRepository.RemoveExpected(expectedCash);
                        vendorsRepository.SaveChanges();


                        Log.Debug("expected created unexpected: " + JsonConvert.SerializeObject(unexpected));
                        return result;
                    }

                    result.kTTransaction = ktTransaction;
                    result.ok = true;
                    result.ktReference = ktTransaction.TransRef;
                    Log.Debug("expected found transaction and all ok for now: " + JsonConvert.SerializeObject(result));
                } 
                else
                {
                    var diff = DateTime.Now - expectedCash.DateTime;

                    if (diff.TotalMinutes > 10)
                    {
                        // never received 
                        var notPos = new NotPosGold("TIME_OUT", expectedCash.Amount,
                                    expectedCash.ExpectedSuffix, "TIME_OUT", expectedCash.SatisGramFiyat, expectedCash.TransactionId,
                                    expectedCash.VendorId, "TIME_OUT");
                        vendorsRepository.AddNotPosGold(notPos);
                        vendorsRepository.RemoveExpected(expectedCash);
                        vendorsRepository.SaveChanges();
                        Log.Debug("expected not ok since 10 min and no money");
                        result.expected = null;
                        result.ok = false;
                    }
                }


                return result;
            });
            
        }
    }
}
