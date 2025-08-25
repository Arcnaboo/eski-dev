using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Gold.Domain.Users.Interfaces;
using Gold.Domain.Users.Repositories;
using Gold.Core.Users;
using Microsoft.AspNetCore.Authorization;
using Gold.Api.Utilities;
using Gold.Api.Models.Users;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Gold.Api.Services;
using System.IO;
using Gold.Domain.Events.Interfaces;
using Gold.Domain.Transactions.Interfaces;
using Gold.Domain.Events.Repositories;
using Gold.Domain.Transactions.Repositories;
using Gold.Api.Models.KuwaitTurk;


namespace Gold.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly IUsersRepository Repository;
        private readonly IEventsRepository EventsRepo;
        private readonly ITransactionsRepository TransRepo;


        /// <summary>
        /// 
        /// </summary>
        public UsersController()
        {
            Repository = new UsersRepository();
            EventsRepo = new EventsRepository();
            TransRepo = new TransactionsRepository();
        }

        /// <summary>
        /// Returns List of Users Logs 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_user_logs")]
        public ActionResult<List<InternalLog>> GetuserLogs(string id)
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

            var result = new List<InternalLog>();
            try
            {
                var logs = Repository.GetAllLogsOfUser(Guid.Parse(id));

                result.AddRange(logs);
            }
            catch (Exception e)
            {
                Log.Error("Exception at get user logs " + e.Message);
                Log.Error(e.StackTrace);
            }

            return result;
        }

        /// <summary>
        /// bütün userları liste olarak ceker
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getusers")]
        public ActionResult<List<User>> GetUsers(string memberId, DateTime? date, DateTime? endDate, string tckimlik)
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

            if (memberId != null)
            {
                int membId = int.Parse(memberId);
                var user = Repository.GetAllUsers().Where(x => x.MemberId == membId).FirstOrDefault();

                if (user == null)
                {
                    return Ok(new List<User>());
                }
                var result = new List<User>();
                result.Add(user);
                return Ok(result);
            }

            if (date.HasValue && endDate.HasValue)
            {
                var _users = Repository.GetAllUsers().Where(x => x.DateCreated.Date >= date.Value.Date && x.DateCreated <= endDate.Value.Date).ToList();
                return Ok(_users);
            }

            if (date.HasValue)
            {
                var _users = Repository.GetAllUsers().Where(x => x.DateCreated.Date == date.Value.Date).ToList();

                return Ok(_users);

            }

            if (tckimlik != null)
            {
                var user = Repository.GetAllUsers().Where(x => x.TcKimlikNo == tckimlik).FirstOrDefault();
                if (user == null)
                {
                    return Ok(new List<User>());
                }
                var result = new List<User>();
                result.Add(user);
                return Ok(result);
            }

            var users = Repository.GetAllUsers().ToList();
            return Ok(users);
        }

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
        private List<Gold.Api.Models.Events.EventTransactionModel> GetModels(List<Gold.Core.Events.Transaction2> transactions)
        {

            var transactionModels = new List<Gold.Api.Models.Events.EventTransactionModel>();
            if (transactions.Any())
            {
                foreach (var tran in transactions)
                {
                    decimal price = CalculatePrice(tran.Amount);

                    var user = Repository.GetAllUsers().Where(u => u.UserId.ToString() == tran.Source).FirstOrDefault();
                    var photo = "http://www.fintag.net/images/temp_profile_photos/" + user.MemberId + ".jpg";
                    var transModel = new Gold.Api.Models.Events.EventTransactionModel
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private List<Gold.Api.Models.Events.EventModel> GetUserEvents(User user)
        {
            var result = new List<Gold.Api.Models.Events.EventModel>();

            var id = user.UserId;

            // weddings 
            var weddings = EventsRepo.GetAllWeddings().Where(wed => wed.CreatedBy == id && !wed.GoldClaimed).ToList();

            foreach (var wed in weddings)
            {
                var transactions = EventsRepo.GetWeddingTransactions(wed.WeddingId)
                    .OrderByDescending(x => x.TransactionDateTime).ToList();
                var transactionModels = GetModels(transactions);
                var wedModel = new Gold.Api.Models.Events.WeddingModel
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
                var eventModel = new Gold.Api.Models.Events.EventModel
                {
                    EventType = "weddings",
                    EventObject = wedModel
                };
                result.Add(eventModel);
            }

            // special events

            var events = EventsRepo.GetAllEvents().Where(evt => evt.CreatedBy == id && !evt.GoldClaimed).ToList();

            foreach (var wed in events)
            {
                var transactions = EventsRepo.GetEventsTransactions(wed.EventId)
                    .OrderByDescending(x => x.TransactionDateTime).ToList();
                var transactionModels = GetModels(transactions);
                var wedModel = new Gold.Api.Models.Events.SpecialEventModel
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
                var eventModel = new Gold.Api.Models.Events.EventModel
                {
                    EventType = "special",
                    EventObject = wedModel
                };
                result.Add(eventModel);
            }
            return result;
        }

        /// <summary>
        /// Hareketin kaynağını User Modele çevirir
        /// Kaynak bir Usersa kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractSourceUser(Gold.Core.Transactions.Transaction transaction)
        {
            var usr = TransRepo
                .GetAllUsers()
                .Where(u => u.UserId.ToString() == transaction.Source)
                .FirstOrDefault();

            if (usr == null)
            {
                return null;
            }

            var result = new Gold.Api.Models.Transactions.UserModel3
            {
                Balance = usr.Balance,
                MemberId = usr.MemberId,
                Name = usr.FirstName + " " + usr.FamilyName,
                UserId = usr.UserId.ToString()
            };

            return result;
        }

        /// <summary>
        /// Hareketin kaynağını User Modele çevirir
        /// Kaynak Fintagsa kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractSourceFintag(Gold.Core.Transactions.Transaction transaction)
        {
            var usr = TransRepo
                .GetAllUsers()
                .Where(u => u.MemberId == 111222333)
                .FirstOrDefault();
            if (usr == null)
            {
                return null;
            }
            var result = new Gold.Api.Models.Transactions.UserModel3
            {
                Balance = usr.Balance,
                MemberId = usr.MemberId,
                Name = usr.FirstName + " " + usr.FamilyName,
                UserId = usr.UserId.ToString()
            };

            return result;

        }

        /// <summary>
        /// Hareketin kaynağını User Modele çevirir
        /// Kaynak Düğünse kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractSourceWedding(Gold.Core.Transactions.Transaction transaction)
        {
            var wed = TransRepo.GetWedding(transaction.Source);

            if (wed == null)
            {
                return null;
            }

            var result = new Gold.Api.Models.Transactions.UserModel3
            {
                Balance = wed.BalanceInGold,
                MemberId = 111222333,
                Name = wed.WeddingName,
                UserId = Guid.NewGuid().ToString()
            };

            return result;

        }

        /// <summary>
        /// Hareketin kaynağını User Modele çevirir
        /// Kaynak Etkinlikse kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractSourceEvent(Gold.Core.Transactions.Transaction transaction)
        {
            var evt = TransRepo.GetEvent(transaction.Source);

            if (evt == null)
            {
                return null;
            }

            var result = new Gold.Api.Models.Transactions.UserModel3
            {
                Balance = evt.BalanceInGold,
                MemberId = 111222333,
                Name = evt.EventName,
                UserId = Guid.Empty.ToString()
            };

            return result;
        }

        /// <summary>
        /// Hareketin kaynağını User Modele çevirir
        /// Kaynak kayıtsız kullanıcıysa kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractSourceUnregisteredUser(Gold.Core.Transactions.Transaction transaction)
        {
            var usr = TransRepo
               .GetAllUsers()
               .Where(u => u.UserId.ToString() == transaction.Source)
               .FirstOrDefault();

            if (usr == null)
            {
                return null;
            }

            var result = new Gold.Api.Models.Transactions.UserModel3
            {
                Balance = 0.0M,
                MemberId = 111222333,
                Name = usr.FirstName + " " + usr.FamilyName,
                UserId = usr.UserId.ToString()
            };

            return result;
        }

        /// <summary>
        /// Hareketin kaynağını User Modele çevirir
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractSource(Gold.Core.Transactions.Transaction transaction)
        {
            if (transaction.SourceType == "User")
            {
                /*
                type = "Kullanıcıya Altın Transferi";
                if (transaction.TransactionType == "SILVER" || transaction.TransactionType == "TRY_FOR_SILVER")
                {
                    type = "Kullanıcıya Gümüş Transferi";
                }*/
                return ExtractSourceUser(transaction);
            }
            else if (transaction.SourceType == "Fintag")
            {
                /*type = "Goldtag Altın Alımı";
                if (transaction.TransactionType == "SILVER" || transaction.TransactionType == "TRY_FOR_SILVER")
                {
                    type = "Goldtag Gümüş Alımı";
                }*/
                return ExtractSourceFintag(transaction);
            }
            else if (transaction.SourceType == "Wedding")
            {
                //type = "Düğünden altın transferi";
                return ExtractSourceWedding(transaction);
            }
            else if (transaction.SourceType == "Event")
            {
                //type = "Etkinlikten altın transferi";
                return ExtractSourceEvent(transaction);
            }
            else // Unregistered
            {
                //type = "Altın transferi";
                return ExtractSourceUnregisteredUser(transaction);
            }

        }


        /// asdsad
        /// 

        /// <summary>
        /// Harekette ki işlem sonucu altın yada para alanı User Modele çevirir
        /// Kaynak Usersa kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractDestinationUser(Gold.Core.Transactions.Transaction transaction)
        {
            var usr = Repository
                .GetAllUsers()
                .Where(u => u.UserId.ToString() == transaction.Destination)
                .FirstOrDefault();

            if (usr == null)
            {
                throw new ArgumentException("Kullanıcı bulunamadı");
            }

            var result = new Gold.Api.Models.Transactions.UserModel3
            {
                Balance = usr.Balance,
                MemberId = usr.MemberId,
                UserId = usr.UserId.ToString(),
                Name = usr.FirstName + " " + usr.FamilyName
            };

            return result;
        }

        /// <summary>
        /// Harekette ki işlem sonucu altın yada para alanı User Modele çevirir
        /// Kaynak KrediKartısa kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractDestinationVirtualPos(Gold.Core.Transactions.Transaction transaction)
        {

            var result = new Gold.Api.Models.Transactions.UserModel3
            {
                Balance = 0.0M,
                MemberId = 111222333,
                UserId = Guid.Empty.ToString(),
                Name = "Kart ile Altın Alımı"
            };

            return result;
        }

        /// <summary>
        /// Harekette ki işlem sonucu altın yada para alanı User Modele çevirir
        /// Kaynak Düğünse kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractDestinationWedding(Gold.Core.Transactions.Transaction transaction)
        {
            var wed = TransRepo.GetWedding(transaction.Destination);
            if (wed == null)
            {
                return null;
            }

            var result = new Gold.Api.Models.Transactions.UserModel3
            {
                Balance = 0.0M,
                MemberId = 111222333,
                UserId = Guid.Empty.ToString(),
                Name = wed.WeddingName
            };

            return result;
        }

        /// <summary>
        /// Harekette ki işlem sonucu altın yada para alanı User Modele çevirir
        /// Kaynak Etkinlikse kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractDestinationEvent(Gold.Core.Transactions.Transaction transaction)
        {
            var wed = TransRepo.GetEvent(transaction.Destination);

            if (wed == null)
            {
                return null;
            }

            var result = new Gold.Api.Models.Transactions.UserModel3
            {
                Balance = 0.0M,
                MemberId = 111222333,
                UserId = Guid.Empty.ToString(),
                Name = wed.EventName
            };

            return result;
        }

        /// <summary>
        /// Harekette ki işlem sonucu altın yada para alanı User Modele çevirir
        /// Kaynak Altıngünüyse kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractDestinationGoldDayUser(Gold.Core.Transactions.Transaction transaction)
        {


            var result = new Gold.Api.Models.Transactions.UserModel3
            {
                Balance = 0.0M,
                MemberId = 111222333,
                UserId = Guid.Empty.ToString(),
                Name = "Altın Günü"
            };

            return result;
        }

        /// <summary>
        /// Harekette ki işlem sonucu altın yada para alanı User Modele çevirir
        /// Kaynak Bağışsa kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractDestinationCharity(Gold.Core.Transactions.Transaction transaction)
        {


            var result = new Gold.Api.Models.Transactions.UserModel3
            {
                Balance = 0.0M,
                MemberId = 111222333,
                UserId = Guid.Empty.ToString(),
                Name = "Charity: " + transaction.Destination
            };

            return result;
        }

        /// <summary>
        /// Harekette ki işlem sonucu altın yada para alanı User Modele çevirir
        /// Kaynak EFT ise kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractDestinationIBAN(Gold.Core.Transactions.Transaction transaction)
        {


            var result = new Gold.Api.Models.Transactions.UserModel3
            {
                Balance = 0.0M,
                MemberId = 111222333,
                UserId = Guid.Empty.ToString(),
                Name = "GoldTag Altın Satışı"
            };

            return result;
        }

        /// <summary>
        /// Harekette ki işlem sonucu altın yada para alanı User Modele çevirir
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private Gold.Api.Models.Transactions.UserModel3 ExtractDestination(Gold.Core.Transactions.Transaction transaction)
        {
            if (transaction.DestinationType == "User")
            {
                return ExtractDestinationUser(transaction);
            }
            else if (transaction.DestinationType == "VirtualPos")
            {
                return ExtractDestinationVirtualPos(transaction);
            }
            else if (transaction.DestinationType == "Wedding")
            {
                return ExtractDestinationWedding(transaction);
            }
            else if (transaction.DestinationType == "Event")
            {
                return ExtractDestinationEvent(transaction);
            }
            else if (transaction.DestinationType == "GoldDayUser")
            {
                return ExtractDestinationGoldDayUser(transaction);
            }
            else if (transaction.DestinationType == "Charity")
            {
                return ExtractDestinationCharity(transaction);
            }
            else // Iban
            {
                return ExtractDestinationIBAN(transaction);
            }

        }

        private string UnderstandGelisGidis(string idKim)
        {

            return (idKim == "source") ? "gidis" : "gelis";
        }


        private string GetTransactionType(Gold.Core.Transactions.Transaction transaction)
        {
            if (transaction.TransactionType == "GOLD")
            {
                if (transaction.SourceType == "Fintag" && transaction.DestinationType == "User")
                {
                    return "Altın Alımı";
                } 
                else if (transaction.SourceType == "User" && transaction.DestinationType == "Fintag")
                {
                    return "Altın Satışı";
                }
                else if (transaction.SourceType == "User" && transaction.DestinationType == "User")
                {
                    return "Altın Transferi";
                }
                else if (transaction.SourceType == "User" && transaction.DestinationType == "Wedding")
                {
                    return "Düğüne Altın Transferi";
                }
                else if (transaction.SourceType == "User" && transaction.DestinationType == "Event")
                {
                    return "Etkinliğe Altın Transferi";
                }
                return "Altın Transferi";
            } 
            else // SILVER
            {
                if (transaction.SourceType == "Fintag" && transaction.DestinationType == "User")
                {
                    return "Gümüş Alımı";
                }
                else if (transaction.SourceType == "User" && transaction.DestinationType == "Fintag")
                {
                    return "Gümüş Satışı";
                }
                else if (transaction.SourceType == "User" && transaction.DestinationType == "User")
                {
                    return "Gümüş Transferi";
                }
                else if (transaction.SourceType == "User" && transaction.DestinationType == "Wedding")
                {
                    return "Düğüne Gümüş Transferi";
                }
                else if (transaction.SourceType == "User" && transaction.DestinationType == "Event")
                {
                    return "Etkinliğe Gümüş Transferi";
                }
                return "Gümüş Transferi";
            }

        }

        private Gold.Api.Models.Transactions.TransactionModel ParseTransactionAsModel(Gold.Core.Transactions.Transaction transaction, int key, string idKim)
        {
            string type = GetTransactionType(transaction);
            var source = ExtractSource(transaction);

            var dest = ExtractDestination(transaction);

            if (source == null || dest == null)
            {
                return null;
            }

            int idToUse;
            if (idKim == "source")
            {
                idToUse = dest.MemberId;
            }
            else
            {
                idToUse = source.MemberId;
            }

            string gelisGidis = UnderstandGelisGidis(idKim);



            var result = new Gold.Api.Models.Transactions.TransactionModel
            {
                Key = key,
                IdKim = idKim,
                Source = source,
                DestinationUser = dest,
                Destination = "not used",
                Amount = transaction.GramAmount,
                Price = transaction.TlAmount,
                DateTime = transaction.TransactionDateTime.ToString(),
                PhotoId = idToUse,
                GelisGidis = gelisGidis,
                Type = type
            };

            return result;
        }

        private List<Gold.Api.Models.Transactions.TransactionModel> UserTransactions(User user)
        {

            try
            {
                var id = user.UserId.ToString();
                var transactions = TransRepo
                .GetAllTransactions()
                .Where(x => (x.Source == id || x.Destination == id) && x.Confirmed && 
                        (x.TransactionType == "GOLD" || x.TransactionType == "SILVER"))  //(x.TransactionType != "TRY"))
                .OrderByDescending(x => x.TransactionDateTime)
                .ToList();

                var result = new List<Gold.Api.Models.Transactions.TransactionModel>(transactions.Count);
                int _id = 1;
                foreach (var trans in transactions)
                {
                    string idKim = (trans.Source == id) ? "source" : "dest";
                    //string gelisGidis = 
                    var model = ParseTransactionAsModel(trans, _id, idKim);
                    if (model != null)
                    {
                        result.Add(model);
                        _id++;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Exception at GetTransactionsWithId() " + e.Message);
                Log.Error(e.StackTrace);
            }

            return new List<Gold.Api.Models.Transactions.TransactionModel>();
        }

        /// <summary>
        /// Bir kullanıcıyı ceker member id yada userid yeterli
        /// </summary>
        /// <param name="memberIdorUserId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_user")]
        public ActionResult<GetUserResultModel> GetUserModel(string memberIdorUserId)
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

            GetUserResultModel result = new GetUserResultModel { Success = false};
            try
            {
                Log.Information("User data requested GetUserModel()");
                Guid userId = Guid.Empty;
                bool useMemberId = true;

                if (Guid.TryParse(memberIdorUserId, out userId))
                {
                    useMemberId = false;
                }

                User user = null;
                if (useMemberId)
                {
                    user = Repository
                        .GetAllUsers()
                        .Where(usr => usr.MemberId == int.Parse(memberIdorUserId))
                        .FirstOrDefault();
                    
                } 
                else
                {
                    user = Repository
                        .GetAllUsers()
                        .Where(usr => usr.UserId == Guid.Parse(memberIdorUserId))
                        .FirstOrDefault();
                }

                if (requestee.Role != "Admin" && requestee.UserId != user.UserId)
                {
                    return Unauthorized();
                }

                var balance = (Math.Truncate(100 * user.Balance) / 100);
                var price = (balance) * ((decimal)GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetCurrentPrice()));

                var silverBalance = Repository.GetSilverBalance(user.UserId);
                if (silverBalance == null)
                {
                    silverBalance = new SilverBalance(user.UserId);
                    Repository.AddSilverBalance(silverBalance);
                }
                var sBalance = (Math.Truncate(100 * silverBalance.Balance) / 100);

                var userLevel = Repository.GetUserLevel(user.UserId);
                if (userLevel == null)
                {
                    userLevel = new UserLevel(user.UserId, (user.Verified) ? 2 : 1);
                    Repository.AddUserLevel(userLevel);
                }

                var refKode = Repository.GetUserReferanse(user.UserId);
                if (refKode == null)
                {
                    refKode = new ReferansCode(user.UserId, Utility.GenerateRandMemberRef(Repository));
                    Repository.AddRefKod(refKode);
                }

                result.User = new UserModel
                {
                    UserId = user.UserId.ToString(),
                    Balance = balance,
                    MemberId = user.MemberId,
                    Name = user.FirstName + " " + user.FamilyName,
                    Blocked = (user.BlockedGrams.HasValue) ? user.BlockedGrams.Value : 0,
                    Price = price,
                    SilverBalance = sBalance,
                    Events = GetUserEvents(user),
                    Transactions = UserTransactions(user),
                    Verified = user.Verified,
                    Birthdate = user.Birthdate.ToString(),
                    TCK = user.TcKimlikNo,
                    Email = user.Email,
                    Phone = user.Phone,
                    RefCode = refKode.ReferansKod
                };

                result.Success = true;
                Repository.AddLog(new InternalLog(user.UserId, "UserData requested from server ", null));
                Repository.SaveChanges();

            }
            catch (Exception e)
            {
                Log.Error("Error at GetUserModel() " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
            }

            Log.Information("GetUserModel() completed for " + memberIdorUserId);
            return Ok(result);
        }

        /// <summary>
        /// bildirimler
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_notifications")]
        public ActionResult<List<NotificationModel>> GetUserNotifications(string id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId.ToString() != id)
            {
                return Unauthorized();
            }
            List<NotificationModel>  result = new List<NotificationModel>();
            try
            {
                var query = Repository
                    .GetAllNotifications()
                    .Where(n => n.UserId == Guid.Parse(id))
                    .OrderByDescending(n => n.NotificationDateTime).ToList();

                foreach (var notification in query)
                {
                    result.Add(new NotificationModel(notification));
                }

                
            } 
            catch (Exception e)
            {
                Log.Error("Exception at GetUserNotifications() - " + e.Message);
                Log.Error(e.StackTrace);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("cekilis_hak_istek")]
        public ActionResult<CekilisModel> CekilisHakIstek(string userid, string hakid)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userId;

            if (!Authenticator.ValidateToken(token, out userId))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userId).FirstOrDefault();

            if (requestee == null || (userid != null && Guid.Parse(userid) != requestee.UserId && requestee.Role != "Admin"))
            {
                return Unauthorized();
            }
            var result = new CekilisModel { };
            try
            {
                var hak = Repository.GetAllCekilisHaks().Where(x => x.HakId == Guid.Parse(hakid)).FirstOrDefault();
                if (hak == null)
                {
                    return Ok(result);
                }
                var cekilis = Repository.GetAllCekilis().Where(x => x.CekilisId == hak.CekilisId).FirstOrDefault();
                if (cekilis == null)
                {
                    return Ok(result);
                }
                result.Name = cekilis.Name;
                result.Prize = cekilis.Prize;
                result.SonKatilimTarihi = cekilis.SonKatilimTarihi.ToString();
                result.PlanlananCekilisTarihi = cekilis.PlanlananCekilisTarihi.ToString();
                result.CekilisHakki = hak.CekilisHakki;
                result.SonHakAlisTarihi = hak.SonHakAlisTarihi.ToString();
                result.YeniHakAlabilir = false;
                result.HakId = hak.HakId.ToString();
                result.TimeToNextHak = hak.SonHakAlisTarihi.AddDays(1).ToString();
                var diff = DateTime.Now - hak.SonHakAlisTarihi;
                if (diff.TotalDays >= 1)
                {
                    hak.CekilisHakki += 1;
                    hak.SonHakAlisTarihi = DateTime.Now;
                    result.YeniHakAlabilir = false;
                    result.TimeToNextHak = DateTime.Now.AddDays(1).ToString();
                    result.CekilisHakki = hak.CekilisHakki;
                    Repository.SaveChanges();
                    
                } 
                
                    
            }
            catch (Exception e)
            {
                Log.Error("Error at cekili  hak: " + e.Message);
                Log.Error(e.StackTrace);
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("get_cekilisler")]
        public ActionResult<List<CekilisModel>> GetCekilisler(string id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || (id != null && Guid.Parse(id) != requestee.UserId && requestee.Role != "Admin"))
            {
                return Unauthorized();
            }
            var result = new List<CekilisModel>();
            try
            {
                var cekiliss = Repository.GetAllCekilis().Where(x => x.SonKatilimTarihi >= DateTime.Now).ToList();

                foreach (var cekilis in cekiliss)
                {
                    CekilisHak hak = Repository.GetAllCekilisHaks().Where(x => x.CekilisId == cekilis.CekilisId && x.UserId == Guid.Parse(id)).FirstOrDefault();
                    bool yeniHakAlabilir = false;
                    string timeToNext = null;
                    if (hak == null)
                    {
                        hak = new CekilisHak(Guid.Parse(id), cekilis.CekilisId);
                        Repository.AddCekilisHakki(hak);
                        Repository.SaveChanges();
                        hak = Repository.GetAllCekilisHaks().Where(x => x.UserId == Guid.Parse(id) && x.CekilisId == cekilis.CekilisId)
                            .FirstOrDefault();
                        yeniHakAlabilir = true;
                    } 
                    else
                    {
                        var diff = DateTime.Now - hak.SonHakAlisTarihi;
                        if (diff.TotalDays >= 1)
                        {
                            yeniHakAlabilir = true;
                        } else
                        {
                            timeToNext = hak.SonHakAlisTarihi.AddDays(1).ToString();
                        }
                    }
                    
                    var model = new CekilisModel 
                    {
                        Name = cekilis.Name,
                        Prize = cekilis.Prize,
                        SonKatilimTarihi = cekilis.SonKatilimTarihi.ToString(),
                        PlanlananCekilisTarihi = cekilis.PlanlananCekilisTarihi.ToString(),
                        CekilisHakki = hak.CekilisHakki,
                        SonHakAlisTarihi = hak.SonHakAlisTarihi.ToString(),
                        YeniHakAlabilir = yeniHakAlabilir,
                        HakId = (yeniHakAlabilir) ? hak.HakId.ToString() : null,
                        TimeToNextHak = timeToNext
                    };
                    result.Add(model);
                }

                

            }
            catch (Exception e)
            {
                Log.Fatal("Exception at getcekilis() - " + e.Message);
                Log.Fatal(e.StackTrace);

            }

            return Ok(result);

        }


        [HttpGet]
        [Route("get_user_status")]
        public ActionResult<UserStatus> GetUserStatus(string id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId.ToString() != id)
            {
                return Unauthorized();
            }

            try
            {
                return AIService.GetUserStatus(Guid.Parse(id));
            } 
            catch (Exception e)
            {
                Log.Error("Exception at Users.GetUserStatus() - " + e.Message);
                Log.Error(e.StackTrace);
            }
            return new UserStatus { Events = false, Notifications = false, Transactions = false, UserId = id };
        }

        [HttpGet]
        [Route("get_new_notifications")]
        public ActionResult<List<NotificationModel>> GetNewNotifications(string id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId.ToString() != id)
            {
                return Unauthorized();
            }

            List<NotificationModel> result = new List<NotificationModel>();
            try
            {
                var query = Repository
                    .GetAllNotifications()
                    .Where(n => n.UserId == Guid.Parse(id) && !n.Delivered)
                    .OrderByDescending(n => n.NotificationDateTime).ToList();


                AIService.CheckIn(Guid.Parse(id));
                foreach (var notification in query)
                {
                    result.Add(new NotificationModel(notification));
                }


            }
            catch (Exception e)
            {
                Log.Error("Exception at GetUserNotifications() - " + e.Message);
                Log.Error(e.StackTrace);
            }
            return Ok(result);
        }

        /// <summary>
        /// okundu bildirim
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("see_notification")]
        public ActionResult<string> SeeNotification(string id)
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

            try
            {
                var notification = Repository
                    .GetAllNotifications()
                    .Where(n => n.NotificationId == Guid.Parse(id))
                    .FirstOrDefault();

                if (notification.UserId != requestee.UserId)
                {
                    return Unauthorized();
                }

                notification.SeeNotification();
                Repository.AddLog(new InternalLog(notification.UserId, "Notification seen " + notification.NotificationId, null));
                Repository.SaveChanges();
             
            }
            catch (Exception e)
            {
                Log.Fatal("Exception at GetUserNotifications() - " + e.Message);
                Log.Fatal(e.StackTrace);
                return Ok("Bir hata oluştu. Lütfen tekrar deneyiniz");
            }
            return Ok("ok");
        }

        [HttpGet]
        [Route("deliver_notification")]
        public ActionResult<string> DeliverNotification(string id)
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

            try
            {
                var notification = Repository
                    .GetAllNotifications()
                    .Where(n => n.NotificationId == Guid.Parse(id))
                    .FirstOrDefault();

                if (notification.UserId != requestee.UserId)
                {
                    return Unauthorized();
                }

                notification.DeliverNotification();
                Repository.AddLog(new InternalLog(notification.UserId, "Notification delivered " + notification.NotificationId, null));
                Repository.SaveChanges();

            }
            catch (Exception e)
            {
                Log.Fatal("Exception at GetUserNotifications() - " + e.Message);
                Log.Fatal(e.StackTrace);
                return Ok("Bir hata oluştu. Lütfen tekrar deneyiniz");
            }
            return Ok("ok");
        }

        /// <summary>
        /// bildirim sil
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("delete_notification")]
        public ActionResult<NotificationModel> DeleteNotification(string id)
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

            Notification deleted = null;
            try
            {
                var notification = Repository
                    .GetAllNotifications()
                    .Where(n => n.NotificationId == Guid.Parse(id))
                    .FirstOrDefault();

                if (notification.UserId != requestee.UserId)
                {
                    return Unauthorized();
                }

                deleted = Repository.DeleteNotification(notification);

                Repository.AddLog(new InternalLog(deleted.UserId, "Notification deleted " + deleted.NotificationId, null));

                Repository.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Fatal("Exception at DeleteNotification() - " + e.Message);
                Log.Fatal(e.StackTrace);
            }

            return Ok(new NotificationModel(deleted) ?? null);
        }

        /// <summary>
        /// delete multiple notifixations
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("delete_notifications")]
        public ActionResult<List<NotificationModel>> DeleteNotifications(DelNotificationsParamModel model)
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
            List<NotificationModel> result = new List<NotificationModel>();
            try
            {


                foreach (string notId in model.NotificationIds)
                {
                    var id = Guid.Parse(notId);
                    var notification = Repository
                        .GetAllNotifications()
                        .Where(x => x.NotificationId == id)
                        .FirstOrDefault();

                    if (notification.UserId != requestee.UserId)
                    {
                        return Unauthorized();
                    }
                    var removed = Repository.DeleteNotification(notification);
                    result.Add(new NotificationModel(removed));

                    Repository.AddLog(new InternalLog(removed.UserId, "Notification deleted " + removed.NotificationId, null));
                }

                
                Repository.SaveChanges();

            }
            catch (Exception e)
            {
                Log.Fatal("Exception at DeleteNotifications() - " + e.Message);
                Log.Fatal(e.StackTrace);
            }
            return Ok(result);
        }


        [HttpPost]
        [Route("enter_tck")]
        public ActionResult<EditProfileResultModel> EnterTck(EnterTckParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string user_id;


           
            if (!Authenticator.ValidateToken(token, out user_id))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers()
                .Where(x => x.UserId.ToString() == user_id).FirstOrDefault();

            if (requestee == null)
            {
                return Unauthorized();
            }
            var result = new EditProfileResultModel { Success = false };
            try
            {
                var tckUser = Repository.GetAllUsers()
                    .Where(x => x.TcKimlikNo == model.TCK)
                    .FirstOrDefault();


                if (tckUser != null)
                {
                    result.Message = "Bu TCK önceden sisteme zaten girilmiş.";
                    return Ok(result);
                }

                var user = Repository.GetAllUsers()
                    .Where(x => x.UserId.ToString() == model.UserId)
                    .FirstOrDefault();

                if (user == null || user.UserId != requestee.UserId)
                {
                    result.Message = "Kullanıcı bulunamadı. Tekrar deneyiniz.";
                    return Ok(result);
                }
                var userLevel = Repository.GetUserLevel(user.UserId);
                if (userLevel == null)
                {
                    userLevel = new UserLevel(user.UserId);
                    Repository.AddUserLevel(userLevel);
                }

                if (userLevel.Value != 0)
                {
                    result.Message = "Kullanıcı seviyesi hatalı.";
                    return Ok(result);
                }

                var client = new Kimlik.KPSPublicSoapClient(Kimlik.KPSPublicSoapClient.EndpointConfiguration.KPSPublicSoap);
                var ti = new CultureInfo("tr-TR", false).TextInfo;
                model.FirstName = ti.ToUpper(model.FirstName);
                model.FamilyName = ti.ToUpper(model.FamilyName);
                DateTime birthDate;
                if (model.Birthdate == null || !DateTime.TryParse(model.Birthdate, out birthDate))
                {
                    result.Message = "Doğum tarihi gerekmektedir.";
                    return Ok(result);
                }
                var difference = DateTime.Now - birthDate;
                if (difference.TotalDays < 18 * 365)
                {
                    result.Message = "Bu uygulamada işlem yapabilmek için 18 yaşınızı doldurmanız gerekmektedir.";
                    return Ok(result);
                }

                var kimlikResult = client.TCKimlikNoDogrulaAsync(long.Parse(model.TCK), model.FirstName, model.FamilyName, birthDate.Year).Result.Body.TCKimlikNoDogrulaResult;
                if (!kimlikResult)
                {
                    result.Message = "Doğum yılı, isim soyisim ve TCK uyuşmuyor.";
                    return Ok(result);
                    
                }

                user.FamilyName = model.FamilyName;
                user.FirstName = model.FirstName;
                user.Birthdate = birthDate;
                user.TcKimlikNo = model.TCK;
                userLevel.Value = 1;
                result.Success = true;
                result.Message = "Kullanıcı 1. seviyeye yükseldi. Artık alım-satım yapabilirsiniz.";
                Repository.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Error("Exception at verify tck() - " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu. Lütfen tekrar deneyiniz.";
                
            }
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("request_code")]
        public ActionResult<RequestCodeModel> RequestCode(string userid)
        {
            var result = new RequestCodeModel
            {
                Success = false
            };

            try
            {
                var ip = Request.HttpContext.Connection.RemoteIpAddress;

                var bannedIp = Repository
                    .GetAllBannedIps().Where(x => x.IP == ip.ToString()).FirstOrDefault();

                if (bannedIp != null)
                {
                    result.Message = "Yasaklı IP adresi.";
                    return Ok(result);
                    
                }

                if (userid == null)
                {
                    result.Message = "Verilen bilgiler uyuşmamaktadır, lütfen kontrol edip giriş yapınız.";
                    return Ok(result);
                    
                }


                User usr;
                usr = Repository.GetAllUsers()
                    .Where(x => x.UserId.ToString() == userid && x.Banned)
                    .FirstOrDefault();



                if (usr == null || usr.AdminNotes == null ||
                    usr.AdminNotes == "Onayli uye" || usr.AdminNotes == "TEMP_BAN" || usr.AdminNotes == "")
                {
                    result.Message = "Verilen bilgiler uyuşmamaktadır, lütfen kontrol edip giriş yapınız.";
                    return Ok(result); ;
                }

                SMSService.SendSms("0" + usr.Phone, "Goldtag onay kodu: " + usr.AdminNotes);
                result.Message = "Kod yollandı.";
                result.Success = true;
                

            }
            catch (Exception e)
            {
                Log.Error("Exception at req code() - " + e.Message);
                result.Message = "Bir hata oluştu: " + e.Message;

            }
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("enter_code")]
        public ActionResult<LoginUserResultModel> EnterCode(string userid, string code)
        {
            var result = new LoginUserResultModel { Success = false, Sms = false };
            try
            {
                

                var ip = Request.HttpContext.Connection.RemoteIpAddress;

                var bannedIp = Repository
                    .GetAllBannedIps().Where(x => x.IP == ip.ToString()).FirstOrDefault();

                if (bannedIp != null)
                {
                    result.Message = "Yasaklı IP adresi.";
                    return Ok(result);
                }

                if (userid == null ||code == null)
                {
                    result.Message = "Data yok, lütfen kontrol edip giriş yapınız.";
                    return Ok(result);
                }


                User usr;
                usr = Repository.GetAllUsers()
                    .Where(x => x.UserId.ToString() == userid)
                    .FirstOrDefault();



                if (usr == null || usr.AdminNotes == null || usr.AdminNotes == "Onayli uye" )
                {
                    result.Message = "Verilen bilgiler uyuşmamaktadır, lütfen kontrol edip giriş yapınız.";
                    return Ok(result);
                }


                if (usr.AdminNotes != "" && usr.AdminNotes != code)
                {
                    result.Message = "Hatali kod";
                    result.Sms = true;
                    return Ok(result);
                }

                Repository.AddLog(new InternalLog(usr.UserId, "Login attempt for " + usr.Phone, null));

                usr.Banned = false;
                usr.AdminNotes = "Onayli uye";


                var lastLogin = Repository
                    .GetAllLogins()
                    .Where(x => x.UserId == usr.UserId).OrderByDescending(x => x.LoginDateTime)
                    .FirstOrDefault();

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
                // silver balance check 
                var silverBalance = Repository.GetSilverBalance(usr.UserId);
                if (silverBalance == null)
                {
                    silverBalance = new SilverBalance(usr.UserId);
                    Repository.AddSilverBalance(silverBalance);
                }

                var userLevel = Repository.GetUserLevel(usr.UserId);
                if (userLevel == null)
                {
                    int level = (usr.Verified) ? 2 : 1;
                    userLevel = new UserLevel(usr.UserId, level);
                    Repository.AddUserLevel(userLevel);
                }

                var refKode = Repository.GetUserReferanse(usr.UserId);
                if (refKode == null)
                {
                    refKode = new ReferansCode(usr.UserId, Utility.GenerateRandMemberRef(Repository));
                    Repository.AddRefKod(refKode);
                }
                var user = new UserModel
                {
                    Balance = usr.Balance,
                    SilverBalance = silverBalance.Balance,
                    UserLevel = userLevel.Value,
                    MemberId = usr.MemberId,
                    UserId = usr.UserId.ToString(),
                    Name = name,
                    LoginId = login.LoginId.ToString(),
                    TCK = usr.TcKimlikNo,
                    Birthdate = usr.Birthdate.Date.ToShortDateString(),
                    Email = usr.Email,
                    Phone = usr.Phone,
                    RefCode = refKode.ReferansKod
                };


                Repository.AddLog(new InternalLog(usr.UserId, "Successful Login for " + usr.Email, null));
                Repository.SaveChanges();

                AIService.RegisterOnlineStatus(usr.UserId);

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
                Log.Error("Exception at entercode() - " + e.Message);
                result.Message = "Bir hata oluştu lütfen da sonra tekrar deneyiniz";
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("new_signup")]
        public ActionResult<CreateUserResultModel> SignUp2(CreateUserModel2 model)
        {
            User _user = null;
            CreateUserResultModel result = new CreateUserResultModel { Success = false };
            try
            {

                if (string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Phone))
                {
                    result.Message = "Bilgiler eksik.";
                    return Ok(result);
                }

                if (!Utility.ValidPhone(model.Phone, Repository))
                {
                    result.Message = "Bu telefon numarası ile kayıt yapılamaz.";
                    return Ok(result);
                }

                result.Message = "Lütfen cep telefonunuza gelen mesajı kontrol ediniz.";

                Log.Information("NewSignUp for - " + model.Phone);
                /*
                if (!Utility.ValidateUser(model, Repository, out string error))
                {
                    result.Message = error;
                    return Ok(result);
                }*/
                var id = Utility.GenerateRandMemberId(Repository);
                
                Log.Information("id generated for user " + id);
                var newUser = new User(
                    id,
                    model.FirstName,
                    "",
                    id.ToString() + "@goldtag",
                    model.Password,
                    "",
                    model.Phone,
                    "",
                    "Member");
                var rand = new Random();
                newUser.AdminNotes = rand.Next(1000, 9999).ToString();
                Repository.AddUser(newUser);
                Repository.SaveChanges();


                //var date = DateTime.ParseExact(model.Birthdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                var usr = Repository
                    .GetAllUsers()
                    .Where(x => x.MemberId == id)
                    .FirstOrDefault();
                _user = usr;

                if (model.RefCode != null && int.TryParse(model.RefCode, out int x))
                {
                    var usrRef = new UserRef(usr.UserId, x);
                    Repository.AddUserRef(usrRef);
                }

                Repository.AddUserLevel(new UserLevel(usr.UserId));
                Repository.AddSilverBalance(new SilverBalance(usr.UserId));
                Repository.AddRefKod(new ReferansCode(usr.UserId, Utility.GenerateRandMemberRef(Repository)));
                Repository.AddLog(new InternalLog(usr.UserId, "signup ok for" + usr.Phone, null));
                Repository.SaveChanges();

                System.IO.File.Copy("\\inetpub\\wwwroot\\images\\empty.jpg", "\\inetpub\\wwwroot\\images\\temp_profile_photos\\" + usr.MemberId + ".jpg");
                SMSService.SendSms("0" + usr.Phone, "Goldtag verification: " + usr.AdminNotes);
                //EmailService.RequestVerification(usr.Email, usr.MemberId, usr.FirstName + " " + usr.FamilyName, "0" + usr.Phone);
                result.UserId = usr.UserId.ToString();
                result.Success = true;


            }
            catch (Exception e)
            {
                result.Message = "Hata oluştu.";
                Log.Error("Exception at signup: " + e.Message);
                Log.Error(e.StackTrace);
                if (e.InnerException != null)
                {
                    Log.Error("Exception at signup inner exeptiopn" + e.InnerException.Message);
                    Log.Error(e.InnerException.StackTrace);
                }

                if (_user != null)
                {
                    Repository.RemoveUsers(_user);
                    Repository.SaveChanges();
                }

            }
            return Ok(result);
        }

        /// <summary>
        /// kayıt olma
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Create User result model</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("signup")]
        public ActionResult<CreateUserResultModel> SignUp(CreateUserModel model)
        {
            User _user = null;
            CreateUserResultModel result = new CreateUserResultModel { Success = false };
            try
            {
                Log.Information("SignUp for - " + model.Email);
                
                if (!Utility.ValidateUser(model, Repository, out string error))
                {                   
                    result.Message = error;
                    return Ok(result);
                }
                var id = Utility.GenerateRandMemberId(Repository);
                if (model.Email == null || model.Email == "")
                {
                    model.Email = id.ToString() + "@goldtag.org";
                    result.Message = "Lütfen cep telefonunuza gelen mesajı kontrol ediniz.";
                } else
                {
                    result.Message = "Lütfen cep telefonunuza yada emailinize gelen mesajı kontrol ediniz.";
                }
                Log.Information("sign up user validated");
                
                Log.Information("id generated for user " + id);
                var newUser = new User(
                    id, 
                    model.FirstName, 
                    model.FamilyName,
                    model.Email,
                    model.Password,
                    model.TCK,
                    model.Phone,
                    model.Birthdate,
                    model.RoleName);

                Repository.AddUser(newUser);
                Repository.SaveChanges();
                

                var date = DateTime.ParseExact(model.Birthdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                var usr = Repository
                    .GetAllUsers()
                    .Where(x => x.MemberId == id)
                    .FirstOrDefault();
                _user = usr;

                var silverBalance = new SilverBalance(usr.UserId);
                Repository.AddSilverBalance(silverBalance);

                Repository.AddLog(new InternalLog(usr.UserId, "signup ok for" + usr.Email, null));
                Repository.SaveChanges();

                System.IO.File.Copy("\\inetpub\\wwwroot\\images\\empty.jpg", "\\inetpub\\wwwroot\\images\\temp_profile_photos\\" + usr.MemberId + ".jpg");
                
                EmailService.RequestVerification(usr.Email, usr.MemberId, usr.FirstName + " " + usr.FamilyName, "0" + usr.Phone);
                result.UserId = usr.UserId.ToString();
                result.Success = true;
                
                
            }
            catch (Exception e) 
            {
                result.Message = "Hata oluştu.";
                Log.Error("Exception at signup: " + e.Message);
                Log.Error(e.StackTrace);
                if (e.InnerException != null)
                {
                    Log.Error("Exception at signup inner exeptiopn" + e.InnerException.Message);
                    Log.Error(e.InnerException.StackTrace);
                }
               
                if (_user != null)
                {
                    Repository.RemoveUsers(_user);
                    Repository.SaveChanges();
                }
                
            }
            return Ok(result);
        }

        /// <summary>
        /// şifremi unuttum code onay
        /// </summary>
        /// <param name="code"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("forgot_password_new_password")]
        public ActionResult<string> ForgotPasswordNewPassword(string code, string password)
        {
            try
            {
                var ip = Request.HttpContext.Connection.RemoteIpAddress;

                var banned = Repository
                   .GetAllBannedIps()
                   .Where(x => x.IP == ip.ToString())
                   .Any();

                if (banned)
                {
                    return Ok("Yasaklı IP adresi.");
                }

                if (AIService.RegisterForgotPassCodePass(ip.ToString()))
                {
                    var bannedIp = new BannedIp(ip.ToString());

                    Repository.AddBannedIp(bannedIp);
                    Repository.SaveChanges();
                    var message = string.Format("{0} ip adresi ForgotPasswordNewPassword dan yasaklandi.", ip.ToString());
                    SMSService.SendSms("05323878550", message);
                    return Unauthorized();
                }

                var generated = int.Parse(code);
                var forgot = Repository.GetAllForgotPasswords().Where(x => x.GeneratedCode == generated).FirstOrDefault();
                if (forgot != null)
                {
                    var user = Repository.GetAllUsers().Where(x => x.UserId == forgot.UserId).FirstOrDefault();

                    if (user != null)
                    {
                        user.Password = password;

                        Repository.AddLog(new InternalLog(user.UserId, "Forgot password code entered ok for " + user.Email, null));
                        Repository.RemoveForgotPass(forgot);
                        Repository.SaveChanges();
                        return Ok("Yeni şifrenizle giriş yapabilirsiniz.");
                    }
                    else
                    {
                        return Ok("Kullanıcı hatalı.");
                    }
                }
                else
                {
                    return Ok("Hatalı bir kod girildi.");
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception at forgotpassword: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok("Bir hata oluştu, lütfen tekrar deneyiniz.");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("forgot_password_code")]
        public ActionResult<string> ForgotPasswordCode(string code)
        {
            try
            {
                var ip = Request.HttpContext.Connection.RemoteIpAddress;

                var banned = Repository
                   .GetAllBannedIps()
                   .Where(x => x.IP == ip.ToString())
                   .Any();

                if (banned)
                {
                    return Ok("Yasaklı IP adresi.");
                }

                if (AIService.RegisterForgotPassCode(ip.ToString()))
                {
                    var bannedIp = new BannedIp(ip.ToString());

                    Repository.AddBannedIp(bannedIp);
                    Repository.SaveChanges();
                    var message = string.Format("{0} ip adresi ForgotPasswordCode dan yasaklandi.", ip.ToString());
                    SMSService.SendSms("05323878550", message);
                    return Unauthorized();
                }

                var generated = int.Parse(code);
                var forgot = Repository.GetAllForgotPasswords().Where(x => x.GeneratedCode == generated).FirstOrDefault();
                if (forgot != null)
                {
                    
                     return Ok("OK:" + code);
                    
                }
                else
                {
                    return Ok("NO");
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception at forgotpassword: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok("Bir hata oluştu, lütfen tekrar deneyiniz.");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("get_forgot_code")]
        public ActionResult<string> GetForgotCode(string email)
        {
            try
            {
                var ip = Request.HttpContext.Connection.RemoteIpAddress;

                var banned = Repository
                   .GetAllBannedIps()
                   .Where(x => x.IP == ip.ToString())
                   .Any();

                if (banned)
                {
                    return Ok("Yasaklı IP adresi.");
                }

                if (AIService.RegisterForgotPass(email, ip.ToString()))
                {
                    var bannedIp = new BannedIp(ip.ToString());

                    Repository.AddBannedIp(bannedIp);
                    Repository.SaveChanges();
                    var message = string.Format("{0} ip adresi ForgotPassword den yasaklandi.", ip.ToString());
                    SMSService.SendSms("05323878550", message);
                    return Unauthorized();
                }
                var search = email;

                if (!search.Contains("@") && !Utility.ValidPhone(search, null))
                {
                    return Ok("Lütfen başında 0 olmadan on haneli telefon numarası giriniz.");
                }
                var user = Repository
                    .GetAllUsers()
                    .Where(x => x.Email.ToLower() == search.ToLower() || x.Phone == search)
                    .FirstOrDefault();

                if (user != null)
                {
                    
                    var forgotPass = Repository.GetAllForgotPasswords().Where(x => x.UserId == user.UserId).FirstOrDefault();                    
                    var message = string.Format("Şifrenizi geri alabilmek için gerekli kod: {0}", forgotPass.GeneratedCode);

                    SMSService.SendSms("0" + user.Phone, message);
                    Repository.AddLog(new InternalLog(user.UserId, "Forgot password code for " + user.Email, null));
                    Repository.SaveChanges();

                    return Ok("Lütfen " + email + " kontrol ediniz.");

                }
                else
                {
                    return Ok("Email veya telefon numarası hatalı.");
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception at forgotpassword: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok("Bir hata oluştu, lütfen tekrar deneyiniz.");
            }
        }


        /// <summary>
        /// şifremi unuttum başlangıç
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("forgot_password")]
        public ActionResult<string> ForgotPassword(string email)
        {
            try
            {
                var ip = Request.HttpContext.Connection.RemoteIpAddress;

                var banned = Repository
                   .GetAllBannedIps()
                   .Where(x => x.IP == ip.ToString())
                   .Any();

                if (banned)
                {
                    return Ok("Yasaklı IP adresi.");
                }

                if (AIService.RegisterForgotPass(email, ip.ToString()))
                {
                    var bannedIp = new BannedIp(ip.ToString());

                    Repository.AddBannedIp(bannedIp);
                    Repository.SaveChanges();
                    var message = string.Format("{0} ip adresi ForgotPassword den yasaklandi.", ip.ToString());
                    SMSService.SendSms("05323878550", message);
                    return Unauthorized();
                }


                var search = email;

                if (!search.Contains("@") && !Utility.ValidPhone(search, null))
                {
                    
                    return Ok("Lütfen başında 0 olmadan on haneli telefon numarası giriniz.");
                    
                }


                var user = Repository
                    .GetAllUsers()
                    .Where(x => x.Email.ToLower() == search.ToLower() || x.Phone == search)
                    .FirstOrDefault();

                if (user != null)
                {
                    var forgotPass = new ForgotPassword(user.UserId, Utility.GenerateRandForgotPasswordCode(Repository));
                    Repository.AddForgotPass(forgotPass);
                    var message = string.Format("Şifrenizi geri alabilmek için gerekli kod: {0}", forgotPass.GeneratedCode);

                    if (email.Contains("@"))
                    {
                        EmailService.SendEmail(email, "GoldTag Şifremi Unuttum Servisi", message, false);
                        SMSService.SendSms("0" + user.Phone, message);
                    } 
                    else
                    {
                        SMSService.SendSms("0" + user.Phone, message);
                    }
                    
                    
                    Repository.AddLog(new InternalLog(user.UserId, "Forgot password for " + user.Email, null));
                    Repository.SaveChanges();

                    return Ok("Lütfen " + email + " kontrol ediniz.");

                }
                else
                {
                    return Ok("Email veya telefon numarası hatalı.");
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception at forgotpassword: " + e.Message);
                Log.Error(e.StackTrace);
                return Ok("Bir hata oluştu, lütfen tekrar deneyiniz.");
            }
        }


        [HttpGet]
        [Route("get_profile")]
        public ActionResult<EditProfileResultModel> GetProfile(string userid)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string user_id;

            if (!Authenticator.ValidateToken(token, out user_id))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == user_id).FirstOrDefault();

            if (requestee == null || user_id != userid)
            {
                return Unauthorized();
            }

            try
            {
                var user = requestee;

                var userLevel = Repository.GetUserLevel(user.UserId);
                if (userLevel == null)
                {
                    userLevel = new UserLevel(user.UserId);
                    Repository.AddUserLevel(userLevel);
                    Repository.SaveChanges();
                }

                var usermodel = new UserModel
                {
                    Birthdate = user.Birthdate.ToString(),
                    Email = user.Email,
                    FirstName = user.FirstName,
                    FamilyName = user.FamilyName,
                    MemberId = user.MemberId,
                    UserId = user.UserId.ToString(),
                    UserLevel = userLevel.Value
                };
                return Ok(new EditProfileResultModel {User = usermodel, Success = true, Message ="Kullanici" });
            }
            catch (Exception e)
            {
                return Ok(new EditProfileResultModel { User = null, Success = false, Message = "Kullanici hatasi" });
            }
        }


        [HttpPost]
        [Route("new_edit_profile")]
        public ActionResult<EditProfileResultModel> NewEditProfile(EditProfileParamModel2 model)
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

            if (requestee.Role != "Admin" && requestee.UserId.ToString() != model.UserId)
            {
                return Unauthorized();
            }

            var result = new EditProfileResultModel { Success = false };

            try
            {
                var user = Repository.GetAllUsers().Where(x => x.UserId.ToString() == model.UserId).FirstOrDefault();

                var userLevel = Repository.GetUserLevel(user.UserId);
                if (userLevel == null || userLevel.Value >= 1)
                {
                    result.Message = "Sadece Standart Üyeler İsim Soyisim ve Doğum Tarihini değiştirebilir.";
                    return Ok(result);
                }

                var ip = Request.HttpContext.Connection.RemoteIpAddress;


                Repository.AddLog(new InternalLog(user.UserId, "Edit profile attempt for " + user.Phone, null));
                Repository.SaveChanges();

                

                if (string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.FamilyName)
                        || string.IsNullOrEmpty(model.BirthDate) || string.IsNullOrEmpty(model.Email))
                {
                    result.Message = "Bilgiler eksik girilmiştir.";
                    return Ok(result);
                }

                var emailUsed = Repository.GetAllUsers().Where(x => x.Email == model.Email).FirstOrDefault();

                if (emailUsed.UserId != user.UserId)
                {
                    result.Message = "Girilen email adresi başka bir üye tarafından kullanılmaktadır.";
                }

                DateTime birthDate;
                if (!DateTime.TryParse(model.BirthDate, out birthDate))
                {
                    result.Message = "Doğum tarihi gerekmektedir.";
                    return Ok(result);
                }
                var ti = new CultureInfo("tr-TR", false).TextInfo;


                var change = new ProfileChange(user.UserId, "email", user.Email, model.Email, ip.ToString());

                Repository.AddChange(change);

                change = new ProfileChange(user.UserId, "firstname", user.FirstName, model.FirstName, ip.ToString());
                Repository.AddChange(change);
                change = new ProfileChange(user.UserId, "familyname", user.FamilyName, model.FamilyName, ip.ToString());
                Repository.AddChange(change);
                change = new ProfileChange(user.UserId, "birthdate", user.Birthdate.ToString(), birthDate.ToString(), ip.ToString());
                Repository.AddChange(change);
                model.FirstName = ti.ToUpper(model.FirstName);
                model.FamilyName = ti.ToUpper(model.FamilyName);

                user.FirstName = model.FirstName;
                user.FamilyName = model.FamilyName;
                user.Email = model.Email;
                user.Birthdate = birthDate;


                Repository.SaveChanges();

                result.User = new UserModel
                {
                    Birthdate = user.Birthdate.ToShortDateString(),
                    Email = user.Email,
                    Phone = user.Phone,
                    Name = user.FirstName + " " + user.FamilyName,
                    TCK = user.TcKimlikNo,
                    MemberId = user.MemberId,
                    UserLevel = userLevel.Value
                };
                result.Success = true;
                result.Message = "Bilgiler değişmiştir";
            }
            catch (Exception e)
            {
                Log.Error("Exception at forgotpassword: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu, lütfen tekrar deneyiniz.";
            }


            return Ok(result);
        }

        /// <summary>
        /// profil editle
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("edit_profile")]
        public ActionResult<EditProfileResultModel> EditProfile(EditProfileParamModel model)
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

            if (requestee.Role != "Admin" && requestee.UserId.ToString() != model.UserId)
            {
                return Unauthorized();
            }

            var result = new EditProfileResultModel { Success = false };

            try
            {
                var user = Repository.GetAllUsers().Where(x => x.UserId.ToString() == model.UserId).FirstOrDefault();
                var ip = Request.HttpContext.Connection.RemoteIpAddress;


                Repository.AddLog(new InternalLog(user.UserId, "Edit profile attempt for " + user.Email, null));
                Repository.SaveChanges();


                if (model.Email != null)
                {
                    if (Repository.GetAllUsers().Where(x => x.Email == model.Email).Any())
                    {
                        throw new Exception("Verilen EPosta adresi kullanılamaz. Lütfen tekrar deneyiniz.");
                    }

                    var change = new ProfileChange(user.UserId, "email", user.Email, model.Email, ip.ToString());
                    Repository.AddChange(change);


                    //user.Banned = true; // depreciated at 2.0.0
                    Repository.SaveChanges();

                    var chng = Repository.GetAllChanges().Where(x => x.UserId == user.UserId && x.OldValue == user.Email &&
                            x.NewValue == model.Email && x.IP == ip.ToString()).FirstOrDefault();



                    //EmailService.RequestNewEmailVerification(model.Email, chng.ChangeId.ToString(), user.FirstName + " " + user.FamilyName);
                    result.Success = true;
                    result.Message = "Eposta değişmiştir.";

                    Repository.AddLog(new InternalLog(user.UserId, "Edit email for " + user.Email + " to new email: " + model.Email, null));
                    Repository.SaveChanges();
                }

                if (model.Password != null)
                {
                    if (user.Password != model.CurrentPassword)
                    {
                        result.Message = "Güncel şifre hatalı.";
                        result.Success = false;
                        Repository.AddLog(new InternalLog(user.UserId, "Failed Edit password for " + user.Email, null));
                        Repository.SaveChanges();
                    }
                    else if (model.Password == model.CurrentPassword)
                    {
                        result.Message = "Yeni şifreniz en son şifrenizle aynı olamaz.";
                        result.Success = false;
                        Repository.AddLog(new InternalLog(user.UserId, "Failed Edit password for " + user.Email, null));
                        Repository.SaveChanges();
                    }
                    else
                    {
                        
                        var change = new ProfileChange(user.UserId, "password", user.Password, model.Password, ip.ToString());
                        Repository.AddChange(change);
                        user.Password = model.Password;
                        Repository.SaveChanges();
                        result.Success = true;
                        result.Message = "Şifre değiştirildi.";
                        Repository.AddLog(new InternalLog(user.UserId, "Success Edit password for " + user.Email, null));
                        Repository.SaveChanges();
                    }

                    
                }

                if (model.Phone != null)
                {

                    result.Success = true;
                    result.Message = "Telefon numarası değişikliği için lütfen Goldtag müşteri hizmetlerini arayınız.";
                   /* var change = new ProfileChange(user.UserId, "phone", user.Phone, model.Phone, ip.ToString());
                    Repository.AddChange(change);
                    user.Phone = model.Phone;
                    Repository.SaveChanges();
                    result.Success = true;
                    result.Message = "Telefon numarası değiştirildi";
                    Repository.AddLog(new InternalLog(user.UserId, "Success Edit phone for " + user.Email, null));
                    Repository.SaveChanges();*/
                }


                result.User = new UserModel
                {
                    Birthdate = user.Birthdate.ToShortDateString(),
                    Email = user.Email,
                    Phone = user.Phone,
                    Name = user.FirstName + " " + user.FamilyName,
                    TCK = user.TcKimlikNo,
                    MemberId = user.MemberId
                };

            }
            catch (Exception e)
            {
                Log.Error("Exception at forgotpassword: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
            }


            return Ok(result);
        }

        /// <summary>
        /// profil resmi upload et
        /// </summary>
        /// <param name="id"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("upload/{id:Guid}")]
        public ActionResult<UploadPhotoResultModel> UploadPhoto([FromRoute]Guid id, [FromForm]IFormFile body)
        {
            var result = new UploadPhotoResultModel { Success = false };
            Log.Information("UploadPhoto() Started ");

            Repository.AddLog(new InternalLog(id, "Upload Photo attempt for " + id, null));
            Repository.SaveChanges();
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

                if (fileBytes.Length > 5000000)
                {
                    result.Message = "Dosya cok buyuk";
                    return Ok(result);
                }
                var user = Repository.GetAllUsers().Where(u => u.UserId == id).FirstOrDefault();
                if (user == null)
                {
                    Repository.AddLog(new InternalLog(id, "Upload Photo attempt fail " + id, null));
                    Repository.SaveChanges();
                    return Ok("no user");
                }
                
                System.IO.File.WriteAllBytes("\\inetpub\\wwwroot\\images\\temp_profile_photos\\" + user.MemberId + ".jpg", fileBytes);

                result.Success = true;
                result.Message = "Fotoğraf yüklendi";
                result.PhotoSource = "http://www.fintag.net/images/temp_profile_photos/" + user.MemberId + ".jpg";

            } 
            catch (Exception e)
            {
                Log.Error("Error att upload: " + e.Message);
                Log.Error(e.StackTrace);
                Exception inner = e.InnerException;
                while (inner != null)
                {
                    Log.Error(inner.Message);
                    Log.Error(inner.StackTrace);
                    inner = inner.InnerException;
                }
                result.Message = "Bir hata oluştu: " + e.Message;
                result.Success = false;
            }

            Repository.AddLog(new InternalLog(id, "Upload Photo attempt done for " + id + " " + result.Message, null));
            Repository.SaveChanges();

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("get_kimlik_image")]
        public ActionResult GetKimlikImage(string id)
        {
            try
            {

                var image = System.IO.File.OpenRead("C:\\user_kimlik\\" + id +".jpg");
                return File(image, "image/jpeg");
            }
            catch (Exception e)
            {
                Log.Error("Error at get_k_image: " + e.Message);
                Log.Error(e.StackTrace);
                return BadRequest("Unable to return image: " + e.Message);
            }
        }

        [HttpGet]
        [Route("get_kimlik_info")]
        public ActionResult<KimlikInfo> GetKimlikIno(string id)
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
                var info = Repository.GetKimlikInfoByUser(Guid.Parse(id));


                return Ok(info);
            }
            catch (Exception e)
            {
                Log.Error("Error at get_k_image: " + e.Message);
                Log.Error(e.StackTrace);
                return BadRequest("error: " + e.Message);
            }
            
        }

        [HttpGet]
        [Route("get_unconfirmed_kimlik_infos")]
        public ActionResult<List<KimlikInfo>> GetUnconfirmedInfos()
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
            var result = new List<KimlikInfo>();
            try
            {
                result.AddRange(Repository.GetAllKimlikInfos().Where(x => !x.Confirmed));
            }
            catch (Exception e)
            {
                Log.Error("Error at get_unconfirmed_kimlik_infos: " + e.Message);
                Log.Error(e.StackTrace);
    
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("verify_kimlik")]
        public ActionResult<VerifyKimlikInfoResultModel> VerifyKimlikInfo(string id, bool verified)
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
            var result = new VerifyKimlikInfoResultModel { Success = false };
            try
            {
                var info = Repository.GetKimlikInfoByItsId(Guid.Parse(id));

                if (verified)
                {
                    info.Confirm();

                    var user = Repository.GetAllUsers().Where(x => x.UserId == info.UserId).FirstOrDefault();

                    user.Verified = true;

                    var userLevel = Repository.GetUserLevel(user.UserId);
                    if (userLevel == null)
                    {
                        userLevel = new UserLevel(user.UserId, 2);
                        Repository.AddUserLevel(userLevel);
                    }
                    userLevel.Value = 2;
                    var message = "Hesabınız 2. seviyeye çıkartılmıştır. Limitsiz alım-satım yapabilirsiniz.";

                    var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";

                    Repository.AddNotification(new Notification(user.UserId, message, false, "info", null, photo));
                    Repository.RemoveKimlikInfo(info);
                    Repository.SaveChanges();

                    result.Success = true;
                    result.Message = "Kullanici onaylandi";
                } 
                else
                {
                    var user = Repository.GetAllUsers().Where(x => x.UserId == info.UserId).FirstOrDefault();
                    var message = "Hesabınız 2. seviye yükseltme talebiniz başarısızdır. Lütfen tekrar deneyiniz.";

                    var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";

                    Repository.AddNotification(new Notification(user.UserId, message, false, "info", null, photo));
                    Repository.RemoveKimlikInfo(info);
                    Repository.SaveChanges();
                    result.Success = true;
                    result.Message = "Kullanici onaylanmadi";
                }
                

            }
            catch (Exception e)
            {
                Log.Error("Error at VerifyKimlik: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Error: " + e.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("upload_kimlik/{id:Guid}")]
        public ActionResult<UploadPhotoResultModel> UploadKimlikPhoto([FromRoute] Guid id, [FromForm] IFormFile body)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId != id)
            {
                return Unauthorized();
            }
            var result = new UploadPhotoResultModel { Success = false };
            Log.Information("UploadPhoto() Started ");

            Repository.AddLog(new InternalLog(id, "Upload Kimlik Photo attempt for " + id, null));
            Repository.SaveChanges();
            try
            {

                var info = Repository.GetKimlikInfoByUser(id);

                if (info != null)
                {
                    result.Message = "Zaten bir kimlik fotoğrafı yüklenmiş, lütfen sonucu bekleyiniz.";
                    return Ok(result);
                }

                byte[] fileBytes;
                Log.Debug("FileBytes defined");
                using (var memoryStream = new MemoryStream())
                {
                    Log.Debug("using memory stream");
                    body.CopyTo(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                var user = Repository.GetAllUsers().Where(u => u.UserId == id).FirstOrDefault();
                if (user == null)
                {
                    Repository.AddLog(new InternalLog(id, "Upload Photo attempt fail " + id, null));
                    Repository.SaveChanges();
                    result.Message = "Kullanıcı bulunamadı";
                    return Ok(result);
                }

                System.IO.File.WriteAllBytes("\\user_kimlik\\" + id + ".jpg", fileBytes);


                var kimlikInfo = new KimlikInfo(id, "https://www.fintag.net/users/get_kimlik_image?id=" + id);
                Repository.AddKimlikInfo(kimlikInfo);
                Repository.SaveChanges();

                result.Success = true;
                result.Message = "Fotoğraf yüklendi";
                result.PhotoSource = kimlikInfo.KimlikImageLink;
                

            }
            catch (Exception e)
            {
                Log.Error("Error att upload: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
                result.Success = false;
            }

            Repository.AddLog(new InternalLog(id, "Upload Photo attempt done for " + id + " " + result.Message, null));
            Repository.SaveChanges();

            return Ok(result);
        }

        /// <summary>
        /// kullanıcı logout etme
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("logout")]
        public ActionResult<string> LogoutUser(LogoutUserModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId.ToString() != model.UserId)
            {
                return Unauthorized();
            }
            var result = "ok";

            try
            {
                Repository.AddLog(new InternalLog(Guid.Parse(model.UserId), "Logout attempt for " + model.UserId, null));
                Repository.SaveChanges();
                var login = Repository.GetLogin(Guid.Parse(model.LoginId));

                if (login.UserId.ToString() == model.UserId)
                {

                    login.LogOut();

                    Repository.AddLog(new InternalLog(Guid.Parse(model.UserId), "Logout done for " + model.UserId, null));
                    
                    Repository.SaveChanges();
                }
                
            }
             catch (Exception e)
            {
                Log.Error("Exception at logout(): " + e.Message);
                Log.Error(e.StackTrace);

                result = "Hata oluştu";
            }


            return Ok(result);
        }

        /// <summary>
        /// kullanıcı giriş yap
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("admin_login")]
        public ActionResult<LoginUserResultModel> LoginAdminUser(LoginUserModel model)
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
                    
                    result.Message ="Gelen parametreler null";
                    return Ok(result);
                }

                Log.Information("LoginAdminUser() started for - " + model.Email);
                var usr = Repository.GetAllUsers()
                    .Where(x => x.Email.ToLower() == model.Email.ToLower())
                    .FirstOrDefault();


                Log.Debug("ARC: " + model.Email + " * " + model.Password);


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
                Repository.AddLog(new InternalLog(usr.UserId, "LoginAdmin attempt for " + usr.Email, null));
                if (usr.Password != model.Password)
                {
                    Repository.AddLog(new InternalLog(usr.UserId, "Failed LoginAdmin attempt for " + usr.Email, null));
                    Repository.SaveChanges();
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
                    Repository.AddLog(new InternalLog(usr.UserId, "Failed LoginAdmin attempt for " + usr.Email, null));
                    Repository.SaveChanges();
                    result.Message = "Yasaklı hesap.";
                    return Ok(result);
                }
                if (usr.Role != "Admin")
                {
                    Repository.AddLog(new InternalLog(usr.UserId, "Failed LoginAdmin attempt for " + usr.Email, null));
                    Repository.SaveChanges();
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

                Repository.AddLog(new InternalLog(usr.UserId, "Successful LoginAdmin for " + usr.Email, null));
                Repository.SaveChanges();
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
                Log.Error("Exception at LoginAdmin() - " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu lütfen da sonra tekrar deneyiniz";
            }

            return Ok(result);
        }

        /// <summary>
        /// kullanıcı giriş yap
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public ActionResult<LoginUserResultModel> LoginUser(LoginUserModel model)
        {
            var result = new LoginUserResultModel { Success = false, Sms = false };
            try
            {
                Log.Information("LoginUser() started for - " + model.Email);

                var ip = Request.HttpContext.Connection.RemoteIpAddress;

                var bannedIp = Repository
                    .GetAllBannedIps().Where(x => x.IP == ip.ToString()).FirstOrDefault();

                if (bannedIp != null)
                {
                    result.Message = "Yasaklı IP adresi.";
                    return Ok(result);
                }

                if (model == null || model.Email == null || model.Password == null)
                {
                    result.Message = "Verilen bilgiler uyuşmamaktadır, lütfen kontrol edip giriş yapınız.";
                    return Ok(result);
                }

                
                if (model.AppVersion == null || model.AppVersion != "2.0.0")
                {
                    result.Message = "Lütfen uygulamayı güncelleyiniz.";
                    return Ok(result);
                }
                model.Email = model.Email.Trim();
                User usr;
                usr = Repository.GetAllUsers()
                    .Where(x => (x.Email.ToLower() == model.Email.ToLower() || x.Phone == model.Email))
                    .FirstOrDefault();



                if (usr == null)
                {
                    result.Message = "Verilen bilgiler uyuşmamaktadır, lütfen kontrol edip giriş yapınız.";
                    return Ok(result);
                }

                if (usr.AdminNotes != null && usr.AdminNotes == "TEMP_BAN")
                {
                    result.Message = "Bu hesap geçici olarak yasaklıdır, lütfen müşteri hizmetlerini arayınız.";
                    return Ok(result);
                }
                

                if (usr.Role != "Member")
                {
                    result.Message = "Erişim hakkı yok.";
                    return Ok(result);
                }

                Repository.AddLog(new InternalLog(usr.UserId, "Login attempt for " + usr.Email, null));

                

                if (usr.Password != model.Password)
                {

                    result.Message = "Hatalı şifre.";
                    Repository.AddLog(new InternalLog(usr.UserId, "Failed Login attempt for " + usr.Email, null));
                    Repository.SaveChanges();
                    if (AIService.RegisterWrongPass(model.Email, ip.ToString()))
                    {

                        var newbannedIp = new BannedIp(ip.ToString());
                        Repository.AddBannedIp(newbannedIp);
                        usr.AdminNotes = "TEMP_BAN";
                        Repository.SaveChanges();
                        result.Message = "30 hatalı şifre denemesi ile bu ip adresi yasaklanmıştır.";
                         
                        var msg = string.Format("Kullanıcı id: {0} geçici olarak banlandı", usr.UserId);
                        EmailService.SendEmail("dolunaysabuncuoglu@gmail.com", "30 dan fazla hatalı giriş", msg, false);
                    }
                    return Ok(result);
                }
                
                if (usr.Banned)
                {
                    result.Message = "Lütfen sms olarak gönderilen doğrulama kodunu giriniz.";
                    result.Sms = true;
                    result.UserId = usr.UserId.ToString();
                    Repository.AddLog(new InternalLog(usr.UserId, "Failed Login attempt for " + usr.Email, null));
                    Repository.SaveChanges();
                    return Ok(result);
                }
                /*if (!usr.Verified)
                { // depreciated dont use
                    Repository.AddLog(new InternalLog(usr.UserId, "Failed Login attempt for " + usr.Email, null));
                    Repository.SaveChanges();
                    result.Message = "Lütfen hesabınızı aktif ediniz\n" + usr.Email + " kontrol ediniz.";
                    return Ok(result);
                }*/
                
                var lastLogin = Repository
                    .GetAllLogins()
                    .Where(x => x.UserId == usr.UserId).OrderByDescending(x => x.LoginDateTime)
                    .FirstOrDefault();
                /*
                if (lastLogin != null && !lastLogin.LogoutDateTime.HasValue)
                {
                    Repository.AddLog(new InternalLog(usr.UserId, "Failed Login attempt for " + usr.Email, null));
                    Repository.SaveChanges();
                    result.Message = "Aynı anda birden fazla cihazdan giriş yapılamaz.";
                    return Ok(result);
                }*/

                
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
                // silver balance check 
                var silverBalance = Repository.GetSilverBalance(usr.UserId);
                if (silverBalance == null)
                {
                    silverBalance = new SilverBalance(usr.UserId);
                    Repository.AddSilverBalance(silverBalance);
                }

                var userLevel = Repository.GetUserLevel(usr.UserId);
                if (userLevel == null)
                {
                    int level = (usr.Verified) ? 2 : 1;
                    userLevel = new UserLevel(usr.UserId, level);
                    Repository.AddUserLevel(userLevel);
                }

                var refKode = Repository.GetUserReferanse(usr.UserId);
                if (refKode == null)
                {
                    refKode = new ReferansCode(usr.UserId, Utility.GenerateRandMemberRef(Repository));
                    Repository.AddRefKod(refKode);
                }
                var user = new UserModel 
                {
                    Balance = usr.Balance,
                    SilverBalance = silverBalance.Balance,
                    UserLevel = userLevel.Value,
                    MemberId = usr.MemberId,
                    UserId = usr.UserId.ToString(),
                    Name = name,
                    LoginId = login.LoginId.ToString(),
                    TCK = usr.TcKimlikNo,
                    Birthdate = usr.Birthdate.Date.ToShortDateString(),
                    Email = usr.Email,
                    Phone = usr.Phone,
                    RefCode = refKode.ReferansKod
                };


                Repository.AddLog(new InternalLog(usr.UserId, "Successful Login for " + usr.Email, null));
                Repository.SaveChanges();

                AIService.RegisterOnlineStatus(usr.UserId);

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
                Log.Error("Exception at LoginUser() - " + e.Message);
                result.Message = "Bir hata oluştu lütfen da sonra tekrar deneyiniz";
            }
            
            return Ok(result);
        }

   
        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("adduser")]
        public ActionResult<CreateUserResultModel> AddUser(CreateUserModel model)
        {
            string message;
            // validate model
            if (!Utility.ValidateUser(model, Repository, out message))
            {
                
                return BadRequest(new CreateUserResultModel
                         {
                            Result = false,
                            Message =  message
                        }   
                );
            }
            var role = Repository.GetAllRoles().Where(x => x.RoleName == model.RoleName).FirstOrDefault();
            if (role == null)
            {
                return BadRequest(new CreateUserResultModel { Result = false });
            }
            var id = Utility.GenerateRandMemberId(Repository);

            var user = new User(id, model.FirstName,
                model.FamilyName,
                model.Email, model.Password, model.TCK,
                model.Phone, model.Birthdate, role.RoleId);

            

            Repository.AddUser(user);
            var res = new CreateUserResultModel
            {
                Result = true,
                User = user
            };
            try
            {
                Repository.SaveChanges();
            } catch (Exception e)
            {
                
                res.Result = false;
                res.Message = e.Message;
                return BadRequest(res);
            }
         
            return Ok(res);
        }*/

    }
}