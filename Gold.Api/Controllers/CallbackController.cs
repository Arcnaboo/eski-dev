using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Gold.Api.Models.Admin;
using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Models.KuwaitTurk.Vpos;
using Gold.Api.Models.SiPay;
using Gold.Api.Models.Users;
using Gold.Api.Models.UtilityModels;
using Gold.Api.Models.Vendors;
using Gold.Api.Services;
using Gold.Api.Utilities;
using Gold.Core.Transactions;
using Gold.Core.Users;
using Gold.Core.Vendors;
using Gold.Domain.Events.Interfaces;
using Gold.Domain.Events.Repositories;
using Gold.Domain.Transactions.Interfaces;
using Gold.Domain.Transactions.Repositories;
using Gold.Domain.Users.Interfaces;
using Gold.Domain.Users.Repositories;
using Gold.Domain.Vendors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;


namespace Gold.Api.Controllers
{
    /// <summary>
    /// Callback Controller
    /// Purpose is to become bridge between Admin panel and App
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class CallbackController : Controller
    {
        #region statics
        private static readonly string SIGNUP_VERFIED_FAIL = "<!DOCTYPE html" +
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
            "Hesabınız onaylanmadı. Lütfen tekrar deneyiniz</td></tr><tr>" +
            "<td align=\"center\"" +
            "style=\"border-radius: 5px; border: 1px solid #ebebeb; box-shadow: 0px 6px 25px rgba(0, 0, 0, 0.16);\">" +
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

        private static readonly string VPOS_FORMAT = "<!DOCTYPE html" +
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
            "İşlem Durumu.</td></tr><tr>" +
            "<td align=\"center\"" +
            "style=\"border-radius: 5px; border: 1px solid #ebebeb; box-shadow: 0px 6px 25px rgba(0, 0, 0, 0.16);\">" +
            "<h3 style=\"color: #CAB55F; font-size: 24px; font-weight: 800; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif\">" +
            "Sonuç: {0} </h3>" +
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

        private static readonly string SIGNUP_VERFIED = "<!DOCTYPE html" +
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
            "Hesabınız aktif hale getirildi.</td></tr><tr>" +
            "<td align=\"center\"" +
            "style=\"border-radius: 5px; border: 1px solid #ebebeb; box-shadow: 0px 6px 25px rgba(0, 0, 0, 0.16);\">" +
            "<h3 style=\"color: #CAB55F; font-size: 24px; font-weight: 800; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif\">" +
            "üye numarası: {0} </h3>" +
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


        private static readonly string EMAIL_VERFIED = "<!DOCTYPE html" +
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
            "E-mail onaylandı.</td></tr><tr>" +
            "<td align=\"center\"" +
            "style=\"border-radius: 5px; border: 1px solid #ebebeb; box-shadow: 0px 6px 25px rgba(0, 0, 0, 0.16);\">" +
            "<h3 style=\"color: #CAB55F; font-size: 24px; font-weight: 800; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif\">" +
            "Üye ismi: {0} </h3>" +
            "<h3 style=\"color: #CAB55F; font-size: 24px; font-weight: 800; font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif\">" +
            "üye numarası: {1} </h3>" +
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

        #endregion

        /// <summary>
        private readonly IUsersRepository Repository;
        private readonly ITransactionsRepository TransRepo;
        private readonly IEventsRepository EventsRepo;
        private readonly IVendorsRepository VendorsRepo;

        /// <summary>
        /// Creates new Callback Controller
        /// </summary>
        public CallbackController(IUsersRepository usersRepository, IVendorsRepository vendorsRepository)
        {
            Repository = usersRepository;
            TransRepo = new TransactionsRepository();
            EventsRepo = new EventsRepository();
            VendorsRepo = vendorsRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("acc_status")]
        public ActionResult<AccountStatusResult> AccStatus()
        {
            try
            {

                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }

                var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

                if (requestee == null || requestee.Role != "Admin")
                {
                    return Unauthorized();
                }

                return Ok(KTApiService.AccountStatus());
            }
            catch (Exception e)
            {
                Log.Error("Error at get accounts: " + e.Message + "\n" + e.StackTrace);

                return Ok(new AccountStatusResult { Success = false, Reference = e.Message });
            }
        }


        [HttpGet]
        [Route("arc_test_sell")]
        public ActionResult ArcTestMethod2()
        {
            try
            {
                /*
                 // WORKING
                var param = new InterBankTransferParams {
                    Amount = 10,
                    ReceiverAccount = "96001821",
                    ReceiverSuffix = "2",
                    Description = "DENEME GOLDTAG",
                    SenderSuffix = "5",
                    TransferType = 3
                };
                var res = KTApiService.InterBankTransfer(param);

                return Ok(res);*/
                //return Ok(KTApiService.GetHesapHareketleri("5", "itemCount=5"));

                /*    var fxResult = ;// current price
                    Log.Information(JsonConvert.SerializeObject(fxResult));
                    return Ok(JsonConvert.SerializeObject(fxResult));*/
                //


                //return Ok(KTApiService.AccountStatus());


                // OKISG 
                /*  var to = "5";
                  var from = "105";
                  var rate = KTApiService.GetCurrentPriceFromKtApi()
                      .value.FxRates
                      .Where(x => x.FxId == 24)
                      .FirstOrDefault();
                  var param = new PreciousMetalsSellParams
                  {
                      Amount = 0.1m,
                      SuffixFrom = from,
                      SuffixTo = to,
                      UserName = KuwaitAccessHandler.FINTAG,
                      SellRate = rate.SellRate

                  };*/

                return Ok("invalid");
               // return Ok(KTApiService.PreciousMetalSell(param));
            }
            catch (Exception e)
            {
                return Ok(e.Message + "\n" + e.StackTrace);
            }
        }

        [HttpGet]
        [Route("arc_test2")]
        public ActionResult ArcTestMethod()
        {
            try
            {

                // WORKING
                /* var param = new InterBankTransferParams {
                     Amount = 10,
                     ReceiverAccount = "96001821",
                     ReceiverSuffix = "2",
                     Description = "DENEME GOLDTAG",
                     SenderSuffix = "5",
                     TransferType = 3
                 };
                 var res = KTApiService.InterBankTransfer(param);

                 return Ok(res);*/
                //return Ok(KTApiService.GetHesapHareketleri("5", "itemCount=5"));

                /*    var fxResult = ;// current price
                    Log.Information(JsonConvert.SerializeObject(fxResult));
                    return Ok(JsonConvert.SerializeObject(fxResult));*/
                //


                //return Ok(KTApiService.AccountStatus());


                // OKISG 
                /*  var to = "105";
                  var from = "5";
                  var rate = KTApiService.GetCurrentPriceFromKtApi()
                      .value.FxRates
                      .Where(x => x.FxId == 24)
                      .FirstOrDefault();
                  var param = new PreciousMetalsBuyParams
                  {
                      Amount = 0.1m,
                      SuffixFrom = from,
                      SuffixTo = to,
                      UserName = "fintag",
                      BuyRate = rate.BuyRate

                  };


                  return Ok(KTApiService.PreciousMetalBuy(param));*/

                var para = new GetPaymentTypesParams { Amount = 10, SenderSuffix = 5, ReceiverIBAN = "TR640001000254301491345012" };

                //return Ok(KTApiService.GetPaymentTypesForIBAN(para));
                return Ok("ARC_TEST_2");
            }
            catch (Exception e)
            {
                return Ok(e.Message + "\n" + e.StackTrace);
            }
        }

        [HttpGet]
        [Route("kasim_execute")]
        public ActionResult KasimExecute(int code)
        {
            try
            {


                //KampanyaService.KasimSilverKampanyasiExecute(TransRepo, code);
                return Ok("executed finished");
            } catch (Exception e)
            {
                return Ok(e.Message);
            }
        }

        [HttpGet]
        [Route("kasim_start")]
        public ActionResult KasimStart()
        {
            try
            {
                //SMSService.SendSms("05323878550", "Kasim gumus kampanya onay kodu: " + KampanyaService.GetPassword());

                //return Ok("https://www.fintag.net/callback/kasim_execute?code=CEP_TELE_GELEN_KOD");

                return Ok("kampanya bitti");
            } 
            catch (Exception e)
            {
                return Ok(e.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("delete_user")]
        public ActionResult<string> DeleteUser(int? memberId)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userId;

            if (!Authenticator.ValidateToken(token, out userId))
            {
                return Unauthorized();
            }

            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userId).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            if (memberId == null)
            {
                return Ok("memberid verilmemis");
            }

            try
            {

                var user = Repository.GetAllUsers().Where(x => x.MemberId == memberId.Value).FirstOrDefault();

                Repository.RemoveUsers(user);

                Repository.SaveChanges();

                return Ok("User: " + user.FirstName + " silindi");
            }
            catch (Exception e)
            {
                return Ok("Exception: " + e.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("lift_user_ban")]
        public ActionResult<string> LiftUserBan(int? memberId)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userId;

            if (!Authenticator.ValidateToken(token, out userId))
            {
                return Unauthorized();
            }

            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userId).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            if (memberId == null)
            {
                return Ok("memberid verilmemis");
            }

            try
            {

                var user = Repository.GetAllUsers().Where(x => x.MemberId == memberId.Value).FirstOrDefault();

                user.AdminNotes = "Onayli uye";
                user.Banned = false;

                Repository.SaveChanges();

                return Ok("User: " + user.FirstName + " TEMP bani kalkti, giris yapabilir");
            }
            catch (Exception e)
            {
                return Ok("Exception: " + e.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("verify_user_sms")]
        public ActionResult<string> VerifyUserSms(int? memberId)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userId;

            if (!Authenticator.ValidateToken(token, out userId))
            {
                return Unauthorized();
            }

            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userId).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            if (memberId == null)
            {
                return Ok("memberid verilmemis");
            }

            try
            {

                var user = Repository.GetAllUsers().Where(x => x.MemberId == memberId.Value).FirstOrDefault();

                user.AdminNotes = "Onayli uye";
                user.Banned = false;

                Repository.SaveChanges();

                return Ok("User: " + user.FirstName + " sms bani kalkti, giris yapabilir");
            }
            catch (Exception e)
            {
                return Ok("Exception: " + e.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("send_all_message")]
        public ActionResult<string> SendAllUsersMessage(string message)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }

            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(message))
            {
                return Ok("message empty");
            }

            try
            {

                SMSService.SendSMSToAllUsers(Repository, message);
                return Ok("Message notification olarak herkese iletildi");
            }
            catch (Exception e)
            {
                return Ok("Exception: " + e.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("send_all_notification")]
        public ActionResult<string> SendAllUsersNotificaiton(string message)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }

            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(message))
            {
                return Ok("message empty");
            }

            try
            {
                var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";
                var users = Repository.GetAllUsers().Where(x => x.Banned == false && x.AdminNotes != "TEMP_BAN" && x.Role == "Member").ToList();

                foreach (var user in users)
                {
                    var noti = new Notification2(user.UserId, message, false, "info", null, photo);
                    TransRepo.AddNotification(noti);
                }
                TransRepo.SaveChanges();
                return Ok("Message notification olarak herkese iletildi");
            } 
            catch (Exception e)
            {
                return Ok("Exception: " + e.Message);
            } 
        }

        private VendorTransactionAdminModel ParseVendorTransactionAsAdminModel(VendorTransaction transaction)
        {

            var source = VendorsRepo.GetVendors().Where(x => x.VendorId == transaction.Source).FirstOrDefault();
            var destination = VendorsRepo.GetVendors().Where(x => x.VendorId == transaction.Destination).FirstOrDefault();


            var confirmedByAdmin = VendorsRepo.GetVendorConfirmations().Where(x => x.TransactionId == transaction.TransactionId).Any();

            var model = new VendorTransactionAdminModel
            {
                TransactionId = transaction.TransactionId.ToString(),
                Reference = transaction.VendorReferenceId,
                Source = (source != null) ? source.Name : "Error",
                Destination = (destination != null) ? destination.Name : "Error",
                ConfirmedByVendor = transaction.ConfirmedByVendor,
                ConfirmedByGoldtag = transaction.ConfirmedByGoldtag,
                Cancelled = transaction.Cancelled,
                Finalised = transaction.Succesful,
                GramAmount = transaction.GramAmount,
                TlAmount = transaction.TlAmount,
                TransactionDateTime = transaction.TransactionDateTime.ToString(),
                VendorConfirmedDateTime = (transaction.VendorConfirmedDateTime.HasValue) ? transaction.VendorConfirmedDateTime.Value.ToString() : null,
                GoldtagConfirmedDateTime = (transaction.GoldtagConfirmedDateTime.HasValue) ? transaction.GoldtagConfirmedDateTime.Value.ToString() : null,
                TransactionFinalisedDateTime = (transaction.TransactionFinalisedDateTime.HasValue) ? transaction.TransactionFinalisedDateTime.Value.ToString() : null,
                ConfirmedByAdmin = confirmedByAdmin
            };


            return model;
        }

        private VendorTransactionAdminModel ParseVendorTransactionNewAsAdminModel(VendorTransactionNew transaction)
        {

            var source = VendorsRepo.GetVendors().Where(x => x.VendorId == transaction.Source).FirstOrDefault();
            var destination = VendorsRepo.GetVendors().Where(x => x.VendorId == transaction.Destination).FirstOrDefault();


            var confirmedByAdmin = VendorsRepo.GetVendorConfirmationsNew().Where(x => x.TransactionId == transaction.TransactionId).Any();

            var model = new VendorTransactionAdminModel
            {
                TransactionId = transaction.TransactionId.ToString(),
                Reference = transaction.VendorReferenceId,
                Source = (source != null) ? source.Name : "Error",
                Destination = (destination != null) ? destination.Name : "Error",
                ConfirmedByVendor = transaction.ConfirmedByVendor,
                ConfirmedByGoldtag = transaction.ConfirmedByGoldtag,
                Cancelled = transaction.Cancelled,
                Finalised = transaction.Succesful,
                GramAmount = transaction.GramAmount,
                TlAmount = transaction.TlAmount,
                TransactionDateTime = transaction.TransactionDateTime.ToString(),
                VendorConfirmedDateTime = (transaction.VendorConfirmedDateTime.HasValue) ? transaction.VendorConfirmedDateTime.Value.ToString() : null,
                GoldtagConfirmedDateTime = (transaction.GoldtagConfirmedDateTime.HasValue) ? transaction.GoldtagConfirmedDateTime.Value.ToString() : null,
                TransactionFinalisedDateTime = (transaction.TransactionFinalisedDateTime.HasValue) ? transaction.TransactionFinalisedDateTime.Value.ToString() : null,
                ConfirmedByAdmin = confirmedByAdmin
            };


            return model;
        }

        private VendorTransactionAdminModel ParseVenTransactionAsAdminModel(VenTransaction transaction)
        {
            var source = VendorsRepo.GetVendors().Where(x => x.VendorId == transaction.Source).FirstOrDefault();
            var destination = VendorsRepo.GetVendors().Where(x => x.VendorId == transaction.Destination).FirstOrDefault();
            var confirmedByAdmin = VendorsRepo.GetVendorConfirmationsNew().Where(x => x.TransactionId == transaction.TransactionId).Any();

            var model = new VendorTransactionAdminModel
            {
                TransactionId = transaction.TransactionId.ToString(),
                Reference = transaction.VendorReferenceId,
                Source = (source != null) ? source.Name : "Error",
                Destination = (destination != null) ? destination.Name : "Error",
                ConfirmedByVendor = transaction.ConfirmedByVendor,
                ConfirmedByGoldtag = transaction.ConfirmedByGoldtag,
                Cancelled = transaction.Cancelled,
                Finalised = transaction.Succesful,
                GramAmount = transaction.GramAmount,
                TlAmount = transaction.TlAmount,
                TransactionDateTime = transaction.TransactionDateTime.ToString(),
                VendorConfirmedDateTime = (transaction.VendorConfirmedDateTime.HasValue) ? transaction.VendorConfirmedDateTime.Value.ToString() : null,
                GoldtagConfirmedDateTime = (transaction.GoldtagConfirmedDateTime.HasValue) ? transaction.GoldtagConfirmedDateTime.Value.ToString() : null,
                TransactionFinalisedDateTime = (transaction.TransactionFinalisedDateTime.HasValue) ? transaction.TransactionFinalisedDateTime.Value.ToString() : null,
                ConfirmedByAdmin = confirmedByAdmin
            };


            return model;
        }

        [Authorize]
        [HttpGet]
        [Route("admin_confirm_vendor_transaction")]
        public ActionResult<string> AdminConfirmVendorTransaction(string id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }

            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }


            
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Ok("bad id");
                }

                var transaction = VendorsRepo.GetVendorTransaction(Guid.Parse(id));

                if (transaction == null)
                {
                    return Ok("no transaction");
                }


                var confirmIt = new VendorConfirmation(Guid.Parse(id));

                VendorsRepo.AddConfirmation(confirmIt);
                VendorsRepo.SaveChanges();

                
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor gold trans: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(e.Message);
            }

            return Ok("ok");
        }

        [Authorize]
        [HttpPost]
        [Route("get_vendor_silver_transactions")]
        public ActionResult<List<VendorTransactionAdminModel>> GetVendorSilverTransactions(VendorTransactionRequestParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }

            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }


            var result = new List<VendorTransactionAdminModel>();
            try
            {



                if (!VendorTransactionRequestParamModel.ValidateModel(model))
                {
                    return Ok(result);
                }

                var from = DateTime.ParseExact(model.DateFrom, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var to = DateTime.ParseExact(model.DateTo, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                List<VendorTransaction> quert;
                List<VendorTransactionNew> query;
                List<VenTransaction> queryNew;
                if (model.VendorId != null)
                {
                    var id = Guid.Parse(model.VendorId);

  
                    quert = VendorsRepo.GetVendorTransactions()
                    .Where(x => (x.Destination == id || x.Source == id) &&
                            x.TransactionDateTime.Date >= from.Date &&
                            x.TransactionDateTime.Date <= to.Date &&
                            x.Succesful &&
                            x.TransactionFinalisedDateTime.HasValue &&
                            (x.Comment.StartsWith("arc_sell_silver") ||
                            x.Comment.StartsWith("arc_buy_silver")))
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();
                    query = VendorsRepo.GetVendorTransactionsNew()
                    .Where(x => (x.Destination == id || x.Source == id) &&
                            x.TransactionDateTime.Date >= from.Date &&
                            x.TransactionDateTime.Date <= to.Date &&
                            x.Succesful &&
                            x.TransactionFinalisedDateTime.HasValue &&
                            (x.Comment.StartsWith("arc_sell_silver") ||
                            x.Comment.StartsWith("arc_buy_silver")))
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();
                    queryNew = VendorsRepo.GetVenTransactions()
                    .Where(x => (x.Destination == id || x.Source == id) &&
                            x.TransactionDateTime.Date >= from.Date &&
                            x.TransactionDateTime.Date <= to.Date &&
                            x.Succesful &&
                            x.TransactionFinalisedDateTime.HasValue &&
                            (x.Comment.StartsWith("arc_sell_silver") ||
                            x.Comment.StartsWith("arc_buy_silver")))
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();
                }
                else
                {
                    quert = VendorsRepo.GetVendorTransactions()
                    .Where(x => x.TransactionDateTime.Date >= from.Date &&
                            x.TransactionDateTime.Date <= to.Date &&
                            x.Succesful &&
                            x.TransactionFinalisedDateTime.HasValue &&
                            (x.Comment.StartsWith("arc_sell_silver") ||
                            x.Comment.StartsWith("arc_buy_silver")))
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();
                    query = VendorsRepo.GetVendorTransactionsNew()
                    .Where(x => x.TransactionDateTime.Date >= from.Date &&
                            x.TransactionDateTime.Date <= to.Date &&
                            x.Succesful &&
                            x.TransactionFinalisedDateTime.HasValue &&
                            (x.Comment.StartsWith("arc_sell_silver") ||
                            x.Comment.StartsWith("arc_buy_silver")))
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();
                    queryNew = VendorsRepo.GetVenTransactions()
                    .Where(x => x.TransactionDateTime.Date >= from.Date &&
                            x.TransactionDateTime.Date <= to.Date &&
                            x.Succesful &&
                            x.TransactionFinalisedDateTime.HasValue &&
                            (x.Comment.StartsWith("arc_sell_silver") ||
                            x.Comment.StartsWith("arc_buy_silver")))
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();
                }

                foreach (var trans in quert)
                {
                    var transModel = ParseVendorTransactionAsAdminModel(trans);

                    result.Add(transModel);
                }
                foreach (var trans in query)
                {
                    var transModel = ParseVendorTransactionNewAsAdminModel(trans);

                    result.Add(transModel);
                }
                foreach (var trans in queryNew)
                {
                    var transModel = ParseVenTransactionAsAdminModel(trans);

                    result.Add(transModel);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor gold trans: " + e.Message);
                Log.Error(e.StackTrace);

            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("get_vendor_gold_transactions")]
        public ActionResult<List<VendorTransactionAdminModel>> GetVendorGoldTransactions(VendorTransactionRequestParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }

            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            
            
            var result = new List<VendorTransactionAdminModel>();
            try
            {

                if (!VendorTransactionRequestParamModel.ValidateModel(model))
                {
                    return Ok(result);
                }

                var from = DateTime.ParseExact(model.DateFrom, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var to = DateTime.ParseExact(model.DateTo, "dd-MM-yyyy", CultureInfo.InvariantCulture);


                List<VendorTransactionNew> query;
                List<VendorTransaction> quert;
                List<VenTransaction> queryNew;

                if (model.VendorId != null)
                {
                    var id = Guid.Parse(model.VendorId);
                   
                    quert = VendorsRepo.GetVendorTransactions()
                    .Where(x => (x.Destination == id || x.Source == id) && x.TransactionDateTime.Date >= from.Date &&
                            x.TransactionDateTime.Date <= to.Date &&
                            x.Succesful &&
                            x.TransactionFinalisedDateTime.HasValue &&
                            !x.Comment.StartsWith("arc_sell_silver") &&
                            !x.Comment.StartsWith("arc_buy_silver"))
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();

                    query = VendorsRepo.GetVendorTransactionsNew()
                    .Where(x => (x.Destination == id || x.Source == id) && x.TransactionDateTime.Date >= from.Date &&
                            x.TransactionDateTime.Date <= to.Date &&
                            x.Succesful &&
                            x.TransactionFinalisedDateTime.HasValue &&
                            !x.Comment.StartsWith("arc_sell_silver") &&
                            !x.Comment.StartsWith("arc_buy_silver"))
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();

                    queryNew = VendorsRepo.GetVenTransactions()
                    .Where(x => (x.Destination == id || x.Source == id) && x.TransactionDateTime.Date >= from.Date &&
                            x.TransactionDateTime.Date <= to.Date &&
                            x.Succesful &&
                            x.TransactionFinalisedDateTime.HasValue &&
                            !x.Comment.StartsWith("arc_sell_silver") &&
                            !x.Comment.StartsWith("arc_buy_silver"))
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();
                }
                else
                {
                    quert = VendorsRepo.GetVendorTransactions()
                    .Where(x => x.TransactionDateTime.Date >= from.Date &&
                            x.TransactionDateTime.Date <= to.Date &&
                            x.Succesful &&
                            x.TransactionFinalisedDateTime.HasValue &&
                            !x.Comment.StartsWith("arc_sell_silver") &&
                            !x.Comment.StartsWith("arc_buy_silver"))
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();

                    query = VendorsRepo.GetVendorTransactionsNew()
                    .Where(x => x.TransactionDateTime.Date >= from.Date &&
                            x.TransactionDateTime.Date <= to.Date &&
                            x.Succesful &&
                            x.TransactionFinalisedDateTime.HasValue &&
                            !x.Comment.StartsWith("arc_sell_silver") &&
                            !x.Comment.StartsWith("arc_buy_silver"))
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();

                    queryNew = VendorsRepo.GetVenTransactions()
                    .Where(x => x.TransactionDateTime.Date >= from.Date &&
                            x.TransactionDateTime.Date <= to.Date &&
                            x.Succesful &&
                            x.TransactionFinalisedDateTime.HasValue &&
                            !x.Comment.StartsWith("arc_sell_silver") &&
                            !x.Comment.StartsWith("arc_buy_silver"))
                    .OrderByDescending(x => x.TransactionDateTime)
                    .ToList();
                }

                foreach (var trans in quert)
                {
                    var transModel = ParseVendorTransactionAsAdminModel(trans);

                    result.Add(transModel);
                }

                foreach (var trans in query)
                {
                    result.Add(ParseVendorTransactionNewAsAdminModel(trans));
                }
                foreach (var trans in queryNew)
                {
                    result.Add(ParseVenTransactionAsAdminModel(trans));
                }
            }
            catch (Exception e)
            {
                Log.Error("Error at get vendor gold trans: " + e.Message);
                Log.Error(e.StackTrace);

            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("get_vendors")]
        public ActionResult<List<VendorModel>> GetVendors()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }

            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            List<VendorModel> vendors = new List<VendorModel>();
            try
            {

                var query = VendorsRepo.GetVendors().ToList();

                foreach(var vendor in query)
                {
                    var model = new VendorModel
                    {
                        Id = vendor.VendorId.ToString(),
                        Name = vendor.Name,
                        Phone = vendor.Phone,
                        Balance = vendor.Balance.ToString(),
                        SBalance = (vendor.SilverBalance.HasValue) ? vendor.SilverBalance.Value.ToString() : "0"
                    };

                    vendors.Add(model);
                }


            }
            catch (Exception e)
            {
                Log.Error("Error at get vendors: " + e.Message);
                Log.Error(e.StackTrace);

            }

            return Ok(vendors);
        }

        [Authorize]
        [HttpGet]
        [Route("get_eft_buy_requests_count")]
        public ActionResult<int> GetEftBuyRequestsCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {

                var requets = TransRepo
                    .GetAllUserBankTransferRequests()
                    .Where(x => !x.MoneyReceived && DateTime.Now <= x.CodeStartDateTime.AddHours(2))
                    .ToList();

                int count = 0;

                foreach (var req in requets)
                {
                    var transfer = TransRepo
                        .GetAllTransferRequests()
                        .Where(x => x.TransferRequestId == req.TransferRequestId)
                        .FirstOrDefault();
                    if (transfer.RequestConfirmed)
                    {
                        count++;
                    }
                }
                    
                return Ok(count);

            }
            catch (Exception e)
            {
                Log.Error("Error: " + e.Message);
                Log.Error(e.StackTrace);

            }

            return Ok(0);
        }

        [Authorize]
        [HttpGet]
        [Route("get_eft_sell_requests_count")]
        public ActionResult<int> GetEftSellRequests()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var requests = TransRepo.GetAllTransactions()
                    .Where(x => (x.TransactionType == "GOLD" || x.TransactionType == "SILVER")
                            && x.SourceType == "User" &&
                            x.DestinationType == "IBAN" &&
                            x.Destination == "Fintag" &&
                            !x.Cancelled &&
                            !x.Confirmed)
                            .ToList();

                int counter = 1;

                foreach (var tr in requests)
                {
                    var request = TransRepo.GetAllTransferRequests()
                        .Where(x => x.TransactionRecord == tr.TransactionId)
                        .FirstOrDefault();

                    if (request == null || request.RequestConfirmed == false)
                    {
                        continue;
                    }
                    counter++;
                }
                return Ok(counter);
            }
            catch (Exception e)
            {
                Log.Error("Err: " + e.Message);
                Log.Error(e.StackTrace);
            }
            return Ok(0);
        }

        [Authorize]
        [HttpGet]
        [Route("get_eft_sell_requests")]
        public ActionResult<List<SellGoldRequestModel>> GetEftSellRequests(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            var result = new List<SellGoldRequestModel>();
            try
            {
                var requests = TransRepo
                    .GetAllTransactions()
                    .Where(x => (x.TransactionType == "GOLD" || x.TransactionType == "SILVER") && 
                            x.SourceType == "User" &&
                            x.DestinationType == "IBAN" &&
                            x.Destination == "Fintag" &&
                            !x.Cancelled &&
                            !x.Confirmed)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                foreach (var trans in requests)
                {
                    var user = TransRepo.GetAllUsers()
                        .Where(x => x.UserId.ToString() == trans.Source)
                        .FirstOrDefault();
                    if (user == null)
                    {
                        // belki silinmis bir kullanicinin kalan transaction i
                        continue;
                    }

                    var request = TransRepo.GetAllTransferRequests()
                        .Where(x => x.TransactionRecord == trans.TransactionId)
                        .FirstOrDefault();

                    if (request == null || request.RequestConfirmed == false)
                    {
                        continue;
                    }

                    var name = user.FirstName + " " + user.FamilyName;

                    var link = string.Format("https://www.fintag.net/callback/confirm_eft_sell?transId={0}&odendi={1}", trans.TransactionId, trans.TlAmount);
                    if (trans.TransactionType == "SILVER")
                    {
                        link = string.Format("https://www.fintag.net/callback/confirm_eft_sell_silver?transId={0}&odendi={1}",
                        trans.TransactionId, trans.TlAmount);
                    }


                    var denyLink = string.Format("https://www.fintag.net/callback/deny_eft_sell?transId={0}", trans.TransactionId);
                    var model = new SellGoldRequestModel
                    {
                        Name = name,
                        UserId = user.UserId.ToString(),
                        Grams = trans.GramAmount,
                        Price = trans.TlAmount,
                        OnayLink = link,
                        DeleteLink = denyLink,
                        TransactionType = trans.TransactionType
                    };
                    result.Add(model);
                }
            }
            catch (Exception e)
            {
                Log.Error("Err: " + e.Message);
                Log.Error(e.StackTrace);
            }
            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("get_eft_buy_requests")]
        public ActionResult<List<UserBankTransferRequestModel2>> GetEftBuyRequests(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            var result = new List<UserBankTransferRequestModel2>();

            var format = "https://www.fintag.net/callback/confirm_eft?ubt={0}&ntr={1}&tid={2}";
            var formatDeny = "https://www.fintag.net/callback/deny_eft?ubt={0}&ntr={1}&tid={2}";
            try
            {
                
                var requests = TransRepo
                .GetAllUserBankTransferRequests()
                .Where(x => !x.MoneyReceived && DateTime.Now <= x.CodeStartDateTime.AddHours(4))
                .OrderByDescending(x => x.CodeStartDateTime)
                .Skip(limit * (page - 1))
                .Take(limit)
                .ToList();

                var realRequests = new List<UserBankTransferRequest>();

                

                foreach (var req in requests)
                {
                    var bank = TransRepo.GetAllBanks()
                        .Where(x => x.BankId == req.BankId)
                        .FirstOrDefault();
                    var user = TransRepo.GetAllUsers()
                        .Where(x => x.UserId == req.UserId)
                        .FirstOrDefault();

                    if (user == null)
                    {
                        continue;
                    }

                    var transfer = TransRepo.GetAllTransferRequests()
                        .Where(x => x.TransferRequestId == req.TransferRequestId)
                        .FirstOrDefault();

                    if (transfer == null || !transfer.RequestConfirmed)
                    {
                        continue;
                    }
                    var transaction = TransRepo.GetAllTransactions()
                        .Where(x => x.TransactionId == transfer.TransactionRecord)
                        .FirstOrDefault();
                    if (transaction == null)
                    {
                        continue;
                    }
                    string ttype = "GOLD";
                    decimal kur;
                    if (transaction.TransactionType == "TRY_FOR_SILVER")
                    {
                        format = "https://www.fintag.net/callback/confirm_eft_silver?ubt={0}&ntr={1}&tid={2}";
                        ttype = "SILVER";
                        kur = GlobalGoldPrice.GetSilverPrices().BuyRate;
                    } else
                    {
                        format = "https://www.fintag.net/callback/confirm_eft?ubt={0}&ntr={1}&tid={2}";
                        kur = GlobalGoldPrice.GetCurrentPrice().BuyRate;
                    }
                            

                    var model = new UserBankTransferRequestModel2
                    {
                        UserId = user.UserId.ToString(),
                        TransactionType = ttype,
                        Name = user.FirstName + " " + user.FamilyName,
                        Bank = bank.BankName,
                        Grams = transaction.GramAmount,
                        FinalPrice = transaction.Amount,
                        Kur = kur,
                        Code = req.SpecialCode,
                        OnayLink = string.Format(format, req.BankTransferId, transfer.TransferRequestId, transaction.TransactionId),
                        DeleteLink = string.Format(formatDeny, req.BankTransferId, transfer.TransferRequestId, transaction.TransactionId),
                        CodeStart = req.CodeStartDateTime.ToString(),
                        CodeEnd = req.CodeStartDateTime.AddHours(1).ToString()
                    };
                    result.Add(model);

                }
                
                
                

            }
            catch (Exception e)
            {
                Log.Error("Error: " + e.Message);
                Log.Error(e.StackTrace);

            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_sales_count")]
        public ActionResult<int> AllSilverSalesCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository
                .GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" &&
                            x.Confirmed &&
                            x.SourceType != "Fintag" &&
                            x.DestinationType != "Fintag")
                    .OrderByDescending(x => x.TransactionDateTime)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_sales")]
        public ActionResult<List<TransactionAdminModel>> AllSilverSales(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository
                .GetAllUsers()
                .Where(x => x.UserId.ToString() == userid)
                .FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" &&
                            x.Confirmed &&
                            x.SourceType != "Fintag" &&
                            x.DestinationType != "Fintag")
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                var result = new List<TransactionAdminModel>();
                foreach (var transaction in totalGoldSold)
                {
                    result.Add(new TransactionAdminModel(transaction, Repository));
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        /// <summary>
        /// API endpoint, HTTP GET route all_gold_sales_count
        /// Returns the count of all gold sales
        /// </summary>
        /// <returns>integer - all gold sales count</returns>
        [Authorize]
        [HttpGet]
        [Route("all_gold_sales_count")]
        public ActionResult<int> AllGoldSalesCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository
                .GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" &&
                            x.Confirmed &&
                            x.SourceType != "Fintag" &&
                            x.DestinationType != "Fintag")
                    .OrderByDescending(x => x.TransactionDateTime)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        /// <summary>
        /// Returns All gold sales as List of Transactions
        /// Musteri id, TCK, datetime, nereden, nereye, altin, komisyonlu, baz, komisyon
        /// </summary>
        /// <param name="limit">How much data per page</param>
        /// <param name="page">which page to be returned</param>
        /// <returns>List of Transactions</returns>
        [Authorize]
        [HttpGet]
        [Route("all_gold_sales")]
        public ActionResult<List<TransactionAdminModel>> AllGoldSales(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository
                .GetAllUsers()
                .Where(x => x.UserId.ToString() == userid)
                .FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" &&
                            x.Confirmed &&
                            x.SourceType != "Fintag" &&
                            x.DestinationType != "Fintag")
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                var result = new List<TransactionAdminModel>();
                foreach (var transaction in totalGoldSold)
                {
                    result.Add(new TransactionAdminModel(transaction, Repository));
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        /// <summary>
        /// Returns the count of all gold sales thats made today
        /// </summary>
        /// <returns>integer - all gold sales today count</returns>
        [Authorize]
        [HttpGet]
        [Route("all_gold_sales_today_count")]
        public ActionResult<int> AllGoldSalesTodayCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" &&
                            x.Confirmed &&
                            x.SourceType != "Fintag" &&
                            x.DestinationType != "Fintag" &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_sales_today_count")]
        public ActionResult<int> AllSilverSalesTodayCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" &&
                            x.Confirmed &&
                            x.SourceType != "Fintag" &&
                            x.DestinationType != "Fintag" &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        /// <summary>
        /// Returns All gold sales today as List of Transactions
        /// </summary>
        /// <param name="limit">How much data per page</param>
        /// <param name="page">which page to be returned</param>
        /// <returns>List of Transactions</returns>
        [Authorize]
        [HttpGet]
        [Route("all_gold_sales_today")]
        public ActionResult<List<Transaction>> AllGoldSalesToday(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" &&
                            x.Confirmed &&
                            x.SourceType != "Fintag" &&
                            x.DestinationType != "Fintag" &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_sales_today")]
        public ActionResult<List<Transaction>> AllSilverSalesToday(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" &&
                            x.Confirmed &&
                            x.SourceType != "Fintag" &&
                            x.DestinationType != "Fintag" &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        /// <summary>
        /// Returns the count of all gold sales thats made with credit card
        /// </summary>
        /// <returns>integer - all gold sales by credit card count</returns>
        [Authorize]
        [HttpGet]
        [Route("all_gold_sales_vpos_count")]
        public ActionResult<int> AllGoldSalesVposCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.Confirmed &&
                            (x.DestinationType == "VirtualPos" 
                            || x.DestinationType == "Wedding" 
                            || x.DestinationType == "Event"))
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_sales_vpos_count")]
        public ActionResult<int> AllSilverSalesVposCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.Confirmed &&
                            (x.DestinationType == "VirtualPos"
                            || x.DestinationType == "Wedding"
                            || x.DestinationType == "Event"))
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        /// <summary>
        /// Returns All gold sales today as List of Transactions
        /// </summary>
        /// <param name="limit">How much data per page</param>
        /// <param name="page">which page to be returned</param>
        /// <returns>List of Transactions</returns>
        [Authorize]
        [HttpGet]
        [Route("all_gold_sales_vpos")]
        public ActionResult<List<Transaction>> AllGoldSalesVpos(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.Confirmed &&
                            (x.DestinationType == "VirtualPos" 
                            || x.DestinationType == "Wedding" 
                            || x.DestinationType == "Event"))
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }


        [Authorize]
        [HttpGet]
        [Route("all_silver_sales_vpos")]
        public ActionResult<List<Transaction>> AllSilverSalesVpos(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.Confirmed &&
                            (x.DestinationType == "VirtualPos"
                            || x.DestinationType == "Wedding"
                            || x.DestinationType == "Event"))
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_sales_vpos_today_count")]
        public ActionResult<int> AllSilverSalesVposTodayCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.Confirmed &&
                            (x.DestinationType == "VirtualPos"
                            || x.DestinationType == "Wedding"
                            || x.DestinationType == "Event") &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_gold_sales_vpos_today_count")]
        public ActionResult<int> AllGoldSalesVposTodayCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.Confirmed &&
                            (x.DestinationType == "VirtualPos"
                            || x.DestinationType == "Wedding"
                            || x.DestinationType == "Event") &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_gold_sales_vpos_today")]
        public ActionResult<List<Transaction>> AllGoldSalesVposToday(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.Confirmed &&
                            (x.DestinationType == "VirtualPos"
                            || x.DestinationType == "Wedding"
                            || x.DestinationType == "Event") &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_sales_vpos_today")]
        public ActionResult<List<Transaction>> AllSilverSalesVposToday(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.Confirmed &&
                            (x.DestinationType == "VirtualPos"
                            || x.DestinationType == "Wedding"
                            || x.DestinationType == "Event") &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }


        [Authorize]
        [HttpGet]
        [Route("all_gold_sales_eft_count")]
        public ActionResult<int> AllGoldSalesEFTCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" &&
                            x.Confirmed &&
                            x.DestinationType == "IBAN")
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_sales_eft_count")]
        public ActionResult<int> AllSilverSalesEFTCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" &&
                            x.Confirmed &&
                            x.DestinationType == "IBAN")
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_gold_sales_eft")]
        public ActionResult<List<Transaction>> AllGoldSalesEFT(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.Confirmed &&
                            x.DestinationType == "IBAN")
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_sales_eft")]
        public ActionResult<List<Transaction>> AllSilverSalesEFT(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.Confirmed &&
                            x.DestinationType == "IBAN")
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_gold_sales_eft_today_count")]
        public ActionResult<int> AllGoldSalesEFTTodayCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" &&
                            x.Confirmed &&
                            x.DestinationType == "IBAN" &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_sales_eft_today_count")]
        public ActionResult<int> AllSilverSalesEFTTodayCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" &&
                            x.Confirmed &&
                            x.DestinationType == "IBAN" &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_gold_sales_eft_today")]
        public ActionResult<List<Transaction>> AllGoldSalesEFTToday(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.Confirmed &&
                            x.DestinationType == "IBAN" &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_sales_eft_today")]
        public ActionResult<List<Transaction>> AllSilverSalesEFTToday(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.Confirmed &&
                            x.DestinationType == "IBAN" &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_gold_transfers_count")]
        public ActionResult<int> AllGoldTransfersCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "GOLD" && x.SourceType == "User" &&
                            x.Confirmed &&
                            (x.DestinationType == "User" || x.DestinationType == "Wedding" || x.DestinationType == "Event"))
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_transfers_count")]
        public ActionResult<int> AllSilverTransfersCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "SILVER" && x.SourceType == "User" &&
                            x.Confirmed &&
                            (x.DestinationType == "User"))// || x.DestinationType == "Wedding" || x.DestinationType == "Event"))
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_gold_transfers")]
        public ActionResult<List<Transaction>> AllGoldTransfers(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "GOLD" && x.SourceType == "User" &&
                              x.Confirmed &&
                            (x.DestinationType == "User" || x.DestinationType == "Wedding" || x.DestinationType == "Event"))
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_transfers")]
        public ActionResult<List<Transaction>> AllSilverTransfers(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "SILVER" && x.SourceType == "User" &&
                              x.Confirmed &&
                            (x.DestinationType == "User"))// || x.DestinationType == "Wedding" || x.DestinationType == "Event"))
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_gold_transfers_today_count")]
        public ActionResult<int> AllGoldTransfersTodayCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "GOLD" && x.SourceType == "User" && x.Confirmed &&
                            (x.DestinationType == "User" || x.DestinationType == "Wedding" || x.DestinationType == "Event") &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_transfers_today_count")]
        public ActionResult<int> AllSilverTransfersTodayCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "SILVER" && x.SourceType == "User" && x.Confirmed
                            && x.TransactionDateTime.Date == DateTime.Now.Date &&
                            x.DestinationType == "User")
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_gold_transfers_today")]
        public ActionResult<List<Transaction>> AllGoldTransfersToday(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "GOLD" && x.SourceType == "User" && x.Confirmed &&
                            (x.DestinationType == "User" || x.DestinationType == "Wedding" || x.DestinationType == "Event") &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_transfers_today")]
        public ActionResult<List<Transaction>> AllSilverTransfersToday(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "SILVER" && x.SourceType == "User" && x.Confirmed &&
                            x.DestinationType == "User" &&
                            x.TransactionDateTime.Date == DateTime.Now.Date)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }


        [Authorize]
        [HttpGet]
        [Route("all_gold_buys_count")]
        public ActionResult<int> AllGoldBuysCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.SourceType == "Fintag" && x.DestinationType == "User" && x.Confirmed)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_buys_count")]
        public ActionResult<int> AllSilverBuysCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.SourceType == "Fintag" && x.DestinationType == "User" && x.Confirmed)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_gold_buys")]
        public ActionResult<List<Transaction>> AllGoldBuys(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" 
                    && x.SourceType == "Fintag" && x.DestinationType == "User" && x.Confirmed)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_buys")]
        public ActionResult<List<Transaction>> AllSilverBuys(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER"
                    && x.SourceType == "Fintag" && x.DestinationType == "User" && x.Confirmed)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_gold_buys_today_count")]
        public ActionResult<int> AllGoldBuysTodayCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.SourceType == "Fintag" && x.DestinationType == "User" &&
                    x.TransactionDateTime.Date == DateTime.Now.Date && x.Confirmed)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_buys_today_count")]
        public ActionResult<int> AllSilverBuysTodayCount()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.SourceType == "Fintag" && x.DestinationType == "User" &&
                    x.TransactionDateTime.Date == DateTime.Now.Date && x.Confirmed)
                            .ToList().Count;

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(0);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_gold_buys_today")]
        public ActionResult<List<Transaction>> AllGoldBuysToday(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.SourceType == "Fintag" && x.DestinationType == "User" &&
                    x.TransactionDateTime.Date == DateTime.Now.Date && x.Confirmed)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("all_silver_buys_today")]
        public ActionResult<List<Transaction>> AllSilverBuysToday(int limit, int page)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.SourceType == "Fintag" && x.DestinationType == "User" &&
                    x.TransactionDateTime.Date == DateTime.Now.Date && x.Confirmed)
                    .OrderByDescending(x => x.TransactionDateTime)
                            .Skip(limit * (page - 1))
                            .Take(limit)
                            .ToList();

                return Ok(totalGoldSold);
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(new List<Transaction>());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("dashboard")]
        public ActionResult<DashBoardInfoResultModel> DashBoardBilgi()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            DashBoardInfoResultModel result = new DashBoardInfoResultModel { Success = false };

            try
            {
                // ok
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.Confirmed
                    && x.SourceType != "Fintag" && x.DestinationType != "Fintag")
                    .Select(x => x.GramAmount).Sum();

                var totalGoldSoldToday = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.Confirmed &&
                    x.SourceType != "Fintag" &&
                            x.TransactionDateTime.Date == DateTime.Now.Date &&
                            x.DestinationType != "Fintag")
                    .Select(x => x.GramAmount).Sum();

                var totalGoldSoldCC = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.Confirmed &&
                            (x.DestinationType == "VirtualPos" || x.DestinationType == "Wedding" ||
                            x.DestinationType == "Event"))
                    .Select(x => x.GramAmount).Sum();

                var totalGoldSoldCCToday = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.Confirmed &&
                            x.TransactionDateTime.Date == DateTime.Now.Date &&
                             (x.DestinationType == "VirtualPos" || x.DestinationType == "Wedding" ||
                            x.DestinationType == "Event"))
                    .Select(x => x.GramAmount).Sum();

                var totalGoldSoldEFT = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.Confirmed &&
                            x.DestinationType == "IBAN")
                    .Select(x => x.GramAmount).Sum();

                var totalGoldSoldEFTToday = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY" && x.Confirmed &&
                            x.TransactionDateTime.Date == DateTime.Now.Date &&
                            x.DestinationType == "IBAN")
                    .Select(x => x.GramAmount).Sum();

                var goldTransfers = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "GOLD" && x.SourceType == "User" && x.Confirmed &&
                            (x.DestinationType == "User" || x.DestinationType == "Wedding" || x.DestinationType == "Event"))
                    .Select(x => x.GramAmount).Sum();

                var goldTransfersToday = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "GOLD" && x.SourceType == "User" && x.Confirmed &&
                            x.TransactionDateTime.Date == DateTime.Now.Date &&
                            (x.DestinationType == "User" || x.DestinationType == "Wedding" || x.DestinationType == "Event"))
                    .Select(x => x.GramAmount).Sum();
                
                var goldBozdur = TransRepo.GetAllTransactions()
                    .Where(x => x.Confirmed && x.TransactionType == "TRY" && x.SourceType == "Fintag" && x.DestinationType == "User")
                    .Select(x => x.GramAmount).Sum();

                var goldBozdurToday = TransRepo.GetAllTransactions()
                    .Where(x => x.Confirmed && x.TransactionType == "TRY" && x.SourceType == "Fintag" &&
                    x.TransactionDateTime.Date == DateTime.Now.Date && x.DestinationType == "User")
                    .Select(x => x.GramAmount).Sum();
                // ============
                var totalMembers = Repository.GetAllUsers()
                    .Where(x => x.Role == "Member").Count();

                var totalMembersToday = Repository.GetAllUsers()
                    .Where(x => x.Role == "Member" && x.DateCreated.Date == DateTime.Now.Date).Count();

                var totalWeddings = EventsRepo.GetAllWeddings().Count();

                var totalWeddingToday = EventsRepo.GetAllWeddings()
                    .Where(x => x.DateCreated.Date == DateTime.Now.Date).Count();

                var totalEvents = EventsRepo.GetAllEvents().Count();

                var totalEventsToday = EventsRepo.GetAllEvents()
                    .Where(x => x.DateCreated.Date == DateTime.Now.Date).Count();

                result.DashBoard = new DashBoardInfo {
                    TotalGoldSold = totalGoldSold,
                    TotalGoldSoldToday = totalGoldSoldToday,
                    TotalGoldSoldCC = totalGoldSoldCC,
                    TotalGoldSoldTodayCC = totalGoldSoldCCToday,
                    TotalGoldSoldEFT = totalGoldSoldEFT,
                    TotalGoldSoldTodayEFT = totalGoldSoldEFTToday,
                    TotalGoldTransfer = goldTransfers,
                    TotalGoldTransferToday = goldTransfersToday,
                    TotalGoldBozdurma = goldBozdur,
                    TotalGoldBozdurmaToday = goldBozdurToday,
                    MemberCount = totalMembers,
                    NewMembers = totalMembersToday,
                    WeddingCount = totalWeddings,
                    WeddingCountToday = totalWeddingToday,
                    EventCount = totalEvents,
                    EventCountToday = totalEventsToday
                };
                result.Success = true;
                result.Message = "Request ready.";

            }
            catch (Exception e)
            {
                Log.Error("Exception at DashBoardBilgi(): " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = e.Message;
                result.Success = false;
            }


            return Ok(result);
        }


        [Authorize]
        [HttpGet]
        [Route("dashboard_silver")]
        public ActionResult<DashBoardInfoResultModelSilver> DashBoardBilgiSilver()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            DashBoardInfoResultModelSilver result = new DashBoardInfoResultModelSilver { Success = false };

            try
            {
                var totalGoldSold = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.Confirmed
                    && x.SourceType != "Fintag" && x.DestinationType != "Fintag")
                    .Select(x => x.GramAmount).Sum();

                var totalGoldSoldToday = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.Confirmed &&
                    x.SourceType != "Fintag" &&
                            x.TransactionDateTime.Date == DateTime.Now.Date &&
                            x.DestinationType != "Fintag")
                    .Select(x => x.GramAmount).Sum();

                var totalGoldSoldCC = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.Confirmed &&
                            (x.DestinationType == "VirtualPos" || x.DestinationType == "Wedding" ||
                            x.DestinationType == "Event"))
                    .Select(x => x.GramAmount).Sum();

                var totalGoldSoldCCToday = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.Confirmed &&
                            x.TransactionDateTime.Date == DateTime.Now.Date &&
                             (x.DestinationType == "VirtualPos" || x.DestinationType == "Wedding" ||
                            x.DestinationType == "Event"))
                    .Select(x => x.GramAmount).Sum();

                var totalGoldSoldEFT = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.Confirmed &&
                            x.DestinationType == "IBAN")
                    .Select(x => x.GramAmount).Sum();

                var totalGoldSoldEFTToday = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.Confirmed &&
                            x.TransactionDateTime.Date == DateTime.Now.Date &&
                            x.DestinationType == "IBAN")
                    .Select(x => x.GramAmount).Sum();

                var goldTransfers = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "SILVER" && x.SourceType == "User" &&
                            (x.DestinationType == "User" || x.DestinationType == "Wedding" || x.DestinationType == "Event"))
                    .Select(x => x.GramAmount).Sum();

                var goldTransfersToday = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "SILVER" && x.SourceType == "User" &&
                            x.TransactionDateTime.Date == DateTime.Now.Date &&
                            (x.DestinationType == "User" || x.DestinationType == "Wedding" || x.DestinationType == "Event"))
                    .Select(x => x.GramAmount).Sum();

                var goldBozdur = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.SourceType == "Fintag" && x.DestinationType == "User")
                    .Select(x => x.GramAmount).Sum();

                var goldBozdurToday = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionType == "TRY_FOR_SILVER" && x.SourceType == "Fintag" &&
                    x.TransactionDateTime.Date == DateTime.Now.Date && x.DestinationType == "User")
                    .Select(x => x.GramAmount).Sum();
                // ============

                result.DashBoard = new DashBoardInfoSilver
                {
                    TotalsilverSold = totalGoldSold,
                    TotalsilverSoldToday = totalGoldSoldToday,
                    TotalsilverSoldCC = totalGoldSoldCC,
                    TotalsilverSoldTodayCC = totalGoldSoldCCToday,
                    TotalsilverSoldEFT = totalGoldSoldEFT,
                    TotalsilverSoldTodayEFT = totalGoldSoldEFTToday,
                    TotalsilverTransfer = goldTransfers,
                    TotalsilverTransferToday = goldTransfersToday,
                    TotalsilverBozdurma = goldBozdur,
                    TotalsilverBozdurmaToday = goldBozdurToday
                };
                result.Success = true;
                result.Message = "Request ready.";

            }
            catch (Exception e)
            {
                Log.Error("Exception at DashBoardBilgi(): " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = e.Message;
                result.Success = false;
            }


            return Ok(result);
        }

        [HttpGet]
        [Route("oauth")]
        public ActionResult<string> OauthRedirect(string x)
        {
            return Ok("Ok");
        }



        [Authorize]
        [HttpGet]
        [Route("change_prices")]
        public ActionResult<string> ChangeGoldPrices(string name, string price)
        {

            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }
            if (price == null)
            {
                return Ok("price belirtilmedi");
            }

            if (name == null)
            {
                return Ok("name belirtilmeli");
            }
            else if (name == "expected_run")
            {
                try
                {
                    var amount = decimal.Parse(price);
                    var gprice = TransRepo.GetRobotStatus();
                    var old = gprice.Amount.Value;
                    gprice.Amount = amount;
                    TransRepo.SaveChanges();
                    return Ok("Eski Robot status: " + old + " - Yeni Status: " + gprice.Amount);
                }
                catch (Exception e)
                {
                    return Ok("Hata olustu: " + e.Message);
                }
            }
            else if (name == "SilverBuy")
            {
                try
                {
                    var amount = decimal.Parse(price);
                    var gprice = TransRepo.AccessSilverBuy();
                    var old = gprice.Amount.Value;
                    gprice.Amount = amount;
                    TransRepo.SaveChanges();
                    return Ok("Eski Silver Satış: " + old + " - Yeni Satış: " + gprice.Amount);
                } 
                catch (Exception e)
                {
                    return Ok("Hata olustu: " + e.Message);
                }
            }
            else if (name == "SilverSell")
            {
                try
                {
                    var amount = decimal.Parse(price);
                    var gprice = TransRepo.AccessSilverSell();
                    var old = gprice.Amount.Value;
                    gprice.Amount = amount;
                    TransRepo.SaveChanges();
                    return Ok("Eski Silver Alis: " + old + " - Yeni Satış: " + gprice.Amount);
                }
                catch (Exception e)
                {
                    return Ok("Hata olustu: " + e.Message);
                }
            }
            else if (name == "Buy")
            {             
                try
                {
                    var amount = decimal.Parse(price);

                    var gprice = TransRepo.AccessBuyPrice();
                    var old = gprice.Amount.Value;
                    gprice.Amount = amount;

                    TransRepo.SaveChanges();
                    return Ok("Eski Satış: " + old + " - Yeni Satış: " + gprice.Amount);
                }
                catch (Exception e)
                {
                    return Ok("hata oluştu:" + e.Message);
                }
            }
            else if (name == "Sell")
            {
                try
                {
                    var amount = decimal.Parse(price);

                    var gprice = TransRepo.AccessSellPrice();

                    var old = gprice.Amount.Value;
                    gprice.Amount = amount;

                    TransRepo.SaveChanges();

                    return Ok("Eski Satış: " + old + " - Yeni Satış: " + gprice.Amount);

                }
                catch (Exception e)
                {
                    return Ok("hata oluştu:" + e.Message);
                }
            }
            else if (name == "Kar")
            {
                try
                {
                    var amount = decimal.Parse(price);

                    var gprice = TransRepo.AccessKar();

                    var old = gprice.Amount.Value;
                    gprice.Amount = amount;

                    TransRepo.SaveChanges();

                    return Ok("Eski kar: " + old + " - Yeni kar: " + gprice.Amount);

                }
                catch (Exception e)
                {
                    return Ok("hata oluştu:" + e.Message);
                }
            }
            else if (name == "SatisKomisyon")
            {
                try
                {
                    var amount = decimal.Parse(price);

                    var gprice = TransRepo.AccessSatisKomisyon();

                    var old = gprice.Amount.Value;
                    gprice.Amount = amount;

                    TransRepo.SaveChanges();

                    return Ok("Eski SatisKomisyon: " + old + " - Yeni SatisKomisyon: " + gprice.Amount);

                }
                catch (Exception e)
                {
                    return Ok("hata oluştu:" + e.Message);
                }
            }
            else if (name == "automatic")
            {
                try
                {
                    var amount = decimal.Parse(price);

                    var gprice = TransRepo.AccessAutomatic();

                    var old = gprice.Amount.Value;
                    gprice.Amount = amount;

                    TransRepo.SaveChanges();

                    return Ok("Eski automatic: " + old + " - Yeni automatic: " + gprice.Amount);

                }
                catch (Exception e)
                {
                    return Ok("hata oluştu:" + e.Message);
                }

            }
            else if (name == "BankaMuaveleVergisi")
            {
                try
                {
                    var amount = decimal.Parse(price);

                    var gprice = TransRepo.AccessBankTax();

                    var old = gprice.Percentage.Value;
                    gprice.Percentage = amount;

                    TransRepo.SaveChanges();

                    return Ok("Eski BankaMuaveleVergisi: " + old + " - Yeni BankaMuaveleVergisi: " + gprice.Percentage.Value);

                }
                catch (Exception e)
                {
                    return Ok("hata oluştu:" + e.Message);
                }
            }
            else if (name == "SanalPosKomisyon")
            {
                try
                {
                    var amount = decimal.Parse(price);

                    var gprice = TransRepo.AccessVposCommission();

                    var old = gprice.Percentage.Value;
                    gprice.Percentage = amount;

                    TransRepo.SaveChanges();

                    return Ok("Eski vposkomisyon: " + old + " - Yeni vposkomisyon: " + gprice.Percentage.Value);

                }
                catch (Exception e)
                {
                    return Ok("hata oluştu:" + e.Message);
                }
            }
            else if (name == "BUY_PERCENTAGE")
            {
                try
                {
                    var amount = decimal.Parse(price);

                    var gprice = TransRepo.AccessBuyPercentage();

                    var old = gprice.Percentage.Value;
                    gprice.Percentage = amount;

                    TransRepo.SaveChanges();

                    return Ok("Eski BUY_PERCENTAGE: " + old + " - Yeni BUY_PERCENTAGE: " + gprice.Percentage.Value);

                }
                catch (Exception e)
                {
                    return Ok("hata oluştu:" + e.Message);
                }
            }
            else if (name == "SELL_PERCENTAGE")
            {
                try
                {
                    var amount = decimal.Parse(price);

                    var gprice = TransRepo.AccessSellPercentage();

                    var old = gprice.Percentage.Value;
                    gprice.Percentage = amount;

                    TransRepo.SaveChanges();

                    return Ok("Eski SELL_PERCENTAGE: " + old + " - Yeni SELL_PERCENTAGE: " + gprice.Percentage.Value);

                }
                catch (Exception e)
                {
                    return Ok("hata oluştu:" + e.Message);
                }
            }
            else if (name == "SELL_PERCENTAGE_SILVER")
            {
                try
                {
                    var amount = decimal.Parse(price);

                    var gprice = TransRepo.AccessSellPercentageSilver();

                    var old = gprice.Percentage.Value;
                    gprice.Percentage = amount;

                    TransRepo.SaveChanges();

                    return Ok("Eski SELL_PERCENTAGE_SILVER: " + old + " - Yeni SELL_PERCENTAGE_SILVER: " + gprice.Percentage.Value);

                }
                catch (Exception e)
                {
                    return Ok("hata olustu: " + e.Message);
                }
            }

            else if (name == "BUY_PERCENTAGE_SILVER")
            {
                try
                {
                    var amount = decimal.Parse(price);

                    var gprice = TransRepo.AccessBuyPercentageSilver();

                    var old = gprice.Percentage.Value;
                    gprice.Percentage = amount;

                    TransRepo.SaveChanges();

                    return Ok("Eski BUY_PERCENTAGE_SILVER: " + old + " - Yeni BUY_PERCENTAGE_SILVER: " + gprice.Percentage.Value);

                }
                catch (Exception e)
                {
                    return Ok("hata olustu: " + e.Message);
                }
            }

            else
            {
                return Ok("name hatalı: Sell / Buy / Kar / BankaMuaveleVergisi / SanalPosKomisyon . (case sensitive)");
            }

        }

        [HttpGet]
        public ActionResult<string> Get(string code)
        {
            Log.Information("Callback requested");
            if (code != null)
            {
                KuwaitAccessHandler.AuthorizationCode = code;
                Log.Information("Received auth code " + code);
            }

            return Ok(code);
        }

        /*
        [HttpGet]
        [Route("sipay_pay_test")]
        public ActionResult<GetPosResponseModel> SiPayTest(decimal creditCard, decimal amount)
        {
            var sipay = SiPay.StartServices();

            var model = new GetPosParamModel { Amount = amount, CreditCard = creditCard, CurrencyCode = "TRY" };

           // var pos = sipay.GetVirtualPos(model).Result;

            return Ok(null);
        }

        [HttpGet]
        [Route("redir")]
        public ActionResult<string> SiPayRedirect()
        {
            


            return Ok("sipay normal redirect yedik");
        }*/

        

        [Authorize]
        [HttpGet]
        [Route("arc_test")]
        public async Task<ActionResult> TestMethod(string reference)
        {

            var model = new TransactionReceiptParams { TransactionReference = reference };

            var content = await KTApiService.GetReceiptAsync(model);

            return Ok(content);
            //return Ok(KTApiService.GetCurrentPriceFromKtApi());
            /*
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string id;

 
            var html = "<!DOCTYPE html>" +
            "<html>" +
            "<body style=\"background-color:powderblue; \"> " +
            "<h1> This is a heading </h1>" +
            "<p> This is a paragraph.</p>" +
            "</body>" +
            "</html>";

            return Content(html, "text/html");*/
        }


        [Authorize]
        [HttpGet]
        [Route("deny_eft_sell")]
        public ActionResult<string> DenyEftSell(string transId)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var transaction = TransRepo
                    .GetAllTransactions()
                    .Where(x => x.TransactionId.ToString() == transId)
                    .FirstOrDefault();
                if (transaction == null)
                {
                    return Ok("Hatalı istek, transId doğru mu kontrol ediniz.");
                }
                transaction.Cancel("Admin onaylamadi");
                TransRepo.SaveChanges();

                return Ok("Islem basariyla iptal edilmistir.");
            }
            catch (Exception e)
            {
                Log.Error("Exception at deny eft sell: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok("Hara olustu: " + e.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("confirm_eft_sell")]
        public ActionResult<string> ConfirmEftSell(string transId, decimal odendi)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var transaction = TransRepo
                    .GetAllTransactions()
                    .Where(x => x.TransactionId.ToString() == transId)
                    .FirstOrDefault();
                if (transaction == null)
                {
                    return Ok("Hatalı istek, transId doğru mu kontrol ediniz.");
                }
                if (transaction.Comment == "HALLEDILIYOR")
                {
                    return Ok("Bu islem su an zaten hallediliyor.");
                }
                if (transaction.Comment == "DONE")
                {
                    return Ok("Bu islem zaten bitmis.");
                }

                var tCommand = transaction.Comment;

                transaction.Comment = "HALLEDILIYOR";
                TransRepo.SaveChanges();
                

                var user = Repository
                    .GetAllUsers()
                    .Where(x => x.UserId.ToString() == transaction.Source)
                    .FirstOrDefault();
                
                if (user == null)
                {
                    return Ok("Hatalı istek, hareketle alakalı kullanıcı bulunamadı.");
                }
                transaction.Confirm();

                if (user.BlockedGrams.HasValue)
                {
                    var blocked = user.BlockedGrams.Value;

                    user.BlockedGrams = user.BlockedGrams.Value - transaction.GramAmount;
                    if (user.BlockedGrams.Value == 0)
                    {
                        user.BlockedGrams = null;
                    }
                }
                user.Balance -= transaction.GramAmount;

                transaction.Comment = "DONE";

                var tlTransactin = new Transaction("TRY", 
                    "D94AF0AD-5FE9-4A6C-949E-C66AEF21C907", 
                    "Fintag", 
                    user.UserId.ToString(), 
                    "User", 
                    odendi, 
                    "Kullanıcı altın bozdurdu", 
                    true, 
                    transaction.GramAmount, 
                    transaction.TlAmount);
                TransRepo.AddTransaction(tlTransactin);
                TransRepo.SaveChanges();

                var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";
                var message = string.Format("{0}TRY Banka hesabınıza aktarılmıştır.", transaction.TlAmount);
                var notification = new Notification2(user.UserId, message, false, "info", null, photo);
                TransRepo.AddNotification(notification);
                TransRepo.SaveChanges();
                Repository.SaveChanges();
                return Ok("Kullanıcıdan " + transaction.GramAmount + "kadar altın alındı, kullanıcının total balance: " + user.Balance);

            }
            catch (Exception e)
            {
                Log.Error("Exception at ConfirmEftSell: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok("Hata oluştu :" + e.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("confirm_eft_sell_silver")]
        public ActionResult<string> ConfirmEftSellSilver(string transId, decimal odendi)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
                var transaction = TransRepo
                    .GetAllTransactions()
                    .Where(x => x.TransactionId.ToString() == transId)
                    .FirstOrDefault();
                if (transaction == null)
                {
                    return Ok("Hatalı istek, transId doğru mu kontrol ediniz.");
                }
                if (transaction.Comment == "HALLEDILIYOR")
                {
                    return Ok("Bu islem su an zaten hallediliyor.");
                }
                if (transaction.Comment == "DONE")
                {
                    return Ok("Bu islem zaten bitmis.");
                }

                var tCommand = transaction.Comment;

                transaction.Comment = "HALLEDILIYOR";
                TransRepo.SaveChanges();


                var user = Repository
                    .GetAllUsers()
                    .Where(x => x.UserId.ToString() == transaction.Source)
                    .FirstOrDefault();

                if (user == null)
                {
                    return Ok("Hatalı istek, hareketle alakalı kullanıcı bulunamadı.");
                }
                transaction.Confirm();


                var silverBalance = Repository.GetSilverBalance(user.UserId);
                if (silverBalance == null)
                {
                    return Ok("Kullanici silverbalanci yok, error!!");
                }


                if (silverBalance.BlockedGrams.HasValue)
                {
                    var blocked = silverBalance.BlockedGrams.Value;

                    silverBalance.BlockedGrams = silverBalance.BlockedGrams.Value - transaction.GramAmount;
                    if (silverBalance.BlockedGrams.Value == 0)
                    {
                        silverBalance.BlockedGrams = null;
                    }
                }
                silverBalance.Balance -= transaction.GramAmount;

                transaction.Comment = "DONE";

                var tlTransactin = new Transaction("TRY_FOR_SILVER",
                    "D94AF0AD-5FE9-4A6C-949E-C66AEF21C907",
                    "Fintag",
                    user.UserId.ToString(),
                    "User",
                    odendi,
                    "Kullanıcı silver bozdurdu",
                    true,
                    transaction.GramAmount,
                    transaction.TlAmount);
                TransRepo.AddTransaction(tlTransactin);
                TransRepo.SaveChanges();

                var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";
                var message = string.Format("{0}TRY Banka hesabınıza aktarılmıştır.", transaction.TlAmount);
                var notification = new Notification2(user.UserId, message, false, "info", null, photo);
                TransRepo.AddNotification(notification);
                TransRepo.SaveChanges();
                Repository.SaveChanges();
                return Ok("Kullanıcıdan " + transaction.GramAmount + "kadar silver alındı, kullanıcının total balance: " + silverBalance.Balance);

            }
            catch (Exception e)
            {
                Log.Error("Exception at ConfirmEftSellSilver: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok("Hata oluştu :" + e.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("deny_eft")]
        public ActionResult<string> DenyBankPayment(string ubt, string ntr, string tid)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {

                var banktransfer = TransRepo.GetAllUserBankTransferRequests()
                .Where(x => x.BankTransferId.ToString() == ubt)
                .FirstOrDefault();

                if (banktransfer == null)
                {
                    return Ok("tanımlanamayan used bank transfer id: " + ubt);
                }
                var normTransfer = TransRepo.GetAllTransferRequests()
                    .Where(x => x.TransferRequestId.ToString() == ntr)
                    .FirstOrDefault();

                if (normTransfer == null)
                {
                    return Ok("tanımlanamayan normal transfer id: " + ntr);
                }

                if (banktransfer.MoneyReceived)
                {
                    return Ok("zaten onaylanmis şu tarihte: " + normTransfer.ConfirmationDateTime.ToString());
                }

                var transaction = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionId.ToString() == tid)
                    .FirstOrDefault();

                if (transaction == null)
                {
                    return Ok("tanımlanamayan transaction id: " + tid);
                }

                transaction.Cancel("Onaylanmadi");
                TransRepo.RemoveUserBankTransferRequest(banktransfer);
                TransRepo.RemoveTransferRequest(normTransfer);
                TransRepo.RemoveTransaction(transaction);
                
                TransRepo.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Error("error at confirm etf: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(e.Message + "\n" + e.StackTrace);
            }


            return Ok("ISLEM BASARILI");
        }

        [Authorize]
        [HttpGet]
        [Route("confirm_eft")]
        public ActionResult<string> ConfirmBankPayment(string ubt, string ntr, string tid)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {
    
                var banktransfer = TransRepo.GetAllUserBankTransferRequests()
                .Where(x => x.BankTransferId.ToString() == ubt)
                .FirstOrDefault();

                if (banktransfer == null)
                {
                    return Ok("tanımlanamayan used bank transfer id: " + ubt);
                }
                var normTransfer = TransRepo.GetAllTransferRequests()
                    .Where(x => x.TransferRequestId.ToString() == ntr)
                    .FirstOrDefault();

                if (normTransfer == null)
                {
                    return Ok("tanımlanamayan normal transfer id: " + ntr);
                }

                if (banktransfer.MoneyReceived)
                {
                    return Ok("zaten onaylanmis şu tarihte: " + normTransfer.ConfirmationDateTime.ToString());
                }

                var transaction = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionId.ToString() == tid)
                    .FirstOrDefault();

                if (transaction == null)
                {
                    return Ok("tanımlanamayan transaction id: " + tid);
                }


                var user = Repository.GetAllUsers()
                    .Where(x => x.UserId.ToString() == transaction.Source)
                    .FirstOrDefault();

               
  
                if (user == null)
                {
                    return Ok("Bulunamayan user: " + transaction.Source);
                }

                
                var goldTransaction = new Transaction(
                    "GOLD",
                    "D94AF0AD-5FE9-4A6C-949E-C66AEF21C907", 
                    "Fintag",
                    user.UserId.ToString(),
                    "User",
                    normTransfer.GramsOfGold,
                    "EFT ile altın alımı",
                    true,
                    normTransfer.GramsOfGold,
                    transaction.Amount);
                goldTransaction.YekunDestination = transaction.Yekun + normTransfer.GramsOfGold;
                
                user.ManipulateBalance(normTransfer.GramsOfGold);
                banktransfer.MoneyReceived = true;
                normTransfer.CompleteTransfer();
                transaction.Confirm();
                Repository.SaveChanges();
                TransRepo.AddTransaction(goldTransaction);
                var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";
                var message = string.Format("{0}gr Goldtag hesabınıza eklenmiştir.", normTransfer.GramsOfGold);
                var notification = new Notification2(user.UserId, message, false, "info", null, photo);
                TransRepo.AddNotification(notification);
                TransRepo.SaveChanges();
            } 
            catch (Exception e)
            {
                Log.Error("error at confirm etf: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(e.Message + "\n" + e.StackTrace);
            }
            

            return Ok("ISLEM BASARILI");
        }


        [Authorize]
        [HttpGet]
        [Route("confirm_eft_silver")]
        public ActionResult<string> ConfirmBankPaymentSilver(string ubt, string ntr, string tid)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.Role != "Admin")
            {
                return Unauthorized();
            }

            try
            {

                var banktransfer = TransRepo.GetAllUserBankTransferRequests()
                .Where(x => x.BankTransferId.ToString() == ubt)
                .FirstOrDefault();

                if (banktransfer == null)
                {
                    return Ok("tanımlanamayan used bank transfer id: " + ubt);
                }
                var normTransfer = TransRepo.GetAllTransferRequests()
                    .Where(x => x.TransferRequestId.ToString() == ntr)
                    .FirstOrDefault();

                if (normTransfer == null)
                {
                    return Ok("tanımlanamayan normal transfer id: " + ntr);
                }

                if (banktransfer.MoneyReceived)
                {
                    return Ok("zaten onaylanmis şu tarihte: " + normTransfer.ConfirmationDateTime.ToString());
                }

                var transaction = TransRepo.GetAllTransactions()
                    .Where(x => x.TransactionId.ToString() == tid)
                    .FirstOrDefault();

                if (transaction == null)
                {
                    return Ok("tanımlanamayan transaction id: " + tid);
                }


                var user = Repository.GetAllUsers()
                    .Where(x => x.UserId.ToString() == transaction.Source)
                    .FirstOrDefault();



                if (user == null)
                {
                    return Ok("Bulunamayan user: " + transaction.Source);
                }

                var silverBalance = Repository.GetSilverBalance(user.UserId);
                if (silverBalance == null)
                {
                    return Ok("Kullanicinin silver balance i yok nasil alabilir? Bugo lmus olabilir arda yi arayin");
                }

                var silverTransaction = new Transaction(
                    "SILVER",
                    "D94AF0AD-5FE9-4A6C-949E-C66AEF21C907",
                    "Fintag",
                    user.UserId.ToString(),
                    "User",
                    normTransfer.GramsOfGold,
                    "EFT ile gümüş alımı",
                    true,
                    normTransfer.GramsOfGold,
                    transaction.Amount);
                silverTransaction.YekunDestination = transaction.Yekun + normTransfer.GramsOfGold;
                silverBalance.Balance += normTransfer.GramsOfGold;
                banktransfer.MoneyReceived = true;
                normTransfer.CompleteTransfer();
                transaction.Confirm();
                Repository.SaveChanges();
                TransRepo.AddTransaction(silverTransaction);
                var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";
                var message = string.Format("{0}gr Goldtag gümüş hesabınıza eklenmiştir.", normTransfer.GramsOfGold);
                var notification = new Notification2(user.UserId, message, false, "info", null, photo);
                TransRepo.AddNotification(notification);
                TransRepo.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Error("error at confirm etf: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok(e.Message + "\n" + e.StackTrace);
            }


            return Ok("ISLEM BASARILI");
        }

        /*
        [HttpGet]
        [Route("success")]
        public ActionResult<string> SiPaySuccess(int sipay_status, int order_id, string invoice_id)
        {
            Log.Information("CALLBACK SIPAY SUCCESS");

            
            var transaction = TransRepo.GetAllTransactions().Where(t => t.TransactionId.ToString() == invoice_id).FirstOrDefault();

            if (transaction == null)
            {
                Log.Error("ERR-TRans bulunamadı DB-TR-01:" + invoice_id);
                return  Ok("Sistemde bir hata oluştu lütfen GoldTag ı arayınız: \n" +
                    "Error Kod: DB-TR-01\n" +
                    "TransId:" + invoice_id
                    );
            }
            else
            {
                var user = Repository.GetAllUsers().Where(u => u.UserId.ToString() == transaction.Source).FirstOrDefault();

                if (user == null)
                {
                    Log.Error("ERR-User bulunamadı DB-TR-02: " + transaction.Source);
                    return Ok("Sistemde bir hata oluştu lütfen GoldTag ı arayınız: \n" +
                    "Error Kod: DB-TR-02\n" +
                    "Uid: " + transaction.Source);
                    
                }
                else
                {
                    var goldTransaction = new Transaction("GOLD",
                        "Fintag",
                        "Fintag",
                        user.UserId.ToString(),
                        "User",
                        transaction.GramAmount,
                        "Kredi kartı ile altin",
                        true,
                        transaction.GramAmount,
                        transaction.TlAmount);

                    
                    user.ManipulateBalance(transaction.GramAmount);
                    Repository.SaveChanges();
                    transaction.Confirm();
                    TransRepo.AddTransaction(goldTransaction);
                    TransRepo.SaveChanges();
                    Log.Information("User a Gold Verildi: userid" + user.UserId + " / transid " + invoice_id);
                    

                }
            }
            
            return Ok("Tebrikler, Altın alımı başarılı bu ekranı kapatabilirsiniz: \nReferans: " + invoice_id);
        }

        [HttpGet]
        [Route("fail")]
        public ActionResult<string> SiPayFail(int sipay_status, int order_id, int invoice_id, string error)
        {
            Log.Information("callback fail: " + error);
            var res = "Error: " + error + "\nsipaystatus:" + sipay_status + "\n";

            res += "order_id:" + order_id + "\n";

            res += "invoice_id:" + invoice_id;

            Log.Information(res);
            return Ok(res);
        }
        */

        [HttpGet]
        [Route("verify")]
        public ActionResult SignUpVerification(string id)
        {

            try
            {
                var user = Repository.GetAllUsers()
                    .Where(u => u.MemberId == int.Parse(id))
                    .FirstOrDefault();
                user.VerifySignUp();
                var name = user.FirstName + " " + user.FamilyName;
                Repository.SaveChanges();
                var html = string.Format(SIGNUP_VERFIED, user.MemberId);
                return Content(html, "text/html");
            }
            catch (Exception e)
            {
                return Content(SIGNUP_VERFIED_FAIL, "text/html");
            }
            
            // TODO: Make this method return View() and add a new MVC View to the project
            //return Ok("Sayın " + name + " Goldtag'e hoşgeldiniz.");
        }

        [HttpGet]
        [Route("email_verify")]
        public ActionResult<string> EmailVerification(string id)
        {
            Log.Information("EmailVerification() " + id);
            string name = "";
            try
            {

                var change = Repository.GetAllChanges().Where(x => x.ChangeId.ToString() == id).FirstOrDefault();
                if (change == null)
                {
                    throw new Exception("Id hatalı");
                }

                var user = Repository.GetAllUsers()
                    .Where(u => u.UserId == change.UserId)
                    .FirstOrDefault();

                user.Email = change.NewValue;
                user.Banned = false;
                // Todo add verification true false to Chane

                name = user.FirstName + " " + user.FamilyName;
                Repository.SaveChanges();

                var html = string.Format(EMAIL_VERFIED, name, user.MemberId);
                return Content(html, "text/html");

            }
            catch (Exception e)
            {
                Log.Error("error at email verify: " + e.Message);
                Log.Error(e.StackTrace);

                return Content(SIGNUP_VERFIED_FAIL, "text/html");
            }

            // TODO: Make this method return View() and add a new MVC View to the project
            //return Ok("Sayın " + name + "Emailiniz değişmiştir.");
        }

        [HttpPost]
        [Route("kart_onay_event")]
        public ActionResult KtSuccEvent()
        {
            Log.Information("KT VPOS - SUCCESS");
            try
            {
                var response = Request.Form["AuthenticationResponse"];
                var res = System.Web.HttpUtility.UrlDecode(response);

                var xm = new XmlSerializer(typeof(VPosTransactionResponseContract));
                var model = new VPosTransactionResponseContract();
                using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(res)))
                {
                    model = xm.Deserialize(ms) as VPosTransactionResponseContract;
                }

                if (int.Parse(model.ResponseCode) == 0)
                {
                    var provizyonResponse = VposService.Provizyon(model);

                    using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(provizyonResponse)))
                    {
                        model = xm.Deserialize(ms) as VPosTransactionResponseContract;
                        if (int.Parse(model.ResponseCode) == 0)
                        {
                            var transactionTry = TransRepo.GetAllTransactions().Where(x => x.TransactionId.ToString() == model.MerchantOrderId).FirstOrDefault();
                            if (transactionTry == null)
                            {
                                return Ok("Kredi karti islemi basarili fakat sistem hatasi, lutfen call center i arayiniz. Referans Transaction Id =" + model.MerchantOrderId);
                            }
                            transactionTry.Confirm();
                            var user = TransRepo.GetAllUsers().Where(x => x.UserId.ToString() == transactionTry.Source).FirstOrDefault();

                            string destination;
                            string eventName;
                            string destType;
                            string createdBy;
                            int ecode;
                            if (transactionTry.DestinationType == "Event")
                            {
                                var _event = TransRepo.GetAllEvents().Where(x => x.EventId.ToString() == transactionTry.Destination).FirstOrDefault();
                                destination = _event.EventId.ToString();
                                eventName = _event.EventName;
                                destType = "Event";
                                createdBy = _event.CreatedBy.ToString();
                                ecode = _event.EventCode;
                                _event.BalanceInGold += transactionTry.GramAmount;
                            } else
                            {
                                var wedding = TransRepo.GetAllWeddings().Where(x => x.WeddingId.ToString() == transactionTry.Destination).FirstOrDefault();
                                destination = wedding.WeddingId.ToString();
                                eventName = wedding.WeddingName;
                                destType = "Wedding";
                                createdBy = wedding.CreatedBy.ToString();
                                ecode = wedding.WeddingCode;
                                wedding.BalanceInGold += transactionTry.GramAmount;
                            }
                            
                            var destinationUser = TransRepo.GetAllUsers()
                                .Where(xm => xm.UserId.ToString() == createdBy)
                                .FirstOrDefault();

                            destinationUser.Balance += transactionTry.GramAmount;

                            var random = new Random();

                            var randString = "Event Wedding ARC: " + random.Next(0, 9999) + " " + DateTime.Now;
                            var goldTransaction = new Gold.Core.Transactions.Transaction(
                                "GOLD", 
                                user.UserId.ToString(),
                                "User",
                                destination,
                                destType,
                                transactionTry.GramAmount,
                                randString,
                                true,
                                transactionTry.GramAmount,
                                transactionTry.TlAmount);

                            TransRepo.AddTransaction(goldTransaction);
                            TransRepo.SaveChanges();

                            goldTransaction = TransRepo.GetAllTransactions()
                                .Where(x => x.Comment == randString).FirstOrDefault();
                            
                            var transferRequest = new Gold.Core.Transactions.TransferRequest(
                                    user.UserId,
                                    goldTransaction.GramAmount,
                                    destType,
                                    destination,
                                    goldTransaction.TransactionId,
                                    randString);

                            transferRequest.CompleteTransfer();
                            TransRepo.AddTransferRequest(transferRequest);
                            
                            TransRepo.SaveChanges();

                            transferRequest = TransRepo
                                .GetAllTransferRequests()
                                .Where(x => x.Comments == randString).FirstOrDefault();


                            var destMessage = string.Format("{0} isimli düğününüze, {1} tarafindan {2}gr altın gönderildi.", 
                                eventName, user.FirstName + " " + user.FamilyName, transferRequest.GramsOfGold);

                            var photo = "http://www.fintag.net/images/wedding_photos/" + ecode + ".jpg";
                            var srcMessage = string.Format("{0} isimli düğüne, {1}gr altın gönderdiniz.",
                                eventName, transferRequest.GramsOfGold);
                            var id = transferRequest.TransferRequestId.ToString();
                            TransRepo.AddNotification(new Notification2(Guid.Parse(createdBy), destMessage, true, "transfer", id, photo));
                            TransRepo.AddNotification(new Notification2(user.UserId, srcMessage, true, "transfer", id, photo));


                            transferRequest.Comments = "Kredi kartı ile etkinliğe altın transferi.";
                            goldTransaction.Comment =  "Kredi kartı ile etkinliğe altın transferi.";
                            TransRepo.SaveChanges();

                            var result = string.Format("{0} isimli etkinlike {1}gr. aktarilmistir.", eventName, transactionTry.GramAmount);
                            var html = string.Format(VPOS_FORMAT, result);
                            return Content(html, "text/html");
                        }
                        else
                        {
                            return Ok("Islem onaylanmadi : " + model.ResponseMessage);
                        }
                    }



                }
                else
                {
                    var message = "İşlem onaylanmadı " + model.ResponseMessage;
                    var html = string.Format(VPOS_FORMAT, message);
                    return Content(html, "text/html");
                }

            }
            catch (Exception e)
            {
                Log.Error("Exception at ktsuccess");
                Log.Error(e.StackTrace);
                var message = "İşlem hatası lütfen tekrar deneyiniz";
                var html = string.Format(VPOS_FORMAT, message);
                return Content(html, "text/html");
            }

        }

        [HttpPost]
        [Route("kart_onay_normal")]
        public ActionResult KtSuccNormal()
        {
            Log.Information("KT VPOS - SUCCESS");
            try
            {
                var response = Request.Form["AuthenticationResponse"];
                var res = System.Web.HttpUtility.UrlDecode(response);

                var xm = new XmlSerializer(typeof(VPosTransactionResponseContract));
                var model = new VPosTransactionResponseContract();
                using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(res)))
                {
                    model = xm.Deserialize(ms) as VPosTransactionResponseContract;
                }

                if (int.Parse(model.ResponseCode) == 0)
                {
                    var provizyonResponse = VposService.Provizyon(model);

                    using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(provizyonResponse)))
                    {
                        model = xm.Deserialize(ms) as VPosTransactionResponseContract;
                        if (int.Parse(model.ResponseCode) == 0)
                        {
                            var transactionTry = TransRepo.GetAllTransactions().Where(x => x.TransactionId.ToString() == model.MerchantOrderId).FirstOrDefault();
                            if (transactionTry == null)
                            {
                                return Ok("Kredi kartı işlemi başarılı fakat sistem hatası, lütfen destek hattını arayınız. Referans: " + model.MerchantOrderId);
                            }
                            var user = TransRepo.GetAllUsers().Where(x => x.UserId.ToString() == transactionTry.Source).FirstOrDefault();
                            var goldTransaction = new Gold.Core.Transactions.Transaction(
                                "GOLD", 
                                "D94AF0AD-5FE9-4A6C-949E-C66AEF21C907",
                                "Fintag",
                                user.UserId.ToString(),
                                "User",
                                transactionTry.GramAmount,
                                "Kredi kartı ile altın",
                                true,
                                transactionTry.GramAmount,
                                transactionTry.TlAmount);

                            user.Balance += transactionTry.GramAmount;
                            transactionTry.Confirm();

                            TransRepo.AddTransaction(goldTransaction);
                            TransRepo.SaveChanges();
                            var message = "Hesabınıza " + goldTransaction.GramAmount + " gram Altın eklenmiştir.";
                            var html = string.Format(VPOS_FORMAT, message);
                            return Content(html, "text/html");
                        } 
                        else
                        {
                            var message = "İşlem onaylanmadı " + model.ResponseMessage;
                            var html = string.Format(VPOS_FORMAT, message);
                            return Content(html, "text/html");
                        }
                    }

                    
                    
                }
                else
                {
                    Log.Information("KT VPOS -Card declined");
                    return Ok("İşlem onaylanmadı: " + model.ResponseMessage);
                }

            } catch (Exception e)
            {
                Log.Error("Exception at ktsuccess");
                Log.Error(e.StackTrace);
                var message = "İşlem hatası lütfen tekrar deneyiniz";
                var html = string.Format(VPOS_FORMAT, message);
                return Content(html, "text/html");
            }

        }

        [HttpPost]
        [Route("kart_onay_silver")]
        public ActionResult KtSuccSilver()
        {
            Log.Information("KT VPOS SILVER - SUCCESS");
            try
            {
                var response = Request.Form["AuthenticationResponse"];
                var res = System.Web.HttpUtility.UrlDecode(response);

                var xm = new XmlSerializer(typeof(VPosTransactionResponseContract));
                var model = new VPosTransactionResponseContract();
                using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(res)))
                {
                    model = xm.Deserialize(ms) as VPosTransactionResponseContract;
                }

                if (int.Parse(model.ResponseCode) == 0)
                {
                    var provizyonResponse = VposService.Provizyon(model);

                    using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(provizyonResponse)))
                    {
                        model = xm.Deserialize(ms) as VPosTransactionResponseContract;
                        if (int.Parse(model.ResponseCode) == 0)
                        {
                            var transactionTry = TransRepo.GetAllTransactions().Where(x => x.TransactionId.ToString() == model.MerchantOrderId).FirstOrDefault();
                            if (transactionTry == null)
                            {
                                return Ok("Kredi kartı işlemi başarılı fakat sistem hatası, lütfen destek hattını arayınız. Referans: " + model.MerchantOrderId);
                            }
                            var user = TransRepo.GetAllUsers().Where(x => x.UserId.ToString() == transactionTry.Source).FirstOrDefault();
                            var silverTrans = new Gold.Core.Transactions.Transaction(
                                "SILVER",
                                "D94AF0AD-5FE9-4A6C-949E-C66AEF21C907",
                                "Fintag",
                                user.UserId.ToString(),
                                "User",
                                transactionTry.GramAmount,
                                "Kredi kartı ile gümüş",
                                true,
                                transactionTry.GramAmount,
                                transactionTry.TlAmount);

                            var silverBalance = TransRepo.GetSilverBalance(user.UserId);
                            if (silverBalance == null)
                            {
                                silverBalance = new Gold.Core.Transactions.SilverBalance(user.UserId);
                                TransRepo.AddSilverBalance(silverBalance);
                            }

                            silverBalance.Balance += transactionTry.GramAmount;
                            transactionTry.Confirm();

                            TransRepo.AddTransaction(silverTrans);
                            TransRepo.SaveChanges();
                            var message = "Hesabınıza " + silverTrans.GramAmount + " gram Gümüş eklenmiştir.";
                            var html = string.Format(VPOS_FORMAT, message);
                            return Content(html, "text/html");
                        }
                        else
                        {
                            var message = "İşlem onaylanmadı " + model.ResponseMessage;
                            var html = string.Format(VPOS_FORMAT, message);
                            return Content(html, "text/html");
                        }
                    }



                }
                else
                {
                    Log.Information("KT VPOS SILVER - Card declined");
                    return Ok("İşlem onaylanmadı: " + model.ResponseMessage);
                }

            }
            catch (Exception e)
            {
                Log.Error("Exception at ktsuccess");
                Log.Error(e.StackTrace);
                var message = "İşlem hatası lütfen tekrar deneyiniz";
                var html = string.Format(VPOS_FORMAT, message);
                return Content(html, "text/html");
            }

        }

        [HttpPost]
        [Route("ktfail")]
        public ActionResult<string> KtFail()
        {
            var response = Request.Form["AuthenticationResponse"];
            var res = System.Web.HttpUtility.UrlDecode(response);

            var xm = new XmlSerializer(typeof(VPosTransactionResponseContract));
            var model = new VPosTransactionResponseContract();
            using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(res)))
            {
                model = xm.Deserialize(ms) as VPosTransactionResponseContract;
            }

            return Ok("İşlem onaylanmadı: " + model.ResponseMessage);
        }



        
        [HttpGet]
        [Route("admin_code")]
        public ActionResult<LoginUserResultModel> LoginAdminCode(string sms_code)
        {
            var result = new LoginUserResultModel { Success = false };
            try
            {
                var ip = Request.HttpContext.Connection.RemoteIpAddress;
                /*
                if (ip.ToString() != "213.254.130.140")
                {
                    result.Message = "Wrong IP source";
                    return Ok(result);
                }*/

                var bannedIp = Repository.GetAllBannedIps().Where(x => x.IP == ip.ToString()).FirstOrDefault();

                if (bannedIp != null)
                {
                    result.Message = "Yasaklı IP adresi.";
                    return Ok(result);
                }

                if (sms_code == null)
                {
                    result.Message = "null sms_code";
                    return Ok(result);
                }
                var token = AdminHelperService.ValidateSMSCode(sms_code);

                if (token == "INVALID")
                {
                    result.Message = "invalid sms code";
                    return Ok(result);
                }

                string user_id;
                if (!Authenticator.ValidateToken(token, out user_id))
                {
                    result.Message = "invalid token";
                    return Ok(result);
                }
                var gid = Guid.Parse(user_id);
                var usr = Repository.GetAllUsers()
                    .Where(x => x.UserId == gid)
                    .FirstOrDefault();


                if (usr == null)
                {
                    result.Message = "invalid user";
                    return Ok(result);
                }

                if (usr.AdminNotes != null && usr.AdminNotes == "TEMP_BAN")
                {
                    result.Message = "Bu hesap geçici olarak yasaklıdır, lütfen müşteri hizmetlerini arayınız.";
                    return Ok(result);
                }
                if (usr.Banned)
                {
                    result.Message = "Yasaklı hesap.";
                    return Ok(result);
                }
                if (usr.Role != "Admin")
                {
                    result.Message = "Erişim hakkı yok.";
                    return Ok(result);
                }

                var date = DateTime.Now;
                var random = new Random();
                var rand = ip.ToString() + " : " + random.Next(0, 9999999);
                var login = new Login(usr.UserId, ip.ToString(), date, rand);

                Repository.AddLogin(login);
                Repository.SaveChanges();
                login = Repository
                    .GetAllLogins()
                    .Where(z => z.IP == ip.ToString() && z.Random != null && z.Random == rand && z.UserId == usr.UserId).FirstOrDefault();

                var name = (usr.FirstName.Length + 1 + usr.FamilyName.Length > 18) ? "Sn. " + usr.FamilyName : usr.FirstName + " " + usr.FamilyName;
                if (name.Length > 18)
                {
                    name = name.Substring(0, 14) + "...";
                }
                var user = new UserModel
                {
                    UserId = usr.UserId.ToString(),
                    Name = name,
                    Email = usr.Email,
                    LoginId = login.LoginId.ToString(),
                };
                return Ok(new LoginUserResultModel
                {
                    Success = true,
                    Message = "Sms validated",
                    AuthToken = token,
                    User = user
                });
            }
            catch (Exception e)
            {
                Log.Error("Exception at LoginAdminSmsCode() - " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu lütfen da sonra tekrar deneyiniz";
            }

            return Ok(result);

        }


        
        [HttpPost]
        [Route("admin_login")]
        public ActionResult<LoginUserResultModel> LoginAdminUser(LoginUserModel model)
        {
            var result = new LoginUserResultModel { Success = false };
            try
            {
                var ip = Request.HttpContext.Connection.RemoteIpAddress;
                /*
                if (ip.ToString() != "213.254.130.140")
                {
                    result.Message = "Wrong IP source";
                    return Ok(result);
                }*/

                var bannedIp = Repository.GetAllBannedIps().Where(x => x.IP == ip.ToString()).FirstOrDefault();

                if (bannedIp != null)
                {
                    result.Message = "Yasaklı IP adresi.";
                    return Ok(result);
                }

                if (model == null || model.Email == null || model.Password == null)
                {

                    result.Message = "Gelen parametreler null";
                    return Ok(result);
                }

                
                var usr = Repository.GetAllUsers()
                    .Where(x => x.Email.ToLower() == model.Email.ToLower())
                    .FirstOrDefault();


                if (usr == null)
                {
                    result.Message = "Email Hatalı: " + model.Email;
                    return Ok(result);
                }

                if (usr.AdminNotes != null && usr.AdminNotes == "TEMP_BAN")
                {
                    result.Message = "Bu hesap geçici olarak yasaklıdır, lütfen müşteri hizmetlerini arayınız.";
                    return Ok(result);
                }
                
                if (usr.Password != model.Password)
                {
                    result.Message = "Hatalı şifre.";
                    if (AIService.RegisterWrongPass(model.Email, ip.ToString()))
                    {

                        var newbannedIp = new BannedIp(ip.ToString());
                        Repository.AddBannedIp(newbannedIp);
                        Repository.SaveChanges();
                        result.Message = "30 hatalı şifre denemesi ile bu ip adresi yasaklanmıştır.";
                        var msg = string.Format("Admin id: {0} geçici olarak banlandı", usr.UserId);
                        EmailService.SendEmail("dolunaysabuncuoglu@gmail.com", "30 dan fazla hatalı giriş", msg, false);
                    }
                    return Ok(result);
                }

                if (usr.Banned)
                {
                    result.Message = "Yasaklı hesap.";
                    return Ok(result);
                }
                if (usr.Role != "Admin")
                {
                    result.Message = "Erişim hakkı yok.";
                    return Ok(result);
                }

                

                var date = DateTime.Now;
                var random = new Random();
                var rand = ip.ToString() + " : " + random.Next(0, 9999999);

                var token = Authenticator.GetToken(usr.UserId.ToString());

                var name = (usr.FirstName.Length + 1 + usr.FamilyName.Length > 18) ? "Sn. " + usr.FamilyName : usr.FirstName + " " + usr.FamilyName;
                if (name.Length > 18)
                {
                    name = name.Substring(0, 14) + "...";
                }
                var user = new UserModel
                {
                    UserId = usr.UserId.ToString(),
                    Name = name,
                    Email = usr.Email
                };

                var mumtazTel = "05433893303";
                var dolunayTel = "05323878550";
                var emreTel = "05063337007";

                var code = AdminHelperService.GenerateSMSCode(token);
                var message = "Admin login sms code: " + code;
                SMSService.SendSms(mumtazTel, message);
                SMSService.SendSms(dolunayTel, message);
                SMSService.SendSms(emreTel, message);
                return Ok(new LoginUserResultModel
                {
                    Success = true,
                    Message = "SMS BEKLENIYOR",
                    Sms = true
                });
            }
            catch (Exception e)
            {
                Log.Error("Exception at LoginAdmin() - " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu lütfen da sonra tekrar deneyiniz";
            }

            return Ok(result);
        }

    }
}