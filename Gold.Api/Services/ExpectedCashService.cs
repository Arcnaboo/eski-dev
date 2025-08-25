using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Services.Interfaces;
using Gold.Core.Vendors;
using Gold.Domain.Vendors;
//using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Gold.Api.Utilities;
using Newtonsoft.Json;
using System.Text;
using System.Globalization;

namespace Gold.Api.Services
{
    public class ExpectedCashService : IHostedService, IAsyncDisposable, IDisposable, IExpectedCashService
    {

        
        private readonly IVendorsRepository vendorsRepository;
        private readonly List<VendorExpected> ExpectedForGold = new List<VendorExpected>();
        private readonly List<VendorExpected> ExpectedForSilver = new List<VendorExpected>();
        private readonly List<string> UsedFirstLasts = new List<string>();
        private readonly List<VendorExpected> MainExpectedsList = new List<VendorExpected>();
        private bool FirstTimer = true;
        private Timer timer = null;
        private bool Running = false;
        private DateTime LastRunTime = DateTime.Now;
        private FxResultModel LastGoodFxResultModel = null;
        private int MinutesWait = 15;
        private int TimeoutMinutes = 20;
        private bool ExpectedRun = true;
        private bool StopSilverSinceSpread = false;
        private bool StopGoldSinceSpread = false;
        /*
         * 
           1) Expectedlarin icinden .Select(x => x.ExpectedSuffix)
           2) ExpectedSuffixlerin hepsi icin 1 kt transaction aranicak daha onceden UsedFIrstLast ta olmayan
           3) KtTransactionlari DoWork e verilcek
           4) DoWork ktTransacion in firstLast valuesunun first id sine bakip, bu id ve benzeri transactionlari last id ye kadar expecteds dan cekicek
           5) altin yada gumus alicak
           7) finalize yada notpos edecek
         */

        public ExpectedCashService(IVendorsRepository repository)
        {
            vendorsRepository = repository;
        }

        public void StartBackStoppedService(string madenType)
        {
            if (madenType == "GOLD")
            {
                StopGoldSinceSpread = false;
            }
            if (madenType == "SILVER")
            {
                StopSilverSinceSpread = false;
            }

        }

        public void AddExpectedCash(VendorExpected expected)
        {
            Thread.BeginCriticalRegion();
            MainExpectedsList.Add(expected);
            Thread.EndCriticalRegion();
            Log.Debug("Added Expected: " + expected.ToString());
        }

        public void SetExpectedRun(bool status)
        {
            ExpectedRun = status;
        }

        public void SetBuyMinutes(int minutes)
        {
            MinutesWait = minutes;
        }

        public void SetTimeoutMinutes(int minutes)
        {
            TimeoutMinutes = minutes;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public async Task<MetalBuyResult> ClosePosition(VendorNotPosition notPosition)
        {
            var fxid = (notPosition.Comments.Contains("SILVER")) ? 26 : 24;
            var vendor = await vendorsRepository.GetVendorAsync(notPosition.VendorId);
            var vendorData = await vendorsRepository.GetVendorDataAsync(vendor.VendorId);
            var suffTo = (fxid == 26) ? vendorData.SLVSuffix : vendorData.GOLDSuffix;
           

            var success = false;
            MetalBuyResult result = null;
            var attempts = 0;
            while (attempts <= 5 && success == false)
            {
                var fxRates = (await KTApiService.GetCurrentPriceFromKtApiASync());
                if (fxRates != null)
                {
                    LastGoodFxResultModel = fxRates;
                }
                if (fxRates == null)
                {
                    fxRates = LastGoodFxResultModel;
                }
                var rate = fxRates.value.FxRates.Where(x => x.FxId == fxid).FirstOrDefault();
                var param = new PreciousMetalsBuyParams
                {
                    Amount = notPosition.Amount,
                    SuffixFrom = vendorData.TLSuffix,
                    SuffixTo = suffTo,
                    UserName = KuwaitAccessHandler.FINTAG,
                    BuyRate = rate.BuyRate
                };
                attempts++;
                var metalBuy = await KTApiService.PreciousMetalBuyAsync(param);
                if (metalBuy == null)
                {
                    continue;
                }
                success = metalBuy.Success;
                result = metalBuy;
            }

            return result;
        }

        private async Task<List<VendorExpected>> UnExpectedByServerStop()
        {
            var result = new List<VendorExpected>();
            // var expecteds = MainExpectedsList;
           // var comment = string.Format("server closing at time {0}", DateTime.Now);
            Thread.BeginCriticalRegion();

            foreach (var expected in MainExpectedsList)
            {
                var entityEntry = await vendorsRepository.AddVendorExpected(expected);

                result.Add(entityEntry.Entity);
            }
            MainExpectedsList.Clear();
            Thread.EndCriticalRegion();
            

            await vendorsRepository.SaveChangesAsync();
            return result;
        }

        private async Task LoadExpectedsAtStart()
        {
            var expecteds = await vendorsRepository.GetVendorExpectedsAsync();
            MainExpectedsList.AddRange(expecteds);
            await vendorsRepository.RemoveVendorExpecteds(expecteds);
        }

        private async Task<VendorUnExpected> UnExpectedByTime(string firstLast, 
            List<VendorExpected> expecteds, string comment, 
            decimal totalGrams, decimal totalExpected, int timeDifference)
        {
            try
            {
                var unexpected = new VendorUnExpected(
                expecteds.Count,
                firstLast,
                expecteds.First().ExpectedSuffix,
                totalGrams,
                comment,
                totalExpected,
                null,
                timeDifference);
                await vendorsRepository.AddVendorUnExpected(unexpected);
                await vendorsRepository.SaveChangesAsync();
                return unexpected;
            }
            catch (Exception e)
            {
                Log.Error("ExpectedCashService UnExpectedByTime Error: " + e.Message);
                Log.Error(e.StackTrace);
                SMSService.SendSms("05448980702", "UnExpectedByTime UnExpected Error: " + e.Message);
                return null;
            }
           
        }

        private async Task<VendorUnExpected> UnExpected(DoWorkParams workParams, string comment, decimal totalGrams, decimal totalExpected)
        {
            try
            {
                var unexpected = new VendorUnExpected(workParams.Expecteds.Count,
                workParams.FirstLast,
                workParams.Expecteds.First().ExpectedSuffix,
                totalGrams,
                comment,
                totalExpected,
                workParams.KTransaction.Amount
                );

                await vendorsRepository.AddVendorUnExpected(unexpected);
                await vendorsRepository.SaveChangesAsync();
                return unexpected;
            }
            catch (Exception e)
            {
                Log.Error("ExpectedCashService UnExpected Error: " + e.Message);
                Log.Error(e.StackTrace);
                SMSService.SendSms("05448980702", "ExpectedCashService UnExpected Error: " + e.Message);
                return null;
            }

           
        }

        private async Task<VendorNotPosition> NotPositioning(List<VendorExpected> expecteds, 
            Vendor vendor, VendorData vendorData, string comments, 
            string firstLast, decimal buyRate, 
            decimal totalTry, decimal totalGram)
        {
            try
            {
                var suffTo = (comments.StartsWith("GOLD")) ? vendorData.GOLDSuffix : vendorData.SLVSuffix;
                var avgPiyasa = expecteds.Select(x => x.PiyasaGramFiyat).Average();
                var avgBuy = expecteds.Select(x => x.SatisGramFiyat).Average();
                var expected = expecteds[0];
                var transaction = vendorsRepository.GetVendorTransactionNew(expecteds[0].TransactionId);
                var notPos = new VendorNotPosition(
                    firstLast,
                    totalGram,
                    vendorData.TLSuffix,
                    suffTo,
                    totalTry,
                    expecteds.Count,
                    vendor.VendorId,
                    comments,
                    avgPiyasa,
                    buyRate,
                    avgBuy);

                await vendorsRepository.AddVendorNotPosition(notPos);

                await vendorsRepository.SaveChangesAsync();

                return notPos;
            }
            catch (Exception e)
            {
                Log.Error("ExpectedCashService NotPositioning Error: " + e.Message);
                Log.Error(e.StackTrace);
                SMSService.SendSms("05448980702", "ExpectedCashService NotPositioning Error: " + e.Message);
                return null;
            }
            
        }
        
        private async Task<VendorFinalized> Finalized(List<VendorExpected> expecteds, Vendor vendor, MetalBuyResult res, string madenType, string comments, string firstLast, decimal totalTry, decimal totalGram, decimal ktRate, decimal fintagRate)
        {
            try
            {
                foreach (var expected in expecteds)
                {
                    var trans = await vendorsRepository.GetVenTransactionAsync(expected.TransactionId);
                    trans.TamamlanmisKTreferans = res.RefId;
                    trans.GercekKur = fintagRate;
                }
                var averageSatisFiyat = expecteds.Select(x => x.SatisGramFiyat).Average();
                var averagePiyasaFiyat = expecteds.Select(x => x.PiyasaGramFiyat).Average();

                /*
                 KAR(Goldtag den altın alma) : GoldAmount * ( SatışGRAMFiyat -KTGramFiyat )
                 */

                var finalKar = totalGram * (averageSatisFiyat - ktRate);

                var tl = totalTry;
                var gram = totalGram * (-1);
                var final = new VendorFinalized(
                    vendor.VendorId,
                    expecteds.Count,
                    firstLast,
                    totalTry,
                    totalGram,
                    averagePiyasaFiyat,
                    ktRate,
                    averageSatisFiyat,
                    madenType,
                    res.RefId,
                    comments,
                    tl,
                    gram,
                    finalKar);
                await vendorsRepository.AddVendorFinalized(final);

                await vendorsRepository.SaveChangesAsync();

                return final;
            }
            catch (Exception e)
            {
                Log.Error("ExpectedCashService Finalized Error: " + e.Message);
                Log.Error(e.StackTrace);
                SMSService.SendSms("05448980702", "ExpectedCashService Finalized Error: " + e.Message);
                return null;
            }
           

            
        }

        
        /// <summary>
        /// Class that represents do work params, 
        /// Ktt transaction
        /// List of expecteds
        /// and first last ids
        /// </summary>
        private class DoWorkParams
        {
            public KTTransaction KTransaction { get; set; }
            public List<VendorExpected> Expecteds { get; set; }
            public string FirstLast { get; set; }
        }
        private static string ExtractFirstLast(string description)
        {
            if (string.IsNullOrEmpty(description) || string.IsNullOrWhiteSpace(description) ||
                !description.Contains("___"))
            {
                return null;
            }
            var index = description.IndexOf("___");
            index++;
            var leftStart = index - 2; // x___
            var rightEnd = index + 2; // ___x

            for (; leftStart >= 0; leftStart--)
            {
                if (!char.IsDigit(description[leftStart]))
                {
                    break;
                }
            }
            leftStart++;

            for (; rightEnd < description.Length; rightEnd++)
            {
                if (!char.IsDigit(description[rightEnd]))
                {
                    break;
                }
            }
            var length = rightEnd - leftStart;
            var firstLast = description.Substring(leftStart, length);
            return firstLast;
        }

        /// <summary>
        /// depreceated
        /// </summary>
        /// <param name="vendorData"></param>
        /// <returns></returns>
        private async Task UpdateVendorData(VendorData vendorData)
        {
            var accountStatus = await KTApiService.AccountStatusAsync();
            if (accountStatus == null || !accountStatus.Success)
            {
                Log.Debug("UpdateVendorData account status null yada succes yok");
                if (accountStatus != null)
                    Log.Debug("UpdateVendorData " + JsonConvert.SerializeObject(accountStatus));
                return;
            }
            var goldSuffix = int.Parse(vendorData.GOLDSuffix);
            var goldAccount = accountStatus.Value.AccountList.Where(x => x.Suffix == goldSuffix).FirstOrDefault();
            if (goldAccount != null)
            {
                //goldAccount.v
            }
        }

        /// <summary>
        /// Does completes buy gold/silver from KT
        /// </summary>
        /// <param name="workParams">Parameters for the job</param>
        /// <returns>List of objects such as FinalizedTransaction</returns>
        private async Task<List<object>> DoWorkNewVersionRealBuy(DoWorkParams workParams)
        {
            var results = new List<object>();
            try
            {
                var firstLast = workParams.FirstLast;
                var ktTransaction = workParams.KTransaction;
                var vendor = await vendorsRepository.GetVendorAsync(workParams.Expecteds[0].VendorId);
                var vendorData = await vendorsRepository.GetVendorDataAsync(vendor.VendorId);
                var groups = workParams.Expecteds.GroupBy(x => x.RateFxId).ToList();
                var totalTRY = workParams.Expecteds.Select(x => x.ExpectedTRY).Sum();
                Log.Debug("ExpectedCashService: do work new version stuff ready for buy");
                if (totalTRY != ktTransaction.Amount)
                {
                    // id ler uyusuyor ama para uyusmuyor
                    Log.Debug("ExpectedCashService: id ler uyusuyor ama para uyusmuyor");
                    var fark = totalTRY - ktTransaction.Amount;
                    var totalGold = 0m;
                    var totalSilver = 0m;
                    var totalPlatin = 0m;
                    foreach (var grp in groups)
                    {
                        switch (grp.Key)
                        {
                            case 24:
                                totalGold = grp.ToList().Select(x => x.Amount).Sum();
                                break;
                            case 26:
                                totalSilver = grp.ToList().Select(x => x.Amount).Sum();
                                break;
                            case 27:
                                totalPlatin = grp.ToList().Select(x => x.Amount).Sum();
                                break;

                        }
                    }
                    var comment = string.Format("Beklenen para uyusmadi fark {0} gramajlar(alt, slv, plt):{1},{2},{3}", fark, totalGold, totalSilver, totalPlatin);
                    var errRes = await UnExpected(workParams, comment, totalGold + totalPlatin + totalSilver, totalTRY);
                    Log.Debug("ExpectedCashService: Created unexpecteds: " + JsonConvert.SerializeObject(errRes));
                    results.Add(errRes);
                    return results;
                }

                foreach (var grp in groups)
                {
                    Log.Debug("ExpectedCashService: for grp: " + grp.ToString());
                    var expecteds = grp.ToList();
                    var grpGrams = expecteds.Select(x => x.Amount).Sum();
                    var grpTry = expecteds.Select(x => x.ExpectedTRY).Sum();
                    string madenType = null;
                    var fintagRate = 0m;
                    var suffTo = "";
                    var stopBecauseofThreshold = false;
                    var threshholdInvalidWarning = false;
                    decimal threshold = 0;
                    if (grp.Key == 24)
                    {
                        madenType = "GOLD";
                        fintagRate = GlobalGoldPrice.GetGoldPricesCached().BuyRate;
                        suffTo = vendorData.GOLDSuffix;

                        if (vendorData.ThresholdGoldActive != null && 
                            vendorData.ThresholdGoldActive.HasValue &&
                            vendorData.ThresholdGoldActive.Value)
                        {
                            /*if (vendorData.BalanceThresholdGold == null ||
                                !vendorData.BalanceThresholdGold.HasValue)
                            {
                                threshholdInvalidWarning = true;
                            }
                            else if (vendor.Balance >= vendorData.BalanceThresholdGold.Value)
                            {
                                stopBecauseofThreshold = true;
                                threshold = vendorData.BalanceThresholdGold.Value;
                            }*/
                        }
                    }
                    if (grp.Key == 26)
                    {
                        fintagRate = GlobalGoldPrice.GetSilverPricesCached().BuyRate;
                        madenType = "SILVER";
                        suffTo = vendorData.SLVSuffix;
                        if (vendorData.ThresholdSilverActive != null &&
                            vendorData.ThresholdSilverActive.HasValue &&
                            vendorData.ThresholdSilverActive.Value)
                        {
                            /*if (vendorData.BalanceThresholdSilver == null ||
                                !vendorData.BalanceThresholdSilver.HasValue)
                            {
                                threshholdInvalidWarning = true;
                            }
                            else if (vendor.SilverBalance.Value >= vendorData.BalanceThresholdSilver.Value)
                            {
                                stopBecauseofThreshold = true;
                                threshold = vendorData.BalanceThresholdSilver.Value;
                            }*/
                        }
                    }
                    if (grp.Key == 27)
                    { 
                            fintagRate = GlobalGoldPrice.GetPlatinPricesCached().BuyRate;
                            madenType = "PLATIN";
                            suffTo = vendorData.PLTSuffix;
                    }

                    var fxRates = await KTApiService.GetCurrentPriceFromKtApiASync();
                    if (fxRates != null)
                    {
                        LastGoodFxResultModel = fxRates;
                    }
                    if (fxRates == null)
                    {
                        fxRates = LastGoodFxResultModel;
                    }
                    var rate = fxRates
                        .value.FxRates.Where(x => x.FxId == grp.Key)
                        .FirstOrDefault();

                    if (rate.IsSpreadApplied == "1")
                    {
                        var res = await NotPositioning(expecteds,
                            vendor, vendorData,
                            "SPREAD APPLIED at " + DateTime.Now.ToString(), firstLast, fintagRate,
                            grpTry, grpGrams);
                        results.Add(res);
                    }

                    if (stopBecauseofThreshold)
                    {
                        var res = await NotPositioning(expecteds,
                            vendor, vendorData,
                            string.Format("PARA GELDI AMA THRESHOLD {0} ASILAMAZ, fxId: {1}", threshold, grp.Key), firstLast, rate.BuyRate, grpTry, grpGrams);
                        results.Add(res);
                        continue;
                    }

                    if (madenType == null)
                    {
                        var res = await NotPositioning(expecteds, vendor, vendorData, string.Format("PARA GELDI AMA MADEN TYPE DA POZISYON KAPANMADI, fxId: {0}", grp.Key), firstLast, rate.BuyRate, grpTry, grpGrams);
                        results.Add(res);

                        continue;
                    }



                    var spread = false;
                    var success = false;
                    MetalBuyResult result = new() { RefId = "NULL" };
                    var attempts = 0;
                    while (attempts <= 5 && success == false)
                    {
                        fxRates = await KTApiService.GetCurrentPriceFromKtApiASync();
                        if (fxRates != null)
                        {
                            LastGoodFxResultModel = fxRates;
                        }
                        if (fxRates == null)
                        {
                            fxRates = LastGoodFxResultModel;
                        }
                        rate = fxRates.value.FxRates.Where(x => x.FxId == grp.Key).FirstOrDefault();
                        if (rate.IsSpreadApplied == "1")
                        {
                            spread = true;
                            break;

                        }
                        attempts++;
                        var param = new PreciousMetalsBuyParams
                        {
                            Amount = grpGrams,
                            SuffixFrom = vendorData.TLSuffix,
                            SuffixTo = suffTo,
                            UserName = KuwaitAccessHandler.FINTAG,
                            BuyRate = rate.BuyRate
                        };
                        Log.Debug("ExpectedCashService: buy metal param ready: " + JsonConvert.SerializeObject(param));
                        var metalBuy = await KTApiService.PreciousMetalBuyAsync(param);
                        if (metalBuy == null)
                        {
                            Log.Debug("ExpectedCashService: metalbuy null");
                            continue;
                        }
                        success = metalBuy.Success;
                        result = metalBuy;
                        Log.Debug("ExpectedCashService: metalbuy: " + JsonConvert.SerializeObject(result)) ;
                    }

                    if (spread)
                    {
                        var res = await NotPositioning(expecteds,
                            vendor, vendorData,
                            "SPREAD APPLIED at " + DateTime.Now.ToString(), firstLast, fintagRate,
                            grpTry, grpGrams);
                        results.Add(res);
                        continue;
                    }

                    if (!success)
                    {
                        var message = string.Format("KT den {0} alamadik - vendor: {1} - grams: {2} - TRY : {3} - notPosition eklendi", madenType, vendor.Name, grpGrams, grpTry);
                        var subject = "Metal Buy Fail";
                        await EmailService.SendEmailAsync("trnmmtz@gmail.com", subject, message, false);
                        await EmailService.SendEmailAsync("dolunaysabuncuoglu@gmail.com", subject, message, false);
                        await EmailService.SendEmailAsync("emreelvanus@gmail.com", subject, message, false);

                        var subResult = await NotPositioning(expecteds, vendor, vendorData, madenType + ":NO_METAL:" + result.RefId, firstLast, rate.BuyRate, grpTry, grpGrams);
                        results.Add(subResult);
                        Log.Debug("ExpectedCashService: " + message);
                        continue;
                    }

                    if (threshholdInvalidWarning)
                    {
                        var message = string.Format("vendor {0} icin {1} turunde {2}gr {3}tl ye alindi, ama threshold istenmesine ragmen threshold ayari duzugn yapilmamis", vendor.Name, madenType, grpGrams, grpTry);
                        /*SMSService.SendSms("05323878550", message);
                        SMSService.SendSms("05433893303", message);
                        SMSService.SendSms("05063337007", message);*/
                        var subject = "Metal Buy Threshold warning";
                        await EmailService.SendEmailAsync("trnmmtz@gmail.com", subject, message, false);
                        await EmailService.SendEmailAsync("dolunaysabuncuoglu@gmail.com", subject, message, false);
                        await EmailService.SendEmailAsync("emreelvanus@gmail.com", subject, message, false);

                    }

                    var finalizedResult = await Finalized(expecteds, vendor, result, madenType, "BUY:OK", firstLast, grpTry, grpGrams, rate.BuyRate, fintagRate);
                    results.Add(finalizedResult);
                    Log.Debug("ExpectedCashService: Added finalized " + JsonConvert.SerializeObject(finalizedResult));
                }
                
            }
            catch (Exception e)
            {
                Log.Error("ExpectedCashService DoWorkNewVersionRealBuy Error: " + e.Message);
                Log.Error(e.StackTrace);
                SMSService.SendSms("05448980702", "ExpectedCashService DoWorkNewVersionRealBuy Error: " + e.Message);
            }


            return results;
        }


        /// <summary>
        /// to be tested
        /// </summary>
        /// <returns></returns>
        private async Task DoWorkTimeoutChecker()
        {
            try
            {
                Log.Debug("ExpectedCashService: TIMEOUT CHECKER");
                var currentTime = DateTime.Now;
                var timedOuts = new List<VendorExpected>();
                var timeoutMinutes = TimeoutMinutes;
                Thread.BeginCriticalRegion();
                Log.Debug("ExpectedCashService: TIMEOUT VALUE " + timeoutMinutes.ToString());
                foreach (var expected in MainExpectedsList)
                {

                    var timeDiff = currentTime - expected.DateTime;
                    if (timeDiff.TotalMinutes >= timeoutMinutes)
                    {
                        Log.Debug("ExpectedCashService: TIMEOUT CHECKER TIMED OUT: " + expected.ToString());
                        timedOuts.Add(expected);
                    }
                }
                foreach (var expected in timedOuts)
                {
                    MainExpectedsList.Remove(expected);
                }
                Thread.EndCriticalRegion();
                //
                Log.Debug("ExpectedCashService: TIMEOUT CHECKER CONTINUE TO REAL CHECK");
                if (timedOuts.Any())
                {
                    var groups = timedOuts.GroupBy(x => x.ExpectedSuffix).ToList();
                    Log.Debug("ExpectedCashService: TIMEOUT CHECKER FOUND GROUPS");
                    foreach (var grp in groups)
                    {
                        Log.Debug("ExpectedCashService: TIMEOUT CHECKER for grp " + grp.ToString());
                        var expecteds = grp.ToList();
                        var fxRates = (await KTApiService.GetCurrentPriceFromKtApiASync());
                        if (fxRates != null)
                        {
                            LastGoodFxResultModel = fxRates;
                        }
                        if (fxRates == null)
                        {
                            fxRates = LastGoodFxResultModel;
                        }
                        var rate = fxRates.value.FxRates.Where(x => x.FxId == expecteds[0].RateFxId).FirstOrDefault();
                        var vendor = await vendorsRepository.GetVendorAsync(expecteds.First().VendorId);
                        var vendorData = await vendorsRepository.GetVendorDataAsync(vendor.VendorId);
                        
                        var timeOutTry = 0m;
                        var timeoutGr = 0m;
                        string madenType = null;
                        var cTime = DateTime.Now;

                        var timeDifference = (int)(expecteds.Select(x => (cTime - x.DateTime).TotalSeconds)).Average();
                        switch (expecteds.First().RateFxId)
                        {
                            case 24:
                                madenType = "GOLD";
                                break;
                            case 26:
                                madenType = "SILVER";
                                break;
                            case 27:
                                madenType = "PLATIN";
                                break;
                        }
                        var smsMessage = "Vendor " + vendor.Name + " zaman asimina ugrayan "+madenType+" BUY - ";
                        foreach (var expected in expecteds)
                        {
                            timeOutTry += expected.ExpectedTRY;
                            timeoutGr += expected.Amount;
                            smsMessage += expected.TransactionId + ":";
                        }
                        var firstLastCode = expecteds[0].TransactionId.ToString() + "___" + expecteds[^1].TransactionId.ToString();
                        var message = smsMessage + "TRY_" + timeOutTry.ToString() + ":GR_" + timeoutGr;
                        var subject = "Expected Timeout Warning";
                        await EmailService.SendEmailAsync("trnmmtz@gmail.com", subject, message, false);
                        await EmailService.SendEmailAsync("dolunaysabuncuoglu@gmail.com", subject, message, false);
                        await EmailService.SendEmailAsync("emreelvanus@gmail.com", subject, message, false);

                        Log.Debug("ExpectedCashService: TIMEOUT CHECKER UnExpectedByTime for Group");
                        await UnExpectedByTime(firstLastCode, expecteds, madenType + ":ZAMAN ASIMI", timeoutGr, timeOutTry, timeDifference);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("ExpectedCashService DoWOrkTimeOUt Error: " + e.Message);
                Log.Error(e.StackTrace);
                SMSService.SendSms("05448980702", "ExpectedCashService DoWOrkTimeOUt Error: " + e.Message);
            }
            
        }


        private static int DEBUg = 0;

        /// <summary>
        /// Does the work for buying gold/silver from KT
        /// </summary>
        /// <param name="state">unused state</param>
        private async void DoWorkNewVersion(object? state)
        {
            try
            {
                DEBUg++;
                Log.Debug("ExpectedCashService: DoWorkNewVersion");
                if (Running)
                {
                    if (DEBUg % 2 == 1)
                        Log.Debug("ExpectedCashService: Already Running");
                    return;
                }
                if (FirstTimer)
                {
                    Log.Debug("ExpectedCashService firstTimer waiting 30 seconds");
                    await Task.Delay(40000);
                    Log.Debug("ExpectedCashService sleep done");
                    FirstTimer = false;

                    LastGoodFxResultModel = null;
                    while (LastGoodFxResultModel == null)
                    {
                        Log.Debug("ExpectedCashService: attempting to take rates at start");

                        LastGoodFxResultModel = await KTApiService.GetCurrentPriceFromKtApiASync();
                        await Task.Delay(100);
                    }

                    await LoadExpectedsAtStart();

                    var useds = await vendorsRepository.GetUsedFirstLasts();
                    foreach (var firstLast in useds)
                    {
                        UsedFirstLasts.Add(firstLast.FirstLast);
                    }

                    var status = await vendorsRepository.GetBuyServiceStatusAsync();
                    if (status.Amount.HasValue && status.Amount.Value == 1)
                    {
                        ExpectedRun = true;
                    }
                    else if (status.Amount.HasValue && status.Amount.Value == 0)
                    {
                        ExpectedRun = false;
                    }
                    
                    MinutesWait = await vendorsRepository.GetPaycellMinutesBuy();
                    TimeoutMinutes = await vendorsRepository.GetExpectedTimeoutMinutes();
                    Log.Debug("ExpectedCashService: Loaded " + MainExpectedsList.Count.ToString() + " expecteds");
                }

                Thread.BeginCriticalRegion();
                var minsWait = MinutesWait;
                Thread.EndCriticalRegion();
                Log.Debug("ExpectedCashService: minsWait: " + minsWait.ToString());
                var diff = DateTime.Now - LastRunTime;
                Log.Debug("ExpectedCashService: diff time is " + diff.TotalMinutes.ToString());
                if (diff.TotalMinutes < minsWait)
                {
                    if (DEBUg % 2 == 1)
                        Log.Debug("ExpectedCashService: Not enough time passed");
                    return;
                }
                LastRunTime = DateTime.Now;
                Running = true;
                Log.Debug("ExpectedCashService: Doing actual work at " + DateTime.Now.ToString());
                

                
                if (!ExpectedRun)
                {
                    Running = false;
                    Log.Debug("ExpectedCashService: ExpectedRun false");
                    return;
                }

                while (KuwaitAccessHandler.AccessToken == null)
                {
                    Log.Debug("ExpectedCashService: expectedRun KT Access Token Not Ready sleeping 10 seconds");
                    await Task.Delay(10000);
                }
                if (MainExpectedsList.Count == 0)
                {
                    Log.Debug("ExpectedCashService: MainExpectedsList Count 0");
                    Running = false;
                    LastRunTime = DateTime.Now;
                    return;
                }
                await DoWorkTimeoutChecker();

                if (MainExpectedsList.Count == 0)
                {
                    Log.Debug("ExpectedCashService: MainExpectedsList Count 0 after timeouts");
                    Running = false;
                    LastRunTime = DateTime.Now;
                    return;
                }

                Thread.BeginCriticalRegion();
                Log.Debug("ExpectedCashService: MainExpectedsList Count 0 after timeouts");
                var expectedSuffixes = MainExpectedsList
                    .Select(x => x.ExpectedSuffix)
                    .Distinct()
                    .ToList();
                
                Thread.EndCriticalRegion();

                var queryString = "beginDate=" + CreateBegin() + "&endDate=" + CreateEnd();
                
                var validKtTransactions = new Dictionary<string, KTTransaction>();

                foreach (var suffix in expectedSuffixes)
                {
                    Log.Debug("ExpectedCashService: MainExpectedsList suffix " + suffix);
                    var hareketler = await KTApiService.GetHesapHareketleriASync(suffix, queryString);
                    if (hareketler == null)
                    {
                        continue;
                    }
                    foreach (var trans in hareketler.Value.AccountActivities)
                    {
                        
                        var fL = ExtractFirstLast(trans.Description);
                        if (fL == null || UsedFirstLasts.Contains(fL))
                        {
                            continue;
                        }
                        Log.Debug("ExpectedCashService: MainExpectedsList suffix " + suffix + " kt transaction found " + JsonConvert.SerializeObject(trans));
                        validKtTransactions.Add(fL, trans);
                        //UsedFirstLasts.Add(fL);
                        //break;
                    }
                }
                if (validKtTransactions.Count == 0)
                {
                    Log.Debug("ExpectedCashService: valid kt trans coount 0");
                    Running = false;
                    LastRunTime = DateTime.Now;
                    return;
                }
                foreach (var firstLast in validKtTransactions.Keys) // 12300___12400
                {
                    UsedFirstLasts.Add(firstLast);
                    await vendorsRepository.AddFirstLast(new UsedFirstLast(firstLast));
                    var firstLastArray = firstLast.Split("___");
                    var firstId = int.Parse(firstLastArray[0]);
                    var lastId = int.Parse(firstLastArray[1]);
                    if (firstId > lastId)
                    {
                        int temp = firstId;
                        firstId = lastId;
                        lastId = firstId;
                    }
                    var properFirstLast = string.Format("{0}___{1}", firstId, lastId);
                    Log.Debug("ExpectedCashService: for firstLast : " + firstLast);
                    // 12300 id li transaciton gold transaction olsun, ama mesela 12301 inci transaction silver
                    Thread.BeginCriticalRegion();
                    var firstExpected = MainExpectedsList
                        .Where(x => x.TransactionId == firstId)
                        .FirstOrDefault();
                    if (firstExpected == null)
                    {
                        Log.Debug("ExpectedCashService: para gelmis ama buyuk olasillikla expected timeout olmus");
                        // para gelmis ama buyuk olasillikla expected timeout olmus
                        Thread.EndCriticalRegion();
                        continue;
                    }
                    var expecteds = MainExpectedsList
                        .Where(x => x.TransactionId >= firstId &&
                            x.TransactionId <= lastId && 
                            x.ExpectedSuffix == firstExpected.ExpectedSuffix).ToList();
                    Log.Debug("ExpectedCashService: got expecteds");
                    foreach (var expected in expecteds)
                    {
                        MainExpectedsList.Remove(expected);
                    }

                    Thread.EndCriticalRegion();
                    
                    var doWorkParam = new DoWorkParams 
                    {
                        Expecteds = expecteds, 
                        FirstLast = properFirstLast, 
                        KTransaction = validKtTransactions[firstLast]
                    };
                    Log.Debug("ExpectedCashService: prepared doWorkParam: " + JsonConvert.SerializeObject(doWorkParam));
                    await DoWorkNewVersionRealBuy(doWorkParam);
                    
                }
                Running = false;
                LastRunTime = DateTime.Now;
            }
            catch (Exception e)
            {
                if (Running)
                {
                    Running = false;
                    LastRunTime = DateTime.Now;
                }
                Log.Error("ExpectedCashService error: " + e.Message);
                SMSService.SendSms("05448980702", "ExpectedCashService error: " + e.Message);
                Log.Error(e.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string CreateBegin()
        {
            
            var begin = DateTime.Now;
            
            if (begin.ToString("tt", CultureInfo.InvariantCulture).Equals("AM") &&
                begin.Hour == 0 && begin.Minute <= 6)
            {
                begin = begin.AddDays(-1);
            }

            return begin.ToString("yyyy-MM-dd");
        }

        private static string CreateEnd()
        {
            var end = DateTime.Now;
            return end.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Debug("ExpectedCashService: Starting");

            

            timer = new Timer(DoWorkNewVersion, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            //return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Debug("ExpectedCashService: Stopping");
            timer?.Change(Timeout.Infinite, 0);

            if (MainExpectedsList.Any())
            {
                // server kapaniyor ama main expected list te halen expected var
                await UnExpectedByServerStop();
            }

            //return await Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ((IAsyncDisposable)timer).DisposeAsync();
        }
    }
}
