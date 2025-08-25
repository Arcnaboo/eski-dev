using Gold.Api.Utilities;
using Gold.Core.Transactions;
using Gold.Domain.Transactions.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Services
{
    public class KampanyaService
    {
        public static int KampDolunayVerification = 10;


        public static int GetPassword()
        {
            return KampDolunayVerification;
        }

        public static void ResetPassword()
        {
            var rand = new Random();

            KampDolunayVerification = rand.Next(1000, 9999);
        }

        public class IllegalAccessException: Exception
        {
            public IllegalAccessException(string message): base(message)
            {
                
            }
        }

        public static void KasimSilverKampanyasiExecute(ITransactionsRepository transactionsRepository, int code)
        {
            try
            {
                if (code != KampDolunayVerification)
                {
                    throw new IllegalAccessException("Invalid verification code: " + code);
                }
                var silverPrices = GlobalGoldPrice.GetSilverPrices();
                var _from = DateTime.ParseExact("2021-11-20", "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var _till = DateTime.ParseExact("2021-11-30", "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var users = transactionsRepository
                    .GetAllUsers()
                    .Where(x => x.DateCreated.Date >= _from.Date && x.DateCreated.Date <= _till.Date)
                    .ToList();
                var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";

                var text = "";

                foreach (var usr in users)
                {
                    var silverBalance = transactionsRepository.GetSilverBalance(usr.UserId);
                    if (silverBalance == null)
                    {
                        silverBalance = new SilverBalance(usr.UserId);
                        transactionsRepository.AddSilverBalance(silverBalance);
                    }
                    silverBalance.Balance += 1.0m;
                    var silverTransaction = new Transaction(
                        "SILVER",
                        "D94AF0AD-5FE9-4A6C-949E-C66AEF21C907",
                        "Fintag",
                        usr.UserId.ToString(),
                        "User",
                        1,
                        "Goldtag Hediye Gümüş",
                        true,
                        1,
                        silverPrices.BuyRate);
                    transactionsRepository.AddTransaction(silverTransaction);
                    var message = "Sayın üyemiz hediye 1gr Gümüş hesabınıza aktarılmıştır. Goldtag ailesi olarak size sağlık ve afiyet dileriz.";
                    var noti = new Notification2(usr.UserId, message, false, "info", null, photo);
                    transactionsRepository.AddNotification(noti);
                    transactionsRepository.SaveChanges();
                    text += "" + usr.UserId.ToString() + "\n";
                }

                System.IO.File.WriteAllText("C:\arc.txt", text);
            } 
            catch (Exception e)
            {
                Log.Error("Error at kampanya execute: " + e.Message);
                Log.Error("stacktrace: " + e.StackTrace);
                Exception exception = e.InnerException;
                while (exception != null)
                {
                    Log.Error("inner exception: " + exception.Message);
                    Log.Error("inner stacktrace: " + exception.StackTrace);
                    exception = exception.InnerException;
                }

                
            }
        }
    }
}
