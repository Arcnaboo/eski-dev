using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Services.Interfaces;
using Gold.Api.Utilities;
using Gold.Core.Vendors;
using Gold.Domain.Vendors;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gold.Api.Services
{
    public class CashSenderService : IHostedService, IAsyncDisposable, IDisposable, ICashSenderService
    {
        

        private readonly IVendorsRepository vendorsRepository;
        private readonly List<VendorCashPayment> CashPaymentForGold = new List<VendorCashPayment>();
        private readonly List<VendorCashPayment> CashPaymentForSilver = new List<VendorCashPayment>();
        private readonly List<VendorCashPayment> CashPaymentForPlatin = new List<VendorCashPayment>();
        private Timer timer = null;
        private bool FirstTimer = true;
        private bool Running = false;
        private DateTime LastRunTime = DateTime.Now;
        private bool SenderRun = true;
        private int MinsWait = 10;
        private FxResultModel LastGoodFxResultModel = null;
        public CashSenderService(IVendorsRepository vendorsRepository)
        {
            this.vendorsRepository = vendorsRepository;
        }

        public void SetSenderRun(bool status)
        {
            SenderRun = status;
        }

        public void SetSellMinutes(int minutes)
        {
            MinsWait = minutes;
        }

        public void AddCashPayment(VendorCashPayment payment)
        {
            Thread.BeginCriticalRegion();
            if (payment.RateFxId == 26)
            {
                CashPaymentForSilver.Add(payment);
            }
            else if (payment.RateFxId == 27)
            {
                CashPaymentForPlatin.Add(payment);
            }
            else
            {
                CashPaymentForGold.Add(payment);
            }
            Thread.EndCriticalRegion();
        }

        public async Task<object> DoWorkSilver(List<VendorCashPayment> payments)
        {
            try
            {
                if (!payments.Any())
                {
                    Log.Debug("CashSenderService: no silver paymnents");
                    return null;
                }

                var totalGrams = payments.Select(x => x.GramAmount).Sum();
                var totalTRY = payments.Select(x => x.TLAmount).Sum();

                var first = await vendorsRepository.GetVenTransactionReadOnly(payments[0].TransactionId);
                var last = await vendorsRepository.GetVenTransactionReadOnly(payments[^1].TransactionId);
                var firstLastVendor = first.VendorReferenceId + "---" + last.VendorReferenceId;
                var firstLast = string.Format("{0}___{1}", first.TransactionId, last.TransactionId);
                var vendor = await vendorsRepository.GetVendorAsync(payments[0].VendorId);
                var vendorData = await vendorsRepository.GetVendorDataAsync(vendor.VendorId);
                var rates = (await KTApiService.GetCurrentPriceFromKtApiASync());
                if (rates != null)
                {
                    LastGoodFxResultModel = rates;
                }
                else
                {
                    rates = LastGoodFxResultModel;
                }
                var rate = rates.value.FxRates.Where(x => x.FxId == 26).FirstOrDefault();
                if (rate.IsSpreadApplied == "1")
                {
                    var avgPiyasaFiyat = payments.Select(x => x.PiyasaGramFiyat).Average();
                    var avgSatisGram = payments.Select(x => x.SatisGramFiyat).Average();
                    var notPos = new VendorNotPositionSell(firstLast,
                        totalTRY, totalGrams, vendorData.AccountSuffix, vendorData.AccountNumber,
                        rate.SellRate, payments.Count, vendor.VendorId,
                        "SILVER:SPREAD APPLIED at " + DateTime.Now.ToString(),
                        avgPiyasaFiyat, rate.SellRate, avgSatisGram);

                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.SaveChangesAsync();
                    return notPos;
                }
                var attempts = 0;
                var success = false;
                MetalSellResult result = null;
                var refid = "";
                var spread = false;
                while (attempts <= 5 && success == false)
                {
                    rates = (await KTApiService.GetCurrentPriceFromKtApiASync());
                    if (rates != null)
                    {
                        LastGoodFxResultModel = rates;
                    }
                    else
                    {
                        rates = LastGoodFxResultModel;
                    }
                    rate = rates.value.FxRates.Where(x => x.FxId == 26).FirstOrDefault();
                    if (rate.IsSpreadApplied == "1")
                    {
                        spread = true;
                        break;
                    }
                    
                    var param = new PreciousMetalsSellParams
                    {
                        Amount = totalGrams,
                        SuffixFrom = vendorData.SLVSuffix,
                        SuffixTo = vendorData.TLSuffix,
                        UserName = KuwaitAccessHandler.FINTAG,
                        SellRate = rate.SellRate
                    };
                    attempts++;
                    result = await KTApiService.PreciousMetalSellAsync(param);
                    if (result == null)
                    {
                        continue;
                    }

                    success = result.Success;
                    refid = result.RefId;
                    Log.Debug("CashSenderService: PreciousMetalSell: " + JsonConvert.SerializeObject(result));
                }

                if (spread)
                {
                    var avgPiyasaFiyat = payments.Select(x => x.PiyasaGramFiyat).Average();
                    var avgSatisGram = payments.Select(x => x.SatisGramFiyat).Average();
                    var notPos = new VendorNotPositionSell(firstLast,
                        totalTRY, totalGrams, vendorData.AccountSuffix, vendorData.AccountNumber,
                        rate.SellRate, payments.Count, vendor.VendorId,
                        "SILVER:SPREAD APPLIED at " + DateTime.Now.ToString(),
                        avgPiyasaFiyat, rate.SellRate, avgSatisGram);

                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.SaveChangesAsync();
                    return notPos;
                }

                if (!success)
                {
                    var avgPiyasaFiyat = payments.Select(x => x.PiyasaGramFiyat).Average();
                    var avgSatisGram = payments.Select(x => x.SatisGramFiyat).Average();
                    var notPos = new VendorNotPositionSell(firstLast,
                        totalTRY, totalGrams, vendorData.AccountSuffix, vendorData.AccountNumber,
                        rate.SellRate, payments.Count, vendor.VendorId,
                        "SILVER:METAL_SELL_FAIL:" + refid, avgPiyasaFiyat, rate.SellRate, avgSatisGram);

                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.SaveChangesAsync();
                    var message = string.Format("KT silver bozmadi - vendor {0} - grams {1} - notPositionSell eklendi", vendor.Name, totalGrams);
                    /*
                    SMSService.SendSms("05323878550", message);
                    SMSService.SendSms("05433893303", message);
                    SMSService.SendSms("05063337007", message);*/
                    var subject = "Metal Sell Silver Fail";
                    await EmailService.SendEmailAsync("trnmmtz@gmail.com", subject, message, false);
                    await EmailService.SendEmailAsync("dolunaysabuncuoglu@gmail.com", subject, message, false);
                    await EmailService.SendEmailAsync("emreelvanus@gmail.com", subject, message, false);
                    return notPos;
                }

                var desc = string.Format("Goldtag den gümüş bozdurma {0}TRY - {1}gr - {2}", totalTRY, totalGrams, firstLast);
                var iparams = new InterBankTransferParams
                {
                    Amount = totalTRY,
                    ReceiverAccount = vendorData.AccountNumber,
                    ReceiverSuffix = vendorData.AccountSuffix,
                    Description = desc,
                    SenderSuffix = vendorData.TLSuffix,
                    TransferType = 3,
                    UserName = KuwaitAccessHandler.FINTAG
                };
                var refId = "";
                success = false;
                attempts = 0;
                InterBanTransferResult bresult = null;
                while (attempts <= 5 && success == false)
                {
                    attempts++;
                    bresult = await KTApiService.InterBankTransferAsync(iparams);
                    if (bresult == null)
                    {
                        continue;
                    }
                    success = bresult.Success;
                    refId = bresult.RefId;
                    Log.Debug("CashSenderService: InterBankTransfer: " + JsonConvert.SerializeObject(bresult));
                }
                if (!success)
                {
                    var avgPiyasaFiyat = payments.Select(x => x.PiyasaGramFiyat).Average();
                    var avgSatisGram = payments.Select(x => x.SatisGramFiyat).Average();
                    var notPos = new VendorNotPositionSell(firstLast,
                        totalTRY, totalGrams, vendorData.AccountSuffix, vendorData.AccountNumber,
                        rate.SellRate, payments.Count, vendor.VendorId,
                        "SILVER:IBAN_FAIL:" + refId, avgPiyasaFiyat, rate.SellRate, avgSatisGram);

                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.SaveChangesAsync();
                    var message = string.Format("KT silver bozdu ama iban transfer olmadi - vendor: {0} - grams: {1} - TRY : {2} - notPositionSell eklendi", vendor.Name, totalGrams, totalTRY);

                    /*SMSService.SendSms("05323878550", message);
                    SMSService.SendSms("05433893303", message);
                    SMSService.SendSms("05063337007", message);*/
                    var subject = "Metal Sell Silver InterBank Transfer Fail";
                    await EmailService.SendEmailAsync("trnmmtz@gmail.com", subject, message, false);
                    await EmailService.SendEmailAsync("dolunaysabuncuoglu@gmail.com", subject, message, false);
                    await EmailService.SendEmailAsync("emreelvanus@gmail.com", subject, message, false);
                    return notPos;
                }
                var averageFintag = totalTRY / totalGrams;
                foreach (var payment in payments)
                {
                    var trans = await vendorsRepository.GetVenTransactionAsync(payment.TransactionId);
                    trans.TamamlanmisKTreferans = refId;
                    trans.GercekKur = averageFintag;
                }
                /*
                 KAR(Goldtag den altın bozdurma) : GoldAmount * (KTGramFiyat - SatışGRAMFiyat)
                 */
                var finalKar = totalGrams * (rate.SellRate - averageFintag);
                var tl = totalTRY * (-1);
                var gram = totalGrams;
                var averagePiyasa = payments.Select(x => x.PiyasaGramFiyat).Average();
                
                var finalized = new VendorFinalized(
                               vendor.VendorId,
                               payments.Count,
                               firstLast,
                               totalTRY,
                               totalGrams,
                               averagePiyasa, 
                               rate.SellRate, 
                               averageFintag,
                               "SILVER", 
                               refId, 
                               "SILVER BOZDURMA OK", 
                               tl, 
                               gram, finalKar);
                await vendorsRepository.AddVendorFinalized(finalized);
                await vendorsRepository.SaveChangesAsync();
                return finalized;
            }
            catch (Exception e)
            {
                Log.Error("CashSenderService DoWorkSilver error: " + e.Message);
                SMSService.SendSms("05448980702", "CashSenderService DoWorkSilver error: " + e.Message);
                Log.Error(e.StackTrace);
                return e;
            }
        }

        public async Task<object> DoWorkGold(List<VendorCashPayment> payments)
        {
            try
            {
                if (!payments.Any())
                {
                    Log.Debug("CashSenderService: no gold paymnents");
                    return null;
                }
                var totalGrams = payments.Select(x => x.GramAmount).Sum();
                var totalTRY = payments.Select(x => x.TLAmount).Sum();

                var first = await vendorsRepository.GetVenTransactionReadOnly(payments[0].TransactionId);
                var last = await vendorsRepository.GetVenTransactionReadOnly(payments[^1].TransactionId);
                var firstLastVendor = first.VendorReferenceId + "---" + last.VendorReferenceId;
                var firstLast = string.Format("{0}___{1}", first.TransactionId, last.TransactionId);
                var vendor = await vendorsRepository.GetVendorAsync(payments[0].VendorId);
                var vendorData = await vendorsRepository.GetVendorDataAsync(vendor.VendorId);
                var rates = (await KTApiService.GetCurrentPriceFromKtApiASync());
                if (rates == null)
                {
                    rates = LastGoodFxResultModel;
                }
                else
                {
                    LastGoodFxResultModel = rates;
                }
                var rate = rates.value.FxRates.Where(x => x.FxId == 24).FirstOrDefault();
                if (rate.IsSpreadApplied == "1")
                {
                    var avgPiyasaFiyat = payments.Select(x => x.PiyasaGramFiyat).Average();
                    var avgSatisGram = payments.Select(x => x.SatisGramFiyat).Average();
                    var notPos = new VendorNotPositionSell(firstLast,
                        totalTRY, totalGrams, vendorData.AccountSuffix, vendorData.AccountNumber,
                        rate.SellRate, payments.Count, vendor.VendorId,
                        "GOLD:SPREAD APPLIED at " + DateTime.Now.ToString(),
                        avgPiyasaFiyat, rate.SellRate, avgSatisGram);

                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.SaveChangesAsync();
                    return notPos;
                }
                var attempts = 0;
                var success = false;
                MetalSellResult result = null;
                var refid = "";
                var spread = false;
                while (attempts <= 5 && success == false)
                {
                    rates = (await KTApiService.GetCurrentPriceFromKtApiASync());
                    if (rates == null)
                    {
                        rates = LastGoodFxResultModel;
                    }
                    else
                    {
                        LastGoodFxResultModel = rates;
                    }
                    rate = rates.value.FxRates.Where(x => x.FxId == 24).FirstOrDefault();
                    if (rate.IsSpreadApplied == "1")
                    {
                        spread = true;
                        break;
                    }
                    var param = new PreciousMetalsSellParams
                    {
                        Amount = totalGrams,
                        SuffixFrom = vendorData.GOLDSuffix,
                        SuffixTo = vendorData.TLSuffix,
                        UserName = KuwaitAccessHandler.FINTAG,
                        SellRate = rate.SellRate
                    };
                    attempts++;
                    result = await KTApiService.PreciousMetalSellAsync(param);
                    if (result == null)
                    {
                        continue;
                    }

                    success = result.Success;
                    refid = result.RefId;
                    Log.Debug("CashSenderService: PreciousMetalSell: " + JsonConvert.SerializeObject(result));
                }

                if (spread)
                {
                    var avgPiyasaFiyat = payments.Select(x => x.PiyasaGramFiyat).Average();
                    var avgSatisGram = payments.Select(x => x.SatisGramFiyat).Average();
                    var notPos = new VendorNotPositionSell(firstLast,
                        totalTRY, totalGrams, vendorData.AccountSuffix, vendorData.AccountNumber,
                        rate.SellRate, payments.Count, vendor.VendorId,
                        "GOLD:SPREAD APPLIED at " + DateTime.Now.ToString(),
                        avgPiyasaFiyat, rate.SellRate, avgSatisGram);

                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.SaveChangesAsync();
                    return notPos;
                }

                if (!success)
                {
                    var avgPiyasaFiyat = payments.Select(x => x.PiyasaGramFiyat).Average();
                    var avgSatisGram = payments.Select(x => x.SatisGramFiyat).Average();
                    var notPos = new VendorNotPositionSell(firstLast,
                        totalTRY, totalGrams, vendorData.AccountSuffix, vendorData.AccountNumber,
                        rate.SellRate, payments.Count, vendor.VendorId,
                        "GOLD:METAL_SELL_FAIL:" + refid,avgPiyasaFiyat, rate.SellRate, avgSatisGram);

                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.SaveChangesAsync();

                    var message = string.Format("KT altin bozmadi - vendor {0} - grams {1} - notPositionSell eklendi", vendor.Name, totalGrams);

                    /*SMSService.SendSms("05323878550", message);
                    SMSService.SendSms("05433893303", message);
                    SMSService.SendSms("05063337007", message);*/
                    var subject = "Metal Sell Gold Fail";
                    await EmailService.SendEmailAsync("trnmmtz@gmail.com", subject, message, false);
                    await EmailService.SendEmailAsync("dolunaysabuncuoglu@gmail.com", subject, message, false);
                    await EmailService.SendEmailAsync("emreelvanus@gmail.com", subject, message, false);
                    return notPos;
                }

                var desc = string.Format("Goldtag den altın bozdurma {0}TRY - {1}gr - {2}", totalTRY, totalGrams, firstLast);
                var iparams = new InterBankTransferParams
                {
                    Amount = totalTRY,
                    ReceiverAccount = vendorData.AccountNumber,
                    ReceiverSuffix = vendorData.AccountSuffix,
                    Description = desc,
                    SenderSuffix = vendorData.TLSuffix,
                    TransferType = 3,
                    UserName = KuwaitAccessHandler.FINTAG
                };
                var refId = "";
                success = false;
                attempts = 0;
                InterBanTransferResult bresult = null;
                while (attempts <= 5 && success == false)
                {
                    attempts++;
                    bresult = await KTApiService.InterBankTransferAsync(iparams);
                    if (bresult == null)
                    {
                        continue;
                    }
                    success = bresult.Success;
                    refId = bresult.RefId;
                    Log.Debug("CashSenderService: InterBankTransfer: " + JsonConvert.SerializeObject(bresult));
                }
                if (!success)
                {
                    var avgPiyasaFiyat = payments.Select(x => x.PiyasaGramFiyat).Average();
                    var avgSatisGram = payments.Select(x => x.SatisGramFiyat).Average();
                    var notPos = new VendorNotPositionSell(firstLast,
                        totalTRY, totalGrams, vendorData.AccountSuffix, vendorData.AccountNumber,
                        rate.SellRate, payments.Count, vendor.VendorId,
                        "GOLD:IBAN_FAIL" + refId, avgPiyasaFiyat, rate.SellRate, avgSatisGram);

                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.SaveChangesAsync();

                    var message = string.Format("KT altin bozdu ama iban transfer olmadi - vendor: {0} - grams: {1} - TRY : {2} - notPositionSell eklendi", vendor.Name, totalGrams, totalTRY);

                    /*SMSService.SendSms("05323878550", message);
                    SMSService.SendSms("05433893303", message);
                    SMSService.SendSms("05063337007", message);*/
                    var subject = "Metal Sell Gold Interbank transfer Fail";
                    await EmailService.SendEmailAsync("trnmmtz@gmail.com", subject, message, false);
                    await EmailService.SendEmailAsync("dolunaysabuncuoglu@gmail.com", subject, message, false);
                    await EmailService.SendEmailAsync("emreelvanus@gmail.com", subject, message, false);
                    return notPos;
                }
                var averageFintag = totalTRY / totalGrams;
                foreach (var payment in payments)
                {
                    var trans = await vendorsRepository.GetVenTransactionAsync(payment.TransactionId);
                    trans.TamamlanmisKTreferans = refId;
                    trans.GercekKur = averageFintag;
                }
                /*
                 KAR(Goldtag den altın bozdurma) : GoldAmount * (KTGramFiyat - SatışGRAMFiyat)
                 */
                var finalKar = totalGrams * (rate.SellRate - averageFintag);
                var tl = totalTRY * (-1);
                var gram = totalGrams;
                var averagePiyasa = payments.Select(x => x.PiyasaGramFiyat).Average();
                var finalized = new VendorFinalized(
                               vendor.VendorId,
                               payments.Count,
                               firstLast,
                               totalTRY,
                               totalGrams,
                               averagePiyasa,
                               rate.SellRate,
                               averageFintag, 
                               "GOLD", 
                               refId, 
                               "GOLD BOZDURMA OK", 
                               tl, 
                               gram,
                               finalKar);
                await vendorsRepository.AddVendorFinalized(finalized);
                await vendorsRepository.SaveChangesAsync();
                return finalized;
            }
            catch (Exception e)
            {
                Log.Error("CashSenderService DoWorkGold error: " + e.Message);
                SMSService.SendSms("05448980702", "CashSenderService DoWorkGold error: " + e.Message);
                Log.Error(e.StackTrace);
                return e;
            }
        }

        public async Task<object> DoWorkPlatin(List<VendorCashPayment> payments)
        {
            try
            {
                if (!payments.Any())
                {
                    Log.Debug("CashSenderService: no platin paymnents");
                    return null;
                }
                var totalGrams = payments.Select(x => x.GramAmount).Sum();
                var totalTRY = payments.Select(x => x.TLAmount).Sum();

                var first = await vendorsRepository.GetVenTransactionReadOnly(payments[0].TransactionId);
                var last = await vendorsRepository.GetVenTransactionReadOnly(payments[^1].TransactionId);
                var firstLastVendor = first.VendorReferenceId + "---" + last.VendorReferenceId;
                var firstLast = string.Format("{0}___{1}", first.TransactionId, last.TransactionId);
                var vendor = await vendorsRepository.GetVendorAsync(payments[0].VendorId);
                var vendorData = await vendorsRepository.GetVendorDataAsync(vendor.VendorId);
                var rates = (await KTApiService.GetCurrentPriceFromKtApiASync());
                if (rates == null)
                {
                    rates = LastGoodFxResultModel;
                }
                else
                {
                    LastGoodFxResultModel = rates;
                }
                var rate = rates.value.FxRates.Where(x => x.FxId == 27).FirstOrDefault();
                if (rate.IsSpreadApplied == "1")
                {
                    var avgPiyasaFiyat = payments.Select(x => x.PiyasaGramFiyat).Average();
                    var avgSatisGram = payments.Select(x => x.SatisGramFiyat).Average();
                    var notPos = new VendorNotPositionSell(firstLast,
                        totalTRY, totalGrams, vendorData.AccountSuffix, vendorData.AccountNumber,
                        rate.SellRate, payments.Count, vendor.VendorId,
                        "PLATIN:SPREAD APPLIED at " + DateTime.Now.ToString(),
                        avgPiyasaFiyat, rate.SellRate, avgSatisGram);

                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.SaveChangesAsync();
                    return notPos;
                }
                var attempts = 0;
                var success = false;
                MetalSellResult result = null;
                var refid = "";
                var spread = false;
                while (attempts <= 5 && success == false)
                {
                    rates = (await KTApiService.GetCurrentPriceFromKtApiASync());
                    if (rates == null)
                    {
                        rates = LastGoodFxResultModel;
                    }
                    else
                    {
                        LastGoodFxResultModel = rates;
                    }
                    rate = rates.value.FxRates.Where(x => x.FxId == 27).FirstOrDefault();
                    if (rate.IsSpreadApplied == "1")
                    {
                        spread = true;
                        break;
                    }
                    var param = new PreciousMetalsSellParams
                    {
                        Amount = totalGrams,
                        SuffixFrom = vendorData.PLTSuffix,
                        SuffixTo = vendorData.TLSuffix,
                        UserName = KuwaitAccessHandler.FINTAG,
                        SellRate = rate.SellRate
                    };
                    attempts++;
                    result = await KTApiService.PreciousMetalSellAsync(param);
                    if (result == null)
                    {
                        continue;
                    }

                    success = result.Success;
                    refid = result.RefId;
                    Log.Debug("CashSenderService: PreciousMetalSell: " + JsonConvert.SerializeObject(result));
                }

                if (spread)
                {
                    var avgPiyasaFiyat = payments.Select(x => x.PiyasaGramFiyat).Average();
                    var avgSatisGram = payments.Select(x => x.SatisGramFiyat).Average();
                    var notPos = new VendorNotPositionSell(firstLast,
                        totalTRY, totalGrams, vendorData.AccountSuffix, vendorData.AccountNumber,
                        rate.SellRate, payments.Count, vendor.VendorId,
                        "PLATIN:SPREAD APPLIED at " + DateTime.Now.ToString(),
                        avgPiyasaFiyat, rate.SellRate, avgSatisGram);

                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.SaveChangesAsync();
                    return notPos;
                }

                if (!success)
                {
                    var avgPiyasaFiyat = payments.Select(x => x.PiyasaGramFiyat).Average();
                    var avgSatisGram = payments.Select(x => x.SatisGramFiyat).Average();
                    var notPos = new VendorNotPositionSell(firstLast,
                        totalTRY, totalGrams, vendorData.AccountSuffix, vendorData.AccountNumber,
                        rate.SellRate, payments.Count, vendor.VendorId,
                        "PLATIN:METAL_SELL_FAIL:" + refid, avgPiyasaFiyat, rate.SellRate, avgSatisGram);

                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.SaveChangesAsync();

                    var message = string.Format("KT platin bozmadi - vendor {0} - grams {1} - notPositionSell eklendi", vendor.Name, totalGrams);

                    /*SMSService.SendSms("05323878550", message);
                    SMSService.SendSms("05433893303", message);
                    SMSService.SendSms("05063337007", message);*/
                    var subject = "Metal Sell Platin Fail";
                    await EmailService.SendEmailAsync("trnmmtz@gmail.com", subject, message, false);
                    await EmailService.SendEmailAsync("dolunaysabuncuoglu@gmail.com", subject, message, false);
                    await EmailService.SendEmailAsync("emreelvanus@gmail.com", subject, message, false);
                    return notPos;
                }

                var desc = string.Format("Goldtag den platin bozdurma {0}TRY - {1}gr - {2}", totalTRY, totalGrams, firstLast);
                var iparams = new InterBankTransferParams
                {
                    Amount = totalTRY,
                    ReceiverAccount = vendorData.AccountNumber,
                    ReceiverSuffix = vendorData.AccountSuffix,
                    Description = desc,
                    SenderSuffix = vendorData.TLSuffix,
                    TransferType = 3,
                    UserName = KuwaitAccessHandler.FINTAG
                };
                var refId = "";
                success = false;
                attempts = 0;
                InterBanTransferResult bresult = null;
                while (attempts <= 5 && success == false)
                {
                    attempts++;
                    bresult = await KTApiService.InterBankTransferAsync(iparams);
                    if (bresult == null)
                    {
                        continue;
                    }
                    success = bresult.Success;
                    refId = bresult.RefId;
                    Log.Debug("CashSenderService: InterBankTransfer: " + JsonConvert.SerializeObject(bresult));
                }
                if (!success)
                {
                    var avgPiyasaFiyat = payments.Select(x => x.PiyasaGramFiyat).Average();
                    var avgSatisGram = payments.Select(x => x.SatisGramFiyat).Average();
                    var notPos = new VendorNotPositionSell(firstLast,
                        totalTRY, totalGrams, vendorData.AccountSuffix, vendorData.AccountNumber,
                        rate.SellRate, payments.Count, vendor.VendorId,
                        "PLATIN:IBAN_FAIL" + refId, avgPiyasaFiyat, rate.SellRate, avgSatisGram);

                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.SaveChangesAsync();

                    var message = string.Format("KT platin bozdu ama iban transfer olmadi - vendor: {0} - grams: {1} - TRY : {2} - notPositionSell eklendi", vendor.Name, totalGrams, totalTRY);

                    /*SMSService.SendSms("05323878550", message);
                    SMSService.SendSms("05433893303", message);
                    SMSService.SendSms("05063337007", message);*/
                    var subject = "Metal Sell Platin Interbank transfer Fail";
                    await EmailService.SendEmailAsync("trnmmtz@gmail.com", subject, message, false);
                    await EmailService.SendEmailAsync("dolunaysabuncuoglu@gmail.com", subject, message, false);
                    await EmailService.SendEmailAsync("emreelvanus@gmail.com", subject, message, false);
                    return notPos;
                }
                var averageFintag = totalTRY / totalGrams;
                foreach (var payment in payments)
                {
                    var trans = await vendorsRepository.GetVenTransactionAsync(payment.TransactionId);
                    trans.TamamlanmisKTreferans = refId;
                    trans.GercekKur = averageFintag;
                }
                /*
                 KAR(Goldtag den altın bozdurma) : GoldAmount * (KTGramFiyat - SatışGRAMFiyat)
                 */
                var finalKar = totalGrams * (rate.SellRate - averageFintag);
                var tl = totalTRY * (-1);
                var gram = totalGrams;
                var averagePiyasa = payments.Select(x => x.PiyasaGramFiyat).Average();
                var finalized = new VendorFinalized(
                               vendor.VendorId,
                               payments.Count,
                               firstLast,
                               totalTRY,
                               totalGrams,
                               averagePiyasa,
                               rate.SellRate,
                               averageFintag,
                               "PLATIN",
                               refId,
                               "PLATIN BOZDURMA OK",
                               tl,
                               gram,
                               finalKar);
                await vendorsRepository.AddVendorFinalized(finalized);
                await vendorsRepository.SaveChangesAsync();
                return finalized;
            }
            catch (Exception e)
            {
                Log.Error("CashSenderService DoWorkPlatin error: " + e.Message);
                SMSService.SendSms("05448980702", "CashSenderService DoWorkPlatin error: " + e.Message);
                Log.Error(e.StackTrace);
                return e;
            }
        }

        private static int DEBUg = 0;

        private async void DoWork(object? state)
        {
            try
            {
                DEBUg++;

                Log.Debug("CashSenderService: Doing work at " + DateTime.Now.ToString());
                if(Running)
                {
                    if (DEBUg % 2 == 1)
                        Log.Debug("ExpectedCashService: Already Running");
                    return;
                }
                if (FirstTimer)
                {
                    Log.Debug("CashSenderService firstTimer waiting 30 seconds");
                    await Task.Delay(40000);
                    Log.Debug("CashSenderService sleep done");
                    FirstTimer = false;
                    var status = await vendorsRepository.GetSellServiceStatusAsync();
                    if (status.Amount.HasValue && status.Amount.Value == 1)
                    {
                        SenderRun = true;
                    }
                    else if (status.Amount.HasValue && status.Amount.Value == 0)
                    {
                        SenderRun = false;
                    }
                    var minutes = await vendorsRepository.GetPaycellMinutesSell();
                    MinsWait = minutes;

                    while (LastGoodFxResultModel == null)
                    {
                        Log.Debug("CashSenderService: taking last good price");
                        LastGoodFxResultModel = await KTApiService.GetCurrentPriceFromKtApiASync();
                        await Task.Delay(100);
                    }
                }
                var minsWait = MinsWait;
                var diff = DateTime.Now - LastRunTime;
                if (diff.TotalMinutes < minsWait)
                {
                    if (DEBUg % 2 == 1)
                        Log.Debug("ExpectedCashService: Not enough time passed");
                    return;
                }
                LastRunTime = DateTime.Now;
                Running = true;
                
                
                if (!SenderRun)
                {
                    Log.Debug("CashSenderService: SenderRun false");
                    Running = false;
                    return;
                }

                while (KuwaitAccessHandler.AccessToken == null)
                {
                    Log.Debug("CashSenderService: KT Access Token Not Ready sleeping 10 seconds");
                    await Task.Delay(10000);
                }

                Thread.BeginCriticalRegion();
                var goldGroups = new List<VendorCashPayment>(CashPaymentForGold).GroupBy(x => x.VendorId);
                var silverGroups = new List<VendorCashPayment>(CashPaymentForSilver).GroupBy(x => x.VendorId);
                var platinGroups = new List<VendorCashPayment>(CashPaymentForPlatin).GroupBy(x => x.VendorId);
                CashPaymentForGold.Clear();
                CashPaymentForSilver.Clear();
                CashPaymentForPlatin.Clear();
                Thread.EndCriticalRegion();

                foreach (var gr in goldGroups)
                {
                    await DoWorkGold(gr.ToList());
                }

                foreach (var gr in silverGroups)
                {
                    await DoWorkSilver(gr.ToList());
                }

                foreach (var gr in platinGroups)
                {
                    await DoWorkPlatin(gr.ToList());
                }

                LastRunTime = DateTime.Now;
                Running = false;
                
            }
            catch (Exception e)
            {
                if (Running)
                {
                    Running = false;
                    LastRunTime = DateTime.Now;
                }
                Log.Error("CashSenderService error: " + e.Message);
                Log.Error(e.StackTrace);
            }
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Debug("CashSenderService: Starting");
            
            timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Debug("CashSenderService: Stopping");
            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            return ((IAsyncDisposable)timer).DisposeAsync();
        }
    }
}
