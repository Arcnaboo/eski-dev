using Gold.Api.Models.Vendors;
using Gold.Core.Vendors;
using Gold.Domain.Transactions.Interfaces;
using Gold.Domain.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Gold.Api.Services
{
    public class VendorGiftService
    {

        /*
         
         48 0
         57 9
         65 A
         90 Z
         97 a
         122 z
         */


        private static List<string> UsedCodes = new List<string>();
        private static List<VendorCreatorRequest> VendorCreatorRequests = new List<VendorCreatorRequest>();

        public static string RequestCreator()
        {
            string code = GenerateKey(6, MasterCreate);

            if (UsedCodes.Contains(code))
            {
                return RequestCreator();
            }
            var now = DateTime.Now;
            var valid = now.AddMinutes(2);
            var request = new VendorCreatorRequest { Code = code, CreatedDateTime = now, Used = false, ValidUntil = valid };
            VendorCreatorRequests.Add(request);
            UsedCodes.Add(code);
            return code;
        }

        public static bool ValidateCode(string code)
        {
            if (code == null || !UsedCodes.Contains(code))
            {
                return false;
            }
            var request = VendorCreatorRequests.Where(x => x.Code == code).FirstOrDefault();

            if (request == null || request.Used || DateTime.Now > request.ValidUntil)
            {
                return false;
            }

            return true;
        }

        public static void UseCode(string code)
        {
            if (code == null) return;

            var request = VendorCreatorRequests.Where(x => x.Code == code).FirstOrDefault();

            if (request == null) return;

            request.Used = true;
        }

        private static readonly string MasterBlast = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private static readonly string MasterCreate = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static bool ValidSecret(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret) || secret.Length != ComputeSha256Hash("asd").Length)
            {
                return false;
            }

            for (int i = 0; i < secret.Length; i++)
            {
                if (!char.IsLetter(secret[i]) && !char.IsDigit(secret[i]))
                {
                    return false;
                }
            }

            return true;
            /*
            bool lowerCase = false;
            bool upperCase = false;
            bool number = false;
            bool punctiation = false;

            for (int i = 0; i < secret.Length; i++)
            {
                char c = secret[i];
                if (char.IsDigit(c))
                {
                    number = true;
                }

                if (char.IsLetter(c) && char.IsLower(c))
                {
                    lowerCase = true;
                }

                if (char.IsLetter(c) && char.IsUpper(c))
                {
                    upperCase = true;
                }

                if (char.IsPunctuation(c))
                {
                    punctiation = true;
                }
            }

            return lowerCase && upperCase && number && punctiation;*/
        }

        private static string GenerateKey(int size, string keySet)
        {

            var random = new Random();

            string key = "";

            for (int i = 0; i < size; i++)
            {
                var index = random.Next(0, keySet.Length - 1);
                key += keySet[index];
            }

            return key;
        }

        public static Gcode GenerateGcode(IVendorsRepository repository, Guid vendorId)
        {
            string code = "";

            while (true)
            {
                code = GenerateKey(8, MasterBlast);

                bool status = repository.GetGcodes().Where(x => x.Code == code).Any();

                if (!status)
                {
                    break;
                }
            }

            Gcode gcode = new Gcode(code, vendorId);
            repository.AddGcode(gcode);
            repository.SaveChanges();
            gcode = repository.GetGcodes().Where(x => x.Code == code).FirstOrDefault();
            return gcode;
        }

        public static string GenerateApiKey(IVendorsRepository repository)
        {
            string format = "{0}-{1}-{2}-{3}";
            string key = "";
            
            while (true)
            {
                
                key = string.Format(format, GenerateKey(4, MasterCreate),
                    GenerateKey(4, MasterCreate),
                    GenerateKey(4, MasterCreate),
                    GenerateKey(4, MasterCreate));
                

                bool status = repository.GetVendors().Where(x => x.ApiKey == key).Any();

                if (!status)
                {
                    break;
                }
            }
            return key;
        }

        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
