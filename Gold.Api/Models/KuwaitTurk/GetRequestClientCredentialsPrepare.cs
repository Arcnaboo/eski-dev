using Gold.Api.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Gold.Api.Models.KuwaitTurk
{
    public class GetRequestClientCredentialsPrepare
    {

        public string AccessToken { get; set; }
        public string Signature { get; set; }
        public HttpClient Client { get; set; }
        public HttpRequestMessage Request { get; set; }



        public static GetRequestClientCredentialsPrepare PrepareGet(string requestUri)
        {
            var result = new GetRequestClientCredentialsPrepare
            {
                AccessToken = KuwaitAccessHandler.AccessToken,
                Signature = KuwaitAccessHandler.SignSHA256RSA(KuwaitAccessHandler.AccessToken),
                Client = new HttpClient(),
                Request = new HttpRequestMessage(HttpMethod.Get, requestUri) 
                { 
                    Headers = { 
                        Authorization = new AuthenticationHeaderValue("Bearer", KuwaitAccessHandler.AccessToken) 
                    }
                }
            };
            result.Request.Headers.Add("Signature", result.Signature);

            return result;
        }

        public static void DisposeGetRequest(GetRequestClientCredentialsPrepare getRequest)
        {
            getRequest.Request.Dispose();
            getRequest.Client.Dispose();
        }
       
    }
}
