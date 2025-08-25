using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Services;
using Gold.Api.Services.Interfaces;
using Gold.Api.Utilities;
using Gold.Core.Vendors;
using Gold.Domain.Vendors;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Robots
{
    public class CashSenderBot : ICashSender, IDisposable
    {

        private readonly IVendorsRepository vendorsRepository;
        private readonly IVendorBuySellService vendorBuySellService;
        private bool running;

        public CashSenderBot(IVendorsRepository vendorsRepository, IVendorBuySellService vendorBuySellService)
        {
            this.vendorBuySellService = vendorBuySellService;
            this.vendorsRepository = vendorsRepository;
            this.running = false;
            Log.Information("CashSenderBot Created");
        }

        public async void Run()
        {
            
            try
            {
                if (running)
                {
                    Log.Debug("sender already running");
                    return;
                }
                running = true;

                var shouldRun = await vendorsRepository.ShouldSenderRun();

                if (!shouldRun)
                {
                    Log.Debug("Not running sender cash");
                    running = false;
                    return;
                }
                if (KuwaitAccessHandler.AccessToken == null)
                {
                    Log.Debug("Sender cash not ready because not connected to KT");
                    running = false;
                    return;
                }

                var cashPayments = vendorsRepository.GetVendorCashPaymentsAsyncEnumerable();
               
                await foreach (var cashPayment in cashPayments)
                {
                    await HandlePayment(cashPayment);
                }

            }
            catch (Exception e)
            {
                Log.Error("error at cash sender: " + e.Message);
                Log.Error(e.StackTrace);

                e = e.InnerException;

                while (e != null)
                {
                    Log.Error("error at cash sender: " + e.Message);
                    Log.Error(e.StackTrace);
                    e = e.InnerException;
                }
            }
            running = false;
        }


        private async Task<MetalSellResult> PreciousMetalSell(VendorTransactionNew transaction,
            string to, string from, decimal sellRate)
        {
            var param = new PreciousMetalsSellParams
            {
                Amount = transaction.GramAmount,
                SuffixFrom = from,
                SuffixTo = to,
                UserName = KuwaitAccessHandler.FINTAG,
                SellRate = sellRate
            };
            var attempts = 0;
            var success = false;
            MetalSellResult result = null;
            string refid = "";
            while (attempts <= 5 && success == false)
            {
                attempts++;
                result = await KTApiService.PreciousMetalSellAsync(param);
                if (result == null)
                {
                    continue;
                }
                
                success = result.Success;
                refid = result.RefId;
            }
            return result;
        }


        private async Task<InterBanTransferResult> InterBankTransfer(decimal tlAmount, string account, string suffix,
            string description, string sendersuff, int transferType)
        {
            var iparams = new InterBankTransferParams
            {
                Amount = tlAmount,
                ReceiverAccount = account,
                ReceiverSuffix = suffix,
                Description = description,
                SenderSuffix = sendersuff,
                TransferType = transferType,
                UserName = KuwaitAccessHandler.FINTAG
            };
            string refId = "";
            var success = false;
            var attempts = 0;
            InterBanTransferResult result = null;
            while (attempts <= 5 && success == false)
            {
                attempts++;
                result = await KTApiService.InterBankTransferAsync(iparams);
                if (result == null)
                {
                    continue;
                }
                success = result.Success;
                refId = result.RefId;
            }
            return result;
        }

        private async Task<bool> HandlePayment(VendorCashPayment payment)
        {
            try
            {
                var vendor = await vendorsRepository.GetVendorAsync(payment.VendorId);
                var transaction = await vendorsRepository.GetVendorTransactionNewAsync(payment.TransactionId);
                var data = await vendorsRepository.GetSpecificVendorDataReadOnlyAsync(vendor.VendorId.ToString());
                var fxResult = await KTApiService.GetCurrentPriceFromKtApiASync();

                if (Utility.AnyNull(vendor, transaction, data, fxResult))
                {
                    return false;
                }

                var silverTransaction = (transaction.Comment.StartsWith("arc_sell_silver"));
                var platinTransaction = (transaction.Comment.StartsWith("arc_sell_platin"));

                string desc, situation, refId, from, to = data.TLSuffix;
                decimal sellRate;
                FxRate fintagRate;
                MetalSellResult metalSell = null;
                if (silverTransaction)
                {
                    var rate = fxResult.value.FxRates.Where(x => x.FxId == 26).FirstOrDefault();
                    metalSell = await PreciousMetalSell(transaction, data.TLSuffix, data.SLVSuffix, rate.SellRate);
                    from = data.SLVSuffix;
                    sellRate = rate.SellRate;
                    situation = "SILVER_BOZDURMA";
                    fintagRate = GlobalGoldPrice.GetSilverPricesCached();
                    desc = string.Format("Goldtag den Gumus bozdurma {0}TRY - {1}gr", transaction.TlAmount, transaction.GramAmount);
                }
                else if (platinTransaction)
                {
                    refId = "fail";
                    from = "plt";
                    sellRate = 0;
                    situation = "PLATIN_BOZDURMA";
                    fintagRate = GlobalGoldPrice.GetPlatinPrices();
                    desc = string.Format("Goldtag den Platin bozdurma {0}TRY - {1}gr", transaction.TlAmount, transaction.GramAmount);
                }
                else
                {
                    var rate = fxResult.value.FxRates.Where(x => x.FxId == 24).FirstOrDefault();
                    metalSell = await PreciousMetalSell(transaction, data.TLSuffix, data.GOLDSuffix, rate.SellRate);
                    from = data.GOLDSuffix;
                    sellRate = rate.SellRate;
                    situation = "GOLD_BOZDURMA";
                    fintagRate = GlobalGoldPrice.GetGoldPricesCached();
                    desc = string.Format("Goldtag den Altin bozdurma {0}TRY - {1}gr", transaction.TlAmount, transaction.GramAmount);
                }
                if (metalSell.Success == false)
                {
                    // unable to sell gold or silver or platin so it is not positioning

                    var notPos = new VendorNotPositionSell(metalSell.RefId, transaction.TlAmount,
                        transaction.GramAmount, from, to, sellRate, transaction.TransactionId,
                        vendor.VendorId, situation, 0, sellRate, fintagRate.SellRate);
                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.RemoveCashPayment(payment);
                    await vendorsRepository.SaveChangesAsync();
                    return false;
                }

                var ibanResult = await InterBankTransfer(transaction.TlAmount, data.AccountNumber, data.AccountSuffix,
                    desc, to, 3);

                var tl = transaction.TlAmount * (-1);
                var gram = transaction.GramAmount;

                if (silverTransaction)
                {
                    vendor.SilverBalance = vendor.SilverBalance.Value - transaction.GramAmount;
                }
                else if (platinTransaction)
                {
                    // todo
                }
                else
                {
                    vendor.Balance = vendor.Balance - transaction.GramAmount;
                }

                if (ibanResult.Success == false)
                {
                    var notPos = new VendorNotPositionSell(ibanResult.RefId, transaction.TlAmount,
                                transaction.GramAmount, data.AccountSuffix, data.AccountNumber, fintagRate.SellRate,
                                transaction.TransactionId,
                                vendor.VendorId, situation, 0, sellRate, fintagRate.SellRate);
                    await vendorsRepository.AddVendorNotPositionSell(notPos);
                    await vendorsRepository.RemoveCashPayment(payment);
                    await vendorsRepository.SaveChangesAsync();
                    return false;
                }

                var finalized = new VendorFinalized(
                                vendor.VendorId,
                                transaction.TransactionId,
                                transaction.VendorReferenceId,
                                transaction.TlAmount,
                                transaction.GramAmount,
                                0, sellRate, fintagRate.SellRate, situation, ibanResult.RefId, "VENDOR_SELL", tl, gram, 0);
                await vendorsRepository.AddVendorFinalized(finalized);
                await vendorsRepository.RemoveCashPayment(payment);

                transaction.CompleteTransaction(situation);
                await vendorsRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                var vendor = await vendorsRepository.GetVendorAsync(payment.VendorId);
                var transaction = await vendorsRepository.GetVendorTransactionNewAsync(payment.TransactionId);
                var data = await vendorsRepository.GetSpecificVendorDataReadOnlyAsync(vendor.VendorId.ToString());
                var notPos = new VendorNotPositionSell("error: " + e.Message, transaction.TlAmount,
                                transaction.GramAmount, data.AccountSuffix, data.AccountNumber, payment.SatisGramFiyat,
                                transaction.TransactionId,
                                vendor.VendorId, "ERROR", 0, payment.KTGramFiyat, payment.SatisGramFiyat);
                await vendorsRepository.AddVendorNotPositionSell(notPos);
                await vendorsRepository.RemoveCashPayment(payment);

                transaction.CompleteTransaction("ERROR: CHECK NOT POSITION");
                await vendorsRepository.SaveChangesAsync();

                return false;
            }
            
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
