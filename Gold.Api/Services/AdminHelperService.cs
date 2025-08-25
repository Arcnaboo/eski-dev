using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Services
{
    public class AdminHelperService
    {

        public static Dictionary<string, string> SMS_ADMIN = new Dictionary<string, string>();


        public static void StartAtStartUp()
        {
            if (SMS_ADMIN == null)
            {
                throw new Exception("SMS ADMIN SERVICE INVALID");
            }
        }


        public static string GenerateSMSCode(string token)
        {
            var rand = new Random();

            int x = rand.Next(111111, 999999);

            while (true)
            {
                if (!SMS_ADMIN.ContainsKey(x.ToString()))
                {
                    break;
                }
                x = rand.Next(111111, 999999);
            }

            SMS_ADMIN[x.ToString()] = token;

            return x.ToString();
        }


        public static string ValidateSMSCode(string code)
        {
            if (SMS_ADMIN.ContainsKey(code))
            {
                var result = SMS_ADMIN[code];

                SMS_ADMIN.Remove(code);

                return result;
            }

            return "INVALID";
        }
    }
}
