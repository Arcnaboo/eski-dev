using Gold.Api.Models.KuwaitTurk.Vpos;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Gold.Api.Services
{
    public class VposService
    {
        /*
         Test Bilgileri:
CustomerId = &quot;400235&quot;;//Müsteri Numarasi
MerchantId = &quot;496&quot; ; //Magaza Kodu
UserName=&quot;apitest&quot;; // Web Yönetim ekranlarindan olusturulan api rollü kullanici
Password=&quot;api123&quot;; // Web Yönetim ekranlarindan olusturulan api rollü kullanici sifresi 
 
Test Kart Bilgileri
Kart No: #4033’6025’6202’0327#
CVV: 861
Expirydate: 01/30
 
SanalPos 3D Model Ödeme Noktası Adresi (Test) :
https://boatest.kuveytturk.com.tr/boa.virtualpos.services/Home/ThreeDModelPayGate
SanalPos 3D Model Ödeme Onaylama Adresi(Test):
https://boatest.kuveytturk.com.tr/boa.virtualpos.services/Home/ThreeDModelProvisionGat

e

        Sanal Pos Güvenli Ödeme Noktası (Production)
https://boa.kuveytturk.com.tr/sanalposservice/Home/ThreeDModelPayGate
Sanal Pos Güvenli Ödeme Onaylama (Production)
https://boa.kuveytturk.com.tr/sanalposservice/Home/ThreeDModelProvisionGate
         */
        

        private static readonly string CustomerId = "96193345";
        private static readonly string MerchantId = "50664";
        private static readonly string UserName = "aakgur";
        private static readonly string Password = "Arda1234Arda";

        private static readonly string PayGate = "https://boa.kuveytturk.com.tr/sanalposservice/Home/ThreeDModelPayGate";
        private static readonly string ProvisionGate = "https://boa.kuveytturk.com.tr/sanalposservice/Home/ThreeDModelProvisionGate";



        private static readonly string APIVersion = "1.0.0";
        // private static readonly string MerchantOrderId = "20200811_satis";// Siparis Numarasi
        // private static readonly string OkUrl = "https://localhost:44397/callback/ktsuccess";
        // private static readonly string FailUrl = "https://localhost:44397/callback/ktfail";

        private static readonly string OkUrlNormal = "https://www.fintag.net/callback/kart_onay_normal"; //Basarili sonuç alinirsa, yönledirelecek sayfa
        private static readonly string OkUrlEvent = "https://www.fintag.net/callback/kart_onay_event"; //Basarili sonuç alinirsa, yönledirelecek sayfa
        private static readonly string OkUrlSilver = "https://www.fintag.net/callback/kart_onay_silver"; //Basarili sonuç alinirsa, yönledirelecek sayfa
        private static readonly string FailUrl ="https://www.fintag.net/callback/ktfail";//Basarisiz sonuç alinirsa, yön


        private static readonly string XmlKartOnay = "" +
                "<KuveytTurkVPosMessage xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                + "<APIVersion>{0}</APIVersion>" 
                + "<OkUrl>{1}</OkUrl>"
                + "<FailUrl>{2}</FailUrl>"
                + "<SubMerchantId>0</SubMerchantId>"
                + "<HashData>{3}</HashData>"
                + "<MerchantId>{4}</MerchantId>"
                + "<CustomerId>{5}</CustomerId>"
                + "<UserName>{6}</UserName>"
                + "<CardNumber>{7}</CardNumber>"
                + "<CardExpireDateYear>{8}</CardExpireDateYear>"
                + "<CardExpireDateMonth>{9}</CardExpireDateMonth>"
                + "<CardCVV2>{10}</CardCVV2>"
                + "<CardHolderName>{11}</CardHolderName>"
                + "<InstallmentCount>0</InstallmentCount>"
                + "<CardType>TROY</CardType>"
                + "<BatchID>0</BatchID>"
                + "<TransactionType>Sale</TransactionType>"
                + "<Amount>{12}</Amount>"
                + "<DisplayAmount>{13}</DisplayAmount>"
                + "<CurrencyCode>0949</CurrencyCode>"
                + "<MerchantOrderId>{14}</MerchantOrderId>"
                + "<TransactionSecurity>3</TransactionSecurity>"
                + "<TransactionSide>Sale</TransactionSide>"
                + "</KuveytTurkVPosMessage>";

        private static readonly string XmlProvizyon = "" +
            "<KuveytTurkVPosMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                "<APIVersion>{0}</APIVersion>" +
                "<HashData>{1}</HashData>" +
                "<MerchantId>{2}</MerchantId>" +
				"<CustomerId>{3}</CustomerId>" +
				"<UserName>{4}</UserName>" +
				"<TransactionType>Sale</TransactionType>" +
				"<InstallmentCount>0</InstallmentCount>" +
	         	"<DisplayAmount>{5}</DisplayAmount>" +
				"<Amount>{6}</Amount>" +
				"<MerchantOrderId>{7}</MerchantOrderId>" +
				"<TransactionSecurity>3</TransactionSecurity>" +
				"<KuveytTurkVPosAdditionalData>" +
				    "<AdditionalData>" +
				        "<Key>MD</Key>" +
				        "<Data>{8}</Data>" + // MD
				    "</AdditionalData>" +
			    "</KuveytTurkVPosAdditionalData>" +
			"</KuveytTurkVPosMessage>";


        public static string Provizyon(VPosTransactionResponseContract contract)
        {
            try
            {
                string hashedPass;
                using (SHA1 sha1Hash = SHA1.Create())
                {
                    //From String to byte array
                    byte[] sourceBytes = Encoding.UTF8.GetBytes(Password);
                    byte[] hashBytes = sha1Hash.ComputeHash(sourceBytes);
                    hashedPass = Convert.ToBase64String(hashBytes);
                }

                string hashData;
                using (SHA1 sha1Hash = SHA1.Create())
                {
                    //From String to byte array ($MerchantId.$MerchantOrderId.$Amount.$UserName.$HashedPassword 
                    byte[] sourceBytes = Encoding.UTF8.GetBytes(MerchantId +
                        contract.MerchantOrderId + 
                        contract.VPosMessage.Amount + 
                        UserName +
                        hashedPass);
                    byte[] hashBytes = sha1Hash.ComputeHash(sourceBytes);
                    hashData = Convert.ToBase64String(hashBytes);
                }


                string xmlMessage = string.Format(XmlProvizyon,
                    APIVersion,
                    hashData,
                    MerchantId,
                    CustomerId,
                    UserName,
                    contract.VPosMessage.Amount,
                    contract.VPosMessage.Amount,
                    contract.VPosMessage.MerchantOrderId,
                    contract.MD);
                Log.Debug(xmlMessage);
                string url = ProvisionGate;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                byte[] requestInFormOfBytes = System.Text.Encoding.ASCII.GetBytes(xmlMessage);
                request.Method = "POST";
                request.ContentType = "text/xml;charset=utf-8";
                request.ContentLength = requestInFormOfBytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(requestInFormOfBytes, 0, requestInFormOfBytes.Length);
                requestStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader respStream = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default);
                string receivedResponse = respStream.ReadToEnd();
                Log.Debug(receivedResponse);
                respStream.Close();
                response.Close();
                return receivedResponse;
            } catch (Exception e)
            {
                Log.Error("Error at vpos.provizyon():" + e.Message);
                Log.Error(e.StackTrace);
                return "Bir hata olustu";
            }
            
        }

        public static string KartOnay(KartOnayModel model)
        {
            try
            {
               // Log.Information("Kartonay: " + model.ToString());
                string hashedPass;
                using (SHA1 sha1Hash = SHA1.Create())
                {
                    //From String to byte array
                    byte[] sourceBytes = Encoding.UTF8.GetBytes(Password);
                    byte[] hashBytes = sha1Hash.ComputeHash(sourceBytes);
                    hashedPass = Convert.ToBase64String(hashBytes);
                }


                string hashData;
                string OkUrl;
                switch (model.CallbackType)
                {
                    case CallbackType.NormalSatinAlma:
                        OkUrl = OkUrlNormal;
                        break;
                    case CallbackType.EventYolla:
                        OkUrl = OkUrlEvent;
                        break;
                    case CallbackType.SilverSatinAlma:
                        OkUrl = OkUrlSilver;
                        break;
                    default:
                        OkUrl = OkUrlNormal;
                        break;
                }
                using (SHA1 sha1Hash = SHA1.Create())
                {
                    //From String to byte array
                    byte[] sourceBytes = Encoding.UTF8.GetBytes(
                        MerchantId +
                        model.TransactionId + 
                        model.Amount + 
                        OkUrl + 
                        FailUrl + 
                        UserName +
                        hashedPass);
                    byte[] hashBytes = sha1Hash.ComputeHash(sourceBytes);
                    hashData = Convert.ToBase64String(hashBytes);
                }

                string xmlMessage = string.Format(XmlKartOnay,
                    APIVersion,
                    OkUrl,
                    FailUrl,
                    hashData,
                    MerchantId,
                    CustomerId,
                    UserName,
                    model.CardNumber,
                    model.ExpiryYear,
                    model.ExpiryMonth,
                    model.Cvv,
                    model.HolderName,
                    model.Amount,
                    model.Amount,
                    model.TransactionId);

                //Log.Debug(xmlMessage);
                string url = PayGate;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                byte[] requestInFormOfBytes = System.Text.Encoding.ASCII.GetBytes(xmlMessage);
                request.Method = "POST";
                request.ContentType = "text/xml;charset=utf-8";
                request.ContentLength = requestInFormOfBytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(requestInFormOfBytes, 0, requestInFormOfBytes.Length);
                requestStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader respStream = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default);
                string receivedResponse = respStream.ReadToEnd();

                respStream.Close();
                response.Close();
                //Log.Debug("KART ONAY- " + receivedResponse);


                return receivedResponse;
            }
            catch (Exception e)
            {
                Log.Error("Error at vpos.KartOnay():" + e.Message);
                Log.Error(e.StackTrace);
                return "Bir hata olustu";
            }
            
        }
        
    }
}
