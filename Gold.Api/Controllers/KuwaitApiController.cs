using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Gold.Api.Models.KuwaitTurk;
using System.Net.Http;
using Gold.Api.Utilities;
using Gold.Api.Models.KuwaitTurk.Api;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Serilog;

namespace Gold.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class KuwaitApiController : ControllerBase
    {


        // https://apitest.kuveytturk.com.tr/prep/v3/accounts/1/transactions?beginDate=2017-08-01&endDate=2017-10-01&itemCount=10 
        [AllowAnonymous]
        [HttpGet]
        [Route("get_token_signature")]
        public ActionResult<GetTokenSignatureResultHareketler> GetAccessTokenAndSignature(string data)
        {  /// https://www.fintag.org/kuwaitapi/get_token_signature?beginDate=2017-08-01&endDate=2021-05-14&itemCount=10


            try
            {
                //var data = string.Format("beginDate={0}&endDate={1}&itemCount={2}", beginDate, endDate, itemCount);
                var accessToken = KuwaitAccessHandler.AccessToken;
                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken + data);


                return Ok(new GetTokenSignatureResultHareketler { AccessToken = accessToken, Signature = signature });

            } 
            catch (Exception e)
            {
                Log.Error("Exceipt: " + e.Message);

                return Ok(new GetTokenSignatureResultHareketler { AccessToken = e.Message, Signature = e.StackTrace });
            }

        }

        private HttpResponseMessage SendAsync(HttpRequestMessage req, HttpClient httpClient)
        {
         
            return httpClient.SendAsync(req).Result;
        }

        /// <summary>
        /// Not Completed
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("get_atm_list")]
        public ActionResult<string> GetAtmList(string cityName)
        {
            var prepared = GetRequestClientCredentialsPrepare.PrepareGet(KuwaitAccessHandler.apiUrl + "/prep/v1/data/atms");

            string res = null;
            try
            {
                var response = SendAsync(prepared.Request, prepared.Client);
                res = response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Exception in GetAtmList()");
            }
            finally
            {
                GetRequestClientCredentialsPrepare.DisposeGetRequest(prepared);
            }

            return Ok(res);
        }

        /// <summary>
        /// Completed
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getgoldprices")]
        public ActionResult<FxRate> GetGoldPrices()
        {
            /*var accessToken = KuwaitAccessHandler.AccessToken;
            var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken);

            var httpClient = new HttpClient();

            var req = new HttpRequestMessage(HttpMethod.Get, KuwaitAccessHandler.apiUrl + "/prep/v1/fx/rates")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },

            };

            req.Headers.Add("Signature", signature);
            try
            {
                
                var response = SendAsync(req, httpClient);

                var responseString = response.Content.ReadAsStringAsync().Result;

                FxResultModel res = JsonConvert.DeserializeObject<FxResultModel>(responseString);
                var gold = res.FxRates.Where(x => x.Name == "Altın").FirstOrDefault();
                if (gold == null)
                {
                    return BadRequest(new FxResultModel
                    {
                        Success = false
                    });
                }
                

                return Ok(gold);
            } catch (Exception e)
            {
                Log.Fatal("GetGoldPrices() exception " + e.Message);
            }
            finally
            {
                req.Dispose();
                httpClient.Dispose();
            }
            return BadRequest();
            */

            return Ok(GlobalGoldPrice.GetCurrentPrice());
        }

        
        class Modl
        {
            public int AccountNumber;
            public int AccountSuffix;
            public int ProcessId;
            public bool IsPTTCollection;
        }
         
        [AllowAnonymous]
        [HttpGet]
        [Route("test")]
        public ActionResult<string> Testmethod()
        {

            var model = new Modl
            {
                AccountNumber = 8002577,
                AccountSuffix = 1,
                ProcessId = 421553,
                IsPTTCollection = false
            };

            

            var accessToken = KuwaitAccessHandler.AccessToken;
            var stringPayload = JsonConvert.SerializeObject(model);
            var sigContent = accessToken + stringPayload;
            var content = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var signature = KuwaitAccessHandler.SignSHA256RSA(sigContent);
            string responseString = null;
            using (var httpClient = new HttpClient())
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, KuwaitAccessHandler.apiUrl + "/prep/v1/collections")
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
                    Content = content
                };
                req.Headers.Add("Signature", signature);
                var response = SendAsync(req, httpClient);
                responseString = response.Content.ReadAsStringAsync().Result;
            }
            return Ok(responseString);
        }

        /// <summary>
        /// Completed
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("validateiban")]
        public ActionResult<IbanValidateResultModel> ValidateIban(IbanValidateParametersModel model)
        {
            var accessToken = KuwaitAccessHandler.AccessToken;
            var stringPayload = JsonConvert.SerializeObject(model);
            var sigContent = accessToken + stringPayload;
            var content = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var signature = KuwaitAccessHandler.SignSHA256RSA(sigContent);
            string responseString = null;
            IbanValidateResultModel result = null;
            using (var httpClient = new HttpClient())
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, KuwaitAccessHandler.apiUrl + "/prep/v1/validation/ibanvalidator")
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
                    Content = content
                };
                req.Headers.Add("Signature", signature);
                var response = SendAsync(req, httpClient);
                responseString = response.Content.ReadAsStringAsync().Result;

                result = JsonConvert.DeserializeObject<IbanValidateResultModel>(responseString);
            }
            return Ok(result);
        }
    }
}
 