using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Models.KuwaitTurk.Authrorization;
using Gold.Api.Robots;
using Gold.Api.Services;
using Gold.Domain;
using Gold.Domain.Transactions.Repositories;
using Gold.Domain.Vendors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gold.Api.Utilities
{
    public class KuwaitAccessHandler : IHostedService
    {

        #region private_keys
       /* private static readonly string PrivateKey = 
                            "MIICdwIBADANBgkqhkiG9w0BAQEFAASCAmEwggJdAgEAAoGBANRMWo1MOpaPs7vJ" +
                            "QiiYRLAiQz1DxS0gJA6wr6sGol56Xb+94FtU4ref6fwD86FMhosKN9l4iLjXMMqk" +
                            "1K0uBAVpRRCehqQPH4pwEOojdTlOzih8PG1iKRhPjgeD5HDN9Z12hjpu/bemlPTy" +
                            "ntyzMG1ZmQGiQMdoaYQenVHbDhlTAgMBAAECgYBFgqJ7dSQRvAdrSuBAjmqfCPjf" +
                            "DFt5BPcJYEyEQO3U5VfgufjFrqt02AUyoNCaVYYP7E6RA+gwLTUqhIGmGlTyH376" +
                            "Ruiz65CqPo8ZfWxOnQiywMJwFk2u4CpmdI3IJODLfF38Ps8Vwaqhr95koNT7e6mS" +
                            "rpPKGxCvj/L9qsO1AQJBAPwL40MKlyp8B63iajMXZITrb2Yc8ZESPLkdu2X21kdT" +
                            "mQKokdauaew9JGHkElu1J9WyUXZULEekLr4/FvRnQUECQQDXoNqiVrj46chMJrh7" +
                            "c8I1h2MlqnYa8MMh43DmBR8uynsGlz0eFIEjcbYM4A3+5FNrEtN7wY2pE6kGJQp3" +
                            "imGTAkEArFk8qAU/5Q82+RJP6Gvgknuji0HTdY3w8+x+znSBhfiGMqkuQIy3ZZFR" +
                            "pZadbxRrDteGmNFqDfsY84KUob9RgQJAOrW9Ub4zFvLwamuQh2x5UIHQaQ0Eo0ky" +
                            "mCOJNdfnKaJP5PeA2JPUpYXsf4zxwpkAbYLuuh91JrgHqXikZO/0qQJBAIqQiiHv" +
                            "oGIUEE5y3RZ1dOsvGPbaavXG/OxtyAvjl5tfBSt1zmHMQi46ZuFrHSr54uc4BDfD" +
                            "gDa5DmMqw0sTrhY=";*/
        // test
        //private static readonly string PrivateKey = "MIICdwIBADANBgkqhkiG9w0BAQEFAASCAmEwggJdAgEAAoGBANRMWo1MOpaPs7vJQiiYRLAiQz1DxS0gJA6wr6sGol56Xb+94FtU4ref6fwD86FMhosKN9l4iLjXMMqk1K0uBAVpRRCehqQPH4pwEOojdTlOzih8PG1iKRhPjgeD5HDN9Z12hjpu/bemlPTyntyzMG1ZmQGiQMdoaYQenVHbDhlTAgMBAAECgYBFgqJ7dSQRvAdrSuBAjmqfCPjfDFt5BPcJYEyEQO3U5VfgufjFrqt02AUyoNCaVYYP7E6RA+gwLTUqhIGmGlTyH376Ruiz65CqPo8ZfWxOnQiywMJwFk2u4CpmdI3IJODLfF38Ps8Vwaqhr95koNT7e6mSrpPKGxCvj/L9qsO1AQJBAPwL40MKlyp8B63iajMXZITrb2Yc8ZESPLkdu2X21kdTmQKokdauaew9JGHkElu1J9WyUXZULEekLr4/FvRnQUECQQDXoNqiVrj46chMJrh7c8I1h2MlqnYa8MMh43DmBR8uynsGlz0eFIEjcbYM4A3+5FNrEtN7wY2pE6kGJQp3imGTAkEArFk8qAU/5Q82+RJP6Gvgknuji0HTdY3w8+x+znSBhfiGMqkuQIy3ZZFRpZadbxRrDteGmNFqDfsY84KUob9RgQJAOrW9Ub4zFvLwamuQh2x5UIHQaQ0Eo0kymCOJNdfnKaJP5PeA2JPUpYXsf4zxwpkAbYLuuh91JrgHqXikZO/0qQJBAIqQiiHvoGIUEE5y3RZ1dOsvGPbaavXG/OxtyAvjl5tfBSt1zmHMQi46ZuFrHSr54uc4BDfDgDa5DmMqw0sTrhY=";
       
        // fintag private key
        private static readonly string PrivateKey = "MIICXgIBAAKBgQDX0gVNfg9WSNT/XOO1mburFMd+6KwrlCrxJp+90liTPJWEEnONeFz099Q1EJUIfxt40a7+IuFBC3HzbRlotFB1o0xr4/w1GfdlA53j8l6PGuAwkyP8tlKdrHz+TIogh8e99Z8R3FWfqAYwoNsRXWEMbxtrM3Li3L0GoGH3dqo4dQIDAQABAoGBAJ4g4YW/wVkFEBryYl0igKB/HxRbQhrD+xqppA0e24s1ro8QPd3/vIw4MSGYPrn095n0eHT39XcE/b7PxvgydWgCzSxHJ+rmAbCsfXS2f8n2KpnHQ5Qol1YhbLlSA1gDsjpXVMxQMFjRbQPJ0lLa1lGELT9d/eIKyisKiqG6EMsBAkEA+sUIeXPnV7jQBqqN/yDETQXHzHrCPXzv4S6uQXnOoHmS33U6LWCxkiZafkFZrSrddsaGJNxpgt16iR5jc/gy4QJBANxSYOMofRyxVzUnG3R7HFo5ZIL9D7a78i4ylfRT0IGq4UgtCw/CdU+Eg9CdVtgCVvaMAHmJ0TfxiTrARtHbjBUCQDBFEv+F3ucUzP4rgE+2t/J3hWEH1DHPxcDbonE6CBr9q9bVktA/R47xUQPygleB48VEK3gW8Txec4LHVa9x/CECQQCM01L+n4io6GnXnbGc2Hwc54Zpe5upr6RzBv52d4RU/YaV/cSORDt7ojYnCArUc1pmqMI87Wx/l7Ghjfk2dmCpAkEA+F0ElAJBosS+UkBSCKYmoZFXOUTD/PMXeoiRr6FCDj/Rf0LunLFKTMwn4vSt14BHspphQpNSQnm81krcg3IjCw==";
        #endregion
        /*
         GET için -> SHA256RSA(PrivateKey, AccessToken, QueryString) = SİGNATURE
            POST için -> SHA256RSA(PrivateKey, AccessToken, Request Body) = SİGNATURE

         
         */
        private static Timer ClienCredentialsAccessTimer;
        private static Timer AuthFlowAccessTimer;

        private static Timer TesterTimer;
        private static TimerState TesterTimerState;

        private static TimerState ClienCredentialsAccessTimerState;
        private static TimerState AuthFlowAccessTimerState;
        public static readonly string GTAGSUFFIX = "5";
        public static readonly string FINTAG = "fintag";
        public static readonly Guid PAYCELL_ID = Guid.Parse("089661D9-D1CF-49F7-6916-08D9D0DB4A3F");
        public static readonly Guid MUMTAZ_ID = Guid.Parse("C0FFD97A-0FBB-401A-CCED-08D9478FA7FC");
        public static readonly Guid MONEYPAY_ID = Guid.Parse("0F862546-5CEF-452F-E468-08D8E72951A4");
        #region private
        // bizim
        public static readonly string api_key = "b93aae4556594e9d9d80046c22774b14";
        public static readonly string api_secret = "c7TCwlcxIR0/usRN1QtKO3rjXW2AC03CguqqqRiTLU3Wglcg782VRQ==";

        // test
        //public static readonly string api_key = "81a7613772634017ae36e211c6af52ca";
        //public static readonly string api_secret = "lRfRg9Ybrdcs6MIrCUdELRsyVJHVvXuxu9ZjJzspGFq/9h1FWxLiVA==";
        #endregion

        // test
        //public static readonly string accessUrl = "https://idprep.kuveytturk.com.tr";

        // live
        public static readonly string accessUrl = "https://id.kuveytturk.com.tr";
        
        //test
        //public static readonly string apiUrl = "https://apitest.kuveytturk.com.tr";

        // live
        public static readonly string apiUrl = "https://api.kuveytturk.com.tr";


        public static readonly string scope = "public accounts";// transfers"; //"loans transfers public accounts offline_access";
        public static readonly string redirect_uri = "ktauth://callback";

        public static string AccessToken { get; set; }
        
        public static string AuthorizationCode { get; set; }

        private class TimerState
        {
            
            public int Counter;
            
        }

        public static int ARDA = 1;
        //public static IServiceProvider serviceProvider;

        public KuwaitAccessHandler(IExpectedChecker robot, ICashSender cashSenderRobot)
        {
            
            vendorExpectedChecker = robot;
            cashSender = cashSenderRobot;
        }


        private static void TesterFlow()
        {
            try
            {
                vendorExpectedChecker.Run();
                cashSender.Run();
            } catch (Exception e)
            {
                Log.Error("Tester: " + e.Message);
                Log.Error(e.StackTrace);
            }
           

        }

        private static void TesterTimerTask(object timerState)
        {
            var state = timerState as TimerState;
            Interlocked.Increment(ref state.Counter);
            TesterFlow();

        }

        private static void ClientCredentialsAccessTimerTask(object timerState)
        {// @"{DateTime.Now:HH:mm:ss.fff}"
            Log.Information("Starting a new access token request with client credentials.");
            var state = timerState as TimerState;
            Interlocked.Increment(ref state.Counter);

            GetAccessTokenWithClientCredentials();

        }

        private static void AuthFlowAccessTimerTask(object timerState)
        {// @"{DateTime.Now:HH:mm:ss.fff}"
            Log.Information("Starting a new access token request with auth flow.");
            var state = timerState as TimerState;
            Interlocked.Increment(ref state.Counter);
            Log.Information("Skipping request with auth flow, task not implemented yet, KuwaitAccessHandler.cs file");
            //GetAccessTokenWithAuthFlow();

        }

        public static Robots.ExpectedCashChecker robot;
        public static Robots.IExpectedChecker goldtagRobot;
        public static Robots.IExpectedChecker vendorExpectedChecker;
        public static Robots.ICashSender cashSender;

        public static async void StartUp()
        {
            
            //KuwaitAccessHandler.robot = new Robots.ExpectedCashChecker(new VendorsRepository(new ServiceCollection().BuildServiceProvider().GetService<VendorsDbContext>()));
            //KuwaitAccessHandler.goldtagRobot = new Robots.ExpectedCashCheckerGoldtag(new TransactionsRepository());
            //vendorExpectedChecker = new ServiceCollection().BuildServiceProvider().GetService<IRobot>(); // new Robots.VendorExpectedChecker(new VendorsRepository(new ServiceCollection().BuildServiceProvider().GetService<VendorsDbContext>()));
            
            //vendorExpectedChecker = new ServiceCollection().BuildServiceProvider().GetService<Robots.VendorExpectedChecker>();
            ClienCredentialsAccessTimerState = new TimerState { Counter = 0 };
            AuthFlowAccessTimerState = new TimerState { Counter = 0 };
            TesterTimerState = new TimerState { Counter = 0 };

            

            ClienCredentialsAccessTimer = new Timer(
                callback: new TimerCallback(ClientCredentialsAccessTimerTask),
                state: ClienCredentialsAccessTimerState,
                dueTime: 0,
                period: 2700000);

            await Task.Delay(1000);

            AuthFlowAccessTimer = new Timer(
                callback: new TimerCallback(AuthFlowAccessTimerTask),
                state: AuthFlowAccessTimerState,
                dueTime: 0,
                period: 2700000
                );

            //await Task.Delay(5000);
           // vendorExpectedChecker = new ServiceCollection().BuildServiceProvider().GetService<VendorExpectedChecker>();
            
            TesterTimer = new Timer(
                new TimerCallback(TesterTimerTask),
                TesterTimerState,
                0,
                5000);
        }


        public async static void GetAccessTokenWithAuthFlow()
        {
            var dict = new Dictionary<string, string>
            {
                { "client_id", api_key },
                { "scope", scope },
                { "response_type", "code" },
                { "state", "arcstate" },
                { "redirect_uri", redirect_uri }
            };
            using (var client = new HttpClient())
            {
                using (var req = new HttpRequestMessage(HttpMethod.Get, accessUrl + "/api/connect/authorize") {
                    Content = new FormUrlEncodedContent(dict)
                })
                {
                    string response;
                    var res = await client.SendAsync(req);
                    if (res.IsSuccessStatusCode)
                    {
                        response = await res.Content.ReadAsStringAsync();
                        Log.Information("response: " + response);
                    }
                    else
                    {
                        Log.Fatal("Auth Flow first request fail with code: " + res.StatusCode.ToString());
                    }
                    
                }
                
            }
        }

        public async static void GetAccessTokenWithClientCredentials()
        {
            Log.Debug("ARDA: GET ACCESS CLIENT CREDENTIALS");
            var dict = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "scope", scope },
                { "client_id", api_key },
                { "client_secret", api_secret }
            };
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, accessUrl + "/api/connect/token") 
            { 
                Content = new FormUrlEncodedContent(dict) 
            };

            var fail = false;
            try
            {
                var res = await client.SendAsync(req);
                var resString = await res.Content.ReadAsStringAsync();

                var resultModel = JsonConvert.DeserializeObject<AccessTokenResultModel>(resString);
                AccessToken = resultModel.AccessToken;

                Log.Debug("ARDA: " + AccessToken);
            } 
            catch (Exception e)
            { 
                Log.Debug("Error in GetAccessToken() nsleeping 10 seconds and trying again");
                Log.Debug(e.Message);
                fail = true;
                Thread.Sleep(10000);
                
            }
            finally
            {
                req.Dispose();
                client.Dispose();
               
            }

            if (fail)
            {
                Log.Fatal("GetAccesToken() failed, requesting again");
                GetAccessTokenWithClientCredentials();
            }

            
        }

        

        /// <summary>
        /// Signs the given input with sha256 rsa algorithm
        /// </summary>
        /// <param name="input">string to be signed</param>
        /// <returns></returns>
        public static string SignSHA256RSA(string input)
        {
            
            
            byte[] signedBytes;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {

                byte[] keyBytes = Convert.FromBase64String(PrivateKey);
                var encoder = new UTF8Encoding();
                byte[] originalData = encoder.GetBytes(input);
                try
                {
                    //// Import the private key used for signing the message
                    ///

                    // live da bizimkinde calisan
                    rsa.ImportRSAPrivateKey(keyBytes, out _);

                    //rsa.ImportPkcs8PrivateKey(keyBytes, out _);
                    //// Sign the data, using SHA256 as the hashing algorithm 
                    
                    //signedBytes = rsa.Si(originalData, CryptoConfig.MapNameToOID("SHA256"));
                    
                    
                    // live da calisan
                    signedBytes = rsa.SignData(originalData, CryptoConfig.MapNameToOID("SHA256"));
                }
                catch (CryptographicException e)
                {
                    Log.Error("error at sign: " + e.Message + "\n" + e.StackTrace);
                    return null;
                }
                finally
                {
                    //// Set the keycontainer to be cleared when rsa is garbage collected.
                    rsa.PersistKeyInCsp = false;
                }
                var result = Convert.ToBase64String(signedBytes);
                return result;
            }    
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartUp();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
