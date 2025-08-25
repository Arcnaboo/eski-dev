using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using System.DirectoryServices.Protocols;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Gold.Api.Models.SiPay;

namespace Gold.Api.Utilities
{
    public class SiPay
    {
        /*
         {          "pos_id", pos.Data[0].PosId.ToString() },
                    { "cc_holder_name", pay3D.HolderName },
                    { "cc_no", pay3D.CreditCard },
                    { "expiry_month", pay3D.ExpiryMonth },
                    { "expiry_year", pay3D.ExpiryYear },
                    { "cvv", pay3D.CVV },
                    { "currency_id", pos.Data[0].CurrencyId.ToString() },
                    { "currency_code", pos.Data[0].CurrencyCode },
                    { "campaign_id", pos.Data[0].CampaignId.ToString() },
                    { "allocation_id", pos.Data[0].AllocationId.ToString() },
                    { "installments_number", pos.Data[0].InstallmentsNumber.ToString() },
                    { "invoice_id", pay3D.Transaction.TransactionId.ToString() },
                    { "invoice_description", pay3D.Transaction.Comment },
                    { "name", pay3D.User.FirstName },
                    { "surname", pay3D.User.FamilyName },
                    { "total", pay3D.Amount },
                    { "discount", "0" },
                    { "coupon", "0" },
                    { "merchant_key", MerchantKey },
                    { "payable_amount", pos.Data[0].AmountToBePaid.ToString() },
                    { "items", itemsAsJson },
                    { "app_id", AppId },
                    { "app_secret", AppSecret },
                    { "cancel_url", FailRedirect },
                    { "return_url", SuccRedirect }
             */


        private readonly string HtmlFormFormat = "<form id=\"the-form\"" +
            " action=\"https://app.sipay.com.tr/ccpayment/api/pay3d\" class=\"payment_3d\" method=\"post\">" +
            "<input type=\"hidden\" name=\"Authorization\"" +
            "value=\"Bearer {0}\">" +
            "<input type=\"hidden\" name=\"sipay_3d\" value=\"2\">" +
            "<input type=\"hidden\" name=\"cc_holder_name\" value=\"{1}\">" +
            "<input type=\"hidden\" name=\"cc_no\" value=\"{2}\">" +
            "<input type=\"hidden\" name=\"expiry_month\" value=\"{3}\">" + // MM
            "<input type=\"hidden\" name=\"expiry_year\" value=\"{4}\">" + // YYYY
            "<input type=\"hidden\" name=\"cvv\" value=\"{5}\">" +
            "<input type=\"hidden\" name=\"pos_id\" value=\"{6}\">" +
            "<input type=\"hidden\" name=\"pos_amount\" value=\"{7}\">" +
            "<input type=\"hidden\" name=\"currency_id\" value=\"{8}\">" +
            "<input type=\"hidden\" name=\"currency_code\" value=\"{9}\">" +
            "<input type=\"hidden\" name=\"campaign_id\" value=\"{10}\">" +
            "<input type=\"hidden\" name=\"allocation_id\" value=\"{11}\">" +
            "<input type=\"hidden\" name=\"installments_number\" value=\"{12}\">" +
            "<input type=\"hidden\" name=\"invoice_id\" value=\"{13}\">" +
            "<input type=\"hidden\" name=\"invoice_description\" value=\"{14}\">" +
            "<input type=\"hidden\" name=\"total\" value=\"{15}\">" +
            "<input type=\"hidden\" name=\"merchant_key\" value=\"{16}\">" +
            "<input type=\"hidden\" name=\"payable_amount\" value=\"{17}\">" +
            "<input type=\"hidden\" name=\"return_url\" value=\"{18}\" > " +
            "<input type=\"hidden\" name=\"cancel_url\" value=\"{19}\">" +
            "<input type=\"hidden\" name=\"items\" value=\"{20}\">" +
            "<input type=\"hidden\" name=\"name\" value=\"{21}\">" +
            "<input type=\"hidden\" name=\"surname\" value=\"{22}\">" +
            "<input type=\"hidden\" name=\"app_id\" value=\"{23}\">" +
            "<input type=\"hidden\" name=\"app_secret\" value=\"{24}\">" +
            "<input type=\"submit\" value=\"submit\">" +
            "</form>" +
            "<script type=\"text/javascript\">" +
            "window.onload = function(){{" +
            "document.getElementById(\"the-form\").submit();" +
            "}}" +
            "</script>";




        // TODO
        // https://temp.nabooo.com/callback/redir // nrmal geri dönüş
        private readonly string SuccRedirect = "https://temp.nabooo.com/callback/success"; // success
        private readonly string FailRedirect = "https://temp.nabooo.com/callback/fail"; // fail


        // Singleton Class Design, Bu obje sistemdeki tek SiPay objesi olucaktır
        private static SiPay _siPay = null;

        // test api
        private readonly string TestHost = "https://provisioning.sipay.com.tr/ccpayment";

        // gerçek api
        private readonly string SipayHost = "https://app.sipay.com.tr/ccpayment";

        

        // Fintag Merchant Key
        private readonly string MerchantKey = "$2y$10$u.E853jeb87arPTTlMxFg.Gf03g.reTNJbUCJxOnYvumAbn2uU5lO";
        private readonly string MerchantId = "71432";

        // Fintag App id etc
        private readonly string AppId = "840f54004c8fbefbb69b5e77cccfad8c";
        private readonly string AppSecret = "90173e5ad98ed8e11899a65320961829";

        // Test
        private readonly string TestId = "8ee8fb5f0ff553b6ecbea59bfa51000d";
        private readonly string TestSecret = "e75c48bc73fa36870e9e1554c4092559";

        /// <summary>
        /// Private constructor so this class can not be freely initialized
        /// </summary>
        private SiPay()
        {

        }

        /// <summary>
        /// Starts the Sipay services as singleton object
        /// </summary>
        /// <returns></returns>
        public static SiPay StartServices()
        {
            if (_siPay == null)
            {
                _siPay = new SiPay();
            }
            return _siPay;
        }


        private class SipayItem
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("price")]
            public decimal Price { get; set; }

            [JsonProperty("qnantity")]
            public int Qty { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

        }

        /// <summary>
        /// Bu method kullanılarak ödeme başlatılır
        /// </summary>
        /// <param name="pay3D">pay3d için gerekli data </param>
        public async Task<string> Pay3DSecure(Pay3DParamModel pay3D)
        {
            Log.Information("PAY 3D SECURE");
            var item = new SipayItem 
            {
                Description = "Altın",
                Name = "Gram Altın",
                Qty = 1,
                Price = decimal.Parse(pay3D.Amount)
            };
            Log.Information("ITEM");
            Log.Information("" + item.Description + " " + item.Name + " " + item.Price);
            var items = new List<SipayItem>();
            items.Add(item);

            var itemsAsJson = JsonConvert.SerializeObject(items);

            try
            {

                var getPosParams = new GetPosParamModel 
                { 
                    Amount = decimal.Parse(pay3D.Amount),
                    CreditCard = decimal.Parse(pay3D.CreditCard),
                    CurrencyCode = "TRY",
                    MerchantKey = MerchantKey
                };


                var pos = await GetVirtualPos(getPosParams);

                /*
                var urlContent = new Dictionary<string, string>
                {
                    { "pos_id", pos.Data[0].PosId.ToString() },
                    { "cc_holder_name", pay3D.HolderName },
                    { "cc_no", pay3D.CreditCard },
                    { "expiry_month", pay3D.ExpiryMonth },
                    { "expiry_year", pay3D.ExpiryYear },
                    { "cvv", pay3D.CVV },
                    { "currency_id", pos.Data[0].CurrencyId.ToString() },
                    { "currency_code", pos.Data[0].CurrencyCode },
                    { "campaign_id", pos.Data[0].CampaignId.ToString() },
                    { "allocation_id", pos.Data[0].AllocationId.ToString() },
                    { "installments_number", pos.Data[0].InstallmentsNumber.ToString() },
                    { "invoice_id", pay3D.Transaction.TransactionId.ToString() },
                    { "invoice_description", pay3D.Transaction.Comment },
                    { "name", pay3D.User.FirstName },
                    { "surname", pay3D.User.FamilyName },
                    { "total", pay3D.Amount },
                    { "discount", "0" },
                    { "coupon", "0" },
                    { "merchant_key", MerchantKey },
                    { "payable_amount", pos.Data[0].AmountToBePaid.ToString() },
                    { "items", itemsAsJson },
                    { "app_id", AppId },
                    { "app_secret", AppSecret },
                    { "cancel_url", FailRedirect },
                    { "return_url", SuccRedirect }
                };
                foreach (var v in urlContent)
                {
                    Log.Information(v.Key + " " + v.Value);
                }*/
                var token = await GetSiPayAccess();
                if (token.StatusCode == 0)
                {
                    throw new Exception("SiPay erişimi sağlanamadı");
                }
                else if (token.StatusCode != 100)
                {
                    throw new Exception(token.StatusDesc);
                }
                itemsAsJson = itemsAsJson.Replace("\"", "&quot;");
                object[] args = new object[]
                {
                    token.Data.Token,
                    pay3D.HolderName,
                    pay3D.CreditCard,
                    pay3D.ExpiryMonth,
                    pay3D.ExpiryYear,
                    pay3D.CVV,
                    pos.Data[0].PosId.ToString(),
                    pos.Data[0].AmountToBePaid.ToString(),
                    pos.Data[0].CurrencyId.ToString(),
                    pos.Data[0].CurrencyCode,
                    pos.Data[0].CampaignId.ToString(),
                    pos.Data[0].AllocationId.ToString(),
                    pos.Data[0].InstallmentsNumber.ToString(),
                    pay3D.Transaction.TransactionId.ToString(),
                    pay3D.Transaction.Comment,
                    pay3D.Amount,
                    MerchantKey,
                    pos.Data[0].AmountToBePaid.ToString(),
                    SuccRedirect,
                    FailRedirect,
                    itemsAsJson,
                    pay3D.User.FirstName,
                    pay3D.User.FamilyName,
                    AppId,
                    AppSecret
                };
                var formContent = string.Format(HtmlFormFormat, args);
                /*
                var formContent = string.Format(HtmlFormFormat, 
                    token.Data.Token,
                    pay3D.HolderName,
                    pay3D.CreditCard,
                    pay3D.ExpiryMonth,
                    pay3D.ExpiryYear,
                    pay3D.CVV,
                    pos.Data[0].PosId.ToString(),
                    pos.Data[0].AmountToBePaid.ToString(),
                    pos.Data[0].CurrencyId.ToString(),
                    pos.Data[0].CurrencyCode,
                    pos.Data[0].CampaignId.ToString(),
                    pos.Data[0].AllocationId.ToString(),
                    pos.Data[0].InstallmentsNumber.ToString(),
                    pay3D.Transaction.TransactionId.ToString(),
                    pay3D.Transaction.Comment,
                    pay3D.Amount,
                    MerchantKey,
                    pos.Data[0].AmountToBePaid.ToString(),
                    SuccRedirect,
                    FailRedirect,
                    itemsAsJson,
                    pay3D.User.FirstName,
                    pay3D.User.FamilyName,
                    AppId,
                    AppSecret

                    );*/
                return formContent;
                /*
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    using (var client = new HttpClient(httpClientHandler))
                    {
                        using (var req = new HttpRequestMessage(HttpMethod.Post, SipayHost + "/api/pay3d") { Content = new FormUrlEncodedContent(urlContent) })
                        {

                            req.Headers.Add("Accept", "*//*");
                            req.Headers.Add("Authorization", "Bearer " + token.Data.Token);
                            var response = await client.SendAsync(req);

                            var content = await response.Content.ReadAsStringAsync();
                            Log.Information("pay3d content - \n" + content);
                            return content;
                        }
                    }
                }*/
            } 
            catch (Exception err)
            {
                Log.Error("Exception at SiPay.Pay3dsecure() " + err.Message);
                Log.Error(err.ToString());

                return "Fintag Sipay Hatası";
            }
        }

        private async Task<GetPosResponseModel> GetVirtualPos(GetPosParamModel model)
        {
            try
            {
       

                var token = await GetSiPayAccess();
                if (token.StatusCode == 0)
                {
                    throw new Exception("SiPay erişimi sağlanamadı");
                } 
                else if (token.StatusCode != 100)
                {
                    throw new Exception(token.StatusDesc);
                }

                using (var client = new HttpClient())
                {
                    using (var req = new HttpRequestMessage(HttpMethod.Post, SipayHost + "/api/getpos") { Content = new JsonContent(model) })
                    {

                        req.Headers.Add("Accept", "application/json");
                        req.Headers.Add("Authorization", "Bearer " + token.Data.Token);
                        var response = await client.SendAsync(req);
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<GetPosResponseModel>(content);
                        return result;
                    }
                }
            }
            catch (Exception error)
            {
                Log.Error("Error at SiPay.GetVirtualPos() : " + error.Message);
                Log.Error(error.ToString());

                return new GetPosResponseModel { StatusCode = 0 };
            }
        }

        private async Task<TokenResponseModel> GetSiPayAccess()
        {
            try
            {
                var dict = new Dictionary<string, string>
                {
                    { "app_id", AppId },
                    { "app_secret", AppSecret }
                };

                using (var client = new HttpClient())
                {
                    using (var req = new HttpRequestMessage(HttpMethod.Post, SipayHost + "/api/token") { Content = new JsonContent(dict) })
                    {

                        req.Headers.Add("Accept", "application/json");
                        var response = await client.SendAsync(req);
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<TokenResponseModel>(content);
                        return result;
                    }
                }
            } 
            catch (Exception error)
            {
                Log.Error("Error at SiPay.GetSiPayAccess() : " + error.Message);
                Log.Error(error.ToString());

                return new TokenResponseModel { StatusCode = 0};
            }
            
        }



    }
}
