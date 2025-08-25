using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using System.Net.Mail;
using System.Net;
using Gold.Domain.Users.Interfaces;
using Microsoft.EntityFrameworkCore;
using Gold.Core.Events;
using Gold.Core.Banks;
using Gold.Core.Transactions;

namespace Gold.Api.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class EmailService
    {

        private static readonly string VERIFICATION_EMAIL_SABLON = "<!DOCTYPE html" +
            "PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
            "<head>" +
            "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />" +
            "<title>Goldtag Email</title>" +
            "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />" +
            "<link rel=\"stylesheet\" type=\"text/css\" href=\"http://www.fintag.net/css/email.css\" />" +
            "</head>" +
            "<body style=\"margin: 0; padding: 0;\">" +
            "<table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" width=\"840\"" +
            "style=\"border-collapse: collapse; background-image: url('https://i.hizliresim.com/JdrP0n.png'); background-repeat: no-repeat; background-position: bottom center;\">" +
            "<tr><td><table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" width=\"840\" style=\"border-collapse: collapse;\"><tr><td align=\"center\"" +
            "style=\"color: #CAB55F; font-weight: bold; font-size: 64px; padding-top: 120px; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif\">" +
            "Goldtag</td></tr><tr><td align=\"center\"style=\"color: #4d4d4d; font-size: 36px; font-weight: 800; padding-top: 105px; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif; padding-bottom: 115px;\">" +
            "Sana ait bir Goldtag hesabı oluşturuldu.</td></tr><tr>" +
            "<td align=\"center\"" +
            "style=\"border-radius: 5px; border: 1px solid #ebebeb; box-shadow: 0px 6px 25px rgba(0, 0, 0, 0.16);\">" +
            "<h3 style=\"color: #CAB55F; font-size: 24px; font-weight: 800; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif\">" +
            "Üye ismi: {0} </h3>" +
            "<h3 style=\"color: #CAB55F; font-size: 24px; font-weight: 800; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif\">" +
            "üye numarası: {1} </h3>" +
            "<a href=\"https://www.fintag.net/callback/verify?id={2}\" " +
            "style=\"color: #4d4d4d; font-size: 22px; font-weight: 800; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\">" +
            "Hesabınızı Aktif Etmek İçin Tıklayınız</a>" +
            "<p style=\"padding: 0 50px 30px 50px; color: #4d4d4d; font-size: 18px; font-weight: 500; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\"></p></td></tr>" +
            "<tr><td align=\"center\" style=\"padding: 100px 20px 0 20px; color: #4d4d4d; font-size: 22px; font-weight: 500; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\">" +
            "<span style=\"font-weight: 800\">Goldtag</span>, bankalar ile entegreli, güvenli bir şekilde müşterilerine altın alım ve satım imkanı sağlar. " +
            "<span style=\"font-weight: 800\">Goldtag</span> müşterileri bu altınları dilerse bir etkiliğe, arkadaşına transfer edebilir veya birikim de yapabilir" +
            "</td></tr>" +
            "<tr><td align=\"center\" style=\"padding-top: 70px; color: #4d4d4d; font-size: 22px; font-weight: 500; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\">" +
            "Tamamen müşteri odaklı olarak çalışan <span style=\"font-weight: 800\">Goldtag</span>, müşterilerine hızlı altın transfer deneyimi sunar. Üstelik tüm bunları yaparken sosyal mesafeye dikkat eder :)" +
            "</td></tr>" +
            "<tr><td align=\"center\" style=\"padding-top: 95px\"><a href=\"http://www.goldtag.org\"" +
            "style=\"color: #4d4d4d; font-size: 22px; font-weight: 800; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\">www.goldtag.org</a></td></tr>" +
            "</table></td></tr>" +
            "<tr><td><table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" width=\"204\"" +
            "style=\"margin-top: 100px; border-collapse: collapse; display: flex; justify-content: center;\">" +
            "<td align=\"center\">" +
            "<a href=\"https://instagram.com\" style=\"margin-right: 10px;\">" +
            "<img src=\"https://cdn4.iconfinder.com/data/icons/miu-black-social-2/60/instagram-64.png\" width=\"40\" height=\"40\" alt=\"Goldtag Instagram\" /></a></td>" +
            "<td align=\"center\">" +
            "<a href=\"https://facebook.com\" style=\"margin-right: 10px;\">" +
            "<img src=\"https://cdn4.iconfinder.com/data/icons/miu-black-social-2/60/facebook-64.png\" width=\"40\" height=\"40\" alt=\"Goldtag Facebook\" /></a></td>" +
            "<td align=\"center\">" +
            "<a href=\"https://linkedin.com\" style=\"margin-right: 10px;\"><img src=\"https://cdn4.iconfinder.com/data/icons/miu-black-social-2/60/linkedin-64.png\" width=\"40\"" +
            "height=\"40\" alt=\"LinkedIn\" /></a></td>" +
            "<td align=\"center\">" +
            "<a href=\"https://youtube.com\" style=\"margin-right: 10px;\"><img src=\"https://cdn4.iconfinder.com/data/icons/miu-black-social-2/60/youtube-64.png\" width=\"40\" height=\"40\" alt=\"Goldtag Youtube\" /></a></td>" +
            "</table></td></tr>" +
            "<tr><td><table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" width=\"352\"" +
            "style=\"margin-top: 85px; border-collapse: collapse; font-size: 14px; font-weight: 400; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\"><tr>" +
            "<td align=\"center\"><a href=\"http://goldtag.org/iletisim\" style=\"color: #4d4d4d\">İletişim</a></td><td align=\"center\">" +
            "<a href=\"http://goldtag.org\" style=\"color: #4d4d4d\">Gizlilik Sözleşmesi</a></td>" +
            "<td align=\"center\"><a href=\"http://goldtag.org\" style=\"color: #4d4d4d\">Şartlar ve Koşullar</a></td></tr></table></td></tr>" +
            "<tr><td><table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin-top: 15px; margin-bottom: 50px;\"><tr>" +
            "<td align=\"center\" style=\"color: #7d7d7d; font-size: 14px; font-weight: 400; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\">&copy; <span style=\"color: #CAB55F\">Goldtag</span> - All Rights Reserved." +
            "</td></tr></table></td></tr></table>" +
            "<td bgcolor=\"#ffffff\" style=\"white-space:nowrap!important;line-height: 0; color: #ffffff !important;\">" +
            "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -" +
            "</td></body></html>";


        private static readonly string NEW_EMAIL_VERIFI_SABLON = "<!DOCTYPE html" +
            "PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
            "<head>" +
            "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />" +
            "<title>Goldtag Email</title>" +
            "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />" +
            "<link rel=\"stylesheet\" type=\"text/css\" href=\"http://www.fintag.net/css/email.css\" />" +
            "</head>" +
            "<body style=\"margin: 0; padding: 0;\">" +
            "<table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" width=\"840\"" +
            "style=\"border-collapse: collapse; background-image: url('https://i.hizliresim.com/JdrP0n.png'); background-repeat: no-repeat; background-position: bottom center;\">" +
            "<tr><td><table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" width=\"840\" style=\"border-collapse: collapse;\"><tr><td align=\"center\"" +
            "style=\"color: #CAB55F; font-weight: bold; font-size: 64px; padding-top: 120px; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif\">" +
            "Goldtag</td></tr><tr><td align=\"center\"style=\"color: #4d4d4d; font-size: 36px; font-weight: 800; padding-top: 105px; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif; padding-bottom: 115px;\">" +
            "E-mail Doğrulama</td></tr><tr>" +
            "<td align=\"center\"" +
            "style=\"border-radius: 5px; border: 1px solid #ebebeb; box-shadow: 0px 6px 25px rgba(0, 0, 0, 0.16);\">" +
            "<h3 style=\"color: #CAB55F; font-size: 24px; font-weight: 800; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif\">" +
            "Üye ismi: {0} </h3>" +
            "<a href=\"https://www.fintag.net/callback/email_verify?id={1}\"" +
            "style=\"color: #4d4d4d; font-size: 22px; font-weight: 800; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\">" +
            "Yeni mail adresinizi doğrulayınız.</a>" +
            "<p style=\"padding: 0 50px 30px 50px; color: #4d4d4d; font-size: 18px; font-weight: 500; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\"></p></td></tr>" +
            "<tr><td align=\"center\" style=\"padding: 100px 20px 0 20px; color: #4d4d4d; font-size: 22px; font-weight: 500; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\">" +
            "<span style=\"font-weight: 800\">Goldtag</span>, bankalar ile entegreli, güvenli bir şekilde müşterilerine altın alım ve satım imkanı sağlar. " +
            "<span style=\"font-weight: 800\">Goldtag</span> müşterileri bu altınları dilerse bir etkiliğe, arkadaşına transfer edebilir veya birikim de yapabilir" +
            "</td></tr>" +
            "<tr><td align=\"center\" style=\"padding-top: 70px; color: #4d4d4d; font-size: 22px; font-weight: 500; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\">" +
            "Tamamen müşteri odaklı olarak çalışan <span style=\"font-weight: 800\">Goldtag</span>, müşterilerine hızlı altın transfer deneyimi sunar. Üstelik tüm bunları yaparken sosyal mesafeye dikkat eder :)" +
            "</td></tr>" +
            "<tr><td align=\"center\" style=\"padding-top: 95px\"><a href=\"http://www.goldtag.org\"" +
            "style=\"color: #4d4d4d; font-size: 22px; font-weight: 800; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\">www.goldtag.org</a></td></tr>" +
            "</table></td></tr>" +
            "<tr><td><table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" width=\"204\"" +
            "style=\"margin-top: 100px; border-collapse: collapse; display: flex; justify-content: center;\">" +
            "<td align=\"center\">" +
            "<a href=\"https://instagram.com\" style=\"margin-right: 10px;\">" +
            "<img src=\"https://cdn4.iconfinder.com/data/icons/miu-black-social-2/60/instagram-64.png\" width=\"40\" height=\"40\" alt=\"Goldtag Instagram\" /></a></td>" +
            "<td align=\"center\">" +
            "<a href=\"https://facebook.com\" style=\"margin-right: 10px;\">" +
            "<img src=\"https://cdn4.iconfinder.com/data/icons/miu-black-social-2/60/facebook-64.png\" width=\"40\" height=\"40\" alt=\"Goldtag Facebook\" /></a></td>" +
            "<td align=\"center\">" +
            "<a href=\"https://linkedin.com\" style=\"margin-right: 10px;\"><img src=\"https://cdn4.iconfinder.com/data/icons/miu-black-social-2/60/linkedin-64.png\" width=\"40\"" +
            "height=\"40\" alt=\"LinkedIn\" /></a></td>" +
            "<td align=\"center\">" +
            "<a href=\"https://youtube.com\" style=\"margin-right: 10px;\"><img src=\"https://cdn4.iconfinder.com/data/icons/miu-black-social-2/60/youtube-64.png\" width=\"40\" height=\"40\" alt=\"Goldtag Youtube\" /></a></td>" +
            "</table></td></tr>" +
            "<tr><td><table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" width=\"352\"" +
            "style=\"margin-top: 85px; border-collapse: collapse; font-size: 14px; font-weight: 400; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\"><tr>" +
            "<td align=\"center\"><a href=\"http://goldtag.org/iletisim\" style=\"color: #4d4d4d\">İletişim</a></td><td align=\"center\">" +
            "<a href=\"http://goldtag.org\" style=\"color: #4d4d4d\">Gizlilik Sözleşmesi</a></td>" +
            "<td align=\"center\"><a href=\"http://goldtag.org\" style=\"color: #4d4d4d\">Şartlar ve Koşullar</a></td></tr></table></td></tr>" +
            "<tr><td><table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin-top: 15px; margin-bottom: 50px;\"><tr>" +
            "<td align=\"center\" style=\"color: #7d7d7d; font-size: 14px; font-weight: 400; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;\">&copy; <span style=\"color: #CAB55F\">Goldtag</span> - All Rights Reserved." +
            "</td></tr></table></td></tr></table>" +
            "<td bgcolor=\"#ffffff\" style=\"white-space:nowrap!important;line-height: 0; color: #ffffff !important;\">" +
            "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -" +
            "</td></body></html>";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="IBAN"></param>
        /// <param name="kur"></param>
        /// <param name="amountTL"></param>
        /// <param name="amountGram"></param>
        /// <param name="odenmesiGereken"></param>
        /// <param name="normalTransferId"></param>
        /// <param name="transId"></param>
        public static void InformFintagAltinBozdur(string name,
           string IBAN,
           decimal kur,
           decimal amountTL,
           decimal amountGram,
           decimal odenmesiGereken,
           Guid normalTransferId,
           Guid transId,
           bool silver = false)
        {

            var format = "Selamlar,\n" +
                "{0} isimli kullanıcı\n" +
                "{1} IBAN\n" +
                "{2} KUR 1 gram altın\n" +
                "{3} TL fiyat\n" +
                "{4} Gram altın\n" +
                "{5} ÖDENMESİ GEREKEN IBAN A\n" +
                "{6} Transfer ID\n" +
                "{7} Transaction ID\n" +
                "https://www.fintag.net/callback/confirm_eft_sell?transId={8}&odendi={9} - Confirm için\n";
            
            if (silver)
            {
                format = "Selamlar,\n" +
                "{0} isimli kullanıcı\n" +
                "{1} IBAN\n" +
                "{2} KUR 1 gram silver\n" +
                "{3} TL fiyat\n" +
                "{4} Gram silver\n" +
                "{5} ÖDENMESİ GEREKEN IBAN A\n" +
                "{6} Transfer ID\n" +
                "{7} Transaction ID\n" +
                "https://www.fintag.net/callback/confirm_eft_sell_silver?transId={8}&odendi={9} - Confirm için\n";
            }
            
            var message = string.Format(format,
                name, IBAN, kur, amountTL, amountGram, odenmesiGereken, normalTransferId, transId, transId, odenmesiGereken);

            SendEmail("dolunaysabuncuoglu@gmail.com", "EFT ile altin bozdurma requesti: " + normalTransferId, message, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="bankName"></param>
        /// <param name="kur"></param>
        /// <param name="amountTL"></param>
        /// <param name="amountGram"></param>
        /// <param name="code"></param>
        /// <param name="codeStartDate"></param>
        /// <param name="bankTrasferId"></param>
        /// <param name="normalTransferId"></param>
        /// <param name="transId"></param>
        public static void InformFintagBankEFTRequest(string name,
            string bankName,
            decimal kur,
            decimal amountTL,
            decimal amountGram,
            int code,
            DateTime codeStartDate,
            Guid bankTrasferId,
            Guid normalTransferId,
            Guid transId,
            bool silver = false)
        {
            try
            {
                 
                var format = "Selamlar,\n" +
                    "Kullanici: {0}\n" +
                    "Banka: {1}\n" +
                    "KUR 1 gram altın: {2}\n" +
                    "Kullanicinin yatiracagi para: {3} TRY\n" +
                    "Verilecek altin: {4} gr\n" +
                    "Aciklama kodu:  {5}\n" +
                    "Kod yaratilma tarihi: {6}\n" +
                    "Kod bitis tarihi: {7} \n" +
                    "bank transfer id: {8}\n" +
                    "normal transfer id: {9}\n" +
                    "transaction id: {10}\n" +
                    "https://www.fintag.net/callback/confirm_eft?ubt={11}&ntr={12}&tid={13}";

                if (silver)
                {
                    format = "Selamlar,\n" +
                    "Kullanici: {0}\n" +
                    "Banka: {1}\n" +
                    "KUR 1 gram silver: {2}\n" +
                    "Kullanicinin yatiracagi para: {3} TRY\n" +
                    "Verilecek silver: {4} gr\n" +
                    "Aciklama kodu:  {5}\n" +
                    "Kod yaratilma tarihi: {6}\n" +
                    "Kod bitis tarihi: {7} \n" +
                    "bank transfer id: {8}\n" +
                    "normal transfer id: {9}\n" +
                    "transaction id: {10}\n" +
                    "https://www.fintag.net/callback/confirm_eft_silver?ubt={11}&ntr={12}&tid={13}";
                }

                var message = string.Format(format, 
                    name, 
                    bankName,
                    kur,
                    amountTL,
                    amountGram,
                    code,
                    codeStartDate,
                    codeStartDate.AddHours(1),
                    bankTrasferId,
                    normalTransferId,
                    transId,
                    bankTrasferId,
                    normalTransferId,
                    transId);
                SendEmail("dolunaysabuncuoglu@gmail.com", "EFT ile altin alimi requesti: " + bankTrasferId, message, false);
            }
            catch(Exception e)
            {
                Log.Error("error at InformFintagBANKEFTRequest: " + e.Message);
                Log.Error(e.StackTrace);
                throw new Exception("Error at inform fintag email", e);
            }
            
        }


        
        
        public static void RequestVerification(string email, int memberId, string memberName, string number)
        {
            try
            {
                /*var html = "<!DOCTYPE html>" +
                    "<html>" +
                    "<body>" +
                    "<h1> Goldtag " +
                    "<h2> Sayın {0} lütfen hesabınızı aktive ediniz.</ h2 >" +
                    "<h3> Kullanıcı numaranız: {1} Lütfen unutmayınız." +
                    "<p><a href=\"https://www.fintag.net/callback/verify?id={2}\"> E - Mail Onay </a></p>" +
                    "</body>" +
                    "</html>";*/

                //var message = string.Format(html, memberName, memberId, memberId);
               // var message = string.Format(VERIFICATION_EMAIL_SABLON, memberName, memberId, memberId);
               // SendEmail(email, "GoldTag'e Hoşgeldiniz", message, true);

                string smessage = string.Format("GoldTag hesabınızı lütfen aktif hale getiriniz https://www.fintag.net/callback/verify?id={0}", memberId);
                SMSService.SendSms(number, smessage);
            }
            catch (Exception e)
            {
                Log.Error("Error at request verifivation " + e.Message);
                Log.Error(e.StackTrace);
                throw new Exception("Exception At request verification: " + e.Message);
            }

            

            
        }

        public static void RequestNewEmailVerification(string email, string changeId, string memberName)
        {
            try
            {
               

                /*var html = "<!DOCTYPE html>" +
                    "<html>" +
                    "<body>" +
                    "<h1> Goldtag " +
                    "<h2> Sayın {0} lütfen yeni e-mail adresinizi doğrulayınız.</ h2 >" +
                    "<p><a href=\"https://www.fintag.net/callback/email_verify?id={1}\"> E - Mail Onay </a></p>" + 
                    "</body>" +
                    "</html>";*/
                var message = string.Format(NEW_EMAIL_VERIFI_SABLON, memberName, changeId);

                SendEmail(email, "GoldTag - Yeni mail adresi doğrulama", message, true);
            }
            catch (Exception e)
            {
                throw new Exception("Exception At request verification: " + e.Message);
            }
        }

        /// <summary>
        /// Sends email to email address with subject and message
        /// email message can be html in that case ishtml must be true
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="isHtml"></param>
        public static void SendEmail(string email, string subject, string message, bool isHtml)
        {
            var fromAddress = new MailAddress("fintagsoft@gmail.com", "Fintag");
            var toAddress = new MailAddress(email, email);
            const string fromPassword = "Arc2020.!";
            

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var emessage = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = message,
                IsBodyHtml = isHtml
            })
            {
               
                smtp.Send(emessage);
            }
            
        }


        /// <summary>
        /// Asynchrounously send email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="isHtml"></param>
        /// <returns></returns>
        public async static Task SendEmailAsync(string email, string subject, string message, bool isHtml)
        {
            var fromAddress = new MailAddress("fintagsoft@gmail.com", "Fintag");
            var toAddress = new MailAddress(email, email);
            const string fromPassword = "Arc2020.!";


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            var eMailMessage = new MailMessage(fromAddress, toAddress) { Subject = subject,
            Body = message, IsBodyHtml = isHtml};

            await smtp.SendMailAsync(eMailMessage);

            eMailMessage.Dispose();

            smtp.Dispose();

        }

        /*public static bool SendEmail(string email, string subject, string message, out string errorMessage)
        {

            try
            {
                // Credentials
                var credentials = new NetworkCredential("arcnaboo@gmail.com", "fintagtest12345");
                // Mail message
                var mail = new MailMessage()
                {
                    From = new MailAddress("noreply@fintag.com"),
                    Subject = subject,
                    Body = message
                };
                mail.IsBodyHtml = true;
                mail.To.Add(new MailAddress(email));
                // Smtp client
                var client = new SmtpClient()
                {
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true,
                    Host = "smtp.gmail.com",
                    EnableSsl = true,
                    Credentials = credentials
                };
                client.Send(mail);
                errorMessage = "";
                return true;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }

        }*/

    }
}
