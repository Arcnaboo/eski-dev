using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Models.Vendors;
using Gold.Api.Services;
using Gold.Api.Services.Interfaces;
using Gold.Api.Utilities;
using Gold.Core.Vendors;
using Gold.Domain.Users.Interfaces;
using Gold.Domain.Users.Repositories;
using Gold.Domain.Vendors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Microsoft.EntityFrameworkCore.Query;

namespace Gold.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class VendorsController : ControllerBase
    {
        private static readonly string FintagVendorId = "AD13439C-2CF3-46F6-A165-4D4B098954D5";
        /*
         
        Vendor Yaratma sistemi: (ESKIDI)

        1) Herhangi bir browserdan su adrese gidilir
            (dev server)https://www.fintag.org/vendors/get_creator_code?name=dolunay_goldtag
            (live server)https://www.fintag.net/vendors/get_creator_code?name=dolunay_goldtag

        2) get_creator_code 0532 387 85 50 numarali telefona Creator Code yollar

        3) herhangi bir browserdan su adrese gidilir 
            (dev server)https://www.fintag.org/vendors/create_vendor?name=VENDORNAME&email=VENDORMAIL&phone=VENDORPHONE&code=CREATORCODE
            (live server)https://www.fintag.net/vendors/create_vendor?name=VENDORNAME&email=VENDORMAIL&phone=VENDORPHONE&code=CREATORCODE
        
        4) create_vendor dolunaysabuncuoglu@gmail.com a Vendor Api_key ve ilk secret i yollar ve browserda gosterir
         */
        /*
         
         

         // login etc
         1) [Anonymous] vendor_login 
                <input username + password(sha256) 
                outputs> status (True/False) + message (status message) + VendorModel(balance + id + AuthToken) 

         2) [Anonymous] vendor_change_password
               <input vendor_id + new_password + old_password + g_code (special code provided by goldtag)
               outputs> status + message
         

         // request info
         3) [Auth] vendor_request_price 
            outputs> status + message +  alis + satis
          
         // request buy sell
         4) [Auth] vendor_buy_gold 
                <input gram_amount + reference_id 
                outputs> status + message gram_amount + ?(nullable)tlamount  + (nullable)?transfer_request_id

         5) [Auth] vendor_sell_gold 
                <input gram_amount + reference_id 
                outputs> status + message + gram_amount + (nullable)?tlamount + (nullable)?transfer_request_id
          
         // complete requests
         6) [Auth] vendor_complete_request 
                <input transfer_reqest_id + confirmed(True/False) + message (vendor message)
                outputs> status + message + (nullable)?transaction_id

         // request data
         7) [Auth] vender_request_transactions
                <input date_from + date_to(date given also included) 
                outputs> status + message + paged_list of TransactionModels (can be empty list)

         8) [Auth] vendor_transaction_details
                <input transaction_id
                outputs> status + message + (nullable)?TransactionModel



         */
        private readonly IVendorsRepository Repository;
        private readonly IUsersRepository UsersRepo;
        private readonly IVendorBuySellService buySellService;
        private readonly ExpectedCashService expectedCashService;
        private readonly CashSenderService cashSenderService;
        public VendorsController(
            ExpectedCashService expectedCashService,
            CashSenderService cashSenderService,
            IVendorBuySellService buySellService, 
            IVendorsRepository repository, 
            IUsersRepository urepo)
        {
            this.cashSenderService = cashSenderService;
            this.Repository = repository;
            this.expectedCashService = expectedCashService;
            this.UsersRepo = urepo;
            this.buySellService = buySellService;

            var datas = Repository.GetVendorDatas().ToList();

            var dict = new Dictionary<Guid, bool>();
            foreach (var data in datas)
            {
                var automatic = (data.Automatic.HasValue && data.Automatic.Value);
                dict.Add(Guid.Parse(data.RelatedId), automatic);
            }
            this.buySellService.SetAutomatics(dict);
        }

        [HttpGet]
        [Route("get_set_prices")]
        public async Task<ActionResult> GetSetPrices(bool automatic, 
            decimal goldbuy, decimal goldsell, decimal silverbuy, decimal silversell,
            decimal platinbuy, decimal platinsell)
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }
                
                if (!automatic)
                {
                    if (goldbuy <= 0 || goldsell <= 0 || silverbuy <= 0 || silversell <= 0 ||
                        platinbuy <= 0 || platinsell <= 0)
                    {
                        return Ok("Negative buy sell rate given");
                    }
                    if (goldsell >= goldbuy || silversell >= silverbuy || platinsell >= platinbuy)
                    {
                        return Ok("sell price should not exceed buy price");
                    }

                    GlobalGoldPrice.SetPricesManually(goldbuy, goldsell, silverbuy, silversell, platinbuy, platinsell);
                }
                else
                {
                    GlobalGoldPrice.SetAutomaticTrue();
                }
                
                

                return Ok(string.Format("Automatic: {0} - GoldBuy {1} - GoldSell {2} - SilverBuy {3} - SilverSell {4}", GlobalGoldPrice.Automatic, GlobalGoldPrice.GoldBuy, GlobalGoldPrice.GoldSell, GlobalGoldPrice.SilverBuy, GlobalGoldPrice.SilverSell));

            }
            catch (Exception e)
            {
                Log.Error("error at get_set_service_status: " + e.Message);
                Log.Error(e.StackTrace);

                return Ok("get_set_service_status error: " + e.Message);
            }
        }


        [HttpGet]
        [Route("get_set_service_status")]
        public async Task<ActionResult> GetSetBuySellServices(int buy, int sell)
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }
                var buyRun = await Repository.GetBuyServiceStatusAsync();
                var sellRun = await Repository.GetSellServiceStatusAsync();
                if (buy == 1)
                {
                    buyRun.Amount = 1;
                    expectedCashService.SetExpectedRun(true);
                }
                if (buy == 0)
                {
                    expectedCashService.SetExpectedRun(false);
                    buyRun.Amount = 0;
                }
                if (sell == 1)
                {
                    sellRun.Amount = 1;
                    cashSenderService.SetSenderRun(true);
                }
                if (sell == 0)
                {
                    sellRun.Amount = 0;
                    cashSenderService.SetSenderRun(false);
                }
                await Repository.SaveChangesAsync();

                return Ok(string.Format("buy runner {0} - sell runner {1}", buyRun.Amount.Value, sellRun.Amount.Value));

            }
            catch (Exception e)
            {
                Log.Error("error at get_set_service_status: " + e.Message);
                Log.Error(e.StackTrace);

                return Ok("get_set_service_status error: " + e.Message);
            }
        }

        [HttpGet]
        [Route("get_set_paycell_minutes_buy")]
        public async Task<ActionResult> GetSetPaycellBuyMinutes(int minutes)
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }
                
                if (minutes == 0)
                {
                    minutes = await Repository.GetPaycellMinutesBuy();
                    return Ok(string.Format("Current paycell buy minutes: {0}", minutes));
                }
                else
                {
                    if (minutes < 1)
                    {
                        return Ok("error minutes vairabl must be >= 1");
                    }

                    await Repository.SetPaycellMinutesBuy(minutes);
                    await Repository.SaveChangesAsync();
                    expectedCashService.SetBuyMinutes(minutes);
                    return Ok(string.Format("Current paycell buy minutes updated to: {0}", minutes));
                }
                
            }
            catch (Exception e)
            {
                Log.Error("error at get_set_paycell_minutes_buy: " + e.Message);
                Log.Error(e.StackTrace);

                return Ok("get_set_paycell_minutes_buy error: " + e.Message);
            }
        }

        [HttpGet]
        [Route("get_set_paycell_minutes_sell")]
        public async Task<ActionResult> GetSetPaycellSellMinutes(int minutes)
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                if (minutes == 0)
                {
                    minutes = await Repository.GetPaycellMinutesSell();
                    return Ok(string.Format("Current paycell sell minutes: {0}", minutes));
                }
                else
                {
                    if (minutes < 1)
                    {
                        return Ok("error minutes vairable must be >= 1");
                    }

                    await Repository.SetPaycellMinutesSell(minutes);
                    await Repository.SaveChangesAsync();
                    cashSenderService.SetSellMinutes(minutes);
                    return Ok(string.Format("CUrrent paycell sell minutes updated to: {0}", minutes));
                }

            }
            catch (Exception e)
            {
                Log.Error("error at get_set_paycell_minutes_sell: " + e.Message);
                Log.Error(e.StackTrace);

                return Ok("get_set_paycell_minutes_sell error: " + e.Message);
            }
        }

        [HttpGet]
        [Route("get_set_expected_timeout_minutes")]
        public async Task<ActionResult> GetSetExpectedToutMinutes(int minutes)
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                if (minutes == 0)
                {
                    minutes = await Repository.GetExpectedTimeoutMinutes();
                    return Ok(string.Format("Current expected timeout minutes: {0}", minutes));
                }
                else
                {
                    if (minutes < 1)
                    {
                        return Ok("error minutes vairable must be >= 1");
                    }

                    await Repository.SetExpectedTimeoutMinutes(minutes);
                    await Repository.SaveChangesAsync();
                    expectedCashService.SetTimeoutMinutes(minutes);
                    return Ok(string.Format("Current expected timeout minutes updated to: {0}", minutes));
                }

            }
            catch (Exception e)
            {
                Log.Error("error at get_set_expected_timeout_minutes: " + e.Message);
                Log.Error(e.StackTrace);

                return Ok("get_set_expected_timeout_minutes error: " + e.Message);
            }
        }

        [HttpGet]
        [Route("get_set_vendor_threshold")]
        public async Task<ActionResult> GetSetVendorThreshold(Guid vendorId, 
            string madenCode, bool status, decimal? threshold)
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }
                var vendor = Repository.GetVendors().Where(x => x.VendorId == vendorId).FirstOrDefault();
                if (vendor == null)
                {
                    return Ok("invalid vendor id");
                }
                var vendorIdStr = vendorId.ToString();
                var data = Repository.GetVendorDatas().Where(x => x.RelatedId == vendorIdStr).FirstOrDefault();
                if (data == null)
                {
                    return Ok("illegal vendor data");
                }

                if (madenCode == "GOLD")
                {
                    data.SetThresholdGold(status, threshold);
                }
                else if (madenCode == "SILVER")
                {
                    data.SetThresholdSilver(status, threshold);
                }

                await Repository.SaveChangesAsync();

                var goldThresh = (data.BalanceThresholdGold.HasValue) ? data.BalanceThresholdGold.Value : 0;
                var silvThresh = (data.BalanceThresholdSilver.HasValue) ? data.BalanceThresholdSilver.Value : 0;

                var result = string.Format("Vendor {0} || GoldThresh is {1} with value {2} || SilverThresh is {3} with value {4}",
                    vendor.Name, data.ThresholdGoldActive, goldThresh, data.ThresholdSilverActive, silvThresh);
                return Ok(result);

            }
            catch (Exception e)
            {
                Log.Error("error at get_set_vendor_threshold: " + e.Message);
                Log.Error(e.StackTrace);

                return Ok("get_set_vendor_threshold error: " + e.Message);
            }
        }

        [HttpGet]
        [Route("get_set_vendor_automatic")]
        public async Task<ActionResult> GetSetVendorAutomatic(Guid vendorId, int automatic, int automatic_sell)
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }
                var vendor = Repository.GetVendors().Where(x => x.VendorId == vendorId).FirstOrDefault();
                if (vendor == null)
                {
                    return Ok("invalid vendor id");
                }
                var vendorIdStr = vendorId.ToString();
                var data = Repository.GetVendorDatas().Where(x => x.RelatedId == vendorIdStr).FirstOrDefault();
                if (data == null)
                {
                    return Ok("illegal vendor data");
                }

                var giveWarning = false;

                if (automatic == 0)
                {
                    data.Automatic = false;
                    await Repository.SaveChangesAsync();
                    buySellService.SetAutomatic(vendorId, false);
                }
                else if (automatic == 1)
                {
                    data.Automatic = true;
                    await Repository.SaveChangesAsync();
                    buySellService.SetAutomatic(vendorId, true);
                }
                else
                {
                    giveWarning = true;
                }

                if (automatic_sell == 0)
                {
                    data.AutomaticSell = false;
                    await Repository.SaveChangesAsync();
                    buySellService.SetAutomaticSell(vendorId, false);
                }
                else if (automatic_sell == 1)
                {
                    data.AutomaticSell = true;
                    await Repository.SaveChangesAsync();
                    buySellService.SetAutomaticSell(vendorId, true);
                }
                else
                {
                    giveWarning = true;
                }

                if (!giveWarning)
                {
                    return Ok(string.Format("Vendor {0} - AutoBuy={1} AutoSell={2}", vendor.Name, data.Automatic.Value, data.AutomaticSell.Value));
                }
                else
                {
                    return Ok(string.Format("WARNING: automaticbuy or automaticsell parameter was not set to 1 or 0..... Vendor {0} - AutoBuy={1} AutoSell={2}", vendor.Name, data.Automatic.Value, data.AutomaticSell.Value));
                }
                
            }
            catch (Exception e)
            {
                Log.Error("error at get_set_vendor_automatic: " + e.Message);
                Log.Error(e.StackTrace);

                return Ok("get_set_vendor_automatic error: " + e.Message);
            }
        }


        [HttpGet]
        [Route("get_set_paycell_automatic")]
        public async Task<ActionResult> GetSetPaycellAutomatic(int automatic, int automatic_sell)
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }
                var paycell = KuwaitAccessHandler.PAYCELL_ID.ToString();
                var mumtaz = KuwaitAccessHandler.MUMTAZ_ID.ToString();
                var moneypay = KuwaitAccessHandler.MONEYPAY_ID.ToString();
                var mmtz = Repository.GetVendorDatas().Where(x => x.RelatedId == mumtaz).FirstOrDefault();
                var cell = Repository.GetVendorDatas().Where(x => x.RelatedId == paycell).FirstOrDefault();
                var mpay = Repository.GetVendorDatas().Where(x => x.RelatedId == moneypay).FirstOrDefault();

                if (automatic == -1)
                {
                    
                    return Ok(string.Format("Current automatic paycell {0} - mumtaz {1} - MoneyPay {2}", cell.Automatic.Value, mmtz.Automatic.Value, mpay.Automatic.Value));
                }
                else if (automatic == 0)
                {
                    mmtz.Automatic = false;
                    cell.Automatic = false;
                    mpay.Automatic = false;
                    await Repository.SaveChangesAsync();

                    buySellService.SetAutomatic(KuwaitAccessHandler.PAYCELL_ID, false);
                    buySellService.SetAutomatic(KuwaitAccessHandler.MUMTAZ_ID, false);
                    buySellService.SetAutomatic(KuwaitAccessHandler.MONEYPAY_ID, false);
                }
                else
                {
                    mmtz.Automatic = true;
                    cell.Automatic = true;
                    mpay.Automatic = true;
                    await Repository.SaveChangesAsync();
                    buySellService.SetAutomatic(KuwaitAccessHandler.PAYCELL_ID, true);
                    buySellService.SetAutomatic(KuwaitAccessHandler.MUMTAZ_ID, true);
                    buySellService.SetAutomatic(KuwaitAccessHandler.MONEYPAY_ID, true);
                }
                return Ok(string.Format("Current changed automatic paycell {0} - mumtaz {1} - MoneyPay {2}", cell.Automatic.Value, mmtz.Automatic.Value, mpay.Automatic.Value));
            }
            catch (Exception e)
            {
                Log.Error("error at get_set_paycell_automatic: " + e.Message);
                Log.Error(e.StackTrace);

                return Ok("get_set_paycell_automatic error: " + e.Message);
            }
        }

        /// <summary>
        /// depreciated
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_expecteds")]
        public ActionResult<List<ExpectedModel>> GetExpecteds(int days)
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }
                int d = (days <= 0) ? 1 : days;

                DateTime date = DateTime.Now.AddDays(-d);
                var expecteds = Repository
                    .GetExpectedCashes()
                    .Where(x => x.DateTime.Date >= date.Date)
                    .OrderBy(x => x.DateTime)
                    .ToList();

                var result = new List<ExpectedModel>();

                foreach (var expected in expecteds)
                {
                    result.Add(new ExpectedModel(expected, Repository));
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                return Ok(new List<object>());
            }
        }

        [HttpGet]
        [Route("get_finalized_golds")]
        public ActionResult<List<FinalizedModel>> GetFinalizedGolds()
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }
                var finalized = Repository
                    .GetFinalizedGolds()
                    .OrderByDescending(x => x.DateTime)
                    .ToList();

                var result = new List<FinalizedModel>();

                foreach(var final in finalized)
                {
                    result.Add(new FinalizedModel(final, Repository));
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                return Ok(new List<object>());
            }
        }


        [HttpGet]
        [Route("get_expecteds_v2")]
        public ActionResult<List<ExpectedModel>> GetVendorExpecteds()
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                var expecteds = Repository.GetVendorExpecteds()
                    .OrderByDescending(x => x.DateTime)
                    .ToList();
                    

                var result = new List<ExpectedModel>();

                foreach (var expected in expecteds)
                {
                    result.Add(new ExpectedModel(expected, Repository));
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("error at get expecteds 2: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<object>());
            }
        }

        [HttpGet]
        [Route("get_finalizeds_v2")]
        public ActionResult<List<FinalizedModel>> GetVendorFinalizeds()
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }
                var finalized = Repository.GetVendorFinalizeds()
                    .OrderByDescending(x => x.DateTime)
                    .ToList();

                var result = new List<FinalizedModel>();

                foreach (var final in finalized)
                {
                    result.Add(new FinalizedModel(final, Repository));
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor finalizeds: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<object>());
            }
        }

        [HttpPost]
        [Route("get_finalizeds_filter")]
        public ActionResult<List<FinalizedModel>> GetVendorFinalizedsFiltered(VendorTransactionRequestParamModel model)
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                if (!VendorTransactionRequestParamModel.ValidateModel(model))
                {
                    return Ok(new List<object>() {"INVALID MODEL" });
                }
                var from = DateTime.ParseExact(model.DateFrom, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var to = DateTime.ParseExact(model.DateTo, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var finalized = new List<VendorFinalized>();

                if (model.VendorId != null)
                {
                    var id = Guid.Parse(model.VendorId);
                    finalized = Repository
                        .GetVendorFinalizeds()
                        .Where(x => x.VendorId == id && x.DateTime >= from && x.DateTime <= to)
                        .OrderByDescending(x => x.DateTime)
                        .ToList();
                }
                else
                {
                    finalized = Repository
                        .GetVendorFinalizeds()
                        .Where(x => x.DateTime >= from && x.DateTime <= to)
                        .OrderByDescending(x => x.DateTime)
                        .ToList();
                }

                var result = new List<FinalizedModel>();

                foreach (var final in finalized)
                {
                    result.Add(new FinalizedModel(final, Repository));
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor finalizeds: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<object>() { "Error at get vendor finalizeds: " + e.Message });
            }
        }

        [HttpGet]
        [Route("get_unexpecteds_v2")]
        public ActionResult<List<UnExpectedModel>> GetUnexpecteds()
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                var unexpecteds = Repository.GetVendorUnExpecteds().OrderByDescending(x => x.DateTime)
                    .ToList();

                var result = new List<UnExpectedModel>();

                foreach (var unex in unexpecteds)
                {
                    result.Add(new UnExpectedModel(unex, Repository));
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor unexpecteds: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<object>());
            }
        }

        [HttpPost]
        [Route("get_unexpecteds_filter")]
        public ActionResult<List<UnExpectedModel>> GetUnexpectedsFiltered(VendorTransactionRequestParamModel model)
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                if (!VendorTransactionRequestParamModel.ValidateModel(model))
                {
                    return Ok(new List<object>() { "INVALID MODEL" });
                }

                var from = DateTime.ParseExact(model.DateFrom, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var to = DateTime.ParseExact(model.DateTo, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                var allUnexpecteds = Repository.GetVendorUnExpecteds()
                        .Where(x => x.DateTime >= from && x.DateTime <= to)
                        .OrderByDescending(x => x.DateTime)
                        .ToList();

                var unexpecteds = new List<VendorUnExpected>();

                if (model.VendorId != null)
                {
                    var id = Guid.Parse(model.VendorId);
                    foreach (var unx in allUnexpecteds)
                    {
                        var transaction = Repository.GetVenTransaction(unx.TransactionId);
                        if (transaction == null)
                        {
                            continue;
                        }
                        if (transaction.Source == id || transaction.Destination == id)
                        {
                            unexpecteds.Add(unx);
                        }
                    }
                }
                else
                {
                    unexpecteds.AddRange(allUnexpecteds);
                }

                var result = new List<UnExpectedModel>();

                foreach (var unex in unexpecteds)
                {
                    result.Add(new UnExpectedModel(unex, Repository));
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor unexpecteds v3: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<object>());
            }
        }

        [HttpGet]
        [Route("get_not_positions")]
        public async Task<ActionResult<List<NotPositionModel>>> GetNotPositions()
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();
                
                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                var notPoses = Repository.GetVendorNotPositionsAsyncEnum();
                
                var result = new List<NotPositionModel>();
                await foreach (var notPos in notPoses)
                {
                    result.Add(new NotPositionModel(notPos));
                }
                foreach (var notPos in result)
                {
                    await notPos.AddName(Repository);
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor not positions: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<object>());
            }
        }

        [HttpPost]
        [Route("get_not_positions_filter")]
        public ActionResult<List<NotPositionModel>> GetNotPositionsFiltered(VendorTransactionRequestParamModel model)
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                if (!VendorTransactionRequestParamModel.ValidateModel(model))
                {
                    return Ok(new List<object>() { "INVALID MODEL" });
                }
                var from = DateTime.ParseExact(model.DateFrom, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var to = DateTime.ParseExact(model.DateTo, "dd-MM-yyyy", CultureInfo.InvariantCulture);

  
                var notPoses = new List<VendorNotPosition>();

                if (model.VendorId != null)
                {
                    var id = Guid.Parse(model.VendorId);
                    
                    var query = Repository.GetVendorNotPositions()
                    .Where(x => x.VendorId == id && x.DateTime >= from && x.DateTime <= to)
                    .OrderByDescending(x => x.DateTime)
                    .ToList();
                    notPoses.AddRange(query);
                }
                else
                {
                    var query = Repository.GetVendorNotPositions()
                    .Where(x => x.DateTime >= from && x.DateTime <= to)
                    .OrderByDescending(x => x.DateTime)
                    .ToList();
                    notPoses.AddRange(query);
                }

                var result = new List<NotPositionModel>();
                foreach (var notPos in notPoses)
                {
                    result.Add(new NotPositionModel(notPos));
                }
                foreach (var notPos in result)
                {
                    notPos.AddNameSynchrounous(Repository);
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor not positions: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<object>() { "Error at get vendor not positions: " + e.Message });
            }
        }

        [HttpGet]
        [Route("get_not_position_sells")]
        public async Task<ActionResult<List<NotPositionSellModel>>> GetNotPositionSells()
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                var notposes = Repository.GetVendorNotPositionSellsAsyncEnum();

                var result = new List<NotPositionSellModel>();

                await foreach (var unex in notposes)
                {
                    result.Add(new NotPositionSellModel(unex));
                }
                foreach (var r in result)
                {
                    await r.AddName(Repository);
                }
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor not position sells: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<object>());
            }
        }

        [HttpPost]
        [Route("get_not_position_sells_filter")]
        public ActionResult<List<NotPositionSellModel>> GetNotPositionSellsFiltered(VendorTransactionRequestParamModel model)
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }
                if (!VendorTransactionRequestParamModel.ValidateModel(model))
                {
                    return Ok(new List<object>() { "INVALID MODEL" });
                }
                var from = DateTime.ParseExact(model.DateFrom, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var to = DateTime.ParseExact(model.DateTo, "dd-MM-yyyy", CultureInfo.InvariantCulture);


                var notPoses = new List<VendorNotPositionSell>();

                if (model.VendorId != null)
                {
                    var id = Guid.Parse(model.VendorId);

                    var query = Repository.GetVendorNotPositionSells()
                    .Where(x => x.VendorId == id && x.DateTime >= from && x.DateTime <= to)
                    .OrderByDescending(x => x.DateTime)
                    .ToList();
                    notPoses.AddRange(query);
                }
                else
                {
                    var query = Repository.GetVendorNotPositionSells()
                    .Where(x => x.DateTime >= from && x.DateTime <= to)
                    .OrderByDescending(x => x.DateTime)
                    .ToList();
                    notPoses.AddRange(query);
                }

                var result = new List<NotPositionSellModel>();

                foreach (var unex in notPoses)
                {
                    result.Add(new NotPositionSellModel(unex));
                }
                foreach (var r in result)
                {
                    r.AddNameSynch(Repository);
                }
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor not position sells: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<object>() { "Error at get vendor not position sells: " + e.Message });
            }
        }

        [HttpPost]
        [Route("close_positions_buy")]
        public async Task<ActionResult<ClosePositionsResult>> ClosePositionsBuy(ClosePositionsParam model)
        {
            var result = new ClosePositionsResult { Success = false, Results = new List<object>() };
            try
            {
                if ("Arc".Length == 3)
                {
                    return Unauthorized("iptal");
                }
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                var ids = new List<string>(model.NotPositionIds.Split(','));

                var notPoses = Repository.GetVendorNotPositionsAsyncEnum(ids);

                var removeList = new List<VendorNotPosition>();
                
                await foreach (var np in notPoses)
                {
                    var metalResult = await expectedCashService.ClosePosition(np);
                    if (metalResult.Success)
                    {
                        removeList.Add(np);
                    }
                    result.Results.Add(metalResult);
                }

                await Repository.RemoveNotPositions(removeList) ;
                result.Success = true;
                result.Message = string.Format("total {0} notpos den {1} kapatildi -- {2} acik kaldi", result.Results.Count, result.Results.Count - removeList.Count);
                    return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor not position sells: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Error: " + e.Message;
                return Ok(result);
            }
        }

        [HttpPost]
        [Route("close_positions_sell")]
        public async Task<ActionResult<ClosePositionsResult>> ClosePositionsSell(ClosePositionsParam model)
        {
            var result = new ClosePositionsResult { Success = false, Results = new List<object>() };
            try
            {
                if ("Arc".Length == 3)
                {
                    return Unauthorized("iptal");
                }
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                var ids = new List<string>(model.NotPositionIds.Split(','));

                var notPoses = Repository.GetVendorNotPositionSellsAsyncEnum(ids);

                var removeList = new List<VendorNotPositionSell>();
                var currentRateGold = GlobalGoldPrice.GetGoldPricesCached();
                var currentSilver = GlobalGoldPrice.GetSilverPricesCached();

                var stillNotPosCount = 0;
                var finalizedCount = 0;

                await foreach (var np in notPoses)
                {
                    removeList.Add(np);
                    var paymentsGold = new List<VendorCashPayment>();
                    var paymentsSilver = new List<VendorCashPayment>();
                    var firstLasts = np.RefId.Split("___");
                    var first = int.Parse(firstLasts[0]);
                    var last = int.Parse(firstLasts[1]);
                    var transactions = Repository.GetVenTransactions(first, last);
                    
                    await foreach (var trans in transactions)
                    {
                        var finalized = await Repository.IsFinalized(trans.TransactionId);
                        
                        if (!finalized)
                        {
                            var fxId = (trans.Comment.StartsWith("arc_sell_silver")) ? 26 : 24;
                            var rate = (fxId == 26) ? currentSilver.SellRate : currentRateGold.SellRate;
                            var mesg = (fxId == 26) ? "SILVER" : "GOLD";
                            var vendor = await Repository.GetVendorAsync(trans.Source);
                            var vendorData = await Repository.GetVendorDataAsync(vendor.VendorId);

                            var payment = new VendorCashPayment(
                                vendor.VendorId,
                                trans.TransactionId, trans.VendorReferenceId, fxId,
                                trans.TlAmount, trans.GramAmount, 0, 0, rate, mesg);

                            if (fxId == 26)
                            {
                                paymentsSilver.Add(payment);
                            }
                            else
                            {
                                paymentsGold.Add(payment);
                            }
                        }
                    }

                    var goldResults = await cashSenderService.DoWorkGold(paymentsGold);

                    var silverResults = await cashSenderService.DoWorkSilver(paymentsSilver);

                    if (goldResults != null)
                    {
                        result.Results.Add(goldResults);
                        if (goldResults is VendorNotPositionSell)
                        {
                            stillNotPosCount++;
                        }
                        else if (goldResults is VendorFinalized) 
                        {
                            finalizedCount++;
                        }
                    }
                }

                await Repository.RemoveNotPositionSells(removeList);
                result.Success = true;
                result.Message = string.Format("total {0} notpossell den {1} tanesi finalized oldu --- {2} tanesi halen notpossell");
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor not position sells: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Error: " + e.Message;
                return Ok(result);
            }
        }

        [HttpGet]
        [Route("mumtaz_test_sell_1000")]
        public async Task<ActionResult<List<FinaliseTransactionResult2>>> MumtazTestSell()
        {

            try
            {
                if ("Arc".Length == 3)
                {
                    return Unauthorized("iptal");
                }
                var testGuid = Guid.Parse("653A4C18-13AA-4560-B98F-D11F3EAFC65F");
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var vendor = Repository.GetVendors().Where(x => x.VendorId.ToString() == userid).FirstOrDefault();

                if (vendor.VendorId != testGuid)
                {
                    return Unauthorized();
                }
                var vendorData = await Repository.GetSpecificVendorDataReadOnlyAsync(userid);
                var result = new List<FinaliseTransactionResult2>();
                
                var currentSilverRate = GlobalGoldPrice.GetSilverPricesCached();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpensesCached();
                decimal silverSalePrice = (currentSilverRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax));
                silverSalePrice = Math.Truncate(100 * silverSalePrice) / 100;

                var transPrice = 0.01m * silverSalePrice;//* currentSilverRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax);
                transPrice = Math.Truncate(100 * transPrice) / 100;
                var rand = new Random();


                var rate = KTApiService.GetCurrentPriceFromKtApi()
                    .value.FxRates.Where(x => x.FxId == 26).FirstOrDefault().SellRate;

                for (var i = 1; i <= 1000; i++)
                {
                    var randString = "arc_sell_silver:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;
                    var vendorTransaction = new VendorTransactionNew(
                        i.ToString() + "303030",
                        vendor.VendorId,
                        Guid.Parse(FintagVendorId),
                        0.01m,
                        transPrice,
                        randString);
                    await Repository.AddVendorTransactionNewAsync(vendorTransaction);
                    await Repository.SaveChangesAsync();
                    vendorTransaction.VendorConfirmed();
                    vendorTransaction.GTagConfirmed();
                    var payment = new VendorCashPayment(
                        vendor.VendorId, 
                        vendorTransaction.TransactionId,
                        vendorTransaction.VendorReferenceId, 
                        26, 
                        vendorTransaction.TlAmount, 
                        vendorTransaction.GramAmount,
                        0, rate, currentSilverRate.SellRate, 
                        "VENDOR_SELL");
                    await Repository.AddVendorCashPayment(payment);
                    var res = new FinaliseTransactionResult2
                    {
                        ResultCode = "0",
                        Message = "islem basarili"
                    };
                    result.Add(res);
                    await Repository.SaveChangesAsync();
                }



                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Error at mumtaz test sell: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<object>());
            }
        }

        [HttpGet]
        [Route("mumtaz_test_buy_1000")]
        public async Task<ActionResult<List<FinaliseTransactionResult>>> MumtazTestBuy()
        {
            if ("Arc".Length == 3)
            {
                return Unauthorized("iptal");
            }
            try
            {
                var testGuid = Guid.Parse("653A4C18-13AA-4560-B98F-D11F3EAFC65F");
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var vendor = Repository.GetVendors().Where(x => x.VendorId.ToString() == userid).FirstOrDefault();

                if (vendor.VendorId != testGuid)
                {
                    return Unauthorized();
                }
                var vendorData = await Repository.GetSpecificVendorDataReadOnlyAsync(userid);
                var result = new List<FinaliseTransactionResult>();
                var currentRate = GlobalGoldPrice.GetSilverPricesCached();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpensesCached();
                decimal silverPrice = (currentRate.BuyRate * taxAndExpenses.BankaMukaveleTax);
                silverPrice = Math.Truncate(100 * silverPrice) / 100;
                var transPrice = (0.01m * silverPrice);

                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();


                var rate = KTApiService.GetCurrentPriceFromKtApi()
                    .value.FxRates.Where(x => x.FxId == 26).FirstOrDefault().BuyRate;

                for (var i = 1; i <= 1000; i++)
                {
                    var randString = "arc_buy_silver:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;
                    var vendorTransaction = new VendorTransactionNew(
                        i.ToString() + "201020",
                        Guid.Parse(FintagVendorId),
                        vendor.VendorId,
                        0.01m,
                        transPrice,
                        randString);
                    await Repository.AddVendorTransactionNewAsync(vendorTransaction);
                    await Repository.SaveChangesAsync();
                    vendorTransaction.VendorConfirmed();
                    vendorTransaction.GTagConfirmed();

                    var expected = new VendorExpected(
                            vendorTransaction.Destination,
                            vendorTransaction.TransactionId,
                            vendorTransaction.VendorReferenceId,
                            vendorData.TLSuffix,
                            26,
                            vendorTransaction.TlAmount,
                            vendorTransaction.GramAmount,
                            0,
                            rate,
                            currentRate.BuyRate
                        );

                    var code = buySellService.GetCodeOf(expected);
                    var res = new FinaliseTransactionResult
                    {
                        ResultCode = "0",
                        Message = "islem basarili",
                        HavaleCode = code
                    };
                    result.Add(res);
                    await Repository.SaveChangesAsync();
                }
                


                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Error at mumtaz test buy: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<object>());
            }
        }

        [HttpGet]
        [Route("kt_transactions")]
        public ActionResult<KTTransactionResultsModel> KtTransactions(string suffix, string beginDate, string endDate, string itemCount)
        {
            
            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                string queryString = "";
                if (itemCount != null)
                {
                    queryString = "itemCount=" + itemCount;
                } 
                else
                {
                    queryString = "beginDate=" + beginDate + "&endDate=" + endDate;
                }

                var results = KTApiService.GetHesapHareketleri(suffix, queryString);

                return Ok(results);
            }
            catch (Exception e)
            {
                Log.Error("Err kt_transactions... " + e.Message + "\n" + e.StackTrace + "\n");
                if (e.InnerException != null)
                {
                    Exception inner = e.InnerException;

                    while (inner != null)
                    {
                        Log.Error("inner: " + inner.Message + "\n" + inner.StackTrace + "\n");
                        inner = inner.InnerException;
                    }
                }
                var result = new KTTransactionResultsModel
                {
                    Success = false
                };
                return Ok(result);
            }

        }

        [HttpGet]
        [Route("update_account")]
        public ActionResult<CreateVendorResultModel> UpdateAccount(string vendorid, string type, string suffix)
        {
            var result = new CreateVendorResultModel { Success = false };
            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    result.Message = "auth";
                    return Unauthorized(result);
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    result.Message = "auth";
                    return Unauthorized(result);
                }
                var vendor = Repository.GetVendors().Where(x => x.VendorId.ToString() == vendorid).FirstOrDefault();

                if (vendor == null)
                {
                    result.Message = "vendor id hatali";
                    return Ok(result);
                }

                var data = Repository.GetVendorDatas().Where(x => x.RelatedId == vendorid).FirstOrDefault();
                if (data == null)
                {
                    data = new VendorData(vendor.VendorId.ToString(), null, null, null, null, null);
                    Repository.AddVendorData(data);
                    Repository.SaveChanges();
                }

                if (type == "tl")
                {
                    data.TLSuffix = suffix;
                }

                if (type == "gold")
                {
                    data.GOLDSuffix = suffix;
                }

                if (type == "silver")
                {
                    data.SLVSuffix = suffix;
                }
                if (type == "number")
                {
                    data.AccountNumber = suffix;
                }
                if (type == "suffix")
                {
                    data.AccountSuffix = suffix;
                }
                Repository.SaveChanges();

                

                result.Message = "ok";
                result.Success = true;
                result.Vendor = new VendorModel {
                    Name = vendor.Name,
                    Balance = vendor.Balance.ToString(),
                    SBalance = (vendor.SilverBalance.HasValue) ? vendor.SilverBalance.Value.ToString() : "0",
                    Email = vendor.Email,
                    Id = vendor.VendorId.ToString(),
                    GOLDSuffix = data.GOLDSuffix,
                    Phone = vendor.Phone,
                    SLVSuffix = data.SLVSuffix,
                    TLSuffix = data.TLSuffix

                };
                return Ok(result);

            }
            catch (Exception e)
            {
                result.Message = e.Message;
                return Ok(result);
            }
        }

        [HttpGet]
        [Route("get_vendors")]
        public ActionResult<List<VendorModel>> GetVendors()
        {

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                var vendors = Repository.GetVendors().ToList();


                var result = new List<VendorModel>();


                foreach(var vendor in vendors)
                {
                    var data = Repository.GetVendorDatas().Where(x => x.RelatedId == vendor.VendorId.ToString()).FirstOrDefault();
                    if (data == null)
                    {
                        data = new VendorData(vendor.VendorId.ToString(), null, null, null, null, null);
                        Repository.AddVendorData(data);
                        Repository.SaveChanges();
                    }
                    result.Add(new VendorModel
                    {
                        Balance = vendor.Balance.ToString(),
                        Id = vendor.VendorId.ToString(),
                        Name = vendor.Name,
                        Phone = vendor.Phone,
                        Email = vendor.Email,
                        SBalance = vendor.SilverBalance.ToString(),
                        GOLDSuffix = data.GOLDSuffix,
                        TLSuffix = data.TLSuffix,
                        SLVSuffix = data.SLVSuffix
                        
                    });
                }
                return Ok(result);

            } 
            catch (Exception e)
            {
                return Ok(e.Message);
            }
        }

        [HttpGet]
        [Route("create_vendor")]
        public ActionResult<CreateVendorResultModel> CreateVendor(string name, string email, string phone)
        {

            var result = new CreateVendorResultModel { Success = false, Message = "Unauth" };

            try
            {

                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    
                    return Unauthorized(result);
                }

                var requestee = UsersRepo.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized(result);
                }
                if (name == null || email == null || phone == null)
                {
                    result.Message = "Invalid params";
                    return BadRequest(result);
                }

                if (Repository.GetVendors().Where(x => x.Name == name).Any())
                {
                    result.Message = "Invalid name";
                    return BadRequest(result);
                }

                var apiKey = VendorGiftService.GenerateApiKey(Repository);
                var secret = VendorGiftService.ComputeSha256Hash(apiKey);
                var vendor = new Vendor(name, email, phone, apiKey, secret);
                vendor.SilverBalance = 0;
                vendor.Balance = 0;
                Repository.AddVendor(vendor);
                Repository.SaveChanges();
                vendor = Repository.GetVendors().Where(x => x.ApiKey == apiKey).FirstOrDefault();

                var vendorData = new VendorData(vendor.VendorId.ToString(), null, null, null, null, null);
                Repository.AddVendorData(vendorData);
                Repository.SaveChanges();
                var fmessage = "Vendor {0} created| api_key: {1} | secret: {2}";

                var message = string.Format(fmessage, name, apiKey, secret);


                Log.Debug("create_vendor: " + message);

                var model = new VendorModel {
                    Balance = vendor.Balance.ToString(),
                    Id = vendor.VendorId.ToString(),
                    Name = vendor.Name,
                    Phone = vendor.Phone,
                    SBalance = vendor.SilverBalance.ToString()
                };

                result.Vendor = model;
                result.Success = true;
                result.Message = "ok";
                return Ok(result);


            }
            catch (Exception e)
            {
                Log.Error("err at create vendor: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = e.Message;
                return Ok(result);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login_v1")]
        public ActionResult<LoginVendorResultModel> LoginVendor(LoginVendorModel model)
        {
            var result = new LoginVendorResultModel { Success = false };
            try
            {
                var arda = 1;
                if (arda == 1)
                {
                    Log.Information("ARDA end cancel");
                    result.Message = "This endpoint is closed";
                    return Unauthorized(result);
                }
                Log.Information("LoginVendor() started for - " + model.ApiKey);

                var ip = Request.HttpContext.Connection.RemoteIpAddress;

                var bannedIp = Repository
                    .GetBannedIps().Where(x => x.IP == ip.ToString()).FirstOrDefault();

                if (bannedIp != null)
                {
                    result.Message = "Yasaklı IP adresi.";
                    return Ok(result);
                }

                if (model == null || model.ApiKey == null || model.Secret == null)
                {
                    result.Message = "Verilen bilgiler uyuşmamaktadır, lütfen kontrol edip giriş yapınız.";
                    return Ok(result);
                }

                Vendor vendor;
                vendor = Repository.GetVendors()
                    .Where(x => x.ApiKey == model.ApiKey)
                    .FirstOrDefault();



                if (vendor == null)
                {
                    result.Message = "Api Key i kontrol edip giriş yapınız.";
                    return Ok(result);
                }

                

                if (vendor.Secret != model.Secret)
                {
                    result.Message = "Secret i kontrol edip giriş yapınız.";


                    if (AIService.RegisterWrongSecret(model.ApiKey, ip.ToString()))
                    {
                        var newBannedIp = new BannedIp2(ip.ToString());
                        Repository.AddBannedIp(newBannedIp);
                        Repository.SaveChanges();
                        result.Message = "5 hatali secret islemi, ip yasaklandi.";
                        var msg = string.Format("Vendor id: {0} geçici olarak banlandı", vendor.VendorId);
                        EmailService.SendEmail("dolunaysabuncuoglu@gmail.com", "5 den fazla hatalı VENDOR giriş", msg, false);
                    }

                    return Ok(result);
                }

             
                var token = Authenticator.GetVendorToken(vendor.VendorId.ToString());

                Log.Information("Vendor login success: vendorId: " + vendor.VendorId.ToString());
                result.Message = "Giriş Başarılı.";
                result.AuthToken = token;
                result.Success = true;

                var vendorData = Repository.GetVendorDatas().Where(x => x.RelatedId == vendor.VendorId.ToString()).FirstOrDefault();

                if (vendorData == null)
                {
                    vendorData = new VendorData(vendor.VendorId.ToString(), null, null, null, null, null);
                    Repository.AddVendorData(vendorData);
                    Repository.SaveChanges();
                }

                var platinBalance = Repository.GetVendorPlatinBalance(vendor.VendorId);
                if (platinBalance == null)
                {
                    platinBalance = new VendorPlatinBalance(vendor.VendorId);
                    Repository.AddVendorPlatinBalance(platinBalance);
                    Repository.SaveChanges();
                }

            }
            catch (Exception e)
            {
                Log.Error("Exception at LoginVendor() - " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu lütfen da sonra tekrar deneyiniz";
                result.Success = false;
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("request_gcode")]
        public ActionResult<RequestGcodeResultModel> RequestGcode(string api_key)
        {
            var result = new RequestGcodeResultModel { Success = false };
            try
            {
                var ip = Request.HttpContext.Connection.RemoteIpAddress;

                var bannedIp = Repository
                    .GetBannedIps().Where(x => x.IP == ip.ToString()).FirstOrDefault();

                if (bannedIp != null)
                {
                    result.Message = "Yasaklı IP adresi.";
                    return Ok(result);
                }

                var vendor = Repository.GetVendors()
                    .Where(x => x.ApiKey == api_key)
                    .FirstOrDefault();

                if (vendor == null)
                {
                    result.Message = "api_key hatali";
                    return Ok(result);
                }

                var unused = Repository.
                    GetGcodes()
                    .Where(x => !x.Used && x.GeneratedFor == vendor.VendorId && DateTime.Now < x.ValidUntil)
                    .FirstOrDefault();

                if (unused != null)
                {
                    var diff = unused.ValidUntil - DateTime.Now;
                    result.Message = string.Format("Kullanilmamis bir Gcode unuz var - {0} saniye sonra tekrar deneyiniz.", diff.TotalSeconds);
                    return Ok(result);
                }
                var gcode = VendorGiftService.GenerateGcode(Repository, vendor.VendorId);
                SMSService.SendSms("0" + vendor.Phone, gcode.ToString());
                result.Message = "Gcode Kayıtlı cep telefonu numaranıza gönderildi.";
                result.Success = true;
            }
            catch (Exception e)
            {
                Log.Error("Error at request gcode: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("change_secret")]
        public ActionResult<ChangeSecretResultModel> ChangeVendorSecret(VendorSecretChange model)
        {
            var result = new ChangeSecretResultModel { Success = false };
            try
            {
                var ip = Request.HttpContext.Connection.RemoteIpAddress;

                var bannedIp = Repository
                    .GetBannedIps().Where(x => x.IP == ip.ToString()).FirstOrDefault();

                if (bannedIp != null)
                {
                    result.Message = "Yasaklı IP adresi.";
                    return Ok(result);
                }

                if (!VendorSecretChange.CheckValid(model))
                {
                    result.Message = "Invalid request.";
                    return Ok(result);
                }

                var gcode = Repository.GetGcodes()
                    .Where(x => x.Code == model.Gcode && !x.Used && DateTime.Now < x.ValidUntil)
                    .FirstOrDefault();

                if (gcode == null)
                {
                    result.Message = "Gcode hatali yada suresi gecmis.";
                    return Ok(result);
                }

                var vendor = Repository.GetVendors()
                    .Where(x => x.ApiKey == model.ApiKey)
                    .FirstOrDefault();

                if (vendor == null)
                {
                    result.Message = "api_key hatali";
                    return Ok(result);
                }

                if (gcode.GeneratedFor != vendor.VendorId)
                {
                    result.Message = "gcode vendor uyusmazligi";
                    return Ok(result);
                }

                if (vendor.Secret != model.OldSecret)
                {
                    result.Message = "old_secret vendor uyusmazligi";
                    return Ok(result);
                }

                if (VendorGiftService.ValidSecret(model.NewSecret))
                {
                    result.Message = "new_secret 256 bit alpha numeric string olmali";
                    return Ok(result);
                }

                gcode.Used = true;
                vendor.Secret = model.NewSecret;
                Repository.SaveChanges();
                result.Message = "Islem basarili";
                result.Success = true;
            }
            catch (Exception e)
            {
                Log.Error("error at change_secret: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu";
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("get_current_price_v1")]
        public ActionResult<VendorPriceCheckResult> GetPriceVendor(VendorPriceCheck model)
        {

            var result = new VendorPriceCheckResult { Success = false };
            try
            {
                var arda = 1;
                if (arda == 1)
                {
                    Log.Information("ARDA end cancel");
                    result.Message = "This endpoint is closed";
                    return Unauthorized(result);
                }
                Log.Information("vendor price check initiated");
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;

                if (!Authenticator.ValidateToken(token, out vendorid))
                {
                    result.Message = "Invalid token";
                    return Unauthorized(result);
                }

                if (!VendorPriceCheck.CheckValid(model))
                {
                    result.Message = "Invalid request";
                    return Ok(result);
                }

                var requestee = Repository
                    .GetVendors()
                    .Where(x => x.VendorId == Guid.Parse(vendorid))
                    .FirstOrDefault();

                var vendor = Repository
                    .GetVendors()
                    .Where(x => x.ApiKey == model.ApiKey)
                    .FirstOrDefault();

                if (requestee == null || vendor == null || requestee.VendorId != vendor.VendorId)
                {
                    result.Message = "Invalid vendor";
                    return Unauthorized(result);
                }

                
                FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();

                FxRate currentSilverRate = GlobalGoldPrice.GetSilverPrices();

                FxRate currentPlatinRate = GlobalGoldPrice.GetPlatinPrices();

                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();

                // buy gold

                decimal price = (currentRate.BuyRate * model.GramAmount.Value * taxAndExpenses.BankaMukaveleTax);
                price = Math.Truncate(100 * price) / 100;

                // sell gold                
                decimal salePrice = (currentRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax) * model.GramAmount.Value);
                salePrice = Math.Truncate(100 * salePrice) / 100;


                // buy silver
                decimal silverPrice = (currentSilverRate.BuyRate * model.GramAmount.Value * taxAndExpenses.BankaMukaveleTax);
                silverPrice = Math.Truncate(100 * silverPrice) / 100;

                // sell silver
                decimal silverSalePrice = (currentSilverRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax) * model.GramAmount.Value);
                silverSalePrice = Math.Truncate(100 * silverSalePrice) / 100;

            /*    // buy platin
                decimal platinBuyPrice = (currentPlatinRate.BuyRate * model.GramAmount.Value * taxAndExpenses.BankaMukaveleTax);
                platinBuyPrice = Math.Truncate(100 * platinBuyPrice) / 100;

                // sell platin
                decimal platinSalePrice = (currentPlatinRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax) * model.GramAmount.Value);
                platinSalePrice = Math.Truncate(100 * platinSalePrice) / 100;*/

                result.SellSilver = silverSalePrice;
                result.BuySilver = silverPrice;
                result.Buy = price;
                result.Sell = salePrice;
                /*result.SellPlatin = platinSalePrice;
                result.BuyPlatin = platinBuyPrice;*/
                result.Message = "Request ok";
                result.Success = true;
            }
            catch (Exception e)
            {
                Log.Error("Exception at vendor price check " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu";

            }
            return Ok(result);
        }


        [HttpPost]
        [Route("sell_silver_v1")]
        public ActionResult<VendorSellGoldResult> VendorSellSilver(VendorSellGoldParams model)
        {
            var result = new VendorSellGoldResult { Success = false };
            try
            {
                var arda = 1;
                if (arda == 1)
                {
                    Log.Information("ARDA end cancel");
                    result.Message = "This endpoint is closed";
                    return Unauthorized(result);
                }
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;

                if (!Authenticator.ValidateToken(token, out vendorid))
                {
                    result.Message = "Invalid token";
                    return Unauthorized(result);
                }

                if (!VendorSellGoldParams.CheckValid(model))
                {
                    result.Message = "Invalid request";
                    return Ok(result);
                }
                if (model.GramAmount < 5m)
                {
                    result.Message = "Amount can not be lower then 5grams per single transaction";
                    return Ok(result);
                }
                var requestee = Repository
                    .GetVendors()
                    .Where(x => x.VendorId == Guid.Parse(vendorid))
                    .FirstOrDefault();

                var vendor = Repository
                    .GetVendors()
                    .Where(x => x.ApiKey == model.ApiKey)
                    .FirstOrDefault();

                if (requestee == null || vendor == null || requestee.VendorId != vendor.VendorId)
                {
                    result.Message = "Invalid vendor";
                    return Unauthorized(result);
                }

                var previousTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.VendorReferenceId == model.Reference)
                    .Any();

                if (previousTransaction)
                {
                    result.Message = "This reference_id already in use.";
                    return Ok(result);
                }
                var currentRate = GlobalGoldPrice.GetSilverPrices();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();
;
                var transPrice = model.GramAmount.Value * currentRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax);
                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = "arc_sell_silver:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var vendorTransaction = new VendorTransaction(model.Reference,
                    vendor.VendorId,
                    Guid.Parse(FintagVendorId),
                    model.GramAmount.Value,
                    transPrice,
                    randString);
                Repository.AddVendorTransaction(vendorTransaction);
                Repository.SaveChanges();

                vendorTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.Comment == randString)
                    .FirstOrDefault();

                if (vendorTransaction == null)
                {
                    result.Message = "Yaratilan islem esnasinda database hatasi. tekrar deneyiniz";
                    return Ok(result);
                }

                result.TransactionId = vendorTransaction.TransactionId.ToString();
                result.Reference = model.Reference;
                result.Success = true;
                result.Price = transPrice;
                result.Message = "ok";


            }
            catch (Exception e)
            {
                Log.Error("error at sell vendor silver: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("sell_platin_v1")]
        public ActionResult<VendorSellGoldResult> VendorSellPlatin(VendorSellGoldParams model)
        {
            var result = new VendorSellGoldResult { Success = false };
            try
            {
                if (Utilities.Utility.APP_VERSION == "2.0.0")
                    return Ok("Not ready yet");
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;

                if (!Authenticator.ValidateToken(token, out vendorid))
                {
                    result.Message = "Invalid token";
                    return Unauthorized(result);
                }

                if (!VendorSellGoldParams.CheckValid(model))
                {
                    result.Message = "Invalid request";
                    return Ok(result);
                }
                if (model.GramAmount < 0.1m)
                {
                    result.Message = "Amount can not be lower then 0.1grams per single transaction";
                    return Ok(result);
                }
                var requestee = Repository
                    .GetVendors()
                    .Where(x => x.VendorId == Guid.Parse(vendorid))
                    .FirstOrDefault();

                var vendor = Repository
                    .GetVendors()
                    .Where(x => x.ApiKey == model.ApiKey)
                    .FirstOrDefault();

                if (requestee == null || vendor == null || requestee.VendorId != vendor.VendorId)
                {
                    result.Message = "Invalid vendor";
                    return Unauthorized(result);
                }

                var previousTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.VendorReferenceId == model.Reference)
                    .Any();

                if (previousTransaction)
                {
                    result.Message = "This reference_id already in use.";
                    return Ok(result);
                }
                var currentRate = GlobalGoldPrice.GetPlatinPrices();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();
                
                var transPrice = model.GramAmount.Value * currentRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax);
                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = "arc_sell_platin:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var vendorTransaction = new VendorTransaction(model.Reference,
                    vendor.VendorId,
                    Guid.Parse(FintagVendorId),
                    model.GramAmount.Value,
                    transPrice,
                    randString);
                Repository.AddVendorTransaction(vendorTransaction);
                Repository.SaveChanges();

                vendorTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.Comment == randString)
                    .FirstOrDefault();

                if (vendorTransaction == null)
                {
                    result.Message = "Yaratilan islem esnasinda database hatasi. tekrar deneyiniz";
                    return Ok(result);
                }

                result.TransactionId = vendorTransaction.TransactionId.ToString();
                result.Reference = model.Reference;
                result.Success = true;
                result.Price = transPrice;
                result.Message = "ok";


            }
            catch (Exception e)
            {
                Log.Error("error at sell vendor platin: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("buy_silver_v1")]
        public ActionResult<VendorBuyGoldResult> VendorBuySilver(VendorBuyGoldParams model)
        {
            var result = new VendorBuyGoldResult { Success = false };
            try
            {
                var arda = 1;
                if (arda == 1)
                {
                    Log.Information("ARDA end cancel");
                    result.Message = "This endpoint is closed";
                    return Unauthorized(result);
                }
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;

                if (!Authenticator.ValidateToken(token, out vendorid))
                {
                    result.Message = "Invalid token";
                    return Unauthorized(result);
                }

                if (!VendorBuyGoldParams.CheckValid(model))
                {
                    result.Message = "Invalid request";
                    return Ok(result);
                }

                if (model.GramAmount.Value > 2500m)
                {
                    result.Message = "Amount can not exeed 2500grams per single transaction";
                    return Ok(result);
                }

                if (model.GramAmount < 5m)
                {
                    result.Message = "Amount can not be lower then 5grams per single transaction";
                    return Ok(result);
                }
                var requestee = Repository
                    .GetVendors()
                    .Where(x => x.VendorId == Guid.Parse(vendorid))
                    .FirstOrDefault();

                var vendor = Repository
                    .GetVendors()
                    .Where(x => x.ApiKey == model.ApiKey)
                    .FirstOrDefault();

                if (requestee == null || vendor == null || requestee.VendorId != vendor.VendorId)
                {
                    result.Message = "Invalid vendor";
                    return Unauthorized(result);
                }

                var previousTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.VendorReferenceId == model.Reference)
                    .Any();

                if (previousTransaction)
                {
                    result.Message = "This reference_id already in use.";
                    return Ok(result);
                }

                var currentRate = GlobalGoldPrice.GetSilverPrices();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();
               

                var transPrice = (model.GramAmount.Value * currentRate.BuyRate * taxAndExpenses.BankaMukaveleTax);

                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = "arc_buy_silver:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var vendorTransaction = new VendorTransaction(
                    model.Reference,
                    Guid.Parse(FintagVendorId),
                    vendor.VendorId,
                    model.GramAmount.Value,
                    transPrice,
                    randString);
                Repository.AddVendorTransaction(vendorTransaction);
                Repository.SaveChanges();

                vendorTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.Comment == randString)
                    .FirstOrDefault();
                if (vendorTransaction == null)
                {
                    result.Message = "Yaratilan islem esnasinda database hatasi. tekrar deneyiniz";
                    return Ok(result);
                }

                result.TransactionId = vendorTransaction.TransactionId.ToString();
                result.Reference = model.Reference;
                result.Price = transPrice;
                result.Success = true;
                result.Message = "ok";


            }
            catch (Exception e)
            {
                Log.Error("error at buy vendor silver: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("buy_platin_v1")]
        public ActionResult<VendorBuyGoldResult> VendorBuyPlatin(VendorBuyGoldParams model)
        {
            var result = new VendorBuyGoldResult { Success = false };
            try
            {
                
                if (Utilities.Utility.APP_VERSION == "2.0.0")
                    return Ok("Not ready yet");
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;

                if (!Authenticator.ValidateToken(token, out vendorid))
                {
                    result.Message = "Invalid token";
                    return Unauthorized(result);
                }

                if (!VendorBuyGoldParams.CheckValid(model))
                {
                    result.Message = "Invalid request";
                    return Ok(result);
                }

                if (model.GramAmount.Value > 40m)
                {
                    result.Message = "Amount can not exeed 40grams per single transaction";
                    return Ok(result);
                }

                if (model.GramAmount < 0.1m)
                {
                    result.Message = "Amount can not be lower then 0.1grams per single transaction";
                    return Ok(result);
                }
                var requestee = Repository
                    .GetVendors()
                    .Where(x => x.VendorId == Guid.Parse(vendorid))
                    .FirstOrDefault();

                var vendor = Repository
                    .GetVendors()
                    .Where(x => x.ApiKey == model.ApiKey)
                    .FirstOrDefault();

                if (requestee == null || vendor == null || requestee.VendorId != vendor.VendorId)
                {
                    result.Message = "Invalid vendor";
                    return Unauthorized(result);
                }

                var previousTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.VendorReferenceId == model.Reference)
                    .Any();

                if (previousTransaction)
                {
                    result.Message = "This reference_id already in use.";
                    return Ok(result);
                }

                var currentRate = GlobalGoldPrice.GetPlatinPrices();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();


                var transPrice = (model.GramAmount.Value * currentRate.BuyRate * taxAndExpenses.BankaMukaveleTax);

                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = "arc_buy_platin:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var vendorTransaction = new VendorTransaction(
                    model.Reference,
                    Guid.Parse(FintagVendorId),
                    vendor.VendorId,
                    model.GramAmount.Value,
                    transPrice,
                    randString);
                Repository.AddVendorTransaction(vendorTransaction);
                Repository.SaveChanges();

                vendorTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.Comment == randString)
                    .FirstOrDefault();
                if (vendorTransaction == null)
                {
                    result.Message = "Yaratilan islem esnasinda database hatasi. tekrar deneyiniz";
                    return Ok(result);
                }

                result.TransactionId = vendorTransaction.TransactionId.ToString();
                result.Reference = model.Reference;
                result.Price = transPrice;
                result.Success = true;
                result.Message = "ok";


            }
            catch (Exception e)
            {
                Log.Error("error at buy vendor platin: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("buy_gold_v1")]
        public ActionResult<VendorBuyGoldResult> VendorBuyGold(VendorBuyGoldParams model)
        {
            var result = new VendorBuyGoldResult { Success = false };
            try
            {
                var arda = 1;
                if (arda == 1)
                {
                    Log.Information("ARDA end cancel");
                    result.Message = "This endpoint is closed";
                    return Unauthorized(result);
                }
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;

                if (!Authenticator.ValidateToken(token, out vendorid))
                {
                    result.Message = "Invalid token";
                    return Unauthorized(result);
                }

                if (!VendorBuyGoldParams.CheckValid(model))
                {
                    result.Message = "Invalid request";
                    return Ok(result);
                }

                if (model.GramAmount.Value > 40m)
                {
                    result.Message = "Amount can not exeed 40grams per single transaction";
                    return Ok(result);
                }

                if (model.GramAmount < 0.1m)
                {
                    result.Message = "Amount can not be lower then 0.1grams per single transaction";
                    return Ok(result);
                }
                var requestee = Repository
                    .GetVendors()
                    .Where(x => x.VendorId == Guid.Parse(vendorid))
                    .FirstOrDefault();

                var vendor = Repository
                    .GetVendors()
                    .Where(x => x.ApiKey == model.ApiKey)
                    .FirstOrDefault();

                if (requestee == null || vendor == null || requestee.VendorId != vendor.VendorId)
                {
                    result.Message = "Invalid vendor";
                    return Unauthorized(result);
                }

                var previousTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.VendorReferenceId == model.Reference)
                    .Any();

                if (previousTransaction)
                {
                    result.Message = "This reference_id already in use.";
                    return Ok(result);
                }

                var currentRate = GlobalGoldPrice.GetCurrentPrice();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();

                var transPrice = (model.GramAmount.Value * currentRate.BuyRate * taxAndExpenses.BankaMukaveleTax);

                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = ":arc_buy:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var vendorTransaction = new VendorTransaction(model.Reference,
                    Guid.Parse(FintagVendorId),
                    vendor.VendorId,
                    model.GramAmount.Value,
                    transPrice,
                    randString);
                Repository.AddVendorTransaction(vendorTransaction);
                Repository.SaveChanges();

                vendorTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.Comment == randString)
                    .FirstOrDefault();
                if (vendorTransaction == null)
                {
                    result.Message = "Yaratilan islem esnasinda database hatasi. tekrar deneyiniz";
                    return Ok(result);
                }

                result.TransactionId = vendorTransaction.TransactionId.ToString();
                result.Reference = model.Reference;
                result.Price = transPrice;
                result.Success = true;
                result.Message = "ok";


            }
            catch (Exception e)
            {
                Log.Error("error at buy vendor gold: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("sell_gold_v1")]
        public ActionResult<VendorSellGoldResult> VendorSellGold(VendorSellGoldParams model)
        {
            var result = new VendorSellGoldResult { Success = false };
            try
            {
                var arda = 1;
                if (arda == 1)
                {
                    Log.Information("ARDA end cancel");
                    result.Message = "This endpoint is closed";
                    return Unauthorized(result);
                }
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;

                if (!Authenticator.ValidateToken(token, out vendorid))
                {
                    result.Message = "Invalid token";
                    return Unauthorized(result);
                }

                if (!VendorSellGoldParams.CheckValid(model))
                {
                    result.Message = "Invalid request";
                    return Ok(result);
                }
                if (model.GramAmount < 0.1m)
                {
                    result.Message = "Amount can not be lower then 0.1grams per single transaction";
                    return Ok(result);
                }
                var requestee = Repository
                    .GetVendors()
                    .Where(x => x.VendorId == Guid.Parse(vendorid))
                    .FirstOrDefault();

                var vendor = Repository
                    .GetVendors()
                    .Where(x => x.ApiKey == model.ApiKey)
                    .FirstOrDefault();

                if (requestee == null || vendor == null || requestee.VendorId != vendor.VendorId)
                {
                    result.Message = "Invalid vendor";
                    return Unauthorized(result);
                }

                var previousTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.VendorReferenceId == model.Reference)
                    .Any();

                if (previousTransaction)
                {
                    result.Message = "This reference_id already in use.";
                    return Ok(result);
                }
                var currentRate = GlobalGoldPrice.GetCurrentPrice();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();
    

                var transPrice = model.GramAmount.Value * currentRate.SellRate * (2-taxAndExpenses.BankaMukaveleTax);
                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = ":arc_sell:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var vendorTransaction = new VendorTransaction(model.Reference,
                    vendor.VendorId,
                    Guid.Parse(FintagVendorId),
                    model.GramAmount.Value,
                    transPrice,
                    randString);
                Repository.AddVendorTransaction(vendorTransaction);
                Repository.SaveChanges();

                vendorTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.Comment == randString)
                    .FirstOrDefault();

                if (vendorTransaction == null)
                {
                    result.Message = "Yaratilan islem esnasinda database hatasi. tekrar deneyiniz";
                    return Ok(result);
                }

                result.TransactionId = vendorTransaction.TransactionId.ToString();
                result.Reference = model.Reference;
                result.Success = true;
                result.Price = transPrice;
                result.Message = "ok";


            }
            catch (Exception e)
            {
                Log.Error("error at sell vendor gold: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("finalise_transaction_v1")]
        public ActionResult<FinaliseTransactionResult> VendorComplete(FinaliseTransactionParams model)
        {
            var result = new FinaliseTransactionResult { ResultCode = "0" };
            try
            {
                var arda = 1;
                if (arda == 1)
                {
                    Log.Information("ARDA finalise cancel");
                    result.ResultCode = "228";
                    result.Message = "This endpoint is closed";
                    return Unauthorized(result);
                }
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;

                if (!Authenticator.ValidateToken(token, out vendorid))
                {
                    result.Message = "Invalid token";
                    result.ResultCode = "1";
                    return Unauthorized(result);
                }
                /*
                if (!FinaliseTransactionParams.CheckValid(model))
                {
                    result.Message = "Invalid request";
                    result.ResultCode = "2";
                    return Ok(result);
                }*/
                var requestee = Repository
                    .GetVendors()
                    .Where(x => x.VendorId == Guid.Parse(vendorid))
                    .FirstOrDefault();

                var vendor = Repository
                    .GetVendors()
                    .Where(x => x.ApiKey == model.ApiKey)
                    .FirstOrDefault();

                


                if (requestee == null || vendor == null || requestee.VendorId != vendor.VendorId)
                {
                    result.Message = "Invalid vendor";
                    result.ResultCode = "3";
                    return Unauthorized(result);
                }

                var vendorData = Repository
                    .GetVendorDatas()
                    .Where(x => x.RelatedId == vendor.VendorId.ToString())
                    .FirstOrDefault();

                var vendorTransaction = Repository.GetVendorTransaction(Guid.Parse(model.TransactionId));
                var previousTransaction = Repository
                    .GetVendorTransactions()
                    .Where(x => x.VendorReferenceId == model.Reference)
                    .FirstOrDefault();

                if (vendorTransaction == null || previousTransaction == null || vendorTransaction.TransactionId != previousTransaction.TransactionId)
                {
                    result.Message = "transaction_id yada vendor_reference hatali, eksik yada vendor_reference uyusmuyor.";
                    result.ResultCode = "4";
                    return Ok(result);
                }

                if (vendorTransaction.Succesful)
                {
                    result.Message = "Daha onceden tamamlanmis transaction.";
                    result.ResultCode = "5";
                    return Ok(result);
                }

                if (vendorTransaction.Cancelled)
                {
                    result.ResultCode = "6";
                    result.Message = "Daha onceden cancel edilmis transaction.";
                    return Ok(result);
                }

                if (!model.Confirmed.Value)
                {
                    vendorTransaction.CancelTransaction("Not confirmed by vendor");
                    Repository.SaveChanges();
                    result.Message = "Vendor islemi onaylamadi.";
                    result.ResultCode = "7";
                }
                else
                {
                    var diff = DateTime.Now - vendorTransaction.TransactionDateTime;
                    if (diff.TotalSeconds > 50)
                    {
                        vendorTransaction.CancelTransaction("Not confirmed by Goldtag: Zaman Asimi");
                        Repository.SaveChanges();
                        result.Message = "Islem zaman asimi.";
                        result.ResultCode = "8";
                    } 
                    else
                    {
                        vendorTransaction.VendorConfirmed();

                        string reason;
                        if (vendorTransaction.Source == Guid.Parse(FintagVendorId)) 
                        {
                            // vendor buys gold or silver from Fintag

                            vendorTransaction.GTagConfirmed();
                            
                            if (vendorTransaction.Comment.StartsWith("arc_buy_silver"))
                            {
                                vendor.SilverBalance = 
                                    (vendor.SilverBalance.HasValue) ? vendor.SilverBalance.Value + vendorTransaction.GramAmount : vendorTransaction.GramAmount;
                                reason = "VENDORA SILVER VERILDI";
                                /*
                                var rate = KTApiService.GetCurrentPriceFromKtApi()
                                    .value.FxRates.Where(x => x.FxId == 26).FirstOrDefault().BuyRate;

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
                            else if (vendorTransaction.Comment.StartsWith("arc_buy_platin"))
                            {

                                var platinBalance = Repository.GetVendorPlatinBalance(vendor.VendorId);
                                if (platinBalance == null)
                                {
                                    platinBalance = new VendorPlatinBalance(vendor.VendorId);
                                }
                                platinBalance.Balance += vendorTransaction.GramAmount;
                                reason = "VENDORA PLATIN VERILDI";
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
                                vendor.Balance += vendorTransaction.GramAmount;
                                reason = "VENDORA ALTIN VERILDI";
                                /*
                                var rate = KTApiService.GetCurrentPriceFromKtApi()
                                    .value.FxRates.Where(x => x.FxId == 24).FirstOrDefault().BuyRate;
                                // expected transaction

                                var expected = new ExpectedCash(
                                       vendorTransaction.Destination,
                                       vendorTransaction.TransactionId,
                                       vendorTransaction.VendorReferenceId,
                                       vendorData.TLSuffix,
                                       24,
                                       vendorTransaction.TlAmount,
                                       vendorTransaction.GramAmount,
                                       rate,
                                       rate,
                                       rate
                                    );
                                Repository.AddExpectedCash(expected);*/
                            }
                            // commented since robot should complete it now
                            vendorTransaction.CompleteTransaction(reason);

                            /*Repository.SaveChanges();
                            result.Message = "islem basarili";
                            result.ResultCode = "0";*/
                        }
                        else 
                        {

                            bool silverTransaction = (vendorTransaction.Comment.StartsWith("arc_sell_silver"));

                            // vendor sells gold or silver to Fintag 
                            if ((!silverTransaction && vendor.Balance < vendorTransaction.GramAmount) ||
                                (silverTransaction && !vendor.SilverBalance.HasValue) ||
                                (silverTransaction && vendor.SilverBalance.Value < vendorTransaction.GramAmount))
                            {
                                vendorTransaction.CancelTransaction("vendor not enough gold or silver balance");
                                Repository.SaveChanges();

                                result.Message = "Yetersiz bakiye.";
                                result.ResultCode = "9";
                            }
                            else
                            {
                                vendorTransaction.GTagConfirmed();
                                string mesg;

                                if (silverTransaction)
                                {
                                    vendor.SilverBalance = vendor.SilverBalance.Value - vendorTransaction.GramAmount;
                                    mesg = "VENDORDAN SILVER ALINDI";
                                    /* var fxResult = KTApiService.GetCurrentPriceFromKtApi();
                                     var rate = fxResult.value.FxRates.Where(x => x.FxId == 26).FirstOrDefault();

                                     var data = Repository.GetVendorDatas()
                                         .Where(x => x.RelatedId == vendorTransaction.Source.ToString())
                                         .FirstOrDefault();

                                     var suffTo = data.TLSuffix;
                                     var sffFrom = data
                                         .SLVSuffix;

                                     var acc = data.AccountNumber;
                                     var suff = data.AccountSuffix;

                                     var param = new PreciousMetalsSellParams
                                     {
                                         Amount = vendorTransaction.GramAmount,
                                         SuffixFrom = sffFrom,
                                         SuffixTo = suffTo,
                                         UserName = KuwaitAccessHandler.FINTAG,
                                         SellRate = rate.SellRate
                                     };
                                     var attempts = 0;
                                     bool success = false;
                                     string refid = "";
                                     while (attempts < 5 && success == false)
                                     {
                                         var metalSell = KTApiService.PreciousMetalSell(param);
                                         success = metalSell.Success;
                                         refid = metalSell.RefId;
                                         attempts++;
                                     }


                                     if (success == true)
                                     {
                                         var iparams = new InterBankTransferParams
                                         {
                                             Amount = vendorTransaction.TlAmount,
                                             ReceiverAccount = acc,
                                             ReceiverSuffix = suff,
                                             Description = string.Format("Goldtag den Gumus bozdurma {0}TRY - {1}gr", vendorTransaction.TlAmount, vendorTransaction.GramAmount),
                                             SenderSuffix = suffTo,
                                             TransferType = 3
                                         };


                                         refid = "";
                                         success = false;
                                         attempts = 0;
                                         while (attempts < 5 && success == false)
                                         {
                                             var interTransfer = KTApiService.InterBankTransfer(iparams);
                                             success = interTransfer.Success;
                                             refid = interTransfer.RefId;
                                             attempts++;

                                         }

                                         if (success)
                                         {
                                             var tl = vendorTransaction.TlAmount * (-1);
                                             var gram = vendorTransaction.GramAmount;
                                             var finalized = new FinalizedGold(vendor.VendorId,
                                                 vendorTransaction.TransactionId,
                                                 vendorTransaction.VendorReferenceId,
                                                 vendorTransaction.TlAmount,
                                                 vendorTransaction.GramAmount,
                                                 rate.SellRate, rate.SellRate, rate.SellRate, refid, mesg, tl, gram);
                                             Repository.AddFinalizedGold(finalized);
                                         }
                                         else
                                         {
                                             var ibankError = new InterBankError(vendorTransaction.TransactionId,
                                                 vendor.VendorId,
                                                 vendorTransaction.TlAmount,
                                                 acc,
                                                 suff,
                                                 3,
                                                 refid,
                                                 "failed interbank transfer after successful precious metal sell");
                                             Repository.AddIBankError(ibankError);
                                         }
                                     }
                                     else
                                     {
                                         // bozfdurma basarisiz
                                         var notPosSell = new NotPosGoldSell(refid, vendorTransaction.GramAmount,
                                             sffFrom, suffTo, rate.SellRate, vendorTransaction.TransactionId, vendor.VendorId,
                                             "precious metal sell fail");

                                         Repository.AddNotPosGoldSell(notPosSell);

                                     }*/

                                    
                                }
                                else if (vendorTransaction.Comment.StartsWith("arc_sell_platin"))
                                {
                                    var platinBalance = Repository.GetVendorPlatinBalance(vendor.VendorId);
                                    if (platinBalance == null || platinBalance.Balance < vendorTransaction.GramAmount)
                                    {
                                        result.Message = "Hatali islem";
                                        result.ResultCode = "9";
                                        return Ok(result);
                                    }
                                    platinBalance.Balance = platinBalance.Balance - vendorTransaction.GramAmount;
                                    mesg = "VENDORDAN PLATIN ALINDI";
                                    /* var fxResult = KTApiService.GetCurrentPriceFromKtApi();
                                     var rate = fxResult.value.FxRates.Where(x => x.FxId == 26).FirstOrDefault();

                                     var data = Repository.GetVendorDatas()
                                         .Where(x => x.RelatedId == vendorTransaction.Source.ToString())
                                         .FirstOrDefault();

                                     var suffTo = data.TLSuffix;
                                     var sffFrom = data
                                         .SLVSuffix;

                                     var acc = data.AccountNumber;
                                     var suff = data.AccountSuffix;

                                     var param = new PreciousMetalsSellParams
                                     {
                                         Amount = vendorTransaction.GramAmount,
                                         SuffixFrom = sffFrom,
                                         SuffixTo = suffTo,
                                         UserName = KuwaitAccessHandler.FINTAG,
                                         SellRate = rate.SellRate
                                     };
                                     var attempts = 0;
                                     bool success = false;
                                     string refid = "";
                                     while (attempts < 5 && success == false)
                                     {
                                         var metalSell = KTApiService.PreciousMetalSell(param);
                                         success = metalSell.Success;
                                         refid = metalSell.RefId;
                                         attempts++;
                                     }


                                     if (success == true)
                                     {
                                         var iparams = new InterBankTransferParams
                                         {
                                             Amount = vendorTransaction.TlAmount,
                                             ReceiverAccount = acc,
                                             ReceiverSuffix = suff,
                                             Description = string.Format("Goldtag den Gumus bozdurma {0}TRY - {1}gr", vendorTransaction.TlAmount, vendorTransaction.GramAmount),
                                             SenderSuffix = suffTo,
                                             TransferType = 3
                                         };


                                         refid = "";
                                         success = false;
                                         attempts = 0;
                                         while (attempts < 5 && success == false)
                                         {
                                             var interTransfer = KTApiService.InterBankTransfer(iparams);
                                             success = interTransfer.Success;
                                             refid = interTransfer.RefId;
                                             attempts++;

                                         }

                                         if (success)
                                         {
                                             var tl = vendorTransaction.TlAmount * (-1);
                                             var gram = vendorTransaction.GramAmount;
                                             var finalized = new FinalizedGold(vendor.VendorId,
                                                 vendorTransaction.TransactionId,
                                                 vendorTransaction.VendorReferenceId,
                                                 vendorTransaction.TlAmount,
                                                 vendorTransaction.GramAmount,
                                                 rate.SellRate, rate.SellRate, rate.SellRate, refid, mesg, tl, gram);
                                             Repository.AddFinalizedGold(finalized);
                                         }
                                         else
                                         {
                                             var ibankError = new InterBankError(vendorTransaction.TransactionId,
                                                 vendor.VendorId,
                                                 vendorTransaction.TlAmount,
                                                 acc,
                                                 suff,
                                                 3,
                                                 refid,
                                                 "failed interbank transfer after successful precious metal sell");
                                             Repository.AddIBankError(ibankError);
                                         }
                                     }
                                     else
                                     {
                                         // bozfdurma basarisiz
                                         var notPosSell = new NotPosGoldSell(refid, vendorTransaction.GramAmount,
                                             sffFrom, suffTo, rate.SellRate, vendorTransaction.TransactionId, vendor.VendorId,
                                             "precious metal sell fail");

                                         Repository.AddNotPosGoldSell(notPosSell);

                                     }*/

                                    vendorTransaction.CompleteTransaction(mesg);
                                    Repository.SaveChanges();
                                    result.Message = "islem basarili";
                                    result.ResultCode = "0";
                                }
                                else
                                {
                                    vendor.Balance -= vendorTransaction.GramAmount;
                                    mesg = "VENDORDAN ALTIN ALINDI";
                                    /*
                                    // KT API
                                    var fxResult = KTApiService.GetCurrentPriceFromKtApi();
                                    var rate = fxResult.value.FxRates.Where(x => x.FxId == 24).FirstOrDefault();

                                    var data = Repository.GetVendorDatas()
                                        .Where(x => x.RelatedId == vendorTransaction.Source.ToString())
                                        .FirstOrDefault();

                                    var suffTo = data.TLSuffix;
                                    var sffFrom = data
                                        .GOLDSuffix;

                                    var acc = data.AccountNumber;
                                    var suff = data.AccountSuffix;

                                    var param = new PreciousMetalsSellParams
                                    {
                                        Amount = vendorTransaction.GramAmount,
                                        SuffixFrom = sffFrom,
                                        SuffixTo = suffTo,
                                        UserName = KuwaitAccessHandler.FINTAG,
                                        SellRate = rate.SellRate
                                    };
                                    var attempts = 0;
                                    bool success = false;
                                    string refid = "";
                                    while (attempts < 5 && success == false)
                                    {
                                        var metalSell = KTApiService.PreciousMetalSell(param);
                                        success = metalSell.Success;
                                        refid = metalSell.RefId;
                                        attempts++;
                                    }

                                    
                                    //Log.Information(metalSell);

                                    if (success == true)
                                    {
                                        var iparams = new InterBankTransferParams
                                        {
                                            Amount = vendorTransaction.TlAmount,
                                            ReceiverAccount = acc,
                                            ReceiverSuffix = suff,
                                            Description = string.Format("Goldtag den Altin bozdurma {0}TRY - {1}gr", vendorTransaction.TlAmount, vendorTransaction.GramAmount),
                                            SenderSuffix = suffTo,
                                            TransferType = 3
                                        };


                                        refid = "";
                                        success = false;
                                        attempts = 0;
                                        while (attempts < 5 && success == false)
                                        {
                                            var interTransfer = KTApiService.InterBankTransfer(iparams);
                                            success = interTransfer.Success;
                                            refid = interTransfer.RefId;
                                            attempts++;

                                        }

                                        if (success)
                                        {
                                            var tl = vendorTransaction.TlAmount * (-1);
                                            var gram = vendorTransaction.GramAmount;
                                            var finalized = new FinalizedGold(vendor.VendorId,
                                                vendorTransaction.TransactionId,
                                                vendorTransaction.VendorReferenceId,
                                                vendorTransaction.TlAmount,
                                                vendorTransaction.GramAmount,
                                                rate.SellRate, rate.SellRate, rate.SellRate, refid, mesg, tl, gram);
                                            Repository.AddFinalizedGold(finalized);
                                        }
                                        else
                                        {
                                            var ibankError = new InterBankError(vendorTransaction.TransactionId,
                                                vendor.VendorId,
                                                vendorTransaction.TlAmount,
                                                acc,
                                                suff,
                                                3,
                                                refid,
                                                "failed interbank transfer after successful precious metal sell");
                                            Repository.AddIBankError(ibankError);
                                        }
                                    }
                                    else
                                    {
                                        // bozfdurma basarisiz
                                        var notPosSell = new NotPosGoldSell(refid, vendorTransaction.GramAmount,
                                            sffFrom, suffTo, rate.SellRate, vendorTransaction.TransactionId, vendor.VendorId,
                                            "precious metal sell fail");

                                        Repository.AddNotPosGoldSell(notPosSell);

                                    }*/


                                }

                                vendorTransaction.CompleteTransaction(mesg);
                            }
                        }
                        
                        Repository.SaveChanges();
                        result.Message = "islem basarili";
                        result.ResultCode = "0";
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("error at finalise trans gold: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bilinmeyen bit hata olustu. Lutfen destek icin bizi arayiniz.";
                result.ResultCode = "10";
            }

            return Ok(result);
        }
    
        /// <summary>
        /// CLOSED
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("transactions_count")]
        public ActionResult<VendorRequestTransactionsCountResult> TransactionsCount(VendorRequestTransactionsCountParams model) 
        {
            var result = new VendorRequestTransactionsCountResult { Success = false, Count = 0 };
            var closed = 1;
            if (closed == 1)
            {
                result.Message = "Endpoint unavailable";
                return Ok(result);
            }
            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;

                if (!Authenticator.ValidateToken(token, out vendorid))
                {
                    result.Message = "Invalid token";
                    return Unauthorized(result);
                }

                if (!VendorRequestTransactionsCountParams.CheckValid(model))
                {
                    result.Message = "Invalid request";
                    return Ok(result);
                }
                var requestee = Repository
                    .GetVendors()
                    .Where(x => x.VendorId == Guid.Parse(vendorid))
                    .FirstOrDefault();

                var vendor = Repository
                    .GetVendors()
                    .Where(x => x.ApiKey == model.ApiKey)
                    .FirstOrDefault();

                if (requestee == null || vendor == null || requestee.VendorId != vendor.VendorId)
                {
                    result.Message = "Invalid vendor";
                    return Unauthorized(result);
                }

                var dateFrom = DateTime.ParseExact(model.DateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var dateTo = DateTime.ParseExact(model.DateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                if (dateTo < dateFrom)
                {
                    result.Message = "Invalid dates";
                    return Ok(result);
                }

                var transactions = Repository.GetVendorTransactions()
                    .Where(x => (x.Source == vendor.VendorId || x.Destination == vendor.VendorId)
                    && x.TransactionDateTime.Date >= dateFrom.Date && x.TransactionDateTime.Date <= dateTo
                    && x.TransactionFinalisedDateTime.HasValue == model.OnlyFinalised.Value)
                    ;



                var count = 0;

                foreach(var transaction in transactions)
                {
                    if (model.TransactionType != null && model.TransactionType == "gold" &&
                        (transaction.Comment.StartsWith("arc_buy_silver") || transaction.Comment.StartsWith("arc_sell_silver")))
                        continue;
                    if (model.TransactionType != null && model.TransactionType == "silver" &&
                        !(transaction.Comment.StartsWith("arc_buy_silver") || transaction.Comment.StartsWith("arc_sell_silver")))
                        continue;

                    count++;
                }

                result.Message = "ok";
                result.Success = true;
                result.Count = count;
                

            }
            catch (Exception e)
            {
                Log.Error("error at buy vendor gold: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }


            return Ok(result);
        }


        private VendorTransactionModel ParseVendorTransactionNewAsModel(VendorTransactionNew transaction)
        {

            var source = Repository.GetVendors().Where(x => x.VendorId == transaction.Source).FirstOrDefault();
            var destination = Repository.GetVendors().Where(x => x.VendorId == transaction.Destination).FirstOrDefault();

            var type = (transaction.Comment.StartsWith("arc_buy_silver:") ||
                transaction.Comment.StartsWith("arc_sell_silver")) ? "silver" : "gold";
            var model = new VendorTransactionModel
            {
                TransactionType = type,
                TransactionId = transaction.TransactionId.ToString(),
                Reference = transaction.VendorReferenceId,
                Source = (source != null) ? source.Name : "Error",
                Destination = (destination != null) ? destination.Name : "Error",
                ConfirmedByVendor = transaction.ConfirmedByVendor,
                ConfirmedByGoldtag = transaction.ConfirmedByGoldtag,
                Cancelled = transaction.Cancelled,
                Finalised = transaction.Succesful,
                GramAmount = transaction.GramAmount,
                TlAmount = transaction.TlAmount,
                TransactionDateTime = transaction.TransactionDateTime.ToString(),
                VendorConfirmedDateTime = (transaction.VendorConfirmedDateTime.HasValue) ? transaction.VendorConfirmedDateTime.Value.ToString() : null,
                GoldtagConfirmedDateTime = (transaction.GoldtagConfirmedDateTime.HasValue) ? transaction.GoldtagConfirmedDateTime.Value.ToString() : null,
                TransactionFinalisedDateTime = (transaction.TransactionFinalisedDateTime.HasValue) ? transaction.TransactionFinalisedDateTime.Value.ToString() : null
            };


            return model;
        }

        private VendorTransactionModel ParseVendorTransactionAsModel(VendorTransaction transaction)
        {

            var source = Repository.GetVendors().Where(x => x.VendorId == transaction.Source).FirstOrDefault();
            var destination = Repository.GetVendors().Where(x => x.VendorId == transaction.Destination).FirstOrDefault();

            var type = (transaction.Comment.StartsWith("arc_buy_silver:") || 
                transaction.Comment.StartsWith("arc_sell_silver")) ? "silver" : "gold";
            var model = new VendorTransactionModel
            {
                TransactionType = type,
                TransactionId = transaction.TransactionId.ToString(),
                Reference = transaction.VendorReferenceId,
                Source = (source != null) ? source.Name : "Error",
                Destination = (destination != null) ? destination.Name : "Error",
                ConfirmedByVendor = transaction.ConfirmedByVendor,
                ConfirmedByGoldtag = transaction.ConfirmedByGoldtag,
                Cancelled = transaction.Cancelled,
                Finalised = transaction.Succesful,
                GramAmount = transaction.GramAmount,
                TlAmount = transaction.TlAmount,
                TransactionDateTime = transaction.TransactionDateTime.ToString(),
                VendorConfirmedDateTime = (transaction.VendorConfirmedDateTime.HasValue) ? transaction.VendorConfirmedDateTime.Value.ToString() : null,
                GoldtagConfirmedDateTime = (transaction.GoldtagConfirmedDateTime.HasValue) ? transaction.GoldtagConfirmedDateTime.Value.ToString() : null,
                TransactionFinalisedDateTime = (transaction.TransactionFinalisedDateTime.HasValue) ? transaction.TransactionFinalisedDateTime.Value.ToString() : null
            };
            

            return model;
        }

        /// <summary>
        /// CLOSED
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("transactions")]
        public ActionResult<VendorRequestTransactionsResult> GetVendorTransactions(VendorRequestTransactionsParams model)
        {
            var result = new VendorRequestTransactionsResult { Success = false, TransactionModels = new List<VendorTransactionModel>() };

            var closed = 1;
            if (closed == 1)
            {
                result.Message = "Endpoint unavailable";
                return Ok(result);
            }
            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;

                if (!Authenticator.ValidateToken(token, out vendorid))
                {
                    result.Message = "Invalid token";
                    return Unauthorized(result);
                }

                if (!VendorRequestTransactionsParams.CheckValid(model))
                {
                    result.Message = "Invalid request";
                    return Ok(result);
                }
                /*var requestee = Repository
                    .GetVendors()
                    .Where(x => x.VendorId == Guid.Parse(vendorid))
                    .FirstOrDefault();*/

                var vendorTask =  Repository.GetVendorAsync(Guid.Parse(vendorid), model.ApiKey);

                vendorTask.Wait();

                var vendor = vendorTask.Result;

                if (vendor == null)// || requestee.VendorId != vendor.VendorId)
                {
                    result.Message = "Invalid vendor";
                    return Unauthorized(result);
                }

                var dateFrom = DateTime.ParseExact(model.DateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var dateTo = DateTime.ParseExact(model.DateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                if (dateTo < dateFrom)
                {
                    result.Message = "Invalid dates";
                    return Ok(result);
                }


                //List<VendorTransaction> transactions;

               // if (model.TransactionType == "gold")
               // {
               var transactions = Repository.GetVendorTransactions()
                    .Where(x => (x.Source == vendor.VendorId || x.Destination == vendor.VendorId)
                    && x.TransactionDateTime.Date >= dateFrom.Date && x.TransactionDateTime.Date <= dateTo
                    && x.TransactionFinalisedDateTime.HasValue == model.OnlyFinalised.Value)
                    .OrderByDescending(x => x.TransactionDateTime)
                    .Skip(model.Limit.Value * (model.Page.Value - 1))
                    .Take(model.Limit.Value)
                    .ToList();

                var newTrans = Repository.GetVendorTransactionsNew()
                    .Where(x => (x.Source == vendor.VendorId || x.Destination == vendor.VendorId)
                    && x.TransactionDateTime.Date >= dateFrom.Date && x.TransactionDateTime.Date <= dateTo
                    && x.TransactionFinalisedDateTime.HasValue == model.OnlyFinalised.Value)
                    .OrderByDescending(x => x.TransactionDateTime)
                    .Skip(model.Limit.Value * (model.Page.Value - 1))
                    .Take(model.Limit.Value)
                    .ToList();
                // }



                var transactionModels = new List<VendorTransactionModel>();

                foreach (var transaction in transactions)
                {       
                    if (model.TransactionType != null && model.TransactionType == "gold" && 
                        (transaction.Comment.StartsWith("arc_buy_silver") || transaction.Comment.StartsWith("arc_sell_silver")))
                        continue;
                    if (model.TransactionType != null && model.TransactionType == "silver" &&
                        !(transaction.Comment.StartsWith("arc_buy_silver") || transaction.Comment.StartsWith("arc_sell_silver")))
                        continue;

                    transactionModels.Add(ParseVendorTransactionAsModel(transaction));
                }

                foreach (var transaction in newTrans)
                {
                    if (model.TransactionType != null && model.TransactionType == "gold" &&
                        (transaction.Comment.StartsWith("arc_buy_silver") || transaction.Comment.StartsWith("arc_sell_silver")))
                        continue;
                    if (model.TransactionType != null && model.TransactionType == "silver" &&
                        !(transaction.Comment.StartsWith("arc_buy_silver") || transaction.Comment.StartsWith("arc_sell_silver")))
                        continue;

                    transactionModels.Add(ParseVendorTransactionNewAsModel(transaction));
                }

                result.Message = "ok";
                result.Success = true;
                result.TransactionModels = transactionModels;


            }
            catch (Exception e)
            {
                Log.Error("error at buy vendor gold: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }


            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("daily_transactions")]
        public ActionResult DailyTransactions(string code, string date) 
        {
            try
            {
                if (!VendorGiftService.ValidateCode(code))
                {
                    return BadRequest("invalid code");
                }

                VendorGiftService.UseCode(code);
                List<VendorTransaction> transactions;
                if (date == null)
                {
                    transactions = Repository.GetVendorTransactions()
                    .Where(x => x.ConfirmedByGoldtag && x.ConfirmedByVendor && x.TransactionFinalisedDateTime.HasValue)
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();
                } 
                else
                {
                    var dateObj = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                    transactions = Repository.GetVendorTransactions()
                        .Where(x => x.ConfirmedByVendor && x.ConfirmedByGoldtag && x.TransactionFinalisedDateTime.HasValue
                        && x.TransactionFinalisedDateTime.Value.Date == dateObj.Date)
                        .OrderByDescending(x => x.TransactionDateTime)
                        .ToList();
                        
                }


                var result = "transaction_id,vendor_reference,source,destination,vendor_confirmed,goldtag_confirmed" +
                    ",cancelled,finalised,gram_amount,price,transaction_date_time,vendor_confirmed_date_time" +
                    ",goldtag_confirmed_date_time,transaction_finalised_date_time\n";

                var format = "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}\n";
                foreach (var trans in transactions)
                {
                    var model = ParseVendorTransactionAsModel(trans);

                    result += string.Format(format, model.TransactionId,
                        model.Reference, model.Source, model.Destination,model.ConfirmedByVendor,
                        model.ConfirmedByGoldtag, model.Cancelled, model.Finalised, model.GramAmount,
                        model.TlAmount, model.TransactionDateTime, model.VendorConfirmedDateTime, 
                        model.GoldtagConfirmedDateTime, model.TransactionFinalisedDateTime);
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("err at daily-trans: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok("err at daily-trans: " + e.Message);
            }
        }


        /* SUb Methods*/
        private bool IsBuyGoldOk(VendorBuyGoldParams model, out string message)
        {
            if (model.GramAmount.Value > 40m)
            {
                message = "Amount can not exeed 40g";
                return false;
            }

            if (model.GramAmount.Value < 0.01m)
            {
                message = "Amount can not be lower then 0.01g";
                return false;
            }
            message = "";
            return true;
        }

        private class BuyOkResult
        {
            public bool Ok { get; set; }
            public string Message { get; set; }
        }
        private async Task<BuyOkResult> IsSellGoldOkAsync(VendorSellGoldParams model)
        {
            return await Task.Run(() =>
            {
                var res = new BuyOkResult { Ok = false };

                if (!model.GramAmount.HasValue || model.GramAmount.Value < 0.01m)
                {
                    res.Message = "Amount can not be lower then 0.01g";//5grams per single transaction";
                    return res;
                }
                res.Ok = true;
                return res;
            });
        }
        private async Task<BuyOkResult> IsBuyGoldrOkAsync(VendorBuyGoldParams model)
        {
            return await Task.Run(() =>
            {
                var res = new BuyOkResult { Ok = false };

                if (!model.GramAmount.HasValue || model.GramAmount.Value < 0.01m)
                {
                    res.Message = "Amount can not be lower then 0.01g";//5grams per single transaction";
                    return res;
                }
                res.Ok = true;
                return res;
            });
        }
        private async Task<BuyOkResult> IsBuySilverOkAsync(VendorBuyGoldParams model)
        {
            return await Task.Run(() =>
            {
                var res = new BuyOkResult { Ok = false };

                if (!model.GramAmount.HasValue || model.GramAmount.Value < 1m)
                {
                    res.Message = "Amount can not be lower then 1g for tests";//5grams per single transaction";
                    return res;
                }
                res.Ok = true;
                return res;
            });
        }

        private async Task<BuyOkResult> IsSellSilverOkAsync(VendorSellGoldParams model)
        {
            return await Task.Run(() =>
            {
                var res = new BuyOkResult { Ok = false };

                if (!model.GramAmount.HasValue || model.GramAmount.Value < 1m)
                {
                    res.Message = "Amount can not be lower then 1g for tests";//5grams per single transaction";
                    return res;
                }
                res.Ok = true;
                return res;
            });
        }

        private bool IsBuySilverOk(VendorBuyGoldParams model, out string message)
        {
            if (model.GramAmount.Value > 2500m)
            {
                message = "Amount can not exeed 2500g";// 2500grams per single transaction";
                return false;
            }

            if (model.GramAmount.Value < 1m)
            {
                message = "Amount can not be lower then 5g";//5grams per single transaction";
                return false;
            }
            message = "";
            return true;
        }

        /*ASYNC END POINTS */

        [HttpPost]
        [Route("payment_made")]
        public async Task<object> VendorMadePayment(MadePaymentParamModel model)
        {



            return Ok();
        }

        [HttpPost]
        [Route("transfer_status")]
        public async Task<object> VendorMadePayment(PaymentCompleteParamModel model)
        {



            return Ok();
        }

        // BUY SILVER
        private async Task<VendorBuyGoldResult> VendorBuySilverResultAsync(VendorBuyGoldParams model)
        {
            var result = new VendorBuyGoldResult { Success = false };
            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;
                var tokenTask = await Authenticator.ValidateTokenAsync(token);
                if (!tokenTask.Validated)
                {
                    result.Message = "Bad Auth";
                    return result;
                }
                var checkValidTask = VendorBuyGoldParams.CheckValidAsync(model);
                var buyOk = IsBuySilverOkAsync(model);
                var vendorTask = Repository.GetVendorAsync(Guid.Parse(tokenTask.Id), model.ApiKey);
                if ((await vendorTask) == null || (await checkValidTask) == false  || (await buyOk).Ok == false)
                {
                    result.Message = "Bad request";
                    return result;
                }
                
                var vendor = vendorTask.Result;

                if (await Repository.IsTherePreviousTransaction(model.Reference))
                {
                    result.Message = "This reference_id already in use.";
                    return result;
                }

                var currentRate = GlobalGoldPrice.GetSilverPricesCached();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpensesCached();
                decimal silverPrice = (currentRate.BuyRate * taxAndExpenses.BankaMukaveleTax);
                silverPrice = Math.Truncate(100 * silverPrice) / 100;
                var transPrice = (model.GramAmount.Value * silverPrice);

                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = "arc_buy_silver:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;
                /*
                var vendorTransaction = new VendorTransactionNew(
                    model.Reference,
                    Guid.Parse(FintagVendorId),
                    vendor.VendorId,
                    model.GramAmount.Value,
                    transPrice,
                    randString);
                await Repository.AddVendorTransactionNewAsync(vendorTransaction);*/
                var vendorTransaction = new VenTransaction(
                   model.Reference,
                   Guid.Parse(FintagVendorId),
                   vendor.VendorId,
                   model.GramAmount.Value,
                   transPrice,
                   randString,
                   currentRate.BuyRate);
                await Repository.AddVenTransactionAsync(vendorTransaction);
                await Repository.SaveChangesAsync();


                result.TransactionId = vendorTransaction.TransactionId.ToString();
                result.Reference = model.Reference;
                result.Price = transPrice;
                result.Success = true;
                result.Message = "ok";


            }
            catch (Exception e)
            {
                Log.Error("error at buy vendor silver: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }

            return result;
        }

        [HttpPost]
        [Route("buy_silver")]
        public async Task<ActionResult<VendorBuyGoldResult>> VendorBuySilverAsync(VendorBuyGoldParams model)
        {
            return Ok(await VendorBuySilverResultAsync(model));
        }

        //SELL SILVER
        private async Task<VendorSellGoldResult> VendorSellSilverResultAsync(VendorSellGoldParams model)
        {
            var result = new VendorSellGoldResult { Success = false };
            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;
                var tokenTask = await Authenticator.ValidateTokenAsync(token);
                if (!tokenTask.Validated)
                {
                    result.Message = "Bad Auth";
                    return result;
                }
                var checkValidTask = VendorSellGoldParams.CheckValidAsync(model);
                var buyOk = IsSellSilverOkAsync(model);
                var vendorTask = Repository.GetVendorAsync(Guid.Parse(tokenTask.Id), model.ApiKey);


                if ((await vendorTask) == null || (await checkValidTask) == false || (await buyOk).Ok == false)
                {
                    result.Message = "Bad request";
                    return result;
                }

                var vendor = vendorTask.Result;

                if (await Repository.IsTherePreviousTransaction(model.Reference))
                {
                    result.Message = "This reference_id already in use.";
                    return result;
                }
                var currentSilverRate = GlobalGoldPrice.GetSilverPricesCached();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpensesCached();
                decimal silverSalePrice = (currentSilverRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax));
                silverSalePrice = Math.Truncate(100 * silverSalePrice) / 100;

                var transPrice = model.GramAmount.Value * silverSalePrice;//* currentSilverRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax);
                transPrice = Math.Truncate(100 * transPrice) / 100;
                var rand = new Random();

                var randString = "arc_sell_silver:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var vendorTransaction = new VenTransaction(model.Reference,
                    vendor.VendorId,
                    Guid.Parse(FintagVendorId),
                    model.GramAmount.Value,
                    transPrice,
                    randString,
                    currentSilverRate.SellRate);
                await Repository.AddVenTransactionAsync(vendorTransaction);
                await Repository.SaveChangesAsync();

                result.TransactionId = vendorTransaction.TransactionId.ToString();
                result.Reference = model.Reference;
                result.Success = true;
                result.Price = transPrice;
                result.Message = "ok";


            }
            catch (Exception e)
            {
                Log.Error("error at sell vendor silver: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }

            return result;
        }

        [HttpPost]
        [Route("sell_silver")]
        public async Task<ActionResult<VendorSellGoldResult>> VendorSellSilverAsync(VendorSellGoldParams model)
        {
            return Ok(await VendorSellSilverResultAsync(model));
        }

        // BUY GOLD
        private async Task<VendorBuyGoldResult> VendorBuyGoldResultAsync(VendorBuyGoldParams model)
        {

            
            var result = new VendorBuyGoldResult { Success = false };

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;
                var tokenTask = await Authenticator.ValidateTokenAsync(token);
                if (!tokenTask.Validated)
                {
                    result.Message = "Bad Auth";
                    return result;
                }
                var checkValidTask = VendorBuyGoldParams.CheckValidAsync(model);
                var buyOk = IsBuyGoldrOkAsync(model);
                var vendorTask = Repository.GetVendorAsync(Guid.Parse(tokenTask.Id), model.ApiKey);
                if ((await vendorTask) == null || (await checkValidTask) == false || (await buyOk).Ok == false)
                {
                    result.Message = "Bad request";
                    return result;
                }
                var vendor = vendorTask.Result;

                if (await Repository.IsTherePreviousTransaction(model.Reference))
                {
                    result.Message = "This reference_id already in use.";
                    return result;
                }

                var currentRate = GlobalGoldPrice.GetGoldPricesCached();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpensesCached();
                
                decimal price = (currentRate.BuyRate  * taxAndExpenses.BankaMukaveleTax);
                price = Math.Truncate(100 * price) / 100;

                var transPrice = (model.GramAmount.Value * price);
                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = ":arc_buy:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var vendorTransaction = new VenTransaction(
                    model.Reference,
                    Guid.Parse(FintagVendorId),
                    vendor.VendorId,
                    model.GramAmount.Value,
                    transPrice,
                    randString,
                    currentRate.BuyRate);

                await Repository.AddVenTransactionAsync(vendorTransaction);

                await Repository.SaveChangesAsync();

                result.TransactionId = vendorTransaction.TransactionId.ToString();
                result.Reference = model.Reference;
                result.Price = transPrice;
                result.Success = true;
                result.Message = "ok";


            }
            catch (Exception e)
            {
                int a = 1;
                result.Message = "Bir hata olustu.";

                do
                {
                    Log.Error("error at buy vendor gold"); 
                    Log.Error(a.ToString() + ": " + e.Message);
                    Log.Error(a.ToString() + ": " + e.StackTrace);
                    e = e.InnerException;
                    a++;
                } while (e != null);
            }

            return result;
        }

        [HttpPost]
        [Route("buy_gold")]
        public async Task<ActionResult<VendorBuyGoldResult>> VendorBuyGoldAsync(VendorBuyGoldParams model)
        {

            var vendorBuyRequestResult = await VendorBuyGoldResultAsync(model);

            return Ok(vendorBuyRequestResult);
            
        }

        // SELL GOLD
        private async Task<VendorSellGoldResult> VendorSellGoldResultAsync(VendorSellGoldParams model)
        {
            var result = new VendorSellGoldResult { Success = false };
            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var tokenTask = await Authenticator.ValidateTokenAsync(token);
                if (!tokenTask.Validated)
                {
                    result.Message = "Bad Auth";
                    return result;
                }
                var checkValidTask = VendorSellGoldParams.CheckValidAsync(model);
                var buyOk = IsSellGoldOkAsync(model);
                var vendorTask = Repository.GetVendorAsync(Guid.Parse(tokenTask.Id), model.ApiKey);
               
                if ((await vendorTask) == null || (await checkValidTask) == false || (await buyOk).Ok == false)
                {
                    result.Message = "Bad request";
                    return result;
                }

                

                var vendor = vendorTask.Result;

                if (await Repository.IsTherePreviousTransaction(model.Reference))
                {
                    result.Message = "This reference_id already in use.";
                    return result;
                }
                var currentRate = GlobalGoldPrice.GetGoldPricesCached();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpensesCached();
                decimal salePrice = (currentRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax));
                salePrice = Math.Truncate(100 * salePrice) / 100;
                var transPrice = model.GramAmount.Value * salePrice;
                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = ":arc_sell:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var vendorTransaction = new VenTransaction(model.Reference,
                    vendor.VendorId,
                    Guid.Parse(FintagVendorId),
                    model.GramAmount.Value,
                    transPrice,
                    randString,
                    currentRate.SellRate);

                await Repository.AddVenTransactionAsync(vendorTransaction);
                await Repository.SaveChangesAsync();
                result.TransactionId = vendorTransaction.TransactionId.ToString();
                result.Reference = model.Reference;
                result.Success = true;
                result.Price = transPrice;
                result.Message = "ok";

            }
            catch (Exception e)
            {
                Log.Error("error at sell vendor gold async: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }

            return result;
        }

        [HttpPost]
        [Route("sell_gold")]
        public async Task<ActionResult<VendorSellGoldResult>> VendorSellGoldAsync(VendorSellGoldParams model)
        {
            return Ok(await VendorSellGoldResultAsync(model));
        }

        // Buy plt
        private async Task<VendorBuyGoldResult> VendorBuyPltResultAsync(VendorBuyGoldParams model)
        {


            var result = new VendorBuyGoldResult { Success = false };

            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;
                var tokenTask = await Authenticator.ValidateTokenAsync(token);
                if (!tokenTask.Validated)
                {
                    result.Message = "Bad Auth";
                    return result;
                }
                var checkValidTask = VendorBuyGoldParams.CheckValidAsync(model);
                var buyOk = IsBuyGoldrOkAsync(model);
                var vendorTask = Repository.GetVendorAsync(Guid.Parse(tokenTask.Id), model.ApiKey);
                if ((await vendorTask) == null || (await checkValidTask) == false || (await buyOk).Ok == false)
                {
                    result.Message = "Bad request";
                    return result;
                }
                var vendor = vendorTask.Result;

                if (await Repository.IsTherePreviousTransaction(model.Reference))
                {
                    result.Message = "This reference_id already in use.";
                    return result;
                }

                var currentRate = GlobalGoldPrice.GetPlatinPricesCached();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpensesCached();

                decimal price = (currentRate.BuyRate * taxAndExpenses.BankaMukaveleTax);
                price = Math.Truncate(100 * price) / 100;

                var transPrice = (model.GramAmount.Value * price);
                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = "arc_buy_platin:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var vendorTransaction = new VenTransaction(
                    model.Reference,
                    Guid.Parse(FintagVendorId),
                    vendor.VendorId,
                    model.GramAmount.Value,
                    transPrice,
                    randString,
                    currentRate.BuyRate);

                await Repository.AddVenTransactionAsync(vendorTransaction);

                await Repository.SaveChangesAsync();

                result.TransactionId = vendorTransaction.TransactionId.ToString();
                result.Reference = model.Reference;
                result.Price = transPrice;
                result.Success = true;
                result.Message = "ok";
            }
            catch (Exception e)
            {
                int a = 1;
                result.Message = "Bir hata olustu.";

                do
                {
                    Log.Error("error at buy vendor platin");
                    Log.Error(a.ToString() + ": " + e.Message);
                    Log.Error(a.ToString() + ": " + e.StackTrace);
                    e = e.InnerException;
                    a++;
                } while (e != null);
            }

            return result;
        }

        [HttpPost]
        [Route("buy_platin")]
        public async Task<ActionResult<VendorBuyGoldResult>> VendorBuyPltAsync(VendorBuyGoldParams model)
        {

            var vendorBuyRequestResult = await VendorBuyPltResultAsync(model);

            return Ok(vendorBuyRequestResult);

        }

        // sell plt
        private async Task<VendorSellGoldResult> VendorSellPltResultAsync(VendorSellGoldParams model)
        {
            var result = new VendorSellGoldResult { Success = false };
            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var tokenTask = await Authenticator.ValidateTokenAsync(token);
                if (!tokenTask.Validated)
                {
                    result.Message = "Bad Auth";
                    return result;
                }
                var checkValidTask = VendorSellGoldParams.CheckValidAsync(model);
                var buyOk = IsSellGoldOkAsync(model);
                var vendorTask = Repository.GetVendorAsync(Guid.Parse(tokenTask.Id), model.ApiKey);

                if ((await vendorTask) == null || (await checkValidTask) == false || (await buyOk).Ok == false)
                {
                    result.Message = "Bad request";
                    return result;
                }



                var vendor = vendorTask.Result;

                if (await Repository.IsTherePreviousTransaction(model.Reference))
                {
                    result.Message = "This reference_id already in use.";
                    return result;
                }
                var currentRate = GlobalGoldPrice.GetPlatinPrices();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpensesCached();
                decimal salePrice = (currentRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax));
                salePrice = Math.Truncate(100 * salePrice) / 100;
                var transPrice = model.GramAmount.Value * salePrice;
                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = "arc_sell_platin:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var vendorTransaction = new VenTransaction(model.Reference,
                    vendor.VendorId,
                    Guid.Parse(FintagVendorId),
                    model.GramAmount.Value,
                    transPrice,
                    randString,
                    currentRate.SellRate);

                await Repository.AddVenTransactionAsync(vendorTransaction);
                await Repository.SaveChangesAsync();
                result.TransactionId = vendorTransaction.TransactionId.ToString();
                result.Reference = model.Reference;
                result.Success = true;
                result.Price = transPrice;
                result.Message = "ok";

            }
            catch (Exception e)
            {
                Log.Error("error at sell vendor platin async: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu.";
            }

            return result;
        }

        [HttpPost]
        [Route("sell_platin")]
        public async Task<ActionResult<VendorSellGoldResult>> VendorSellPltAsync(VendorSellGoldParams model)
        {
            return Ok(await VendorSellPltResultAsync(model));
        }


        private async Task<ValidateFinaliseResult> ValidFinaliseRequest(FinaliseTransactionParams model)
        {

            var result = new ValidateFinaliseResult
            {
                Success = false,
                Message = "",
                Confirmed = false
            };


            try
            {

                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var tokenTask = await Authenticator.ValidateTokenAsync(token);
                if (!tokenTask.Validated)
                {
                    result.Message = "Bad Auth";
                    return result;
                }
                
                var modelTask = FinaliseTransactionParams.CheckValidAsync(model);
                var vendorTask = Repository.GetVendorAsync(Guid.Parse(tokenTask.Id), model.ApiKey);
                //var vendorDataTask = Repository.GetSpecificVendorDataReadOnlyAsync(tokenTask.Id);
                //var transTask = Repository.GetVendorTransactionNewAsync(int.Parse(model.TransactionId), model.Reference);
                if (!(await modelTask) || (await vendorTask) == null)// || (await vendorDataTask) == null)// || (await transTask) == null)
                {
                    result.Message = "Invalid Request - check params";
                    result.ResultCode = "27";
                    return result;
                }


                
                result.vendor = vendorTask.Result;
                result.vendorData = await Repository.GetSpecificVendorDataReadOnlyAsync(tokenTask.Id);
                result.vendorTransaction = await Repository.GetVenTransactionAsync(int.Parse(model.TransactionId), model.Reference);
                result.PlatinBalance = await Repository.GetVendorPlatinBalanceAsync(result.vendor.VendorId);
                if (result.vendorData == null || result.vendorTransaction == null || result.PlatinBalance == null)
                {
                    result.Message = "transaction_id yada vendor_reference hatali, eksik yada vendor_reference uyusmuyor.";
                    result.ResultCode = "4";
                    return result;
                }
                
                if (result.vendorTransaction.Succesful || result.vendorTransaction.TransactionFinalisedDateTime.HasValue)
                {
                    result.Message = "Daha onceden tamamlanmis transaction.";
                    result.ResultCode = "5";
                    return result;
                }

                if (result.vendorTransaction.Cancelled)
                {
                    result.ResultCode = "6";
                    result.Message = "Daha onceden cancel edilmis transaction.";
                    return result;
                }


                result.Confirmed = model.Confirmed.Value;
                result.Success = true;
            }
            catch (Exception e)
            {

                result.Message = "Bir sistem hatasi olustu: " + e.Message;
                Log.Error("Error at validate finalize params: " + e.Message);
                Log.Error(e.StackTrace);
                result.ResultCode = "10";
                e = e.InnerException;

                while (e != null)
                {
                    Log.Error("Inner Error at validate finalize params: " + e.Message);
                    Log.Error(e.StackTrace);
                    e = e.InnerException;
                }
                

            }
            return result;
        }
      
        // finalizaiton transaction
        [HttpPost]
        [Route("finalise_transaction")]
        public async Task<ActionResult<FinaliseTransactionResult2>> VendorCompleteAsync(FinaliseTransactionParams model)
        {
            //Log.Debug("finalise_transaction_async with " + model.ToString());
            
            try
            {
                var result = new FinaliseTransactionResult2 { ResultCode = "0" };
                var validFinalise = await ValidFinaliseRequest(model);
                //Log.Debug("finalise debug: " + validFinalise.ToString());
                // valid parameters check
                if (!validFinalise.Success)
                {
                    result.Message = validFinalise.Message;
                    result.ResultCode = validFinalise.ResultCode;
                    return Ok(result);
                }
                //Log.Debug("ARC=" + model.ToString());
                // confirmed check
                if (!validFinalise.Confirmed)
                {
                    
                    validFinalise.vendorTransaction.CancelTransaction("Not confirmed by vendor");
                    await Repository.SaveChangesAsync();
                    result.Message = "Vendor islemi onaylamadi.";
                    result.ResultCode = "7";
                    return Ok(result);
                }

                
                // Time check
                var diff = DateTime.Now - validFinalise.vendorTransaction.TransactionDateTime;
                if (diff.TotalSeconds > 50)
                {
                    validFinalise.vendorTransaction.CancelTransaction("Not confirmed by Goldtag: Zaman Asimi");
                    await Repository.SaveChangesAsync();
                    result.Message = "Islem zaman asimi.";
                    result.ResultCode = "8";
                    return Ok(result);
                }
                var vendorBuysFromFintag = validFinalise.vendorTransaction.Source == Guid.Parse(FintagVendorId);
                
                // balance check
                if (!vendorBuysFromFintag)
                {
                    var silverTransaction = (validFinalise.vendorTransaction.Comment.StartsWith("arc_sell_silver"));
                    var platinTransaction = (validFinalise.vendorTransaction.Comment.StartsWith("arc_sell_platin"));
                    if ((!silverTransaction && !platinTransaction && validFinalise.vendor.Balance < validFinalise.vendorTransaction.GramAmount) || // yetersiz gold balance
                    (silverTransaction && !validFinalise.vendor.SilverBalance.HasValue) || // system error silver transaction ama silver balacne yok
                    (silverTransaction && validFinalise.vendor.SilverBalance.Value < validFinalise.vendorTransaction.GramAmount) ||
                    (platinTransaction && validFinalise.PlatinBalance == null) ||
                    (platinTransaction && validFinalise.PlatinBalance.Balance < validFinalise.vendorTransaction.GramAmount)
                    ) // yetersiz silver balance
                    {
                        validFinalise.vendorTransaction.CancelTransaction("vendor not enough gold or silver balance");
                        await Repository.SaveChangesAsync();

                        result.Message = "Yetersiz bakiye.";
                        result.ResultCode = "9";
                        return Ok(result);
                    }
                }
                

                validFinalise.vendorTransaction.VendorConfirmed();

                validFinalise.vendorTransaction.GTagConfirmed();

                //Log.Debug("finalize debug before next: " + JsonConvert.SerializeObject(validFinalise));

                if (validFinalise.vendorTransaction.Source == Guid.Parse(FintagVendorId))
                {
                    result = await buySellService.VendorBuysFromFintagRoutine(validFinalise, Repository, expectedCashService);
                }
                else
                {
                    result = await buySellService.VendorSellsToFintagRoutine(validFinalise, Repository, cashSenderService);
                }

                return Ok(result);


            }
            catch (Exception e)
            {
                Log.Error("error at finalise trans gold: " + e.Message);
                Log.Error(e.StackTrace);
                var result = new FinaliseTransactionResult { ResultCode = "10", Message = "Bilinmeyen bit hata olustu. Lutfen destek icin bizi arayiniz." };
                e = e.InnerException;

                while (e != null)
                {
                    Log.Error("finalise async inner exception: " + e.Message);
                    Log.Error(e.StackTrace);
                    e = e.InnerException;
                }
                return Ok(result);
            };

        }

        // current price routine
        [HttpPost]
        [Route("get_current_price")]
        public async Task<ActionResult<VendorPriceCheckResult>> GetPriceVendorAsync(VendorPriceCheck model)
        {

            var result = new VendorPriceCheckResult { Success = false };
            try
            {
                //Log.Information("vendor price check initiated");
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string vendorid;

                if (!Authenticator.ValidateToken(token, out vendorid))
                {
                    result.Message = "Invalid token";
                    return Unauthorized(result);
                }

                if (!VendorPriceCheck.CheckValid(model))
                {
                    result.Message = "Invalid request";
                    return Ok(result);
                }

                //var requestee = await Repository.GetVendorReadOnlyAsync(Guid.Parse(vendorid));

                //var vendor = await Repository.GetVendorReadOnlyAsync(model.ApiKey);

                var vendor = await Repository.GetVendorAsync(Guid.Parse(vendorid), model.ApiKey);
                if (vendor == null)
                {
                    result.Message = "Invalid vendor";
                    return Unauthorized(result);
                }

                /*
                if (requestee == null || vendor == null || requestee.VendorId != vendor.VendorId)
                {
                    result.Message = "Invalid vendor";
                    return Unauthorized(result);
                }*/


                var currentRate = GlobalGoldPrice.GetGoldPricesCached();
                var currentSilverRate = GlobalGoldPrice.GetSilverPricesCached();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpensesCached();
                var currentPlatinRate = GlobalGoldPrice.GetPlatinPricesCached();

                // buy gold

                decimal price = (currentRate.BuyRate * model.GramAmount.Value * taxAndExpenses.BankaMukaveleTax);
                price = Math.Truncate(100 * price) / 100;

                // sell gold                
                decimal salePrice = (currentRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax) * model.GramAmount.Value);
                salePrice = Math.Truncate(100 * salePrice) / 100;


                // buy silver
                decimal silverPrice = (currentSilverRate.BuyRate * model.GramAmount.Value * taxAndExpenses.BankaMukaveleTax);
                silverPrice = Math.Truncate(100 * silverPrice) / 100;

                // sell silver
                decimal silverSalePrice = (currentSilverRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax) * model.GramAmount.Value);
                silverSalePrice = Math.Truncate(100 * silverSalePrice) / 100;

                // buy platin
                decimal platinBuyPrice = (currentPlatinRate.BuyRate * model.GramAmount.Value * taxAndExpenses.BankaMukaveleTax);
                platinBuyPrice = Math.Truncate(100 * platinBuyPrice) / 100;

                // sell platin
                decimal platinSalePrice = (currentPlatinRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax) * model.GramAmount.Value);
                platinSalePrice = Math.Truncate(100 * platinSalePrice) / 100;

                result.SellSilver = silverSalePrice;
                result.BuySilver = silverPrice;
                result.Buy = price;
                result.Sell = salePrice;
                result.SellPlatin = platinSalePrice;
                result.BuyPlatin = platinBuyPrice;
                result.Message = "Request ok";
                result.Success = true;
            }
            catch (Exception e)
            {
                Log.Error("Exception at vendor price check " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata olustu";

            }
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<LoginVendorResultModel>> LoginVendorAsync(LoginVendorModel model)
        {
            var result = new LoginVendorResultModel { Success = false };
            try
            {
                //Log.Debug("LoginVendor() started for - " + model.ApiKey);

                var ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();

                var bannedIp = await Repository.GetBannedIpAsync(ip);

                if (bannedIp != null)
                {
                    result.Message = "Yasaklı IP adresi.";
                    return Ok(result);
                }

                if (model == null || model.ApiKey == null || model.Secret == null)
                {
                    result.Message = "Verilen bilgiler uyuşmamaktadır, lütfen kontrol edip giriş yapınız.";
                    return Ok(result);
                }

                var vendor = await Repository.GetVendorAsync(model.ApiKey);
                
                if (vendor == null)
                {
                    result.Message = "Api Key i kontrol edip giriş yapınız.";
                    return Ok(result);
                }

                if (vendor.Secret != model.Secret)
                {
                    result.Message = "Secret i kontrol edip giriş yapınız.";


                    if (AIService.RegisterWrongSecret(model.ApiKey, ip.ToString()))
                    {
                        var newBannedIp = new BannedIp2(ip.ToString());
                        Repository.AddBannedIp(newBannedIp);
                        Repository.SaveChanges();
                        result.Message = "5 hatali secret islemi, ip yasaklandi.";
                        var msg = string.Format("Vendor id: {0} geçici olarak banlandı", vendor.VendorId);
                        EmailService.SendEmail("dolunaysabuncuoglu@gmail.com", "5 den fazla hatalı VENDOR giriş", msg, false);
                    }

                    return Ok(result);
                }


                var token = Authenticator.GetVendorToken(vendor.VendorId.ToString());

                //Log.Debug("Vendor login success: vendorId: " + vendor.VendorId.ToString());
                result.Message = "Giriş Başarılı.";
                result.AuthToken = token;
                result.Success = true;
                var vendorData = await Repository.GetVendorDataAsync(vendor.VendorId);

                if (vendorData == null)
                {
                    vendorData = new VendorData(vendor.VendorId.ToString(), null, null, null, null, null);
                    await Repository.AddVendorDataAsync(vendorData);
                    await Repository.SaveChangesAsync();
                }
                
                var platinBalance = await Repository.GetVendorPlatinBalanceAsync(vendor.VendorId);
                if (platinBalance == null)
                {
                    platinBalance = new VendorPlatinBalance(vendor.VendorId);
                    Repository.AddVendorPlatinBalance(platinBalance);
                    Repository.SaveChanges();
                }

            }
            catch (Exception e)
            {
                Log.Error("Exception at LoginVendor() - " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu lütfen da sonra tekrar deneyiniz";
                result.Success = false;
                e = e.InnerException;

                while (e != null)
                {
                    Log.Error("loginvendorasync inner exception: " + e.Message);
                    Log.Error(e.StackTrace);
                    e = e.InnerException;
                }
            }

            return Ok(result);
        }
    }
}
