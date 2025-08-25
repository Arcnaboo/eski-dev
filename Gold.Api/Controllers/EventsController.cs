using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Gold.Domain.Events.Interfaces;
using Gold.Domain.Events.Repositories;
using Gold.Api.Models.Events;
using Gold.Core.Events;
using Serilog;
using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Utilities;
using Gold.Api.GoldDayApp;
using Gold.Domain.Transactions.Interfaces;
using System.IO;

namespace Gold.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventsRepository Repository;
        private readonly ITransactionsRepository TransRepo;

        public EventsController()
        {
            Repository = new EventsRepository();
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_user_events")]
        public ActionResult<List<EventModel>> GetUsersEvents(string userId)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || (requestee.Role != "Admin" && requestee.UserId != Guid.Parse(userId)))
            {
                return Unauthorized();
            }
            var result = new List<EventModel>();
            try
            {
                Log.Information("GetUsersEvents() for userid: " + userId);
                var id = Guid.Parse(userId);
                // weddings 
                var weddings = Repository.GetAllWeddings().Where(wed => wed.CreatedBy == id && !wed.GoldClaimed).ToList();

                foreach (var wed in weddings)
                {
                    var transactions = Repository.GetWeddingTransactions(wed.WeddingId).ToList();
                    var transactionModels = GetModels(transactions);
                    var wedModel = new WeddingModel
                    {
                        WeddingName = wed.WeddingName,
                        WeddingDate = wed.WeddingDate.ToShortDateString(),
                        WeddingId = wed.WeddingId.ToString(),
                        BalanceInGold = wed.BalanceInGold,
                        Money = CalculatePrice(wed.BalanceInGold),
                        TransactionModels = transactionModels,
                        WeddingText = wed.WeddingText,
                        TransCount = transactionModels.Count,
                        WeddingCode = wed.WeddingCode
                    };
                    var eventModel = new EventModel
                    {
                        EventType = "weddings",
                        EventObject = wedModel
                    };
                    result.Add(eventModel);
                }

                // special events

                var events = Repository.GetAllEvents().Where(evt => evt.CreatedBy == id && !evt.GoldClaimed).ToList();

                foreach (var wed in events)
                {
                    var transactions = Repository.GetEventsTransactions(wed.EventId).ToList();
                    var transactionModels = GetModels(transactions);
                    var wedModel = new SpecialEventModel
                    {
                        EventName = wed.EventName,
                        EventDate = wed.EventDate.ToShortDateString(),
                        EventId = wed.EventId.ToString(),
                        BalanceInGold = wed.BalanceInGold,
                        Money = CalculatePrice(wed.BalanceInGold),
                        TransactionModels = transactionModels,
                        EventText = wed.EventText,
                        TransCount = transactionModels.Count,
                        EventCode = wed.EventCode
                    };
                    var eventModel = new EventModel
                    {
                        EventType = "special",
                        EventObject = wedModel
                    };
                    result.Add(eventModel);
                }


                Log.Information("GetUsersEvents() done.");

            }
            catch (Exception e)
            {
                Log.Error("Exception at GetUsersEvents(): " + e.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grams"></param>
        /// <returns></returns>
        private decimal CalculatePrice(decimal grams)
        {
            FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();

            decimal price = currentRate.BuyRate * grams;
            price = Math.Truncate(100 * price) / 100;
            return price;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactions"></param>
        /// <returns></returns>
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



        [AllowAnonymous]
        [HttpGet]
        [Route("get_event_wedding")]
        public ActionResult<GetEventResultModel> GetEventWeddingShare(string id)
        {
            var result = new GetEventResultModel { Success = false };

            try
            {
                var ti = new CultureInfo("tr-TR", false).TextInfo;
                var wedding = Repository.GetAllWeddings()
                    .Where(w => w.WeddingId == Guid.Parse(id))
                    .FirstOrDefault();

                if (wedding != null)
                {
                    var user = Repository.GetAllUsers().Where(x => x.UserId == wedding.CreatedBy).FirstOrDefault();

                    var name = ti.ToTitleCase(user.FirstName) + " " + ti.ToTitleCase(user.FamilyName);

                    var wModel = new WeddingModel
                    {
                        CreatedBy = name,
                        WeddingId = wedding.WeddingId.ToString(),
                        WeddingName = wedding.WeddingName,
                        WeddingText = wedding.WeddingText,
                        WeddingDate = wedding.WeddingDate.ToShortDateString(),
                        WeddingCode = wedding.WeddingCode,
                        BalanceInGold = wedding.BalanceInGold,
                        


                    };
                    var eModel = new EventModel { EventType = "Wedding", EventObject = wModel };
                    result.Event = eModel;
                    result.Success = true;
                    return result;
                }
                // burayagelmişsem 

                // buaraya gelmişssem search events 

                var evt = Repository.GetAllEvents()
                    .Where(w => w.EventId == Guid.Parse(id))
                    .FirstOrDefault();


                if (evt != null)
                {
                    var user = Repository.GetAllUsers().Where(x => x.UserId == evt.CreatedBy).FirstOrDefault();

                    var name = ti.ToTitleCase(user.FirstName) + " " + ti.ToTitleCase(user.FamilyName);
                    var wModel = new SpecialEventModel
                    {
                        CreatedBy = name,
                        EventId = evt.EventId.ToString(),
                        EventName = evt.EventName,
                        EventText = evt.EventText,
                        EventDate = evt.EventDate.ToShortDateString(),
                        EventCode = evt.EventCode


                    };
                    var eModel = new EventModel { EventType = "Event", EventObject = wModel };
                    result.Event = eModel;
                    result.Success = true;
                    return result;
                }


            }
            catch (Exception error)
            {
                Log.Error("Exception at get event or wedding: " + error.Message);
                Log.Error(error.StackTrace);

            }
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("get_event_or_wedding")]
        public ActionResult<GetEventResultModel> GetEventOrWedding(int code)
        {
            var result = new GetEventResultModel { Success = false };

            try
            {
                var wedding = Repository.GetAllWeddings()
                    .Where(w => w.WeddingCode == code)
                    .FirstOrDefault();

                if (wedding != null)
                {
                    var wModel = new WeddingModel
                    {
                        WeddingId = wedding.WeddingId.ToString(),
                        WeddingName = wedding.WeddingName,
                        WeddingText = wedding.WeddingText,
                        WeddingDate = wedding.WeddingDate.ToShortDateString(),
                        WeddingCode = wedding.WeddingCode,
                        BalanceInGold = wedding.BalanceInGold,
                  


                    };
                    var eModel = new EventModel { EventType = "Wedding", EventObject = wModel };
                    result.Event = eModel;
                    result.Success = true;
                    return result;
                }
                // burayagelmişsem 

                // buaraya gelmişssem search events 

                var evt = Repository.GetAllEvents()
                    .Where(w => w.EventCode == code)
                    .FirstOrDefault();


                if (evt != null)
                {
                    var wModel = new SpecialEventModel
                    {
                        EventId = evt.EventId.ToString(),
                        EventName = evt.EventName,
                        EventText = evt.EventText,
                        EventDate = evt.EventDate.ToShortDateString(),
                        EventCode = evt.EventCode


                    };
                    var eModel = new EventModel { EventType = "Event", EventObject = wModel };
                    result.Event = eModel;
                    result.Success = true;
                    return result;
                }


            }
            catch (Exception error)
            {
                Log.Error("Exception at get event or wedding: " + error.Message);
                Log.Error(error.StackTrace);

            }
            return Ok(result);
        }

        [HttpGet]
        [Route("get_event_admin")]
        public ActionResult<GetEventResultModel> GetEventAdmin(string search)
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

            var result = new GetEventResultModel { Success = false };

            try
            {
                Event evt = null;
                Wedding wedding = null;
                 
                int evtCode;
                if (int.TryParse(search, out evtCode))
                {
                    wedding = Repository.GetAllWeddings()
                    .Where(w => w.WeddingCode == evtCode)
                    .FirstOrDefault();
                    if (wedding == null)
                    {
                        evt = Repository.GetAllEvents()
                            .Where(w => w.EventCode == evtCode)
                            .FirstOrDefault();
                        if (evt == null)
                        {
                            return Ok(result);
                        }
                    }
                    
                }
                if (wedding == null && evt == null)
                {
                    Guid evtId;
                    if (Guid.TryParse(search, out evtId))
                    {
                        wedding = Repository.GetAllWeddings()
                            .Where(w => w.WeddingId == evtId)
                            .FirstOrDefault();
                        if (wedding == null)
                        {
                            evt = Repository.GetAllEvents()
                                .Where(w => w.EventId == evtId)
                                .FirstOrDefault();
                            if (evt == null)
                            {
                                return Ok(result);
                            }
                        }
                    }
                }

                if (wedding != null)
                {
                    var transactions = Repository
                        .GetWeddingTransactions(wedding.WeddingId)
                        .OrderByDescending(x => x.TransactionDateTime);
                    var models = new List<EventTransactionModel>();
                    foreach(var transaction in transactions)
                    {
                        var model = new EventTransactionModel {
                            From = transaction.Source,
                            Grams = transaction.GramAmount,
                            Money = transaction.TlAmount
                        };
                        models.Add(model);
                    }
                    var wModel = new WeddingModel
                    {
                        CreatedBy = wedding.CreatedBy.ToString(),
                        WeddingId = wedding.WeddingId.ToString(),
                        WeddingName = wedding.WeddingName,
                        WeddingText = wedding.WeddingText,
                        WeddingDate = wedding.WeddingDate.ToShortDateString(),
                        WeddingCode = wedding.WeddingCode,
                        TransCount = models.Count,
                        TransactionModels = models,
                        BalanceInGold = wedding.BalanceInGold


                    };
                    var eModel = new EventModel { EventType = "Wedding", EventObject = wModel };
                    result.Event = eModel;
                    result.Success = true;
                    return result;
                }
                // burayagelmişsem 

                // buaraya gelmişssem search events 

                if (evt != null)
                {
                    var transactions = Repository
                        .GetEventsTransactions(evt.EventId)
                        .OrderByDescending(x => x.TransactionDateTime);
                    var models = new List<EventTransactionModel>();
                    foreach (var transaction in transactions)
                    {
                        var model = new EventTransactionModel
                        {
                            From = transaction.Source,
                            Grams = transaction.GramAmount,
                            Money = transaction.TlAmount
                        };
                        models.Add(model);
                    }
                    var wModel = new SpecialEventModel
                    {
                        EventId = evt.EventId.ToString(),
                        EventName = evt.EventName,
                        EventText = evt.EventText,
                        EventDate = evt.EventDate.ToShortDateString(),
                        EventCode = evt.EventCode,
                        CreatedBy = evt.CreatedBy.ToString(),
                        TransCount = models.Count,
                        TransactionModels = models,
                        BalanceInGold = evt.BalanceInGold


                    };
                    var eModel = new EventModel { EventType = "Event", EventObject = wModel };
                    result.Event = eModel;
                    result.Success = true;
                    return result;
                }


            }
            catch (Exception error)
            {
                Log.Error("Exception at get event: " + error.Message);
                Log.Error(error.StackTrace);

            }
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_event")]
        public ActionResult<GetEventResultModel> GetEvent(int search, Guid userId)
        {
            var result = new GetEventResultModel { Success = false};

            try
            {
                var wedding = Repository.GetAllWeddings()
                    .Where(w => w.WeddingCode == search && !w.GoldClaimed && w.CreatedBy != userId && w.WeddingDate.Date >= DateTime.Now.Date)
                    .FirstOrDefault();

                if (wedding != null)
                {
                    var wModel = new WeddingModel {
                        WeddingId = wedding.WeddingId.ToString(),
                        WeddingName = wedding.WeddingName,
                        WeddingText = wedding.WeddingText,
                        WeddingDate = wedding.WeddingDate.ToShortDateString(),
                        WeddingCode = wedding.WeddingCode
                     

                    };
                    var eModel = new EventModel {EventType = "Wedding", EventObject = wModel };
                    result.Event = eModel;
                    result.Success = true;
                    return result;
                }
                // burayagelmişsem 

                // buaraya gelmişssem search events 

                var evt = Repository.GetAllEvents()
                    .Where(w => w.EventCode == search && !w.GoldClaimed && w.CreatedBy != userId && w.EventDate.Date >= DateTime.Now.Date)
                    .FirstOrDefault();


                if (evt != null)
                {
                    var wModel = new SpecialEventModel
                    {
                        EventId = evt.EventId.ToString(),
                        EventName = evt.EventName,
                        EventText = evt.EventText,
                        EventDate = evt.EventDate.ToShortDateString(),
                        EventCode = evt.EventCode


                    };
                    var eModel = new EventModel { EventType = "Event", EventObject = wModel };
                    result.Event = eModel;
                    result.Success = true;
                    return result;
                }


            }
            catch (Exception error)
            {
                Log.Error("Exception at get event: " + error.Message);
                Log.Error(error.StackTrace);
                
            }
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_weddings")]
        public ActionResult<List<WeddingModel>> GetAllWeddings(int limit, int page)
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

            var result = new List<WeddingModel>();

            try
            {
                Log.Information("Listing all weddings at GetAllWeddings()");
                var weddings = Repository
                    .GetAllWeddings()
                    .OrderByDescending(x => x.DateCreated)
                    .Skip(limit * (page - 1))
                    .Take(limit)
                    .ToList();
                foreach (var wed in weddings)
                {
                    var transactions = Repository.GetWeddingTransactions(wed.WeddingId).ToList();
                    var transactionModels = GetModels(transactions);
                    var wedModel = new WeddingModel
                    {
                        WeddingName = wed.WeddingName,
                        WeddingDate = wed.WeddingDate.ToShortDateString(),
                        WeddingId = wed.WeddingId.ToString(),
                        BalanceInGold = wed.BalanceInGold,
                        Money = CalculatePrice(wed.BalanceInGold),
                        TransactionModels = transactionModels,
                        WeddingText = wed.WeddingText,
                        TransCount = transactionModels.Count,
                        WeddingCode = wed.WeddingCode,
                        CreatedBy = wed.CreatedBy.ToString()
                    };
                    result.Add(wedModel);
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception at GetAllWeddings(): " + e.Message);
                Log.Error(e.StackTrace);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("weddings_count")]
        public ActionResult<int> WeddingsCount()
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
            int result = 0;

            try
            {
                Log.Information("Wedding COunt");
                result = Repository
                    .GetAllWeddings()
                    .ToList().Count;
               
            }
            catch (Exception e)
            {
                Log.Error("Exception at WeddingsCount(): " + e.Message);
                Log.Error(e.StackTrace);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("events_count")]
        public ActionResult<int> EventsCount()
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
            int result = 0;

            try
            {
                Log.Information("Events COunt");
                result = Repository
                    .GetAllEvents()
                    .ToList().Count;

            }
            catch (Exception e)
            {
                Log.Error("Exception at EventsCount(): " + e.Message);
                Log.Error(e.StackTrace);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_events")]
        public ActionResult<List<SpecialEventModel>> GetAllEvents(int limit, int page)
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
            var result = new List<SpecialEventModel>();

            try
            {
                Log.Information("Listing all special events at GetAlEvents()");
                var events = Repository
                    .GetAllEvents()
                    .OrderByDescending(x => x.DateCreated)
                    .Skip(limit * (page - 1))
                    .Take(limit)
                    .ToList();

                foreach (var wed in events)
                {
                    var transactions = Repository.GetEventsTransactions(wed.EventId).ToList();
                    var transactionModels = GetModels(transactions);
                    var wedModel = new SpecialEventModel
                    {
                        EventName = wed.EventName,
                        EventDate = wed.EventDate.ToShortDateString(),
                        EventId = wed.EventId.ToString(),
                        BalanceInGold = wed.BalanceInGold,
                        Money = CalculatePrice(wed.BalanceInGold),
                        TransactionModels = transactionModels,
                        EventText = wed.EventText,
                        TransCount = transactionModels.Count,
                        EventCode = wed.EventCode,
                        CreatedBy = wed.CreatedBy.ToString()
                    };
                    result.Add(wedModel);
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception at getallevents(): " + e.Message);
                Log.Error(e.StackTrace);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("gold_day_users_status")]
        public ActionResult<List<string>> GoldDayUsersStatus(string id)
        {
            List<string> result = new List<string>();
            try
            {
                var day = Repository.GetAllGoldDays().Where(x => x.GoldDayId == Guid.Parse(id)).FirstOrDefault();
                var game = new GoldDayGame(day);

                var currentStatus = game.GetUserStatus();

                foreach(var kp in currentStatus)
                {
                    var user = Repository.GetAllUsers().Where(x => x.UserId == kp.Key).FirstOrDefault();

                    var data = user.FirstName + " " + user.FamilyName + ":" + user.MemberId + ":" + kp.Value;
                    result.Add(data);
                }
                

            }
            catch (Exception e)
            {
                Log.Error("Exception at GoldDayUsersStatus:" + e.Message);
                Log.Error(e.StackTrace);
                result.Clear();

                result.Add("Bir hata oluştu lütfen tekrar deneyiniz.");
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("user_response_gold_day")]
        public ActionResult<string> UserResponseGoldDay(UserResponseGoldDayParamModel model)
        {
            string result;
            try
            {
                var user = Repository.GetAllUsers().Where(x => x.UserId == Guid.Parse(model.UserId)).FirstOrDefault();
                var day = Repository.GetAllGoldDays().Where(x => x.GoldDayId == Guid.Parse(model.GoldDayId)).FirstOrDefault();

                var game = new GoldDayGame(day);

                if (model.Response)
                {
                    game.UserAccepted(user.UserId);
                    result = "Davet kabul edildi, iyi eğlenceler!";
                    var message = user.FirstName + " " + user.FamilyName + " Altın Günü davetinizi kabul etti";
                    var notification = new Notification(day.CreatedBy, message, false, false, "Message", null);
                    Repository.AddNotification(notification);
                } 
                else
                {
                    game.RemoveUser(user.UserId);
                    result = "Davet kabul edilmedi.";
                    var message = user.FirstName + " " + user.FamilyName + " Altın Günü davetinizi kabul etmedi";
                    var notification = new Notification(day.CreatedBy, message, false, false, "Message", null);
                    Repository.AddNotification(notification);
                }
                Repository.SaveChanges();
            } 
            catch (Exception e)
            {
                Log.Error("Exception at add user_response_gold_day:" + e.Message);
                Log.Error(e.StackTrace);
                return Ok("Bir hata oluştu lütfen tekrar deneyiniz.");
            }

            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("add_user_gold_day")]
        public ActionResult<string> AddUserToGoldDay(AddUserToGoldDayParamModel model)
        {
            string result;
            try
            {
                var host = Repository.GetAllUsers().Where(x => x.UserId == Guid.Parse(model.UserId)).FirstOrDefault();
                var newUser = Repository.GetAllUsers().Where(x => x.MemberId == int.Parse(model.MemberId)).FirstOrDefault();
                var day = Repository.GetAllGoldDays().Where(x => x.GoldDayId == Guid.Parse(model.GoldDayId)).FirstOrDefault();

                var game = new GoldDayGame(day);

                game.AddNewUser(newUser.UserId);

                var message = host.FirstName + " " + host.FamilyName + " sizi Altın Gününe davet ediyor";

                var notification = new Notification(newUser.UserId, message, false, true, "GoldDay", day.GoldDayId.ToString());

                Repository.AddNotification(notification);
                Repository.SaveChanges();

                result = newUser.FirstName + " " + newUser.FamilyName + " Altın Gününe davet edildi.";

            } 
            catch (Exception e)
            {
                Log.Error("Exception at add user to gold day:" + e.Message);
                Log.Error(e.StackTrace);
                return Ok("Bir hata oluştu lütfen tekrar deneyiniz.");
            }


            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create_gold_day")]
        public ActionResult<CreateGoldDayResultModel> CreateGoldDay(CreateGoldDayParamModel model)
        {
            var result = new CreateGoldDayResultModel { Success = false };

            try
            {
                Log.Information("Creating Gold Day");


                var day = new GoldDay(Guid.Parse(model.UserId),
                    model.GoldDayName,
                    model.GramAmount,
                    model.IntervalType,
                    model.UserAmount,
                    DateTime.ParseExact(model.StartDateTime, "yyyy-MM-dd", CultureInfo.InvariantCulture));

                Repository.AddGoldDay(day);

                Repository.SaveChanges();

                var created = Repository.GetAllGoldDays()
                    .Where(x => x.GoldDayName == day.GoldDayName && x.CreatedBy == day.CreatedBy && x.GameDataFile == day.GameDataFile).FirstOrDefault();

               

               

                result.GoldDay = new GoldDayModel 
                {
                    StartDate = created.GoldDayStartDateTime.ToString(),
                    GoldDayId = created.GoldDayId.ToString(),
                    GramAmount = created.GramAmount,
                    Interval = created.GoldDayTimeInterval

                };

                result.Success = true;
                result.Message = model.GoldDayName + " oluşturuldu";


            }
            catch (Exception e)
            {
                result.Message = "Bir hata oluştu daha sonra tekrar deneyiniz.";
                Log.Error("Exception at CreateGoldDay(): " + e.Message);
            }

            return Ok(result);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>        
        [HttpPost]
        [Route("delete_event")]
        public ActionResult<DeleteEventResultModel> DeleteEvent(DeleteEventParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null)
            {
                return Unauthorized();
            }

            var result = new DeleteEventResultModel { Success = false };
            Log.Information("DeleteEvent() ");
            try
            {
                var evt = Repository
                .GetAllEvents()
                .Where(x => x.EventId.ToString() == model.Id)
                .FirstOrDefault();

                var wed = Repository
                    .GetAllWeddings()
                    .Where(x => x.WeddingId.ToString() == model.Id)
                    .FirstOrDefault();

                if (evt != null)
                {

                    var user = Repository
                        .GetAllUsers()
                        .Where(x => x.UserId == evt.CreatedBy)
                        .FirstOrDefault();
                    if (user.UserId != requestee.UserId)
                    {
                        return Unauthorized();
                    }
                    result.Message = "Etkinlik kaldırıldı";
                    evt.GoldClaimed = true;
                    Repository.SaveChanges();
                    result.Success = true;
                    

                }
                else if (wed != null)
                {
                    var user = Repository
                        .GetAllUsers()
                        .Where(x => x.UserId == wed.CreatedBy)
                        .FirstOrDefault();
                    if (user.UserId != requestee.UserId)
                    {
                        return Unauthorized();
                    }
                    result.Message = "Düğün kaldırıldı";
                    wed.GoldClaimed = true;
                    Repository.SaveChanges();
                    result.Success = true;
                    
                }
                else
                {
                    result.Message = "Etkinlik/Düğün Bulunamadı!";
                }
            }
            catch (Exception err)
            {
                Log.Error("Error at del wedding: " + err.Message);
                Log.Error(err.StackTrace);
                result.Message = "Sistem Hatası.";
            }

            return Ok(result);
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("edit_event")]
        public ActionResult<EditEventResultModel> EditEvent(EditEventParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null)
            {
                return Unauthorized();
            }
            var result = new EditEventResultModel { Success = false };

            try
            {
               
                


                var evt = Repository.GetAllEvents().Where(x => x.EventId.ToString() == model.Id).FirstOrDefault();

                if (evt != null)
                {
                    if (requestee.UserId != evt.CreatedBy)
                    {
                        return Unauthorized();
                    }
                    DateTime date = DateTime.MinValue;
                    if (model.Date != null)
                    {
                        date = DateTime.ParseExact(model.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        if (date.Date >= DateTime.Now.Date)
                        {
                            evt.EventDate = date;
                        }
                        else
                        {
                            result.Message = "Etkinlik tarihi hatalı.";
                            result.Success = false;
                            return Ok(result);
                        }

                    }
                    if (model.Name != null)
                    {
                        evt.EventName = model.Name;
                    }
                    if (model.Text != null)
                    {
                        evt.EventText = model.Text;
                    }
                    var transactions = Repository.GetEventsTransactions(evt.EventId).ToList();
                    var transactionModels = GetModels(transactions);
                    var money = CalculatePrice(evt.BalanceInGold);
                    result.Success = true;
                    result.Message = "Etkinlik değiştirildi.";
                    result.Event = new SpecialEventModel(evt, transactionModels, money);
                    Repository.SaveChanges();
                }
                else
                {
                    var wed = Repository.GetAllWeddings().Where(x => x.WeddingId.ToString() == model.Id).FirstOrDefault();

                    if (wed != null)
                    {
                        if (requestee.UserId != wed.CreatedBy)
                        {
                            return Unauthorized();
                        }
                        DateTime date = DateTime.MinValue;
                        if (model.Date != null)
                        {
                            date = DateTime.ParseExact(model.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            if (date.Date >= DateTime.Now.Date)
                            {
                                wed.WeddingDate = date;
                            }
                            else
                            {
                                result.Message = "Düğün tarihi hatalı.";
                                result.Success = false;
                                return Ok(result);
                            }

                        }
                        if (model.Name != null)
                        {
                            wed.WeddingName = model.Name;
                        }
                        if (model.Text != null)
                        {
                            wed.WeddingText = model.Text;
                        }
                        var transactions = Repository.GetWeddingTransactions(wed.WeddingId).ToList();
                        var transactionModels = GetModels(transactions);
                        var money = CalculatePrice(wed.BalanceInGold);
                        result.Success = true;
                        result.Message = "Düğün değiştirildi.";
                        result.Event = new WeddingModel(wed, transactionModels, money);
                        Repository.SaveChanges();
                    }
                }

                

            }
            catch (Exception e)
            {
                Log.Error("Exception at EditEvent() " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir Hata Oluştu.";
            }

            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create_wedding")]
        public ActionResult<CreateWeddingResultModel> CreateWedding(CreateWeddingParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId != Guid.Parse(model.UserId))
            {
                return Unauthorized();
            }
            var result = new CreateWeddingResultModel();
            
            try
            {
                var userWeddings = Repository.GetAllWeddings().Where(w => w.CreatedBy.ToString() == model.UserId).ToList();

                foreach (var wed in userWeddings)
                {
                    if (wed.WeddingDate >= DateTime.Now && !wed.GoldClaimed)
                    {
                        result.Success = false;
                        result.Message = "Önceden yaratılmış " + wed.WeddingName + "  henüz tamamlanmamış.";
                        return Ok(result);
                    }
                }

                Log.Information("Creating new wedding: " + model.WeddingName);

                var date = DateTime.ParseExact(model.WeddingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                if (date.Date < DateTime.Now.Date)
                {
                    result.Success = false;
                    result.Message = "Geçmiş tarihe düğün oluşturulamaz.";
                    return Ok(result);
                } 

                var code = Utility.GenerateRandWeddingCode(Repository);
                var wedding = new Wedding(model.WeddingName,
                    model.WeddingText,
                    date,
                    Guid.Parse(model.UserId),
                    code);
                

                Repository.AddWedding(wedding);
                Repository.SaveChanges();
                var createdWedding = Repository
                    .GetAllWeddings()
                    .Where(wed => wed.WeddingName == wedding.WeddingName && 
                            wed.WeddingDate == wedding.WeddingDate && wed.CreatedBy == wedding.CreatedBy)
                    .FirstOrDefault();

                result.Message = model.WeddingName + " oluşturuldu.";
                result.Wedding = new WeddingModel
                {
                    WeddingName = createdWedding.WeddingName,
                    WeddingDate = createdWedding.WeddingDate.ToShortDateString(),
                    WeddingId = createdWedding.WeddingId.ToString(),
                    BalanceInGold = createdWedding.BalanceInGold,
                    Money = CalculatePrice(createdWedding.BalanceInGold),
                    WeddingText = createdWedding.WeddingText,
                    TransactionModels = new List<EventTransactionModel>(),
                    TransCount = 0,
                    WeddingCode = code
                };
                System.IO.File.Copy("\\inetpub\\wwwroot\\images\\wedding_photos\\111222333.jpg", "\\inetpub\\wwwroot\\images\\wedding_photos\\" + code + ".jpg");
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Message = "Sistem hatası: " + e.Message;
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create_event")]
        public ActionResult<CreateSpecialEventResultModel> CreateEvent(CreateSpecialEventParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId != Guid.Parse(model.UserId))
            {
                return Unauthorized();
            }
            var result = new CreateSpecialEventResultModel();

            try
            {

                var userEvent = Repository.GetAllEvents().Where(e => 
                    e.CreatedBy.ToString() == model.UserId && 
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
                Log.Information("Creating new event: " + model.EventName);
                int code = Utility.GenerateRandEventCode(Repository);
                var _event = new Event(model.EventName,
                    model.EventText,
                    date,
                    Guid.Parse(model.UserId),
                    code);


                Repository.AddEvent(_event);
                Repository.SaveChanges();
                var created = Repository
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

        /// <summary>
        /// profil resmi upload et
        /// </summary>
        /// <param name="id"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("upload/{id:Guid}")]
        public ActionResult<UploadPhotoResultModel2> UploadPhoto([FromRoute] Guid id, [FromForm] IFormFile body)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null)
            {
                return Unauthorized();
            }
            var result = new UploadPhotoResultModel2 { Success = false };
            Log.Information("UploadPhoto() Started ");

            try
            {
                byte[] fileBytes;
                Log.Debug("FileBytes defined");
                using (var memoryStream = new MemoryStream())
                {
                    Log.Debug("using memory stream");
                    body.CopyTo(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                var wed = Repository.GetAllWeddings().Where(u => u.WeddingId == id).FirstOrDefault();
                if (wed != null)
                {
                    if (wed.CreatedBy != requestee.UserId)
                    {
                        return Unauthorized();
                    }
                    System.IO.File.WriteAllBytes("\\inetpub\\wwwroot\\images\\wedding_photos\\" + wed.WeddingCode + ".jpg", fileBytes);
                    result.Success = true;
                    result.Message = "Fotoğraf yüklendi";
                    result.PhotoSource = "http://www.fintag.net/images/wedding_photos/" + wed.WeddingCode + ".jpg";
                    return Ok(result);
                }
                var evt = Repository.GetAllEvents().Where(x => x.EventId == id).FirstOrDefault();
                if (evt != null)
                {
                    if (evt.CreatedBy != requestee.UserId)
                    {
                        return Unauthorized();
                    }
                    System.IO.File.WriteAllBytes("\\inetpub\\wwwroot\\images\\event_photos\\" + evt.EventCode + ".jpg", fileBytes);
                    result.Success = true;
                    result.Message = "Fotoğraf yüklendi";
                    result.PhotoSource = "http://www.fintag.net/images/event_photos/" + evt.EventCode + ".jpg";
                    return Ok(result);
                }
                

                

            }
            catch (Exception e)
            {
                Log.Error("Error att upload: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu lütfen daha sonra tekrar deneyiniz";
                result.Success = false;
            }

            return Ok(result);
        }
    }
}