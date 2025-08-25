using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Serilog;
using Gold.Domain.Users.Interfaces;
using Gold.Domain.Users.Repositories;
using Gold.Api.Models.Users;
using Gold.Domain.Events.Interfaces;
using Gold.Domain.Events.Repositories;
using Gold.Domain.Transactions.Interfaces;
using Gold.Domain.Transactions.Repositories;
using Google.Apis.Http;
using Gold.Api.Utilities;

namespace Gold.Api.Services
{


    public class AIService
    {
        private static ITransactionsRepository transactionsRepository = new TransactionsRepository();

        private class WrongSecret
        {
            public DateTime DateTime { get; set; }
            public string ApiKey { get; set; }
            public string IP { get; set; }

            public override string ToString()
            {
                return string.Format("WrongSecret:{0}:{1}:{2}", ApiKey, DateTime, IP);
            }
        }

        private class WrongPassword
        {
            public DateTime DateTime { get; set; }
            public string EmailPhone { get; set; }
            public string IP { get; set; }

            public override string ToString()
            {
                return string.Format("WrongPassword:{0}:{1}:{2}", EmailPhone, DateTime, IP);
            }
        }

        private class ForgotPassRequest
        {
            public DateTime DateTime { get; set; }
            public string EmailPhone { get; set; }
            public string IP { get; set; }

            public override string ToString()
            {
                return string.Format("ForgotPass:{0}:{1}:{2}", EmailPhone, DateTime, IP);
            }
        }

        private class ForgotPassCode
        {
            public DateTime DateTime { get; set; }
            public string IP { get; set; }

            public override string ToString()
            {
                return string.Format("ForgotPassCode:{0}:{1}", DateTime, IP);
            }
        }


        private static Dictionary<Guid, bool> ReceivedFirstData = new Dictionary<Guid, bool>();
        private static Dictionary<Guid, UserRecords> OnlineUserRecords = new Dictionary<Guid, UserRecords>();
        private static Dictionary<Guid, DateTime> OnlineUsers = new Dictionary<Guid, DateTime>();
        private static List<WrongPassword> WrongPasswords = new List<WrongPassword>();
        private static List<ForgotPassRequest> ForgotPassRequests = new List<ForgotPassRequest>();
        private static List<ForgotPassCode> ForgotPassCodes = new List<ForgotPassCode>();
        private static List<ForgotPassCode> ForgotPassCodePassReqs = new List<ForgotPassCode>();
        private static List<WrongSecret> WrongSecrets = new List<WrongSecret>();


        public static bool RegisterWrongSecret(string apiKey, string ip)
        {
            int count = 0;
            foreach (var wrong in WrongSecrets)
            {
                var diff = DateTime.Now - wrong.DateTime;
                if (diff.TotalSeconds < 60 && (wrong.ApiKey == apiKey || wrong.IP == ip))
                {
                    count++;
                }
            }
            var wrongSecret = new WrongSecret { ApiKey = apiKey, DateTime = DateTime.Now, IP = ip };
            WrongSecrets.Add(wrongSecret);
            return (count >= 5);
        }

        public static bool RegisterWrongPass(string emailPhone, string ip)
        {
            int count = 0;

            foreach (var wrong in WrongPasswords)
            {
                var diff = DateTime.Now - wrong.DateTime;

                if (diff.TotalSeconds < 60 && (wrong.EmailPhone == emailPhone || wrong.IP == ip))
                {
                    count++;
                }
            }
            var wrongPass = new WrongPassword { DateTime = DateTime.Now, EmailPhone = emailPhone, IP = ip };
            WrongPasswords.Add(wrongPass);
            Log.Information(wrongPass.ToString());
            return (count >= 5);
        }

        public static bool RegisterForgotPass(string emailPhone, string ip)
        {
            int count = 0;

            foreach (var wrong in ForgotPassRequests)
            {
                var diff = DateTime.Now - wrong.DateTime;

                if (diff.TotalSeconds < 60 && (wrong.EmailPhone == emailPhone || wrong.IP == ip))
                {
                    count++;
                }
            }
            var frequest = new ForgotPassRequest { DateTime = DateTime.Now, EmailPhone = emailPhone, IP = ip };
            ForgotPassRequests.Add(frequest);
            Log.Information(frequest.ToString());
            return (count >= 5);
        }

        public static bool RegisterForgotPassCode(string ip)
        {
            int count = 0;

            foreach (var wrong in ForgotPassCodes)
            {
                var diff = DateTime.Now - wrong.DateTime;

                if (diff.TotalSeconds < 60 &&  wrong.IP == ip)
                {
                    count++;
                }
            }
            var frequest = new ForgotPassCode { DateTime = DateTime.Now, IP = ip };
            ForgotPassCodes.Add(frequest);
            Log.Information(frequest.ToString());
            return (count >= 5);
        }

        public static bool RegisterForgotPassCodePass(string ip)
        {
            int count = 0;

            foreach (var wrong in ForgotPassCodePassReqs)
            {
                var diff = DateTime.Now - wrong.DateTime;

                if (diff.TotalSeconds < 60 && wrong.IP == ip)
                {
                    count++;
                }
            }
            var frequest = new ForgotPassCode { DateTime = DateTime.Now, IP = ip };
            ForgotPassCodePassReqs.Add(frequest);
            Log.Information(frequest.ToString());
            return (count >= 5);
        }

        /// <summary>
        /// Each time a user checks for new Notifications
        /// Checks in here
        /// </summary>
        /// <param name="userId"></param>
        public static void CheckIn(Guid userId)
        {
            if (OnlineUsers.ContainsKey(userId))
            {
                OnlineUsers[userId] = DateTime.Now;
            }
            else
            {
                OnlineUsers.Add(userId, DateTime.Now);
            }
        }

        /// <summary>
        /// When a User logins the user registers its online status
        /// Where User never received initial Data (events and transactions)
        /// And stores Users current Records
        /// </summary>
        /// <param name="userId"></param>
        public static void RegisterOnlineStatus(Guid userId)
        {
            if (OnlineUsers.ContainsKey(userId))
            {
                OnlineUsers[userId] = DateTime.Now;
            }
            else
            {
                OnlineUsers.Add(userId, DateTime.Now);
            }

            if (ReceivedFirstData.ContainsKey(userId))
            {
                ReceivedFirstData[userId] = false;
            } else
            {
                ReceivedFirstData.Add(userId, false);
            }

            if (OnlineUserRecords.ContainsKey(userId))
            {
                OnlineUserRecords[userId] = GetRecords(userId);
            } else
            {
                OnlineUserRecords.Add(userId, GetRecords(userId));
            }
                
        }

        /// <summary>
        /// Returns UserStatus of given userId
        /// if user did not received first data then it returns true for all
        /// if since last check user TransactionCount or EventsCount changes returns true for those
        /// otherwise returns status false
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static UserStatus GetUserStatus(Guid userId)
        {
            var status = new UserStatus
            {
                Events = false,
                Transactions = false,
                UserId = userId.ToString(),
                Notifications = false
            };

            Log.Debug("GetUserStatus() " + userId.ToString());

            try
            {
                if (!ReceivedFirstData[userId])
                {
                    status.Balance = true;
                    status.Events = true;
                    status.Transactions = true;
                    ReceivedFirstData[userId] = true;
                } 
                else
                {
                    var records = OnlineUserRecords[userId];
                    Log.Debug("old records: " + records);
                    var newRecords = GetRecords(userId);
                    Log.Debug("current records: " + records);
                    if (!records.Equals(newRecords))
                    {
                        status.Balance = records.UserBalance != newRecords.UserBalance;
                        status.Events = records.EventsWeddingsCount != newRecords.EventsWeddingsCount;
                        status.Transactions = records.TransactionsCount != newRecords.TransactionsCount;
                        OnlineUserRecords[userId] = newRecords;

                        Log.Debug("Not equals so newwest " + newRecords);
                        

                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception at GetRecords: " + e.Message);
                Log.Error(e.StackTrace);
                throw new Exception("Error at get user status", e);
            }
            Log.Debug("get status result " + status);
            return status;
        }

        /// <summary>
        /// Returns user records
        /// UserRecords hold EventsWeddingsCount TransactionCount
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private static UserRecords GetRecords(Guid userId)
        {
            var records = new UserRecords {UserBalance = 0, TransactionsCount = 0, UserId =userId, EventsWeddingsCount =0, NotificationsCount =0 };
            IUsersRepository repository = new UsersRepository();
            IEventsRepository eventsRepository = new EventsRepository();
            ITransactionsRepository transactionsRepository = new TransactionsRepository();
            try
            {

                var user = repository.GetAllUsers().Where(x => x.UserId == userId).FirstOrDefault();
                records.UserBalance = user.Balance;

                var userEventsCount = 0;

                userEventsCount += eventsRepository.GetAllEvents()
                    .Where(x => x.CreatedBy == userId && !x.GoldClaimed).Count();
                userEventsCount += eventsRepository.GetAllWeddings()
                    .Where(x => x.CreatedBy == userId && !x.GoldClaimed).Count();

                records.EventsWeddingsCount = userEventsCount;

                var tCount = transactionsRepository.GetAllTransactions()
                    .Where(x => (x.Source == userId.ToString() || x.Destination == userId.ToString())
                            && x.TransactionType == "GOLD" && x.Confirmed).Count();
                records.TransactionsCount = tCount;
            } catch(Exception e)
            {
                Log.Error("Exception at GetRecords: " + e.Message);
                Log.Error(e.StackTrace);
            }


            return records;
        }

       

        private static Timer OnlineUserCheckerTimer;
        private static TimerState OnlineUserCheckerTimerState;
        private static Timer GoldPriceTakerTimer;
        private static TimerState GoldPriceTakerTimerState;

        private class TimerState
        {

            public int Counter;

        }

        private static void OnlineCheckerTimerTask(object timerState)
        {// @"{DateTime.Now:HH:mm:ss.fff}"
            Log.Information("Online checker start.");
            var state = timerState as TimerState;
            Interlocked.Increment(ref state.Counter);
           
           
            try
            {
                OnlineCheckerTimerAction();
            }
            catch (Exception e)
            {
                Log.Error("error at onlinechgeckerai: " + e.Message);
                Log.Error(e.StackTrace);
            }
        }

        private static void GoldPriceTakerTimerTask(object timerState)
        { 
            
            var state = timerState as TimerState;
            Interlocked.Increment(ref state.Counter);
            try
            {
                GoldPriceTakerTimerActionAsync();
               
            }
            catch (Exception e)
            {
                Log.Error("error at GoldPriceTakerAi: " + e.Message);
                Log.Error(e.StackTrace);
              
            }
            
        }


        public static void StartUp()
        {
            Log.Information("AI SERVICE STARTED");

            IUsersRepository repository = new UsersRepository();


            var onlineLogins = repository.GetAllLogins()
                .Where(x => !x.LogoutDateTime.HasValue)
                .ToList();
            foreach (var login in onlineLogins)
            {
                login.LogOut();
            }
            repository.SaveChanges();

            OnlineUserCheckerTimerState = new TimerState { Counter = 0 };
            GoldPriceTakerTimerState = new TimerState { Counter = 0 };

            GoldPriceTakerTimer = new Timer(
                callback: new TimerCallback(GoldPriceTakerTimerTask),
                state: GoldPriceTakerTimerState,
                dueTime: 0,
                period: 10000);


            var tim = new System.Threading.Timer(new TimerCallback(GoldPriceTakerTimerTask));

            OnlineUserCheckerTimer = new Timer(
                callback: new TimerCallback(OnlineCheckerTimerTask),
                state: OnlineUserCheckerTimerState,
                dueTime: 0,
                period: 10000);
            
        }


        private static bool EnoughTimePassedSinceLastCheck(Guid userId)
        {
            DateTime lastCheck = OnlineUsers[userId];
            DateTime now = DateTime.Now;

            TimeSpan diff = now.Subtract(lastCheck);

            return (diff.TotalSeconds > 300.0);
        }

        public static async void GoldPriceTakerTimerActionAsync()
        {
            try
            {

                

                var dd = await KTApiService.GetCurrentPriceFromKtApiASync();

                if (dd == null || dd.value == null || dd.value.FxRates == null)
                {
                    return;
                }

                var rates = dd.value.FxRates;
                foreach (var r in rates)
                {
                    if (r == null)
                    {
                        Log.Debug("goldprice taker early exit");
                        return;
                    }
                    Log.Debug("ARDA" + r.ToString());
                }
                var goldBuy = rates.Where(x => x.FxId == 24).FirstOrDefault().BuyRate;
                var goldSell = rates.Where(x => x.FxId == 24).FirstOrDefault().SellRate;

                var silverBuy = rates.Where(x => x.FxId == 26).FirstOrDefault().BuyRate;
                var silverSell = rates.Where(x => x.FxId == 26).FirstOrDefault().SellRate;

                var platinBuy = rates.Where(x => x.FxId == 27).FirstOrDefault().BuyRate;
                var platinSell = rates.Where(x => x.FxId == 27).FirstOrDefault().SellRate;

                // kt den gelen fiyat
                goldBuy = Math.Truncate(100 * goldBuy) / 100;
                goldSell = Math.Truncate(100 * goldSell) / 100;
                silverSell = Math.Truncate(100 * silverSell) / 100;
                silverBuy = Math.Truncate(100 * silverBuy) / 100;
                platinSell = Math.Truncate(100 * platinSell) / 100;
                platinBuy = Math.Truncate(100 * platinBuy) / 100;

                // db de ki ky_buy kt_sell ve update et
                var ktBuyPrice = await transactionsRepository.AccessKtBuyAsync();
                var ktSellPrice = await transactionsRepository.AccessKtSellAsync();
                ktBuyPrice.Amount = goldBuy;
                ktSellPrice.Amount = goldSell;

                // db de kt silver buy silver sell i bul ve uipdate et
                var ktBuySilver = await transactionsRepository.AccessKtSilverBuyAsync();
                var ktSellSilver = await transactionsRepository.AccessKtSilverSellASync();
                ktBuySilver.Amount = silverBuy;
                ktSellSilver.Amount = silverSell;

                //db deki platin

                var ktBuyPlatin = await transactionsRepository.AccessKtPlatinBuyAsync();
                var ktSellPlatin = await transactionsRepository .AccessKtPlatinSellAsync();
                ktBuyPlatin.Amount = platinBuy;
                ktSellPlatin.Amount = platinSell;

                // db de ki buy_percentage sell_percentage
                var buy_percentage = (await transactionsRepository.AccessBuyPercentageAsync()).Percentage.Value;
                var sell_pecentage = (await transactionsRepository.AccessSellPercentageAsync()).Percentage.Value;
                var buy_percent_silver = (await transactionsRepository.AccessBuyPercentageSilverAsync()).Percentage.Value;
                var sell_percent_silver = (await transactionsRepository.AccessSellPercentageSilverAsync()).Percentage.Value;
                var buy_percent_platin = (await transactionsRepository.AccessBuyPercentagePlatinAsync()).Percentage.Value;
                var sell_percent_platin = (await transactionsRepository .AccessSellPercentagePlatinAsync()).Percentage.Value;
                var automatic = (await transactionsRepository.AccessAutomaticAsync()).Amount.Value;

                if (((int)automatic) == 1)
                {
                    var fintagBuyPrice = await transactionsRepository.AccessBuyPriceAsync();
                    var fintagSellPrice = await transactionsRepository.AccessSellPriceAsync();

                    var fintagSilverSell = await transactionsRepository.AccessSilverSellAsync();
                    var fintagSilverBuy = await transactionsRepository .AccessSilverBuyAsync();

                    var fintagPlatinBuy = await transactionsRepository .AccessPlatinBuyAsync();
                    var fintagPlatinSell = await transactionsRepository .AccessPlatinSellAsync();

                    fintagBuyPrice.Amount = goldBuy * (1 + buy_percentage);
                    fintagSellPrice.Amount = goldSell * (1 - sell_pecentage); // 100 * (1-0.002) > 0.998

                    fintagSilverSell.Amount = silverSell * (1 - sell_percent_silver);
                    fintagSilverBuy.Amount = silverBuy * (1 + buy_percent_silver);

                    fintagPlatinSell.Amount = platinSell * (1 - sell_percent_platin);
                    fintagPlatinBuy.Amount = platinBuy * (1 + buy_percent_platin);



                    await GlobalGoldPrice.UpdateTaxExpensesCache();
                    await GlobalGoldPrice.SetPricesAsync(fintagBuyPrice.Amount.Value, fintagSellPrice.Amount.Value, 
                                fintagSilverBuy.Amount.Value, fintagSilverSell.Amount.Value,
                                fintagPlatinBuy.Amount.Value, fintagPlatinSell.Amount.Value);
                }

                await transactionsRepository.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Log.Error("Err reading goldpricetaker prices... " + e.Message + "\n" + e.StackTrace + "\n");
                if (e.InnerException != null)
                {
                    Exception inner = e.InnerException;

                    while (inner != null)
                    {
                        Log.Error("inner: " + inner.Message + "\n" + inner.StackTrace + "\n");
                        inner = inner.InnerException;
                    }
                }
            }
        }

        public static void GoldPriceTakerTimerAction()
        {
            try
            {
                
                ITransactionsRepository transactionsRepository = new TransactionsRepository();

                var dd = KTApiService.GetCurrentPriceFromKtApi();

                if (dd == null || dd.value == null || dd.value.FxRates == null)
                {
                    return;
                }

                var rates = dd.value.FxRates;
                foreach( var r in rates)
                {
                    Log.Debug("ARDA" + r.ToString());
                }
                var goldBuy = rates.Where(x => x.FxId == 24).FirstOrDefault().BuyRate;
                var goldSell = rates.Where(x => x.FxId == 24).FirstOrDefault().SellRate;

                var silverBuy = rates.Where(x => x.FxId == 26).FirstOrDefault().BuyRate;
                var silverSell = rates.Where(x => x.FxId == 26).FirstOrDefault().SellRate;

                var platinBuy = rates.Where(x => x.FxId == 27).FirstOrDefault().BuyRate;
                var platinSell = rates.Where(x => x.FxId == 27).FirstOrDefault().SellRate;

                // kt den gelen fiyat
                goldBuy = Math.Truncate(100 * goldBuy) / 100;
                goldSell = Math.Truncate(100 * goldSell) / 100;
                silverSell = Math.Truncate(100 * silverSell) / 100;
                silverBuy = Math.Truncate(100 * silverBuy) / 100;
                platinSell = Math.Truncate(100 * platinSell) / 100;
                platinBuy = Math.Truncate(100 * platinBuy) / 100;

                // db de ki ky_buy kt_sell ve update et
                var ktBuyPrice = transactionsRepository.AccessKtBuy();
                var ktSellPrice = transactionsRepository.AccessKtSell();
                ktBuyPrice.Amount = goldBuy;
                ktSellPrice.Amount = goldSell;

                // db de kt silver buy silver sell i bul ve uipdate et
                var ktBuySilver = transactionsRepository.AccessKtSilverBuy();
                var ktSellSilver = transactionsRepository.AccessKtSilverSell();
                ktBuySilver.Amount = silverBuy;
                ktSellSilver.Amount = silverSell;

                //db deki platin

                var ktBuyPlatin = transactionsRepository.AccessKtPlatinBuy();
                var ktSellPlatin = transactionsRepository.AccessKtPlatinSell();
                ktBuyPlatin.Amount = platinBuy;
                ktSellPlatin.Amount = platinSell;

                // db de ki buy_percentage sell_percentage
                var buy_percentage = transactionsRepository.AccessBuyPercentage().Percentage.Value;
                var sell_pecentage = transactionsRepository.AccessSellPercentage().Percentage.Value;
                var buy_percent_silver = transactionsRepository.AccessBuyPercentageSilver().Percentage.Value;
                var sell_percent_silver = transactionsRepository.AccessSellPercentageSilver().Percentage.Value;
                var buy_percent_platin = transactionsRepository.AccessBuyPercentagePlatin().Percentage.Value;
                var sell_percent_platin = transactionsRepository.AccessSellPercentagePlatin().Percentage.Value;
                var automatic = transactionsRepository.AccessAutomatic().Amount.Value;

                if (((int)automatic) == 1)
                {
                    var fintagBuyPrice = transactionsRepository.AccessBuyPrice();
                    var fintagSellPrice = transactionsRepository.AccessSellPrice();
                    var fintagSilverSell = transactionsRepository.AccessSilverSell();
                    var fintagSilverBuy = transactionsRepository.AccessSilverBuy();
                    var fintagPlatinBuy = transactionsRepository.AccessPlatinBuy();
                    var fintagPlatinSell = transactionsRepository.AccessPlatinSell();

                    fintagBuyPrice.Amount = goldBuy * (1 + buy_percentage);
                    fintagSellPrice.Amount = goldSell * (1 - sell_pecentage); // 100 * (1-0.002) > 0.998
                    
                    fintagSilverSell.Amount = silverSell * (1 - sell_percent_silver);
                    fintagSilverBuy.Amount = silverBuy * (1 + buy_percent_silver);

                    fintagPlatinSell.Amount = platinSell * (1 - sell_percent_platin);
                    fintagPlatinBuy.Amount = platinBuy * (1 + buy_percent_platin);



                    GlobalGoldPrice.UpdateTaxExpensesCache();
                    GlobalGoldPrice.SetPrices(fintagBuyPrice.Amount.Value, fintagSellPrice.Amount.Value, 
                        fintagSilverBuy.Amount.Value, fintagSilverSell.Amount.Value,
                        fintagPlatinBuy.Amount.Value, fintagPlatinSell.Amount.Value);
                }

                transactionsRepository.SaveChanges();

            }
            catch (Exception e)
            {
                Log.Error("Err reading goldpricetaker prices... " + e.Message + "\n" + e.StackTrace + "\n");
                if (e.InnerException != null)
                {
                    Exception inner = e.InnerException;

                    while (inner != null)
                    {
                        Log.Error("inner: " + inner.Message + "\n" + inner.StackTrace + "\n");
                        inner = inner.InnerException;
                    }
                }
            }
        }
        /*
        public static void OldGoldPriceTakerTimerAction()
        {

            try
            {
                ITransactionsRepository transactionsRepository = new TransactionsRepository();
                var factory = new HttpClientFactory();

                var client = factory.CreateHttpClient(new CreateHttpClientArgs());
                var responseBody = client.GetStringAsync("https://www.kuveytturk.com.tr/finans-portali/").Result;
                var silver = responseBody.Substring(responseBody.IndexOf("GMS (gr)")).Replace(" ", "").Replace("\r\n", "").Replace("GMS(gr)</td><td>", "").Split("</td><td>");
                var silverSell = decimal.Parse(silver[0].Replace(",", ".")); // 6.62
                var silverBuy = decimal.Parse(silver[1].Replace(",", ".")); // 6.66

                silverSell = Math.Truncate(100 * silverSell) / 100;
                silverBuy = Math.Truncate(100 * silverBuy) / 100;

                GlobalGoldPrice.RegisterSilverPricesByAiService(silverSell, silverBuy);

                var ssprice = transactionsRepository.AccessSilverSell();
                var sbprice = transactionsRepository.AccessSilverBuy();
                ssprice.Amount = silverSell;
                sbprice.Amount = silverBuy;


                var fiyats = responseBody
                    .Substring(responseBody.IndexOf("ALT (Gram Altın)"))
                    .Replace(" ", "")
                    .Replace("\r\n", "")
                    .Replace("ALT(GramAltın)</h2><divclass=\"alphabox\"><divclass=\"cellboxinsidebox\"><p>", "")
                    .Replace("<spanclass=\"light-text\">Alış</span></p></div><divclass=\"cellboxinsidebox\">", "")
                    .Substring(0, 19).Replace("<spa", "").Split("<p>");
                fiyats[0] = fiyats[0].Replace(",", ".");
                fiyats[1] = fiyats[1].Replace(",", ".");

                

                var buyPrice = transactionsRepository.AccessKtBuy();
                var sellPrice = transactionsRepository.AccessKtSell();

                buyPrice.Amount = decimal.Parse(fiyats[1]);
                buyPrice.Amount = Math.Truncate(100 * buyPrice.Amount.Value) / 100;
                sellPrice.Amount = decimal.Parse(fiyats[0]);
                sellPrice.Amount = Math.Truncate(100 * sellPrice.Amount.Value) / 100;


                var fintagBuyPrice = transactionsRepository.AccessBuyPrice();
                var fintagSellPrice = transactionsRepository.AccessSellPrice();

                var bpercentage = transactionsRepository.AccessBuyPercentage().Percentage.Value;
                var spercentage = transactionsRepository.AccessSellPercentage().Percentage.Value;

                var automatic = transactionsRepository.AccessAutomatic().Amount.Value;

                if (automatic == 1.00M)
                {
                    fintagBuyPrice.Amount = buyPrice.Amount.Value * (1 + bpercentage);
                    fintagSellPrice.Amount = sellPrice.Amount.Value * (1 - spercentage);
                    sbprice.Amount = sbprice.Amount.Value * (1 + bpercentage);
                    ssprice.Amount = ssprice.Amount.Value * (1 - spercentage);
                }


                transactionsRepository.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Error("Err reading goldpricetaker prices... " + e.Message + "\n" + e.StackTrace + "\n");
                if (e.InnerException != null)
                {
                    Exception inner = e.InnerException;

                    while (inner != null)
                    {
                        Log.Error("inner: " + inner.Message + "\n" + inner.StackTrace + "\n");
                        inner = inner.InnerException;
                    }
                }
            }
        }*/

        public static void OnlineCheckerTimerAction()
        {

            IUsersRepository repository = new UsersRepository();


            var onlineLogins = repository.GetAllLogins()
                .Where(x => !x.LogoutDateTime.HasValue)
                .ToList();

            var toBeRemoved = new List<Guid>();

            foreach (var login in onlineLogins)
            {
                if (OnlineUsers.ContainsKey(login.UserId))
                {
                    if (EnoughTimePassedSinceLastCheck(login.UserId))
                    {
                        login.LogOut();
                        toBeRemoved.Add(login.UserId);
                    }
                }
            }

            repository.SaveChanges();

            foreach (var userId in toBeRemoved)
            {
                OnlineUsers.Remove(userId);
            }
            Log.Information("online checker done - " + toBeRemoved.Count + " users made offline");
        }

    }
}
