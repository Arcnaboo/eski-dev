using Gold.Api.Models.UtilityModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gold.Api.Services
{
    public class IBANValidatorService
    {

        private static string API_KEY = "7194c1da85c8878b827db095c73bed4c5681139b";

        public static bool ValidateIban(string iban)
        {
            IbanValidatorResultModel result = null;
            
            var httpClient = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Get, "https://api.ibanapi.com/v1/validate-basic?api_key=" + API_KEY + "&iban=" + iban);

            try
            {

                var response = SendAsync(req, httpClient);
                var responseString = response.Content.ReadAsStringAsync().Result;

                result = JsonConvert.DeserializeObject<IbanValidatorResultModel>(responseString);
            }
            catch (Exception e)
            {
                return false;

            }
            finally
            {
                req.Dispose();
                httpClient.Dispose();
            }

            return result.Result == 200;
        }

        private static HttpResponseMessage SendAsync(HttpRequestMessage req, HttpClient httpClient)
        {

            return httpClient.SendAsync(req).Result;
        }
    }
}
