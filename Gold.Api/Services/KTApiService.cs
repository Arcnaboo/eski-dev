using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Utilities;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Gold.Api.Services.Interfaces;

namespace Gold.Api.Services
{
    public class KTApiService : BackgroundService, IKtApiService
    {
        public static IHttpClientFactory factory;
        private readonly IHttpClientFactory clientFactory;
        public KTApiService(IHttpClientFactory httpClientFactory)
        {
            factory = clientFactory = httpClientFactory;
        }
       
        public static async Task<AccountStatusResult> AccountStatusAsync()
        {

            try
            {
                var accessToken = KuwaitAccessHandler.AccessToken;

                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken);


                using (var client = new HttpClient())
                {
                    //https://apitest.kuveytturk.com.tr/test/v1/preciousmetal/buy

                    var req = new HttpRequestMessage(HttpMethod.Get,
                        KuwaitAccessHandler.apiUrl + "/prod/v3/accounts")
                    {
                        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                    };
                    req.Headers.Add("Signature", signature);
                    var response = await client.SendAsync(req);
                    var content = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<AccountStatusResult>(content);

                }
            }
            catch (Exception e)
            {
                Log.Error("Error at get accounts async: " + e.Message + "\n" + e.StackTrace);

                return new AccountStatusResult { Success = false, Reference = e.Message };
            }
        }

        public static async Task<string> GetReceiptAsync(TransactionReceiptParams model)
        {
            try
            {
                var accessToken = KuwaitAccessHandler.AccessToken;

                var body = JsonConvert.SerializeObject(model);

                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken + body);

                using (var client = new HttpClient())
                {   // https://api.kuveytturk.com.tr/prod/v3/accounts/transactions/receipts
                    //var req = new HttpRequestMessage(HttpMethod.Post, KuwaitAccessHandler.apiUrl + "/prod/v1/moneytransfer/interbankmoneytransfer")
                    var req = new HttpRequestMessage(HttpMethod.Post, 
                        KuwaitAccessHandler.apiUrl + "/prod/v3/accounts/transactions/receipts")
                    {
                        Headers = { 
                            Authorization = new AuthenticationHeaderValue("Bearer", accessToken)
                        }
                    };

                    req.Content = new JsonContent(model);
                    req.Headers.Add("Signature", signature);
                    var response = await client.SendAsync(req);
                    var content = await response.Content.ReadAsStringAsync();

                    //var result = JsonConvert.DeserializeObject<MetalBuyResult>(content);

                    //return result;

                    return content;
                }
            }
            catch (Exception e)
            {
                Log.Error("error at receipts kt: " + e.Message);
                Log.Error(e.StackTrace);
                return e.Message;
            }
        }

        public static async  Task<MetalBuyResult> PreciousMetalBuyAsync(PreciousMetalsBuyParams model)
        {

            try
            {
                var accessToken = KuwaitAccessHandler.AccessToken;

                var body = JsonConvert.SerializeObject(model);

                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken + body);

                using (var client = new HttpClient())
                {
                    //https://apitest.kuveytturk.com.tr/test/v1/preciousmetal/buy

                    var req = new HttpRequestMessage(HttpMethod.Post,
                        KuwaitAccessHandler.apiUrl + "/prod/v1/preciousmetal/buy")
                    {
                        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                    };

                    req.Content = new JsonContent(model);
                    req.Headers.Add("Signature", signature);

                    var response = await client.SendAsync(req);

                    var content = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<MetalBuyResult>(content);

                    return result;
                }
            }
            catch (Exception e)
            {
                return new MetalBuyResult { Success = false, RefId = e.Message };
            }
        }

        public static async Task<MetalSellResult> PreciousMetalSellAsync(PreciousMetalsSellParams model)
        {

            try
            {
                var accessToken = KuwaitAccessHandler.AccessToken;

                var body = JsonConvert.SerializeObject(model);

                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken + body);


                using (var client = new HttpClient())
                {
                    var req = new HttpRequestMessage(HttpMethod.Post,
                        KuwaitAccessHandler.apiUrl + "/prod/v1/preciousmetal/sell")
                    {
                        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                    };

                    req.Content = new JsonContent(model);
                    req.Headers.Add("Signature", signature);

                    var response = await client.SendAsync(req);
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<MetalSellResult>(content);

                    return result;
                }
            }
            catch (Exception e)
            {
                return new MetalSellResult { Success = false, RefId = e.Message };
            }
        }

        public static async Task<InterBanTransferResult> InterBankTransferAsync(InterBankTransferParams model)
        {

            try
            {
                var accessToken = KuwaitAccessHandler.AccessToken;

                var body = JsonConvert.SerializeObject(model);

                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken + body);


                using (var client = new HttpClient())
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, KuwaitAccessHandler.apiUrl + "/prod/v1/moneytransfer/interbankmoneytransfer")
                    {
                        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
                    };

                    req.Headers.Add("Signature", signature);
                    req.Content = new JsonContent(model);
                    var response = await client.SendAsync(req);

                    var content = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<InterBanTransferResult>(content);

                    return result;
                }
            }
            catch (Exception e)
            {
                Log.Error("Error at interbanktransfer: " + e.Message);
                Log.Error(e.StackTrace);

                return new InterBanTransferResult { Success = false, RefId = e.Message };
            }
        }

        public static async Task<KTTransactionResultsModel> GetHesapHareketleriASync(string suffix, string queryString)
        {

            // Log.Debug(string.Format("hesap hareketleri s{0}s #{1}#", suffix, queryString));
            KTTransactionResultsModel res = null;

            var accessToken = KuwaitAccessHandler.AccessToken;

            var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken + "?" + queryString);
            //Log.Debug(string.Format("signature {0}", signature));

            var httpClient = new HttpClient();

            var req = new HttpRequestMessage(HttpMethod.Get,
                KuwaitAccessHandler.apiUrl + "/prod/v3/accounts/" + suffix + "/transactions?" + queryString)
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },

            };

            req.Headers.Add("Signature", signature);
            try
            {
                //var response = SendAsync(req, httpClient);

                var response = await httpClient.SendAsync(req);

                //var responseString = response.Content.ReadAsStringAsync().Result;

                var responseString = await response.Content.ReadAsStringAsync();

                Log.Debug("KT_HAREKETLER: " + responseString);
                res = JsonConvert.DeserializeObject<KTTransactionResultsModel>(responseString);

            }
            catch (Exception e)
            {
                Log.Fatal("GetHesapHareketleriAsync() exception " + e.Message + "\n" + e.StackTrace);

                Exception inner = e.InnerException;

                while (inner != null)
                {
                    Log.Fatal("inner- " + e.Message + "\n" + e.StackTrace);
                    inner = inner.InnerException;
                }

            }
            finally
            {
                req.Dispose();
                httpClient.Dispose();
            }
            return res;
        }

        public static async Task<FxResultModel> GetCurrentPriceFromKtApiASync()
        {
            FxResultModel result;

            var accessToken = KuwaitAccessHandler.AccessToken;
            var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken);
            var httpClient = new HttpClient();

            var req = new HttpRequestMessage(HttpMethod.Get, KuwaitAccessHandler.apiUrl + "/prod/v1/preciousmetal/rates")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },

            };

            req.Headers.Add("Signature", signature);
            try
            {
                var response = await httpClient.SendAsync(req);
                var responseString = await response.Content.ReadAsStringAsync();
                Log.Debug("PRICE_LOG: " + responseString);
                result = JsonConvert.DeserializeObject<FxResultModel>(responseString);

            }
            catch (Exception e)
            {
                result = null;
                Log.Error("GetCurrentPriceFromKtApi() exception " + e.Message);
                Log.Error(e.StackTrace);

                e = e.InnerException;
                while (e != null)
                {
                    Log.Error("ie: " + e.Message);
                    Log.Error(e.StackTrace);
                    e = e.InnerException;
                }
            }
            finally
            {
                req.Dispose();
                httpClient.Dispose();
            }
            return result;

        }

        public static async Task<string> GetPaymentTypesForIbanAsync(GetPaymentTypesParams model)
        {
            try
            {
                var accessToken = KuwaitAccessHandler.AccessToken;

                var body = JsonConvert.SerializeObject(model);

                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken + body);

                using (var client = new HttpClient())
                {   //liveda /prod/v1/
                    //https://apitest.kuveytturk.com.tr/prep/v1/moneytransfer/paymenttype
                    var req = new HttpRequestMessage(HttpMethod.Post,
                        KuwaitAccessHandler.apiUrl + "/prep/v1/moneytransfer/paymenttype")
                    {
                        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                    };

                    req.Content = new JsonContent(model);
                    req.Headers.Add("Signature", signature);
;
                    var response = await client.SendAsync(req);

                    var content = await response.Content.ReadAsStringAsync();

                    return content;
                }
            }
            catch (Exception e)
            {
                return e.Message + "\n" + e.StackTrace;
            }
        }




        public static AccountStatusResult AccountStatus()
        {

            try
            {
                var accessToken = KuwaitAccessHandler.AccessToken;



                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken);


                using (var client = new HttpClient())
                {
                    //https://apitest.kuveytturk.com.tr/test/v1/preciousmetal/buy

                    var req = new HttpRequestMessage(HttpMethod.Get,
                        KuwaitAccessHandler.apiUrl + "/prod/v3/accounts")
                    {
                        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                    };
                    req.Headers.Add("Signature", signature);

                    var response = SendAsync(req, client);
                    var task = response.Content.ReadAsStringAsync();

                    task.Wait();

                    var content = task.Result;

                    return JsonConvert.DeserializeObject<AccountStatusResult>(content);


                }
            }
            catch (Exception e)
            {
                Log.Error("Error at get accounts: " + e.Message + "\n" + e.StackTrace);

                return new AccountStatusResult { Success = false, Reference = e.Message };
            }
        }

        public static MetalBuyResult PreciousMetalBuy(PreciousMetalsBuyParams model)
        {

            try
            {
                var accessToken = KuwaitAccessHandler.AccessToken;

                var body = JsonConvert.SerializeObject(model);

                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken + body);


                using (var client = new HttpClient())
                {
                    //https://apitest.kuveytturk.com.tr/test/v1/preciousmetal/buy

                    var req = new HttpRequestMessage(HttpMethod.Post,
                        KuwaitAccessHandler.apiUrl + "/prod/v1/preciousmetal/buy")
                    {
                        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                    };

                    req.Content = new JsonContent(model);
                    req.Headers.Add("Signature", signature);

                    var response = SendAsync(req, client);

                    var task = response.Content.ReadAsStringAsync();

                    task.Wait();

                    var content = task.Result;

                    var result = JsonConvert.DeserializeObject<MetalBuyResult>(content);

                    return result;
                }
            }
            catch (Exception e)
            {
                return new MetalBuyResult { Success = false, RefId = e.Message };
            }
        }

        public static MetalSellResult PreciousMetalSell(PreciousMetalsSellParams model)
        {

            try
            {
                var accessToken = KuwaitAccessHandler.AccessToken;

                var body = JsonConvert.SerializeObject(model);

                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken + body);


                using (var client = new HttpClient())
                {
                    //https://apitest.kuveytturk.com.tr/test/v1/preciousmetal/buy

                    var req = new HttpRequestMessage(HttpMethod.Post,
                        KuwaitAccessHandler.apiUrl + "/prod/v1/preciousmetal/sell")
                    {
                        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                    };

                    req.Content = new JsonContent(model);
                    req.Headers.Add("Signature", signature);

                    var response = SendAsync(req, client);
                    var task = response.Content.ReadAsStringAsync();

                    task.Wait();

                    var content = task.Result;
                    var result = JsonConvert.DeserializeObject<MetalSellResult>(content);

                    return result;
                }
            }
            catch (Exception e)
            {
                return new MetalSellResult { Success = false, RefId = e.Message };
            }
        }

        public static InterBanTransferResult InterBankTransfer(InterBankTransferParams model)
        {

            try
            {
                var accessToken = KuwaitAccessHandler.AccessToken;

                var body = JsonConvert.SerializeObject(model);

                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken + body);


                using (var client = new HttpClient())
                {


                    var req = new HttpRequestMessage(HttpMethod.Post, KuwaitAccessHandler.apiUrl + "/prod/v1/moneytransfer/interbankmoneytransfer") 
                    {
                        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },

                    };

                    req.Headers.Add("Signature", signature);
                    req.Content = new JsonContent(model);
                    var response = SendAsync(req, client);

                    var task = response.Content.ReadAsStringAsync();

                    task.Wait();

                    var content = task.Result;

                    var result = JsonConvert.DeserializeObject<InterBanTransferResult>(content);

                    return result;
                }
            }
            catch (Exception e)
            {
                Log.Error("Error at interbanktransfer: " + e.Message);
                Log.Error(e.StackTrace);

                return new InterBanTransferResult { Success = false, RefId = e.Message };
            }
        }

        public static KTTransactionResultsModel GetHesapHareketleri(string suffix, string queryString)
        {

            Log.Debug(string.Format("hesap hareketleri s{0}s #{1}#", suffix, queryString));
            KTTransactionResultsModel res = null;
            
            var accessToken = KuwaitAccessHandler.AccessToken;

            var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken + "?" + queryString);
            Log.Debug(string.Format("signature {0}", signature));

            var httpClient = new HttpClient();
            // https://api.kuveytturk.com.tr/prod/v3/accounts/5/transactions
            var req = new HttpRequestMessage(HttpMethod.Get,
                KuwaitAccessHandler.apiUrl + "/prod/v3/accounts/" + suffix + "/transactions?" + queryString)  
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },

            };

            req.Headers.Add("Signature", signature);
            try
            {
                var response = SendAsync(req, httpClient);
                
                var responseString = response.Content.ReadAsStringAsync().Result;

                Log.Debug("response = " + responseString);
                res = JsonConvert.DeserializeObject<KTTransactionResultsModel>(responseString);            
                
            }
            catch (Exception e)
            {
                Log.Fatal("GetHesapHareketleri() exception " + e.Message + "\n"+ e.StackTrace);

                Exception inner = e.InnerException;

                while (inner != null)
                {
                    Log.Fatal("inner- " + e.Message + "\n" + e.StackTrace);
                    inner = inner.InnerException;
                }
                    
            }
            finally
            {
                req.Dispose();
                httpClient.Dispose();
            }
            return res;
        }

        public static FxResultModel GetCurrentPriceFromKtApi()
        {
            FxResultModel result;

            var accessToken = KuwaitAccessHandler.AccessToken;


            var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken);

            var httpClient = new HttpClient();

            var req = new HttpRequestMessage(HttpMethod.Get, KuwaitAccessHandler.apiUrl + "/prod/v1/preciousmetal/rates")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },

            };

            req.Headers.Add("Signature", signature);
            try
            {

                var response = SendAsync(req, httpClient);

                var responseString = response.Content.ReadAsStringAsync().Result;

                FxResultModel res = JsonConvert.DeserializeObject<FxResultModel>(responseString);
                //result = res.FxRates.Where(x => x.Name == "Altın").FirstOrDefault();
                result = res;
                //Log.Debug("ARDA:" + res);
            }
            catch (Exception e)
            {
                Log.Error("GetCurrentPriceFromKtApi() exception " + e.Message);
                result = null;

                Log.Error(e.StackTrace);

                e = e.InnerException;
                while (e != null)
                {
                    Log.Error("ie: " + e.Message);
                    Log.Error(e.StackTrace);
                    e = e.InnerException;
                }
            }
            finally
            {
                req.Dispose();
                httpClient.Dispose();
            }
            return result;

        }
        
        public static string GetPaymentTypesForIBAN(GetPaymentTypesParams model)
        {
            try
            {
                var accessToken = KuwaitAccessHandler.AccessToken;

                var body = JsonConvert.SerializeObject(model);

                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken + body);

                Log.Debug("ARDA: " + accessToken);
                Log.Debug("ARDA: " + body);
                Log.Debug("ARDA: " + signature);
                using (var client = new HttpClient())
                {   //liveda /prod/v1/
                    //https://apitest.kuveytturk.com.tr/prep/v1/moneytransfer/paymenttype
                    var req = new HttpRequestMessage(HttpMethod.Post,
                        KuwaitAccessHandler.apiUrl + "/prep/v1/moneytransfer/paymenttype")
                    {
                        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                    };

                    req.Content = new JsonContent(model);
                    req.Headers.Add("Signature", signature);

                    var requestTask = client.SendAsync(req);
                    requestTask.Wait();

                    //var response = SendAsync(req, client);

                    var task = requestTask.Result.Content.ReadAsStringAsync();

                    task.Wait();

                    var content = task.Result;

                    Log.Debug("ARDA: " + content);

                    //var result = JsonConvert.DeserializeObject<MetalBuyResult>(content);

                    return content;
                }
            }
            catch (Exception e)
            {
                return e.Message + "\n" + e.StackTrace;
            }
        }

        private static HttpResponseMessage SendAsync(HttpRequestMessage req, HttpClient httpClient)
        {

            return httpClient.SendAsync(req).Result;
           // return task.Result;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        Task<AccountStatusResult> IKtApiService.GetAccStatus()
        {
            throw new NotImplementedException();
        }

        Task<MetalBuyResult> IKtApiService.MetalBuy(PreciousMetalsBuyParams model)
        {
            throw new NotImplementedException();
        }

        Task<MetalSellResult> IKtApiService.MetalSell(PreciousMetalsSellParams model)
        {
            throw new NotImplementedException();
        }

        Task<InterBanTransferResult> IKtApiService.KTHaveleTransfer(InterBankTransferParams model)
        {
            throw new NotImplementedException();
        }

        Task<KTTransactionResultsModel> IKtApiService.KTHesapHareketleri(string suffix, string queryString)
        {
            throw new NotImplementedException();
        }

        async Task<FxResultModel> IKtApiService.KTCurrentPrices()
        {
            FxResultModel result;
            try
            {
                var accessToken = KuwaitAccessHandler.AccessToken;
                var signature = KuwaitAccessHandler.SignSHA256RSA(accessToken);
                var httpClient = clientFactory.CreateClient();

                using (var req = new HttpRequestMessage(HttpMethod.Get, KuwaitAccessHandler.apiUrl + "/prod/v1/preciousmetal/rates")
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },

                })
                {
                    req.Headers.Add("Signature", signature);
                    var response = await httpClient.SendAsync(req);
                    var responseString = await response.Content.ReadAsStringAsync();

                    result = JsonConvert.DeserializeObject<FxResultModel>(responseString);
                }

            }
            catch (Exception e)
            {
                result = null;
                Log.Error("GetCurrentPriceFromKtApi() exception " + e.Message);
                Log.Error(e.StackTrace);

                e = e.InnerException;
                while (e != null)
                {
                    Log.Error("ie: " + e.Message);
                    Log.Error(e.StackTrace);
                    e = e.InnerException;
                }
            }
            
            return result;
        }
    }
}
