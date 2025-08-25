using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Models.Vendors;
using Gold.Api.Services.Interfaces;
using Gold.Api.Utilities;
using Gold.Core.Vendors;
using Gold.Domain;
using Gold.Domain.Vendors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gold.Api.Services
{
    /// <summary>
    /// Service that handles Finalize Transaction Actions for vendor Systems
    /// </summary>
    public class VendorBuySellFinalizeService : BackgroundService , IVendorBuySellService
    {
        
        private readonly Dictionary<string, string> Dict;
        private readonly Random random;
        private readonly string[] strings = { "AAAA-", "BBBB-", "CCCC-", "DDDD-", "EEEE-",
        "FFFF-", "GGGG-", "HHHH-", "JJJJ-", "KKKK-"};

        private Dictionary<Guid, bool> Automatics = new Dictionary<Guid, bool>();
        private Dictionary<Guid, bool> AutomaticSells = new Dictionary<Guid, bool>();

        /// <summary>
        /// Constructs the VendorBuySellFinalizeService
        /// </summary>
        /// <param name="provider"></param>
        public VendorBuySellFinalizeService()
        {
            random = new Random();
            Dict = new Dictionary<string, string>();
            Dict["silver"] = "SLV1";
            Dict["gold"] = "GLD1";
            Dict["C0FFD97A-0FBB-401A-CCED-08D9478FA7FC".ToLower()] = "DAYI";
            Dict["653A4C18-13AA-4560-B98F-D11F3EAFC65F".ToLower()] = "TEST";
            // c0ffd97a-0fbb-401a-cced-08d9478fa7fc
        }

        /// <summary>
        /// Sets automatic buy status for all given vendors
        /// </summary>
        /// <param name="automatics"></param>
        public void SetAutomatics(Dictionary<Guid, bool> automatics)
        {
            foreach (var kvp in automatics)
            {
                if (Automatics.ContainsKey(kvp.Key))
                {
                    Automatics[kvp.Key] = kvp.Value;
                } 
                else
                {
                    Automatics.Add(kvp.Key, kvp.Value);
                }
                
            }
        }

        /// <summary>
        /// Sets automatic sell status for all vendor sells
        /// </summary>
        /// <param name="automatics"></param>
        public void SetAutomaticSells(Dictionary<Guid, bool> automatics)
        {
            foreach (var kvp in automatics)
            {
                if (AutomaticSells.ContainsKey(kvp.Key))
                {
                    AutomaticSells[kvp.Key] = kvp.Value;
                }
                else
                {
                    AutomaticSells.Add(kvp.Key, kvp.Value);
                }

            }
        }

        public void SetAutomatic(Guid key, bool value)
        {
            
            if (Automatics.ContainsKey(key))
            {
                Automatics[key] = value;
            }
            else
            {
                Automatics.Add(key, value);
            }
        }

        public void SetAutomaticSell(Guid key, bool value)
        {

            if (AutomaticSells.ContainsKey(key))
            {
                AutomaticSells[key] = value;
            }
            else
            {
                AutomaticSells.Add(key, value);
            }
        }


        public static string GetCodeOfExpected(VendorExpected expected)
        {
            return "asd";
/*            if (ServiceProvider == null)
                return new ServiceCollection().BuildServiceProvider().GetService<VendorBuySellFinalizeService>().GetCodeOf(expected);
            else
                return ServiceProvider.GetService<VendorBuySellFinalizeService>().GetCodeOf(expected);*/
        }
        
        /// <summary>
        /// FNTG-GLD1-DAYI-3421
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
        public string GetCodeOf(VendorExpected expected)
        {
            var builder = new StringBuilder();
            builder.Append("FNTG-");
            if (expected.RateFxId == 24)
            {
                builder.Append(Dict["gold"] + "-");
            }
            else
            {
                builder.Append(Dict["silver"] + "-");
            }
            builder.Append(strings[random.Next() % strings.Length]);
            builder.Append(string.Format("{0:0000}", expected.TransactionId % 10000));
            return builder.ToString();
        }

        public async Task<FinaliseTransactionResult> TestRoutineBuy(ValidateFinaliseResult validFinalise, IVendorsRepository vendorsRepository, IExpectedCashService expectedCashService)
        {
            string reason;
            // vendor buys gold or silver from Finta

            var silverPrices = GlobalGoldPrice.GetSilverPricesCached();
            var goldPrices = GlobalGoldPrice.GetGoldPricesCached();
            string code = "err";

            var expectedRun = vendorsRepository.GetRobotStatus().Amount.Value == 1.00m;

            if (validFinalise.vendorTransaction.Comment.StartsWith("arc_buy_silver"))
            {
                reason = "VENDORA SILVER VERILDI";
                if (!expectedRun)
                {
                    validFinalise.vendor.SilverBalance = (validFinalise.vendor.SilverBalance.HasValue) ? validFinalise.vendor.SilverBalance.Value + validFinalise.vendorTransaction.GramAmount : validFinalise.vendorTransaction.GramAmount;
                }
                else
                {
                    var rate = KTApiService.GetCurrentPriceFromKtApi()
                    .value.FxRates.Where(x => x.FxId == 26).FirstOrDefault().BuyRate;

                    var expected = new VendorExpected(
                            validFinalise.vendorTransaction.Destination,
                            validFinalise.vendorTransaction.TransactionId,
                            validFinalise.vendorTransaction.VendorReferenceId,
                            validFinalise.vendorData.TLSuffix,
                            26,
                            validFinalise.vendorTransaction.TlAmount,
                            validFinalise.vendorTransaction.GramAmount,
                            0,
                            rate,
                            silverPrices.BuyRate
                        );
                    code = GetCodeOf(expected);
                    await vendorsRepository.AddVendorExpected(expected);
                }


            }
            else if (validFinalise.vendorTransaction.Comment.StartsWith("arc_buy_platin"))
            {
                reason = "VENDORA PLATIN VERILDI";
                code = "invalid";
                // playin not here yet
                /*
                var platinBalance = Repository.GetVendorPlatinBalance(validFinalise.vendor.VendorId);
                if (platinBalance == null)
                {
                    platinBalance = new VendorPlatinBalance(validFinalise.vendor.VendorId);
                }
                platinBalance.Balance += validFinalise.vendorTransaction.GramAmount;
                */
                /*
                var rate = KTApiService.GetCurrentPriceFromKtApi()
                    .value.FxRates.Where(x => x.FxId == 27).FirstOrDefault().BuyRate;

                var expected = new ExpectedCash(
                        vendorTransaction.Destination,
                        vendorTransaction.TransactionId,
                        vendorTransaction.VendorReferenceId,
                        vendorData.TLSuffix,
                        26,
                        vendorTransaction.TlAmount,
                        vendorTransaction.GramAmount,
                        rate,
                        rate,
                        rate
                    );
                Repository.AddExpectedCash(expected);*/
            }
            else
            {
                reason = "VENDORA ALTIN VERILDI";
                if (!expectedRun)
                {
                    validFinalise.vendor.Balance += validFinalise.vendorTransaction.GramAmount;
                }
                else
                {
                    var rate = KTApiService.GetCurrentPriceFromKtApi()
                    .value.FxRates.Where(x => x.FxId == 24).FirstOrDefault().BuyRate;
                    // expected transaction

                    var expected = new VendorExpected(
                            validFinalise.vendorTransaction.Destination,
                            validFinalise.vendorTransaction.TransactionId,
                            validFinalise.vendorTransaction.VendorReferenceId,
                            validFinalise.vendorData.TLSuffix,
                            24,
                            validFinalise.vendorTransaction.TlAmount,
                            validFinalise.vendorTransaction.GramAmount,
                            0,
                            rate,
                            goldPrices.BuyRate
                        );
                    code = GetCodeOf(expected);
                    await vendorsRepository.AddVendorExpected(expected);
                }

            }
            // commented since robot should complete it now
            validFinalise.vendorTransaction.CompleteTransaction(reason);

            await vendorsRepository.SaveChangesAsync();

            return new FinaliseTransactionResult { ResultCode = "0", Message = "islem basarili" , HavaleCode = code };

        }

        public async Task<FinaliseTransactionResult2> VendorBuysFromFintagRoutine(ValidateFinaliseResult validFinalise,
            IVendorsRepository vendorsRepository, IExpectedCashService expectedCashService)
        {
            
            
            string reason;
            // vendor buys gold or silver from Finta

            var silverPrices = GlobalGoldPrice.GetSilverPricesCached();
            var goldPrices = GlobalGoldPrice.GetGoldPricesCached();
            var platPrices = GlobalGoldPrice.GetPlatinPricesCached();
            string code = "";

            //var expectedRun = vendorsRepository.GetRobotStatus().Amount.Value == 1.00m;

            //var automatic = (validFinalise.vendorData.Automatic != null && validFinalise.vendorData.Automatic.HasValue && validFinalise.vendorData.Automatic.Value);

            var automatic = Automatics.GetValueOrDefault(validFinalise.vendor.VendorId, false);

            if (validFinalise.vendorTransaction.Comment.StartsWith("arc_buy_silver"))
            {
                validFinalise.vendor.SilverBalance = (validFinalise.vendor.SilverBalance.HasValue) ? validFinalise.vendor.SilverBalance.Value + validFinalise.vendorTransaction.GramAmount : validFinalise.vendorTransaction.GramAmount;
                reason = "VENDORA SILVER VERILDI";
                if (automatic)
                {
                    //var rate = (await KTApiService.GetCurrentPriceFromKtApiASync()).value.FxRates.Where(x => x.FxId == 26).FirstOrDefault().BuyRate;
                    //var rate = KTApiService.GetCurrentPriceFromKtApi().value.FxRates.Where(x => x.FxId == 26).FirstOrDefault().BuyRate;

                    var expected = new VendorExpected(
                            validFinalise.vendorTransaction.Destination,
                            validFinalise.vendorTransaction.TransactionId,
                            validFinalise.vendorTransaction.VendorReferenceId,
                            validFinalise.vendorData.TLSuffix,
                            26,
                            validFinalise.vendorTransaction.TlAmount,
                            validFinalise.vendorTransaction.GramAmount,
                            0,
                            0,
                            silverPrices.BuyRate
                        );
                    //code = GetCodeOf(expected);
                    //await vendorsRepository.AddVendorExpected(expected); 

                    expectedCashService.AddExpectedCash(expected);
                }
            }
            else if (validFinalise.vendorTransaction.Comment.StartsWith("arc_buy_platin"))
            {

                validFinalise.PlatinBalance.Balance += validFinalise.vendorTransaction.GramAmount;
                reason = "VENDORA PLATIN VERILDI";
                if (automatic)
                {

                    var expected = new VendorExpected(
                            validFinalise.vendorTransaction.Destination,
                            validFinalise.vendorTransaction.TransactionId,
                            validFinalise.vendorTransaction.VendorReferenceId,
                            validFinalise.vendorData.TLSuffix,
                            27,
                            validFinalise.vendorTransaction.TlAmount,
                            validFinalise.vendorTransaction.GramAmount,
                            0,
                            0,
                            platPrices.BuyRate
                        );

                    expectedCashService.AddExpectedCash(expected);

                }
            }
            else
            {
                validFinalise.vendor.Balance += validFinalise.vendorTransaction.GramAmount;
                reason = "VENDORA ALTIN VERILDI";
                if (automatic)
                {
                    //var rate = (await KTApiService.GetCurrentPriceFromKtApiASync()).value.FxRates.Where(x => x.FxId == 24).FirstOrDefault().BuyRate;
                    /*var rate = KTApiService.GetCurrentPriceFromKtApi()
                    .value.FxRates.Where(x => x.FxId == 24).FirstOrDefault().BuyRate;*/
                    // expected transaction

                    var expected = new VendorExpected(
                            validFinalise.vendorTransaction.Destination,
                            validFinalise.vendorTransaction.TransactionId,
                            validFinalise.vendorTransaction.VendorReferenceId,
                            validFinalise.vendorData.TLSuffix,
                            24,
                            validFinalise.vendorTransaction.TlAmount,
                            validFinalise.vendorTransaction.GramAmount,
                            0,
                            0,
                            goldPrices.BuyRate
                        );
                    //code = GetCodeOf(expected);
                    //await vendorsRepository.AddVendorExpected(expected);

                    expectedCashService.AddExpectedCash(expected);
                }

            }
            // commented since robot should complete it now
            validFinalise.vendorTransaction.CompleteTransaction(reason);
            
            await vendorsRepository.SaveChangesAsync();

            return new FinaliseTransactionResult2 { ResultCode = "0", Message = "islem basarili" };//, HavaleCode = code };
                  
        }

        public async Task<FinaliseTransactionResult2> VendorSellsToFintagRoutine(ValidateFinaliseResult validFinalise, IVendorsRepository vendorsRepository, ICashSenderService cashSenderService)
        {
             var silverTransaction = (validFinalise.vendorTransaction.Comment.StartsWith("arc_sell_silver"));
             var platinTransaction = (validFinalise.vendorTransaction.Comment.StartsWith("arc_sell_platin"));
             string mesg;
            //Log.Debug("SEll ROUTINE");
             //var data = await vendorsRepository.GetSpecificVendorDataReadOnlyAsync(validFinalise.vendor.VendorId.ToString());

            //var automatic = (validFinalise.vendorData.Automatic != null && validFinalise.vendorData.Automatic.HasValue && validFinalise.vendorData.Automatic.Value);
            var automatic = Automatics.GetValueOrDefault(validFinalise.vendor.VendorId, false);
            if (silverTransaction)
            {
                validFinalise.vendor.SilverBalance = validFinalise.vendor.SilverBalance.Value - validFinalise.vendorTransaction.GramAmount;
                mesg = "SILVER";
                if (automatic)
                {
                    //var ktSatis = fxResult.value.FxRates.Where(x => x.FxId == 26).FirstOrDefault().SellRate;
                    var payment = new VendorCashPayment(
                        validFinalise.vendor.VendorId, 
                        validFinalise.vendorTransaction.TransactionId,
                        validFinalise.vendorTransaction.VendorReferenceId, 
                        26, validFinalise.vendorTransaction.TlAmount,
                        validFinalise.vendorTransaction.GramAmount,
                        0, 0, 
                        GlobalGoldPrice.GetSilverPricesCached().SellRate, 
                        mesg);

                    cashSenderService.AddCashPayment(payment);
                }

                
                
            }
            else if (platinTransaction)
            {
                mesg = "PLATIN";
                validFinalise.PlatinBalance.Balance -= validFinalise.vendorTransaction.GramAmount;
                
                if (automatic)
                {
                    
                    var payment = new VendorCashPayment(
                        validFinalise.vendor.VendorId,
                        validFinalise.vendorTransaction.TransactionId,
                        validFinalise.vendorTransaction.VendorReferenceId,
                        27, validFinalise.vendorTransaction.TlAmount,
                        validFinalise.vendorTransaction.GramAmount,
                        0, 0,
                        GlobalGoldPrice.GetSilverPricesCached().SellRate,
                        mesg);

                    cashSenderService.AddCashPayment(payment);
                }
            }
            else
            {
                validFinalise.vendor.Balance -= validFinalise.vendorTransaction.GramAmount;
                mesg = "GOLD";
                if (automatic)
                {
                    
                    //var ktSatis = fxResult.value.FxRates.Where(x => x.FxId == 24).FirstOrDefault().SellRate;
                    var payment = new VendorCashPayment(validFinalise.vendor.VendorId, validFinalise.vendorTransaction.TransactionId,
                        validFinalise.vendorTransaction.VendorReferenceId, 24, validFinalise.vendorTransaction.TlAmount, validFinalise.vendorTransaction.GramAmount,
                        0, 0, GlobalGoldPrice.GetGoldPricesCached().SellRate, "mesg");

                    cashSenderService.AddCashPayment(payment);
                }
                
            }

            validFinalise.vendorTransaction.CompleteTransaction(mesg);
            await vendorsRepository.SaveChangesAsync();

            return new FinaliseTransactionResult2 { ResultCode = "0", Message = "islem basarili" }; ;

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        /*

                var suffTo = data.TLSuffix;
                var sffFrom = data.GOLDSuffix;
                var acc = data.AccountNumber;
                var suff = data.AccountSuffix;
                var rate = fxResult.value.FxRates.Where(x => x.FxId == 24).FirstOrDefault();
                var goldPrice = GlobalGoldPrice.GetGoldPricesCached();
                if (Utility.AnyNull(suffTo, sffFrom, acc, suff, rate, goldPrice))
                {
                    return new FinaliseTransactionResult { ResultCode = "10", Message ="sistem hatasi altin vendordata" };
                }
                var param = new PreciousMetalsSellParams
                {
                    Amount = validFinalise.vendorTransaction.GramAmount,
                    SuffixFrom = sffFrom,
                    SuffixTo = suffTo,
                    UserName = KuwaitAccessHandler.FINTAG,
                    SellRate = rate.SellRate
                };
                var attempts = 0;
                bool success = false;
                string refid = "";
                while (attempts <= 5 && success == false)
                {
                    attempts++;
                    var metalSell = await KTApiService.PreciousMetalSellAsync(param);
                    if (metalSell == null)
                    {
                        continue;
                    }
                    success = metalSell.Success;
                    refid = metalSell.RefId;
                }
                var tl = validFinalise.vendorTransaction.TlAmount * (-1);
                var gram = validFinalise.vendorTransaction.GramAmount;
                
                if (success == true)
                {
                    var iparams = new InterBankTransferParams
                    {
                        Amount = validFinalise.vendorTransaction.TlAmount,
                        ReceiverAccount = acc,
                        ReceiverSuffix = suff,
                        Description = string.Format("Goldtag den Altin bozdurma {0}TRY - {1}gr", validFinalise.vendorTransaction.TlAmount, validFinalise.vendorTransaction.GramAmount),
                        SenderSuffix = suffTo,
                        TransferType = 3
                    };


                    refid = "";
                    success = false;
                    attempts = 0;
                    while (attempts <= 5 && success == false)
                    {
                        attempts++;
                        var interTransfer = await KTApiService.InterBankTransferAsync(iparams);
                        if (interTransfer == null)
                        {
                            continue;
                        }
                        success = interTransfer.Success;
                        refid = interTransfer.RefId;


                    }
                    validFinalise.vendor.Balance -= validFinalise.vendorTransaction.GramAmount;

                    if (success)
                    {


                        var finalized = new VendorFinalized(
                            validFinalise.vendor.VendorId,
                            validFinalise.vendorTransaction.TransactionId,
                            validFinalise.vendorTransaction.VendorReferenceId,
                            validFinalise.vendorTransaction.TlAmount,
                            validFinalise.vendorTransaction.GramAmount,
                            0, rate.SellRate, goldPrice.SellRate, "GOLD", refid, "VENDOR_SELL", tl, gram);
                        await Repository.AddVendorFinalized(finalized);
                        mesg = "GOLD_SELL_OK";
                        resultCode = "0";
                        message = "islem basarili";
                    }
                    else
                    {
                        var notPos = new VendorNotPositionSell(refid, validFinalise.vendorTransaction.TlAmount,
                            validFinalise.vendorTransaction.GramAmount, suff, acc, rate.SellRate, validFinalise.vendorTransaction.TransactionId,
                            validFinalise.vendor.VendorId, "GOLD_IBAN", 0, rate.SellRate, goldPrice.SellRate);

                        await Repository.AddVendorNotPositionSell(notPos);
                        //Repository.AddIBankError(ibankError);
                        mesg = "GOLD_IBAN";
                        resultCode = "101";
                        message = "interbank transfer error";
                    }
                }
                else
                {
                    // bozfdurma basarisiz

                    var notPos = new VendorNotPositionSell(refid, validFinalise.vendorTransaction.TlAmount,
                             validFinalise.vendorTransaction.GramAmount, sffFrom, suffTo, rate.SellRate, validFinalise.vendorTransaction.TransactionId,
                             validFinalise.vendor.VendorId, "GOLD_BOZDURMA", 0, rate.SellRate, goldPrice.SellRate);
                    await Repository.AddVendorNotPositionSell(notPos);

                    mesg = "GOLD_BOZDURMA";

                    resultCode = "102";
                    message = "sell altin err";
                }
                
                */
        /*
                var suffTo = data.TLSuffix;
                var sffFrom = data.SLVSuffix;
                var acc = data.AccountNumber;
                var suff = data.AccountSuffix;
                var rate = fxResult.value.FxRates.Where(x => x.FxId == 26).FirstOrDefault();
                var silverPrice = GlobalGoldPrice.GetSilverPricesCached();
                if (Utility.AnyNull(suffTo, sffFrom, acc, suff, rate, silverPrice))
                {
                    return new FinaliseTransactionResult { ResultCode = "10", Message = "Sistem hatasi silver vendordata" };
                }

                var param = new PreciousMetalsSellParams
                {
                    Amount = validFinalise.vendorTransaction.GramAmount,
                    SuffixFrom = sffFrom,
                    SuffixTo = suffTo,
                    UserName = KuwaitAccessHandler.FINTAG,
                    SellRate = rate.SellRate
                };
                var attempts = 0;
                bool success = false;
                string refid = "";
                while (attempts <= 5 && success == false)
                {
                    attempts++;
                    var metalSell = await KTApiService.PreciousMetalSellAsync(param);
                    if (metalSell == null)
                    {
                        continue;
                    }
                    success = metalSell.Success;
                    refid = metalSell.RefId;
                }
                var tl = validFinalise.vendorTransaction.TlAmount * (-1);
                var gram = validFinalise.vendorTransaction.GramAmount;
                
                if (success == true)
                {
                    var iparams = new InterBankTransferParams
                    {
                        Amount = validFinalise.vendorTransaction.TlAmount,
                        ReceiverAccount = acc,
                        ReceiverSuffix = suff,
                        Description = string.Format("Goldtag den Gumus bozdurma {0}TRY - {1}gr", validFinalise.vendorTransaction.TlAmount, validFinalise.vendorTransaction.GramAmount),
                        SenderSuffix = suffTo,
                        TransferType = 3
                    };


                    refid = "";
                    success = false;
                    attempts = 0;
                    while (attempts <= 5 && success == false)
                    {
                        attempts++;
                        var interTransfer = await KTApiService.InterBankTransferAsync(iparams);
                        if (interTransfer == null)
                        {
                            continue;
                        }
                        success = interTransfer.Success;
                        refid = interTransfer.RefId;
                        

                    }
                    
                    
                    validFinalise.vendor.SilverBalance = validFinalise.vendor.SilverBalance.Value - validFinalise.vendorTransaction.GramAmount;
                    if (success)
                    {
                        
                        
                        var finalized = new VendorFinalized(
                            validFinalise.vendor.VendorId,
                            validFinalise.vendorTransaction.TransactionId,
                            validFinalise.vendorTransaction.VendorReferenceId,
                            validFinalise.vendorTransaction.TlAmount,
                            validFinalise.vendorTransaction.GramAmount,
                            0, rate.SellRate, silverPrice.SellRate, "SILVER", refid, "VENDOR_SELL", tl, gram);
                        await Repository.AddVendorFinalized(finalized);
                        mesg = "SILVER_SELL_OK";
                        resultCode = "0";
                        message = "islem basarili";
                    }
                    else
                    {
                        var notPos = new VendorNotPositionSell(refid, validFinalise.vendorTransaction.TlAmount,
                            validFinalise.vendorTransaction.GramAmount, suff, acc, rate.SellRate, validFinalise.vendorTransaction.TransactionId,
                            validFinalise.vendor.VendorId, "SILVER_IBAN", 0, rate.SellRate, silverPrice.SellRate);

                        await Repository.AddVendorNotPositionSell(notPos);
                        mesg = "SILVER_IBAN";

                        resultCode = "101";
                        message = "inter bank transfer error";

                    }
                
                }
                else
                {
                    // bozfdurma basarisiz

                    var notPos = new VendorNotPositionSell(refid, validFinalise.vendorTransaction.TlAmount,
                             validFinalise.vendorTransaction.GramAmount, sffFrom, suffTo, rate.SellRate, validFinalise.vendorTransaction.TransactionId,
                             validFinalise.vendor.VendorId, "SILVER_BOZDURMA", 0, rate.SellRate, silverPrice.SellRate);
                    await Repository.AddVendorNotPositionSell(notPos);
                    mesg = "SILVER_BOZDURMA";

                    resultCode = "102";
                    message = "precious metal sell error";
                }
            */
    }
}
