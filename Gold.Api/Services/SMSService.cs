using Gold.Domain.Users.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using System.Net.Http;

namespace Gold.Api.Services
{
    public class SMSService
    {

        private static readonly string UserCode = "8503072490";
        private static readonly string Password = "X1YYXLZD";
        private static readonly string Header = "8503072490";
        private static readonly string URI = "https://api.netgsm.com.tr/sms/send/get/?usercode={0}&" +
            "password={1}&" +
            "gsmno={2}&" +
            "message={3}&" +
            "msgheader={4}";

        public async static void SendSms(string number, string message)
        {
            try
            {

                var url = string.Format(URI, UserCode, Password, number, message, Header);

                using (var client = new HttpClient())
                {
                    using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        var res = await client.SendAsync(req);

                        var response = await res.Content.ReadAsStringAsync();


                        var debug = string.Format("sms {0} {1} : {2}", number, message, response);
                        Log.Debug(debug);
                        
                    }
                   
                }
            }
            catch (Exception e)
            {
                Log.Error("Err: " + e.Message);
                Log.Error(e.StackTrace);
            }
        }

        public static void SendSMSToAllUsers(IUsersRepository usersRepository, string message)
        {
            
            var users = usersRepository.GetAllUsers()
                .Where(x => !x.Banned && x.Role == "Member" && x.AdminNotes != "TEMP_BAN")
                .ToList();

            foreach (var user in users)
            {
                var number = "0" + user.Phone;
                SendSms(number, message);
            }
        }
    }
}
