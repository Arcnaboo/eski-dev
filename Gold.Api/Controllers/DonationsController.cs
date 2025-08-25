using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Gold.Api.Models.Events;
using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Models.Users;
using Gold.Api.Services;
using Gold.Api.Utilities;
using Gold.Core.Events;
using Gold.Core.Users;
using Gold.Domain.Events.Interfaces;
using Gold.Domain.Events.Repositories;
using Gold.Domain.Transactions.Interfaces;
using Gold.Domain.Transactions.Repositories;
using Gold.Domain.Users.Interfaces;
using Gold.Domain.Users.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Gold.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class DonationsController : ControllerBase
    {
        private readonly IUsersRepository Repository;
        private readonly IEventsRepository EventsRepo;
        private readonly ITransactionsRepository TransRepo;

        public DonationsController()
        {
            Repository = new UsersRepository();
            TransRepo = new TransactionsRepository();
            EventsRepo = new EventsRepository();
        }

        private decimal CalculatePrice(decimal grams)
        {
            FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();

            decimal price = currentRate.BuyRate * grams;
            price = Math.Truncate(100 * price) / 100;
            return price;
        }


        private List<EventTransactionModel> GetModels(List<Transaction2> transactions)
        {

            var transactionModels = new List<EventTransactionModel>();
            if (transactions.Any())
            {
                foreach (var tran in transactions)
                {
                    decimal price = CalculatePrice(tran.Amount);

                    var user = Repository.GetAllUsers().Where(u => u.UserId.ToString() == tran.Source).FirstOrDefault();
                    var photo = "http://www.fintag.net/images/temp_profile_photos/" + user.MemberId + ".jpg";
                    var transModel = new EventTransactionModel
                    {
                        From = user.FirstName + " " + user.FamilyName,
                        Grams = tran.Amount,
                        Money = price,
                        Photo = photo

                    };
                    transactionModels.Add(transModel);
                }
            }
            return transactionModels;
        }


        private List<Gold.Api.Models.Events.EventModel> GetDonatorEvents(User user)
        {
            var result = new List<Gold.Api.Models.Events.EventModel>();

            var id = user.UserId;

            // special events

            var events = EventsRepo.GetAllEvents().Where(evt => evt.CreatedBy == id && !evt.GoldClaimed).ToList();

            foreach (var wed in events)
            {
                var wedModel = new Gold.Api.Models.Events.SpecialEventModel
                {
                    EventName = wed.EventName,
                    EventDate = wed.EventDate.ToShortDateString(),
                    EventId = wed.EventId.ToString(),
                    EventText = wed.EventText,
                    EventCode = wed.EventCode
                };
                var eventModel = new Gold.Api.Models.Events.EventModel
                {
                    EventType = "special",
                    EventObject = wedModel
                };
                result.Add(eventModel);
            }
            return result;
        }

        [HttpGet]
        [Route("get_donators")]
        public ActionResult<List<UserModel>> GetDonators()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                string userid;

                if (!Authenticator.ValidateToken(token, out userid))
                {
                    return Unauthorized();
                }
                var requestee = Repository.GetAllUsers()
                    .Where(x => x.UserId.ToString() == userid).FirstOrDefault();
                if (requestee == null)
                {
                    return Unauthorized();
                }

                var donators = Repository.GetAllUsers()
                    .Where(x => x.Role == "Donator")
                    .ToList();

                var result = new List<UserModel>();
                foreach (var donator in donators)
                {

                    var model = new UserModel
                    {
                        UserId = donator.UserId.ToString(),
                        MemberId = donator.MemberId,
                        Name = donator.FirstName + " " + donator.FamilyName,
                        Events = GetDonatorEvents(donator)
                    };
                    result.Add(model);
                }

                return Ok(result);
            }
            catch (Exception e)
            {

                return Ok(e.Message);
            }


        }

        [HttpGet]
        [Route("get_donator_events")]
        public ActionResult<List<SpecialEventModel>> GetDonatorEvents()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || (requestee.Role != "Donator" && requestee.Role != "Admin"))
            {
                return Unauthorized();
            }
            var result = new List<SpecialEventModel>();
            try
            {

                var donatorIds = Repository.GetAllUsers()
                    .Where(x => x.Role == "Donator").Select(x => x.UserId).ToList();

               
               

                // special events

                var events = EventsRepo
                    .GetAllEvents()
                    .Where(evt => donatorIds.Contains(evt.CreatedBy) && !evt.GoldClaimed).ToList();

                foreach (var wed in events)
                {
                    var transactions = EventsRepo.GetEventsTransactions(wed.EventId).ToList();
                    var transactionModels = GetModels(transactions);
                    var wedModel = new SpecialEventModel
                    {
                        EventName = wed.EventName,
                        EventDate = wed.EventDate.ToShortDateString(),
                        EventId = wed.EventId.ToString(),
                        //BalanceInGold = wed.BalanceInGold,
                        //Money = CalculatePrice(wed.BalanceInGold),
                        //TransactionModels = transactionModels,
                        EventText = wed.EventText,
                        //TransCount = transactionModels.Count,
                        EventCode = wed.EventCode
                    };
                    
                    result.Add(wedModel);
                }


            }
            catch (Exception e)
            {
                Log.Error("Exception at GetDonatorEvents(): " + e.Message);
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("create_event")]
        public ActionResult<CreateSpecialEventResultModel> CreateEvent(CreateDonationEventParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId != Guid.Parse(model.DonatorId))
            {
                return Unauthorized();
            }
            var result = new CreateSpecialEventResultModel();

            try
            {

                var userEvent = EventsRepo.GetAllEvents().Where(e =>
                    e.CreatedBy.ToString() == model.DonatorId &&
                    e.EventName == model.EventName &&
                    e.EventDate == DateTime.ParseExact(model.EventDate, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                    ).FirstOrDefault();

                if (userEvent != null)
                {

                    result.Success = false;
                    result.Message = "Etkinlik: " + model.EventName + " tekrar oluşturulamaz.";
                    return Ok(result);
                }

                var date = DateTime.ParseExact(model.EventDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                if (date.Date < DateTime.Now.Date)
                {
                    result.Success = false;
                    result.Message = "Geçmiş tarihe etkinlik oluşturulamaz.";
                    return Ok(result);
                }
                
                int code = Utility.GenerateRandEventCode(EventsRepo);

                var _event = new Event(model.EventName,
                    model.EventText,
                    date,
                    Guid.Parse(model.DonatorId),
                    code);


                EventsRepo.AddEvent(_event);
                EventsRepo.SaveChanges();
                var created = EventsRepo
                    .GetAllEvents()
                    .Where(wed => wed.EventName == _event.EventName && wed.EventDate == _event.EventDate && wed.CreatedBy == _event.CreatedBy)
                    .FirstOrDefault();

                result.Message = model.EventName + " oluşturuldu.";
                result.Event = new SpecialEventModel
                {
                    EventName = created.EventName,
                    EventDate = created.EventDate.ToShortDateString(),
                    EventId = created.EventId.ToString(),
                    BalanceInGold = created.BalanceInGold,
                    Money = CalculatePrice(created.BalanceInGold),
                    EventText = created.EventText,
                    TransactionModels = new List<EventTransactionModel>(),
                    TransCount = 0,
                    EventCode = code
                };
                System.IO.File.Copy("\\inetpub\\wwwroot\\images\\event_photos\\111222333.jpg", "\\inetpub\\wwwroot\\images\\event_photos\\" + code + ".jpg");
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Message = "Sistem Hatası.";
                Log.Error(result.Message);
            }

            return Ok(result);
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public ActionResult<LoginUserResultModel> LoginDonator(LoginUserModel model)
        {
            var result = new LoginUserResultModel { Success = false };
            try
            {
                var ip = Request.HttpContext.Connection.RemoteIpAddress;

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

                Log.Information("LoginAdminUser() started for - " + model.Email);
                var usr = Repository.GetAllUsers()
                    .Where(x => x.Email.ToLower() == model.Email.ToLower())
                    .FirstOrDefault();


                Log.Debug("ARCDONATOR: " + model.Email + " * " + model.Password);


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
                        var msg = string.Format("Donator id: {0} geçici olarak banlandı", usr.UserId);
                        EmailService.SendEmail("dolunaysabuncuoglu@gmail.com", "30 dan fazla hatalı giriş", msg, false);
                    }
                    return Ok(result);
                }

                if (usr.Banned)
                {
                    result.Message = "Yasaklı hesap.";
                    return Ok(result);
                }
                if (usr.Role != "Donator")
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
                    LoginId = login.LoginId.ToString(),
                    Email = usr.Email
                };

                return Ok(new LoginUserResultModel
                {
                    Success = true,
                    User = user,
                    AuthToken = token,
                    Message = "Giriş Başarılı"
                });
            }
            catch (Exception e)
            {
                Log.Error("Exception at LoginDonator() - " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu lütfen da sonra tekrar deneyiniz";
            }

            return Ok(result);
        }

    }
}
