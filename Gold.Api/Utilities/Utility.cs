using Gold.Api.Models.Users;
using Gold.Api.Services;
using Gold.Core.Transactions;
using Gold.Domain.Events.Interfaces;
using Gold.Domain.Transactions.Interfaces;
using Gold.Domain.Users.Interfaces;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gold.Api.Utilities
{
    public class Utility
    {
        public static string APP_VERSION = "2.0.0";
        private static readonly string[] _PhoneHeaders = {"501",
        "505", "506", "507", "551", "552", "553", "554", "555", "559",
        "530", "531", "532", "533", "534", "535", "536", "537", "538",
        "539", "561", "540", "541", "542", "543", "544", "545", "546",
        "547", "548", "549"};

        private static readonly string[] _Roles = { "Member", "UnregisteredUser" };



        /*
        public static void SendGoldBackToPeopleFromEvent(ITransactionsRepository repository, Guid eventOrWeddingId, string eventOrWeddingName)
        {
            var transactions = repository.GetAllTransactions().Where(x => x.Destination == eventOrWeddingId.ToString());

            if (transactions.Any())
            {
                foreach(var transaction in transactions)
                {
                    var user = repository
                        .GetAllUsers()
                        .Where(x => x.UserId.ToString() == transaction.Source)
                        .FirstOrDefault();

                    user.ManipulateBalance(transaction.GramAmount);

                    var message = string.Format("{0} isimli etkinliğe göndermiş oldunuz {1} miktar altın etkinliğin iptali sebebiyle hesabınıza geri iade edilmiştir.",
                                eventOrWeddingName, transaction.GramAmount);

                    var notification = new Notification(user.UserId, message, false, false, "info", null);

                    repository.AddNotification(notification);

                }

                repository.SaveChanges();
            }
        }            
        */

        /// <summary>
        /// name1 = Arda Ahmet Alemdar
        /// name2 = Arda Alemdar
        /// result == true
        /// 
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="name2"></param>
        /// <returns></returns>
        public static bool AlmosEqualsName(string name1, string name2)
        {
            if (name1 == null || name2 == null)
            {
                return false;
            }
            var set1 = new HashSet<string>();
            var set2 = new HashSet<string>();
            var parts1 = new List<string>(name1.Split(" "));

            var parts2 = new List<string>(name2.Split(" "));
            foreach(string p in parts1)
            {
                if (string.IsNullOrWhiteSpace(p))
                {
                    continue;
                }
                set1.Add(p.Trim());
            }
            foreach(string p in parts2)
            {
                if (string.IsNullOrWhiteSpace(p))
                {
                    continue;
                }
                set2.Add(p.Trim());
            }
            

            var intersection = set1.Intersect(set2);

            return intersection.Count() >= 2;
        }


        public static string MakeOneName(string firstName, string lastName)
        {
            if (lastName == "")
            {
                return firstName;
            }

            return firstName + " " + lastName;
        }

        public static bool AnyNull(params object[] list)
        {

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == null)
                {
                    return true;
                }
            }
            return false;
        }



        public static int GetSpecialCode(string description)
        {
            // descrip = ""description":"319163 - Gönderen:HÜSEYİN BAŞARAN, Alıcı:fintag a.ş, FAST Para Transferi",
            // descrip = "description":"Gönderen: Seher Güdül. Açıklama: 126072 - Para Transferi, Gönderen: PAPARA ELEKTRONİK PARA ANONİM ŞİRKETİ , Alıcı: FİNTAG YAZILIM DANIŞMANLIK ANONİM ŞİRKETİ",


            int specialCodeLength = 6;

            for (var i = 0; i < description.Length - specialCodeLength + 1; i++)
            {
                var builder = new StringBuilder();
                var skip = false;

                for (var j = i; j < i + specialCodeLength; j++)
                {
                    if (!char.IsDigit(description[j]))
                    {
                        i = j;
                        skip = true;
                        break;
                    }
                    builder.Append(description[j]);

                }
                if (skip)
                {
                    continue;
                }
                if (int.TryParse(builder.ToString(), out int specialCode))
                {
                    return specialCode;
                }
            }

            return -1; // error or unable to find special code in description
        }

        public static string ExtractVendorHavaleCode(string description)
        {
            var format = "XXXX-XXXX-XXXX-XXXX";
            //            01234-6789-1234-6789
            var len = format.Length;

            if (description.Length < len)
            {
                return "";
            }
            for (var i = 0; i < description.Length - len + 1; i++)
            {
                var builder = new StringBuilder();
                var dash = 0;
                var norm = 0;
                for (var j = i; j < i + len; j++)
                {
                    if (description[j] == '-')
                    {
                        dash++;
                    } else
                    {
                        norm++;
                    }


                    builder.Append(description[j]);
                }
                if (dash != 3 || norm != 16)
                {
                    continue;
                }
                var potentialCode = builder.ToString();
                Log.Debug("potential code: " + potentialCode);
                if (CanItBeACode(potentialCode))
                {
                    Log.Debug("returning: " + potentialCode);
                    return potentialCode;
                }
            }

            return "";

        }

        public static bool CanItBeACode(string codeString)
        {
            if (!codeString.StartsWith("FNTG"))
            {
                return false;
            }
            if (!codeString.Contains("SLV1") && !codeString.Contains("GLD1"))
            {
                return false;
            }
            if (!codeString.Contains("DAYI") && 
                !codeString.Contains("TEST") && 
                !codeString.Contains("AAAA") &&
                !codeString.Contains("BBBB") &&
                !codeString.Contains("CCCC") &&
                !codeString.Contains("DDDD") &&
                !codeString.Contains("EEEE") &&
                !codeString.Contains("FFFF") &&
                !codeString.Contains("GGGG") &&
                !codeString.Contains("HHHH") &&
                !codeString.Contains("JJJJ") &&
                !codeString.Contains("KKKK"))
            {
                return false;
            }
            var lastFour = codeString[16..];
            if (lastFour.StartsWith("0"))
            {
                lastFour = lastFour[1..];
            }
            if (!int.TryParse(lastFour, out _))
            {
                return false;
            }
            return true;
        }

        public static Guid GetGuid(string description)
        {
            var guidLen = Guid.Empty.ToString().Length;
            if (description.Length < guidLen)
            {
                return Guid.Empty;
            }
            for (var i = 0; i < description.Length - guidLen + 1; i++)
            {
                var builder = new StringBuilder();

                var skip = false;
                for (var j = i; j < i + guidLen; j++)
                {
                    if (!Uri.IsHexDigit(description[j]) && description[j] != '-' && !Char.IsDigit(description[j]))
                    {
                        i = j;
                        skip = true;
                        break;
                    }
                    builder.Append(description[j]);
                }
                if (skip)
                {
                    continue;
                }
                
                if (Guid.TryParse(builder.ToString(), out Guid guid))
                {
                    return guid;
                }
            }

            return Guid.Empty;
        }

        public static Dictionary<string, string> ParseJson(string json)
        {
            Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);


            return result;
        }

        public static bool ValidRole(string roleName)
        {
            foreach (string s in _Roles)
            {
                if (roleName == s)
                {
                    return true;
                }
            }
            return false;
        }

        public static int GenerateRandEventCode(IEventsRepository repository)
        {
            var rand = new Random();
            var ids = repository.GetAllEvents().Select(x => x.EventCode).ToList();
            int id = 0;
            bool ok = false;
            while (!ok)
            {
                id = rand.Next(100000, 999999);
                ok = (!ids.Contains(id));
            }
            return id;
        }

        public static int GenerateRandBankTransferCode(ITransactionsRepository repository)
        {
            var rand = new Random();
            var ids = repository.GetAllUserBankTransferRequests().Select(x => x.SpecialCode).ToList();
            int id = 0;
            bool ok = false;
            while (!ok)
            {
                id = rand.Next(100000, 999999);
                ok = (!ids.Contains(id));
            }
            return id;
        }

        public static int GenerateRandWeddingCode(IEventsRepository repository)
        {
            var rand = new Random();

            if (!repository.GetAllWeddings().Any())
            {
                return rand.Next(100000, 999999);
            }

            var ids = repository.GetAllWeddings().Select(x => x.WeddingCode).ToList();
            
            int id = 0;
            bool ok = false;
            while (!ok)
            {
                id = rand.Next(100000, 999999);
                ok = (!ids.Contains(id));
            }
            return id;
        }

        public static int GenerateRandForgotPasswordCode(IUsersRepository repository)
        {
            var rand = new Random();
            var ids = repository.GetAllForgotPasswords().Select(x => x.GeneratedCode).ToList();
            int id = 0;
            bool ok = false;
            while (!ok)
            {
                id = rand.Next(10000, 99999);
                ok = (!ids.Contains(id));
            }
            return id;
        }

        public static int GenerateRandMemberRef(IUsersRepository repository)
        {
            var rand = new Random();
            var codes = repository.GetReferansCodes().Select(x => x.ReferansKod).ToList();

            int code = 0;
            bool ok = false;

            while (!ok)
            {
                code = rand.Next(11111, 99999);
                ok = (!codes.Contains(code));
            }

            return code;
        }

        public static int GenerateRandMemberId(IUsersRepository repository)
        {
            var rand = new Random();
            var ids = repository.GetAllUsers().Select(x => x.MemberId).ToList();

            int id = 0;
            bool ok = false;
            while (!ok)
            {
                id = rand.Next(111111111, 999999999);
                ok = (!ids.Contains(id));
            }
            return id;
        }

        private static bool ValidPhoneBaslangic(string besBesBes)
        {
            foreach (string basla in _PhoneHeaders)
            {
                if (basla.Equals(besBesBes))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 0544 898 0702
        /// 5448980702
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static bool ValidPhone(string phone, IUsersRepository repository)
        {
            if (!phone.StartsWith("5") || phone.Length != 10) 
            {
                return false;
            }
            if (!ValidPhoneBaslangic(phone.Substring(0, 3)))
            {
                return false;
            }
            if (repository != null && repository.GetAllUsers().Where(x => x.Phone == phone).Any())
            {
                return false;
            }
            return true;
        }

        public static bool ValidateUser(CreateUserModel model, IUsersRepository repository, out string message)
        {

            if (model == null ) //| model.RoleName == null || !ValidRole(model.RoleName)) 
            {
                message = "Hatalı işlem.";
                return false;
            }

            if (model.FirstName == null || model.FamilyName == null || model.FirstName == "" || model.FamilyName == "")
            {
                message = "İsim soyisim gerekmektedir.";
                return false;
            }

            Log.Information("validate user: " + model.FirstName + " " + model.FamilyName);
            var ti = new CultureInfo("tr-TR", false).TextInfo;
            try
            { 
                model.FirstName = ti.ToUpper(model.FirstName);
                model.FamilyName = ti.ToUpper(model.FamilyName);
                Log.Information("validate user2: " + model.FirstName + " " + model.FamilyName);
            }
            catch (Exception e)
            {
                message = "Hata oluştu lütfen tekrar deneyiniz.";
                return false;
            }
            if (model.Email != null && repository.GetAllUsers().Where(usr => usr.Email == model.Email).Any())
            {
                message = "Bu email adresli başka bir hesap bulunmaktadır.";
                return false;
            }

            if (model.Phone == null || !ValidPhone(model.Phone, repository))
            {
                message = "Geçersiz telefon numarası.";
                return false;
            }
            
            DateTime birthDate;
            if (model.Birthdate == null || !DateTime.TryParse(model.Birthdate, out birthDate))
            {
                message = "Doğum tarihi gerekmektedir.";
                return false;
            }
            var difference = DateTime.Now - birthDate;
            if (difference.TotalDays < 18 * 365)
            {
                message = "Bu uygulamada 18 yaş sınırı vardır.";
                return false;
            }
            Log.Information("validate user2: " + model.FirstName + " " + model.FamilyName);
            var client = new Kimlik.KPSPublicSoapClient(Kimlik.KPSPublicSoapClient.EndpointConfiguration.KPSPublicSoap);
            
            var result = client.TCKimlikNoDogrulaAsync(long.Parse(model.TCK), model.FirstName, model.FamilyName, birthDate.Year).Result.Body.TCKimlikNoDogrulaResult;
            if (!result)
            {
                Log.Information("validate user4: " + model.FirstName.ToUpper() + " " + model.FamilyName.ToUpper());
                message = "Doğum yılı, isim soyisim ve TCK uyuşmuyor.";
                return false;
            }
            message = "Bilgiler doğru.";
            return true;
        }

        public static bool ValidateUser2(CreateUserModel model, IUsersRepository repository, out string message)
        {

            if (model == null) //| model.RoleName == null || !ValidRole(model.RoleName)) 
            {
                message = "Hatalı işlem.";
                return false;
            }

            var ti = new CultureInfo("tr-TR", false).TextInfo;
            try
            {
                if (model.FirstName != null)
                    model.FirstName = ti.ToUpper(model.FirstName);
                if (model.FamilyName != null)
                    model.FamilyName = ti.ToUpper(model.FamilyName);
                Log.Information("validate user2: " + model.FirstName + " " + model.FamilyName);
            }
            catch (Exception e)
            {
                message = "Hata oluştu lütfen tekrar deneyiniz.";
                return false;
            }
            if (model.Email != null && repository.GetAllUsers().Where(usr => usr.Email == model.Email).Any())
            {
                message = "Bu email adresli başka bir hesap bulunmaktadır.";
                return false;
            }

            if (model.Phone == null || !ValidPhone(model.Phone, repository))
            {
                message = "Geçersiz telefon numarası.";
                return false;
            }

            /* long kimlikNo;
             if (model.TCK == null || !long.TryParse(model.TCK, out kimlikNo))
             {
                 message = "TCK 11 haneli bir sayı olmalıdır.";
                 return false;
             }*/
            /*DateTime birthDate;
            if (model.Birthdate == null || !DateTime.TryParse(model.Birthdate, out birthDate))
            {
                message = "Doğum tarihi gerekmektedir.";
                return false;
            }
            var difference = DateTime.Now - birthDate;
            if (difference.TotalDays < 18 * 365)
            {
                message = "Bu uygulamada 18 yaş sınırı vardır.";
                return false;
            }*/
            /*Log.Information("validate user2: " + model.FirstName + " " + model.FamilyName);
            var client = new Kimlik.KPSPublicSoapClient(Kimlik.KPSPublicSoapClient.EndpointConfiguration.KPSPublicSoap);
            
            var result = client.TCKimlikNoDogrulaAsync(kimlikNo, model.FirstName, model.FamilyName, birthDate.Year).Result.Body.TCKimlikNoDogrulaResult;
            if (!result)
            {
                Log.Information("validate user4: " + model.FirstName.ToUpper() + " " + model.FamilyName.ToUpper());
                message = "Doğum yılı, isim soyisim ve TCK uyuşmuyor.";
                return false;
            }*/
            message = "Bilgiler doğru.";
            return true;
        }
    }
}
