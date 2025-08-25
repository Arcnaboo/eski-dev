using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Gold.Domain.Users.Interfaces;
using Gold.Domain.Users.Repositories;
using Gold.Api.Models;
using Gold.Core.Transactions;
using Microsoft.EntityFrameworkCore;
using Gold.Domain.Transactions.Interfaces;
using Gold.Domain.Transactions.Repositories;
using Microsoft.AspNetCore.Authorization;
using Gold.Api.Models.Transactions;
using Serilog;
using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Utilities;
using Gold.Api.Models.SiPay;
using Gold.Domain.Events.Interfaces;
using Gold.Domain.Events.Repositories;
using System.IO;
using Gold.Api.Services;
using Gold.Api.Models.KuwaitTurk.Vpos;
using System.Globalization;

namespace Gold.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : ControllerBase
    {

        private readonly ITransactionsRepository Repository;

        private static readonly string FintagUserId = "D94AF0AD-5FE9-4A6C-949E-C66AEF21C907";

        public TransactionsController()
        {
            Repository = new TransactionsRepository();
            
        }

        #region private_methods
        /* ------ PRIVATE UTILITY METHODS */

        /// <summary>
        /// Yekun hesaplar
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private decimal GetYekun(User3 user)
        {

            var transactions = Repository.GetUserTransactions(user.UserId);
            if (transactions.Any())
            {
                var transaction = transactions.OrderByDescending(x => x.TransactionDateTime).FirstOrDefault();

                if (transaction.Source == user.UserId.ToString())
                {
                    return transaction.Yekun;
                }
                else
                {
                    return (transaction.YekunDestination.HasValue) ? transaction.YekunDestination.Value : 0;
                }
            }

            return 0;
        }

        /// <summary>
        /// Hareketin kaynağını User Modele çevirir
        /// Kaynak bir Usersa kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private UserModel3 ExtractSourceUser(Transaction transaction)
        {
            var usr = Repository
                .GetAllUsers()
                .Where(u => u.UserId.ToString() == transaction.Source)
                .FirstOrDefault();

            if (usr == null)
            {
                //throw new ArgumentException("Kullanıcı bulunamadı");
                return null;
            }

            var result = new UserModel3
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
        private UserModel3 ExtractSourceFintag(Transaction transaction)
        {
            var usr = Repository
                .GetAllUsers()
                .Where(u => u.MemberId == 111222333)
                .FirstOrDefault();
            if (usr == null)
            {
                return null;
            }
            var result = new UserModel3
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
        private UserModel3 ExtractSourceWedding(Transaction transaction)
        {
            var wed = Repository.GetWedding(transaction.Source);

            if (wed == null)
            {
                return null;
            }

            var result = new UserModel3
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
        private UserModel3 ExtractSourceEvent(Transaction transaction)
        {
            var evt = Repository.GetEvent(transaction.Source); 

            if (evt == null)
            {
                return null;
            }

            var result = new UserModel3
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
        private UserModel3 ExtractSourceUnregisteredUser(Transaction transaction)
        {
            var usr = Repository
               .GetAllUsers()
               .Where(u => u.UserId.ToString() == transaction.Source)
               .FirstOrDefault();

            if (usr == null)
            {
                return null;
            }

            var result = new UserModel3
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
        private UserModel3 ExtractSource(Transaction transaction)
        {
            if (transaction.SourceType == "User")
            {
                //type = "Altın transferi";
                return ExtractSourceUser(transaction);
            }
            else if (transaction.SourceType == "Fintag")
            {
                //type = "Goldtag Altın transferi";
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

        /// <summary>
        /// Harekette ki işlem sonucu altın yada para alanı User Modele çevirir
        /// Kaynak Usersa kullanılır
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private UserModel3 ExtractDestinationUser(Transaction transaction)
        {
            var usr = Repository
                .GetAllUsers()
                .Where(u => u.UserId.ToString() == transaction.Destination)
                .FirstOrDefault();

            if (usr == null)
            {
                return null;
            }

            var result = new UserModel3
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
        private UserModel3 ExtractDestinationVirtualPos(Transaction transaction)
        {
            
            var result = new UserModel3
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
        private UserModel3 ExtractDestinationWedding(Transaction transaction)
        {
            var wed = Repository.GetWedding(transaction.Destination); 
            if (wed == null)
            {
                return null;
            }

            var result = new UserModel3
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
        private UserModel3 ExtractDestinationEvent(Transaction transaction)
        {
            var wed = Repository.GetEvent(transaction.Destination);

            if (wed == null)
            {
                return null;
            }

            var result = new UserModel3
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
        private UserModel3 ExtractDestinationGoldDayUser(Transaction transaction)
        {
            

            var result = new UserModel3
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
        private UserModel3 ExtractDestinationCharity(Transaction transaction)
        {


            var result = new UserModel3
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
        private UserModel3 ExtractDestinationIBAN(Transaction transaction)
        {


            var result = new UserModel3
            {
                Balance = 0.0M,
                MemberId = 111222333,
                UserId = Guid.Empty.ToString(),
                Name = "Havale/EFT ile Altın"
            };

            return result;
        }

        /// <summary>
        /// Harekette ki işlem sonucu altın yada para alanı User Modele çevirir
        /// </summary>
        /// <param name="transaction">Hareket objesi</param>
        /// <returns>User Modeli</returns>
        private UserModel3 ExtractDestination(Transaction transaction)
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

        private string FindTransactionType(Transaction transaction)
        {

            if (transaction.SourceType == "User" && transaction.DestinationType == "User")
            {
                return "Altın transferi";
            }
            else if (transaction.SourceType == "User" && (transaction.DestinationType == "Wedding" || 
                transaction.DestinationType == "Event"))
            {
                return "Etkinliğe Altın transferi";
            }
            else if (transaction.DestinationType == "IBAN")
            {
                return "EFT ile Altın Bozdurma";
            }
            else
            {
                return transaction.Comment;
            }

        }

        private string GetTransactionType(Transaction transaction)
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

        /// <summary>
        /// Verilen hareketi Modele çevirir
        /// </summary>
        /// <param name="transaction">Hareket</param>
        /// <param name="key">Front end için anahtar</param>
        /// <param name="idKim">id nin kimle alakalı olduğu</param>
        /// <returns>Hareket Modeli</returns>
        private TransactionModel ParseTransactionAsModel(Transaction transaction, int key, string idKim)
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



            var result = new TransactionModel
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



        #endregion


        /*      PUBLIC METHODS --- API ENDPOINTS */

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("unconfirmed_eft_buy_gold_requests_count")]
        public ActionResult<int> GetBankTransferRequestsCount()
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
            var result = 0;
            try
            {
                result = Repository.GetAllUserBankTransferRequests()
                    .Where(x => !x.MoneyReceived)
                    .ToList()
                    .Count;
            }
            catch (Exception e)
            {
                Log.Error("exception at GetBankTransferRequestsCount: " + e.Message);
                Log.Error(e.StackTrace);
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
        [Route("unconfirmed_eft_buy_gold_requests")]
        public ActionResult<List<UserBankTransferRequest>> GetBankTransferRequests(int limit, int page)
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

            var result = new List<UserBankTransferRequest>();
            try
            {
                result = Repository.GetAllUserBankTransferRequests()
                    .Where(x  => !x.MoneyReceived)
                    .OrderByDescending(x => x.CodeStartDateTime)
                    .Skip(limit * (page - 1))
                    .Take(limit)
                    .ToList();
            }
            catch (Exception e)
            {
                Log.Error("exception at GetBankTransferRequests: " + e.Message);
                Log.Error(e.StackTrace);
            }

            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("requests_with_id_count")]
        public ActionResult<int> RequestsWithIdCount(string id)
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
            var result = 0;

            try
            {
                result = Repository.GetAllTransferRequests()
                    .Where(x => x.SourceUserId.ToString() == id || x.Destination == id)
                    .ToList().Count;
            }
            catch(Exception e)
            {
                Log.Error("exception at request with id count : " + e.Message);
                Log.Error(e.StackTrace);
            }

            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="limit"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_requests_with_id")]
        public ActionResult<List<TransferRequest>> GetTransferRequestsWithId(string id, int limit, int page)
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
            var result = new List<TransferRequest>();
            try
            {
                result = Repository.GetAllTransferRequests()
                    .Where(x => x.SourceUserId.ToString() == id || x.Destination == id)
                    .OrderByDescending(x => x.RequestDateTime)
                    .Skip(limit * (page - 1))
                    .Take(limit)
                    .ToList();

            }
            catch (Exception e)
            {
                Log.Error("exception at GetTransferRequestsWithId() : " + e.Message);
                Log.Error(e.StackTrace);
            }
            return Ok(result);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("transactions_with_id_count")]
        public ActionResult<int> TransactionsWithIdCount(string id)
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
                var transactions = Repository
                .GetAllTransactions()
                .Where(x => (x.Source == id || x.Destination == id))
                .ToList();
                result = transactions.Count;
            }
            catch (Exception e)
            {
                Log.Error("Exception at TransactionsWithIdCount() " + e.Message);
                Log.Error(e.StackTrace);
            }
            return Ok(result);
        }

        /// <summary>
        /// Verilen ID ye göre alakalı bütün hareketleri listeler
        /// </summary>
        /// <param name="id">UserId yada EventId </param>
        /// <returns>Hareketler Listesi</returns>
        [HttpGet]
        [Route("get_transactions_with_id")]
        public ActionResult<List<TransactionModel>> GetTransactionsWithId(string id, int limit, int page)
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
                var transactions = Repository
                .GetAllTransactions()
                .Where(x => (x.Source == id || x.Destination == id))
                .OrderByDescending(x => x.TransactionDateTime)
                .Skip(limit * (page - 1))
                .Take(limit)
                .ToList();

                var result = new List<TransactionModel>(transactions.Count);
                int _id = 1;
                foreach (var trans in transactions)
                {
                    string idKim = (trans.Source == id) ? "source" : "dest";
                    //string gelisGidis = 
                    var model = ParseTransactionAsModel(trans, _id, idKim);
                    if (model == null)
                    {
                        continue;
                    }
                    result.Add(model);
                    _id++;
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Exception at GetTransactionsWithId() " + e.Message);
                Log.Error(e.StackTrace);
            }
            return Ok(new List<TransactionModel>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("user_transactions")]
        public ActionResult<List<TransactionModel>> UserTransactions(string id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || (requestee.Role != "Admin" && requestee.UserId != Guid.Parse(id)))
            {
                return Unauthorized();
            }

            try
            {
                var transactions = Repository
                .GetAllTransactions()
                .Where(x => (x.Source == id || x.Destination == id)
                && x.Confirmed && (x.TransactionType == "GOLD" || x.TransactionType == "SILVER"))
                .OrderByDescending(x => x.TransactionDateTime)
                .ToList();

                var result = new List<TransactionModel>(transactions.Count);
                int _id = 1;
                foreach (var trans in transactions)
                {
                    string idKim = (trans.Source == id) ? "source" : "dest";
                    //string gelisGidis = 
                    var model = ParseTransactionAsModel(trans, _id, idKim);
                    if (model == null)
                    {
                        continue;
                    }
                    result.Add(model);
                    _id++;
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error("Exception at GetTransactionsWithId() " + e.Message);
                Log.Error(e.StackTrace);
            }

            return Ok(new List<TransactionModel>());
        }

        private TransactionsDataByDate GetSpecificDayData(List<TransactionsDataByDate> transactionsDataByDates, DateTime key)
        {
            foreach(var transDataByDate in transactionsDataByDates)
            {
                if (transDataByDate.Date.Date == key.Date)
                {
                    return transDataByDate;
                }
            }
            return null;
        }

        /**
         *  var transaction = new Transaction("TRY",
                    model.UserId, 
                    "User", 
                    model.BankId, 
                    "IBAN",
                    transPrice, 
                    randString,
                    false,
                    gram,
                    priceWithout);*/
        private TransactionModel ParseEftUnconfirmedAsModel(Transaction transaction, int _id)
        {
            var source = ExtractSource(transaction);
            var dest = ExtractDestination(transaction);

            var model = new TransactionModel
            {
                Type = "Bekleyen Havale/EFT ile Altın",
                Key = _id,
                IdKim = "source",
                DateTime = transaction.TransactionDateTime.ToString(),
                Amount = transaction.GramAmount,
                Source = source,
                DestinationUser = dest,
                GelisGidis = "gelis",
                PhotoId = dest.MemberId,
                Price = transaction.TlAmount

            };

            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("user_transactions_by_date")]
        public ActionResult<List<TransactionsDataByDate>> ListUserTransactionsByDate(string id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || (requestee.Role != "Admin" && requestee.UserId != Guid.Parse(id)))
            {
                return Unauthorized();
            }

            var transactions = Repository
                .GetAllTransactions()
                .ToList()
                .Where(x => (x.Source == id || x.Destination == id) && x.Confirmed && x.TransactionType == "GOLD")
                .GroupBy(x => x.TransactionDateTime.Date)
                .OrderByDescending(x => x.Key);


            var eft_buys = Repository
                .GetAllTransactions()
                .ToList()
                .Where(x => x.TransactionType == "TRY" && x.Source == id && x.DestinationType == "IBAN" && !x.Confirmed)
                .GroupBy(x => x.TransactionDateTime.Date)
                .OrderByDescending(x => x.Key);
          
            int _id = 1;
            var res = new List<TransactionsDataByDate>();
            foreach(var grp in transactions)
            {
                var key = grp.Key;
                var orderedGrp = grp.OrderByDescending(x => x.TransactionDateTime);
                var data = new TransactionsDataByDate { Date = key, Transactions = new List<TransactionModel>() };
                
                foreach (var trans in orderedGrp)
                {
                    
                    string idKim = (trans.Source == id) ? "source" : "dest";
                    var model = ParseTransactionAsModel(trans, _id, idKim);
                    if (model == null)
                    {
                        continue;
                    }
                    data.Transactions.Add(model);
                    _id++;
                }
                res.Add(data);
            }

            foreach (var grp in eft_buys)
            {
                var key = grp.Key;
                var orderedGrp = grp.OrderByDescending(x => x.TransactionDateTime);
                var data = GetSpecificDayData(res, key);
                if (data == null)
                {
                    data = new TransactionsDataByDate { Date = key, Transactions = new List<TransactionModel>() };
                }
                foreach (var trans in orderedGrp)
                {
                    var model = ParseEftUnconfirmedAsModel(trans, _id);
                    data.Transactions.Add(model);
                    _id++;
                }
                data.Transactions = data.Transactions.OrderBy(x => x.DateTime).ToList();
            }

            return Ok(res);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("transfer_requests_count")]
        public ActionResult<int> TransferRequestsCount(int mode)
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
                if (mode == 1)
                {
                    result = Repository
                    .GetAllTransferRequests()
                    .Where(x => !x.RequestConfirmed)
                    .ToList().Count;

                   
                }
                else if (mode == 2)
                {
                    result = Repository
                    .GetAllTransferRequests()
                    .Where(x => x.RequestConfirmed)
                    .ToList().Count;
                }
                else
                {
                    result = Repository
                    .GetAllTransferRequests()
                    .ToList().Count;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception at transfer request count() " + e.Message);
                Log.Error(e.StackTrace);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="page"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_transfer_requests")]
        public ActionResult<List<TransferRequest>> GetTransferRequests(int limit, int page, int mode) 
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
            var result = new List<TransferRequest>();
            try
            {
                if (mode == 1)
                {
                    var query = Repository
                    .GetAllTransferRequests()
                    .Where(x => !x.RequestConfirmed)
                    .Include(x => x.Transaction)
                    .OrderByDescending(x => x.RequestDateTime)
                    .Skip(limit * (page - 1))
                    .Take(limit)
                    .ToList();
                    result.AddRange(query);
                }
                else if (mode == 2)
                {
                    var query = Repository
                    .GetAllTransferRequests()
                    .Where(x => x.RequestConfirmed)
                    .Include(x => x.Transaction)
                    .OrderByDescending(x => x.RequestDateTime)
                    .Skip(limit * (page - 1))
                    .Take(limit)
                    .ToList();
                    result.AddRange(query);
                }
                else
                {
                    var query = Repository
                    .GetAllTransferRequests()
                    .Include(x => x.Transaction)
                    .OrderByDescending(x => x.RequestDateTime)
                    .Skip(limit * (page - 1))
                    .Take(limit)
                    .ToList();
                    result.AddRange(query);
                }

               
            }
            catch (Exception e)
            {
                Log.Error("Exception at getTransferRequests() " + e.Message);
                Log.Error(e.StackTrace);
                
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("transaction_records_count")]
        public ActionResult<int> TransactionsRecordsCount(int days)
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
            var result = (int)0;
            try
            {
                var date = DateTime.Now.AddDays(-days);
                result = Repository
                .GetAllTransactions()
                .Where(x => x.TransactionDateTime.Date >= date.Date)
                .OrderByDescending(x => x.TransactionDateTime)
                .ToList().Count;




            }
            catch (Exception e)
            {
                Log.Error("Exception at TransactionsRecordsCount() " + e.Message);
                Log.Error(e.StackTrace);

            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="page"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_transaction_records")]
        public ActionResult<List<Transaction>> GetTransactionRecords(int limit, int page, int days)
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
            var result = new List<Transaction>();
            try
            {
                var date = DateTime.Now.AddDays(-days);
                var query = Repository
                .GetAllTransactions()
                .Where(x => x.TransactionDateTime.Date >= date.Date)
                .OrderByDescending(x => x.TransactionDateTime)
                .Skip(limit * (page - 1))
                .Take(limit)
                .ToList();
                result.AddRange(query);
                
                


            }
            catch (Exception e)
            {
                Log.Error("Exception at gettransactionrecords() " + e.Message);
                Log.Error(e.StackTrace);

            }
            return result;
        }

        /// <summary>
        /// Sistemdeki Banka Bilgilerini Listeler
        /// </summary>
        /// <returns>Sistemdeli banka bilgi listesi</returns>
        [HttpGet]
        [Route("get_banks")]
        public ActionResult<List<BankModel>> GetBanks()
        {
            var result = new List<BankModel>();
            try
            {
                var banks = Repository.GetAllBanks().ToList();

                foreach (var bank in banks)
                {
                    result.Add(BankModel.ParseBankAsModel(bank));
                }
            }
            catch (Exception e)
            {
                Log.Error("exception at GetBanks(): " + e.Message);
                Log.Error(e.StackTrace);

            }

            return Ok(result);
        }

        /// <summary>
        /// Banka eft ile altın alma talebinin bildirimlerden çeker
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_bank_request")]
        public ActionResult<GetBankRequestResultModel> GetBankRequest(string id)
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
            var result = new GetBankRequestResultModel
            {
                Success = false
            };

            try
            {
                var bankRequest = Repository.GetAllUserBankTransferRequests()
                    .Where(x => x.BankTransferId.ToString() == id)
                    .FirstOrDefault();


                var trequest = Repository.GetAllTransferRequests()
                    .Where(x => x.TransferRequestId == bankRequest.TransferRequestId)
                    .FirstOrDefault();

                var bank = Repository.GetAllBanks()
                    .Where(x => x.BankId == bankRequest.BankId)
                    .FirstOrDefault();

                var transaction = Repository.GetAllTransactions()
                    .Where(x => x.TransactionId == trequest.TransactionRecord)
                    .FirstOrDefault();

                if (transaction.Source != requestee.UserId.ToString())
                {
                    return Unauthorized();
                }


                result.BankName = bank.BankName;
                result.IBAN = bank.FintagIBAN;
                result.SpecialCode = bankRequest.SpecialCode;
                result.Price = transaction.Amount;
                result.Grams = transaction.GramAmount;
                result.Success = true;
                result.Message = (transaction.TransactionType == "TRY") ? "Havale/EFT ile altın alımı." : "Havale/EFT ile gümüş alımı.";
            }
            catch (Exception e)
            {
                Log.Error("exception at GetBankRequest(): " + e.Message);
                Log.Error(e.StackTrace);
                result.Success = false;
                result.Message = "Bir hata oluştu.";
            }

            return Ok(result);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sell_gold_eft_response")]
        public ActionResult<string> SellGoldEftResponse(SellGoldEftResponseParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId != Guid.Parse(model.Userid))
            {
                return Unauthorized();
            }

            string message = "";
            try
            {
                var user = Repository.GetAllUsers()
                    .Where(x => x.UserId.ToString() == model.Userid)
                    .FirstOrDefault();
                if (user == null)
                {
                    throw new Exception("Kullanıcı bulunamadı. Lütfen tekrar deneyiniz.");
                }

                var transferRequest = Repository.GetAllTransferRequests()
                    .Where(x => x.TransferRequestId.ToString() == model.TransferRequestId)
                    .FirstOrDefault();

                if (transferRequest == null)
                {
                    throw new Exception("Transfer isteği bulunamadı.");
                }

                var transaction = Repository.GetAllTransactions()
                    .Where(x => x.TransactionId == transferRequest.TransactionRecord)
                    .FirstOrDefault();
                if (transaction == null)
                {
                    throw new Exception("İlgili hareket bulunamadı.");
                }

                if (!model.Confirmed)
                {

                    transferRequest.Comments = "Cancelled";
                    transaction.Cancel("Kullanıcı tarafından onaylanmadı.");
                    message = "Kullanıcı tarafından onaylanmadı.";
                } 
                else
                {
                    var availableBalance = user.Balance;

                    if (user.BlockedGrams.HasValue)
                    {
                        availableBalance -= user.BlockedGrams.Value;
                    }

                    if (availableBalance < transaction.GramAmount)
                    {
                        throw new Exception("Yeterli miktarda altınınız bulunmamaktadır, lütfen tekrar deneyiniz.");
                    }

                    if (user.BlockedGrams.HasValue)
                    {
                        user.BlockedGrams = user.BlockedGrams.Value + transaction.GramAmount;
                    }
                    else
                    {
                        user.BlockedGrams = transaction.GramAmount;
                    }

                    transferRequest.RequestConfirmed = true;
                    transferRequest.ConfirmationDateTime = DateTime.Now;
                    

                    var name = user.FirstName + " " + user.FamilyName;

                    var kur = GlobalGoldPrice.GetCurrentPrice().SellRate;

                    var amountTl = transaction.TlAmount;
                    var amountGram = transaction.GramAmount;

                    EmailService.InformFintagAltinBozdur(name,
                        transaction.Comment,
                        kur,
                        amountTl,
                        amountGram,
                        amountTl,
                        transferRequest.TransferRequestId,
                        transaction.TransactionId);

                    transaction.Comment = "Kullanıcı onayladı, Fintag bekleniyor";
                    message = "İki iş günü içerisinde işleminiz gerçekleştirilecektir. İyi günler.";
                    var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";
                    var noti = new Notification2(user.UserId, message, false, "info", null, photo);
                    Repository.AddNotification(noti);

                }
                Repository.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Error("Exception at SellGoldEftResponse(): " + e.Message);
                Log.Error(e.StackTrace);
                message = e.Message;
            }
            return Ok(message);
        }

        /// <summary>
        /// Eft ıle altın satma ıstegı baslatır
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sell_gold_eft")]
        public ActionResult<SellGoldEftResultModel> SellGoldEft(SellGoldEftParamModel model)
        {

            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId != Guid.Parse(model.Userid))
            {
                return Unauthorized();
            }

            var result = new SellGoldEftResultModel { Success = false };
            // Ad soyad iban miktar cins
            try
            {
                var user = Repository.GetAllUsers()
                    .Where(x => x.UserId.ToString() == model.Userid)
                    .FirstOrDefault();
                if (user == null)
                {
                    result.Success = false;
                    result.Message = "Kullanıcı bulunamadı. Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }
                var userLevel = Repository.GetUserLevel(user.UserId);
                if (userLevel == null)
                {
                    result.Success = false;
                    result.Message = "Kullanıcı bulunamadı. Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }
                if (userLevel.Value == 0)
                {
                    result.Success = false;
                    result.Message = "Alım satım işlemleri için hesabınızın onaylanması gerekmektedir.";
                    return Ok(result);
                }
                var ti = new CultureInfo("tr-TR", false).TextInfo;

                var name = user.FirstName + " " + user.FamilyName;
                
                if (!Utility.AlmosEqualsName(ti.ToUpper(model.Name), ti.ToUpper(name)))
                {
                    result.Success = false;
                    result.Message = "Lütfen kendinize ait banka hesabı kullanınız.";
                    return Ok(result);
                }

                if (string.IsNullOrEmpty(model.IBAN) || !IBANValidatorService.ValidateIban(model.IBAN))
                {
                    result.Success = false;
                    result.Message = "Hatalı IBAN numarası. Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }
                GoldType type = GlobalGoldPrice.ParseType(model.GoldType);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
                decimal availableBalance = user.Balance;
                if (user.BlockedGrams.HasValue)
                {
                    availableBalance -= user.BlockedGrams.Value;
                }

                if (availableBalance < gram)
                {
                    result.Success = false;
                    result.Message = "Yeterli miktarda altınınız bulunmamaktadır, lütfen tekrar deneyiniz.";
                    return Ok(result);
                }

                var currentPrice = GlobalGoldPrice.GetCurrentPrice().SellRate;
                var satisKomisyon = GlobalGoldPrice.GetTaxAndExpenses().SatisKomisyon;
                var bsmv = GlobalGoldPrice.GetTaxAndExpenses().BankaMukaveleTax;

                var priceWithout = gram * currentPrice;
                
                var transPrice = (priceWithout  * (2 - bsmv)) - satisKomisyon;

                transPrice = Math.Truncate(100 * transPrice) / 100;

                var karvevergi = priceWithout - transPrice;

                var random = new Random();

                var comment = "Havale/Eft ile altın bozdurma. arc:" + random.Next(100000)+":"+DateTime.Now;

                var transaction = new Transaction(
                    "GOLD", 
                    user.UserId.ToString(),
                    "User", 
                    "Fintag",
                    "IBAN", 
                    gram, 
                    comment,
                    false, 
                    gram,
                    transPrice);
                transaction.Yekun = GetYekun(user);
                transaction.YekunDestination = GetYekun(user);
                Repository.AddTransaction(transaction);
                Repository.SaveChanges();

                transaction = Repository
                    .GetAllTransactions()
                    .Where(x => x.Comment == comment && x.Source == model.Userid)
                    .FirstOrDefault();

                transaction.Comment = model.IBAN;

                var transferRequest = new TransferRequest(user.UserId, 
                    gram, "User", FintagUserId, transaction.TransactionId, comment);

                Repository.AddTransferRequest(transferRequest);

                Repository.SaveChanges();

                transferRequest = Repository
                    .GetAllTransferRequests()
                    .Where(x => x.SourceUserId == user.UserId && x.Comments == comment)
                    .FirstOrDefault();

                
                var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";

                var userModel = new UserModel3
                {
                    Balance = user.Balance,
                    MemberId = user.MemberId,
                    Name = user.FirstName + " " + user.FamilyName,
                    UserId = user.UserId.ToString()

                };

                var transferRequestModel = new TransferRequestModel
                {
                    TransferRequestId = transferRequest.TransferRequestId.ToString(),
                    GramsOfGold = gram,
                    Price = transPrice,
                    BazFiyat = priceWithout,
                    KarVergi = karvevergi,
                    Photo = photo,
                    RequestDateTime = transferRequest.RequestDateTime.ToShortDateString(),
                    Comments = "IBAN: " + model.IBAN,
                    Source = userModel

                };

                result.Message = "Havale/EFT ile Altın bozdurma isteğiniz hazır.";
                result.Transfer = transferRequestModel;
                result.Success = true;

            }
            catch (Exception e)
            {
                Log.Error("Exception at SellGoldEft(): " + e.Message);
                Log.Error(e.StackTrace);
                result.Success = false;
                result.Message = "Hata oluştu! Lütfen tekrar deneyiniz.";
            }

            return Ok(result);
        }
        // Sayın kullanıcımız, 1gr Gümüş alımınız gerçekleşmiştir. İyi günler dileriz.

        [HttpPost]
        [Route("sell_silver_eft_response")]
        public ActionResult<string> SellSilverEftResponse(SellGoldEftResponseParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId != Guid.Parse(model.Userid))
            {
                return Unauthorized();
            }

            string message = "";
            try
            {
                var user = Repository.GetAllUsers()
                    .Where(x => x.UserId.ToString() == model.Userid)
                    .FirstOrDefault();
                if (user == null)
                {
                    throw new Exception("Kullanıcı bulunamadı. Lütfen tekrar deneyiniz.");
                }

                var transferRequest = Repository.GetAllTransferRequests()
                    .Where(x => x.TransferRequestId.ToString() == model.TransferRequestId)
                    .FirstOrDefault();

                if (transferRequest == null)
                {
                    throw new Exception("Transfer isteği bulunamadı.");
                }

                var transaction = Repository.GetAllTransactions()
                    .Where(x => x.TransactionId == transferRequest.TransactionRecord)
                    .FirstOrDefault();
                if (transaction == null)
                {
                    throw new Exception("İlgili hareket bulunamadı.");
                }

                if (!model.Confirmed)
                {

                    transferRequest.Comments = "Cancelled";
                    transaction.Cancel("Kullanıcı tarafından onaylanmadı.");
                    message = "Kullanıcı tarafından onaylanmadı.";
                }
                else
                {
                    var silverBalance = Repository.GetSilverBalance(user.UserId);
                    if (silverBalance == null)
                    {
                        transaction.Cancel("Error tarafindan cancel edildi, silver balance null geldi");
                        transferRequest.Comments = "cancel";
                        message = "Hata Oluştu!!";
                        return Ok(message);
                    }
                    var availableBalance = silverBalance.Balance;

                    if (silverBalance.BlockedGrams.HasValue)
                    {
                        availableBalance -= silverBalance.BlockedGrams.Value;
                    }
                    if (availableBalance < transaction.GramAmount)
                    {
                        throw new Exception("Yeterli miktarda gümüş bulunmamaktadır, lütfen tekrar deneyiniz.");
                    }
                    if (silverBalance.BlockedGrams.HasValue)
                    {
                        silverBalance.BlockedGrams = silverBalance.BlockedGrams.Value + transaction.GramAmount;
                    }
                    else
                    {
                        silverBalance.BlockedGrams = transaction.GramAmount;
                    }

                    transferRequest.RequestConfirmed = true;
                    transferRequest.ConfirmationDateTime = DateTime.Now;


                    var name = user.FirstName + " " + user.FamilyName;

                    var kur = GlobalGoldPrice.GetSilverPrices().SellRate;

                    var amountTl = transaction.TlAmount;
                    var amountGram = transaction.GramAmount;

                    EmailService.InformFintagAltinBozdur(name,
                        transaction.Comment,
                        kur,
                        amountTl,
                        amountGram,
                        amountTl,
                        transferRequest.TransferRequestId,
                        transaction.TransactionId,
                        true);

                    transaction.Comment = "Kullanıcı onayladı, Fintag bekleniyor";
                    message = "İki iş günü içerisinde işleminiz gerçekleştirilecektir. İyi günler.";
                    var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";
                    var noti = new Notification2(user.UserId, message, false, "info", null, photo);
                    Repository.AddNotification(noti);

                }
                Repository.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Error("Exception at sellsilvereftrespose(): " + e.Message);
                Log.Error(e.StackTrace);
                message = "Bir hata oluştu: " + e.Message;
            }
            return Ok(message);
        }

        /// <summary>
        /// Eft ıle altın satma ıstegı baslatır
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sell_silver_eft")]
        public ActionResult<SellGoldEftResultModel> SellSilverEft(SellGoldEftParamModel model)
        {

            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId != Guid.Parse(model.Userid))
            {
                return Unauthorized();
            }

            var result = new SellGoldEftResultModel { Success = false };
            // Ad soyad iban miktar cins
            try
            {
                var user = Repository.GetAllUsers()
                    .Where(x => x.UserId.ToString() == model.Userid)
                    .FirstOrDefault();
                if (user == null)
                {
                    result.Success = false;
                    result.Message = "Kullanıcı bulunamadı. Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }
                var userLevel = Repository.GetUserLevel(user.UserId);
                if (userLevel == null)
                {
                    result.Success = false;
                    result.Message = "Kullanıcı bulunamadı. Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }
                if (userLevel.Value == 0)
                {
                    result.Success = false;
                    result.Message = "Alım satım işlemleri için hesabınızın onaylanması gerekmektedir.";
                    return Ok(result); 
                }
                var ti = new CultureInfo("tr-TR", false).TextInfo;

                var name = user.FirstName + " " + user.FamilyName;

                if (!Utility.AlmosEqualsName(ti.ToUpper(model.Name), ti.ToUpper(name)))
                {
                    result.Success = false;
                    result.Message = "Lütfen kendinize ait banka hesabı kullanınız.";
                    return Ok(result);
                }
                if (string.IsNullOrEmpty(model.IBAN) || !IBANValidatorService.ValidateIban(model.IBAN))
                {
                    result.Success = false;
                    result.Message = "Hatalı IBAN numarası. Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }
                GoldType type = GlobalGoldPrice.ParseType(model.GoldType);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
                var silverBalance = Repository.GetSilverBalance(user.UserId);
                if (silverBalance == null)
                {
                    result.Success = false;
                    result.Message = "Yeterli miktarda gümüş bulunmamaktadır.";
                    return Ok(result);
                }
                decimal availableBalance = silverBalance.Balance;
                if (silverBalance.BlockedGrams.HasValue)
                {
                    availableBalance -= silverBalance.BlockedGrams.Value;
                }

                if (availableBalance < gram)
                {
                    result.Success = false;
                    result.Message = "Yeterli miktarda gümüş bulunmamaktadır, lütfen tekrar deneyiniz.";
                    return Ok(result);
                }

                var currentPrice = GlobalGoldPrice.GetSilverPrices().SellRate;
                var satisKomisyon = GlobalGoldPrice.GetTaxAndExpenses().SatisKomisyon;
                var bsmv = GlobalGoldPrice.GetTaxAndExpenses().BankaMukaveleTax;

                var priceWithout = gram * currentPrice;
                // 1.002 (2 - 1.002) = 0.998 > - sat
                var transPrice = (priceWithout * (2 - bsmv)) - satisKomisyon;

                transPrice = Math.Truncate(100 * transPrice) / 100;

                var karvevergi = priceWithout - transPrice;

                var random = new Random();

                var comment = "Havale/Eft ile silver bozdurma. arc:" + random.Next(100000) + ":" + DateTime.Now;

                var transaction = new Transaction(
                    "SILVER",
                    user.UserId.ToString(),
                    "User",
                    "Fintag",
                    "IBAN",
                    gram,
                    comment,
                    false,
                    gram,
                    transPrice)
                {
                    Yekun = GetYekun(user),
                    YekunDestination = GetYekun(user)
                };

                Repository.AddTransaction(transaction);
                Repository.SaveChanges();

                transaction = Repository
                    .GetAllTransactions()
                    .Where(x => x.Comment == comment && x.Source == model.Userid)
                    .FirstOrDefault();

                transaction.Comment = model.IBAN;

                var transferRequest = new TransferRequest(user.UserId,
                    gram, "User", FintagUserId, transaction.TransactionId, comment);

                Repository.AddTransferRequest(transferRequest);

                Repository.SaveChanges();

                transferRequest = Repository
                    .GetAllTransferRequests()
                    .Where(x => x.SourceUserId == user.UserId && x.Comments == comment)
                    .FirstOrDefault();


                var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";

                var userModel = new UserModel3
                {
                    Balance = user.Balance,
                    MemberId = user.MemberId,
                    Name = user.FirstName + " " + user.FamilyName,
                    UserId = user.UserId.ToString()

                };

                var transferRequestModel = new TransferRequestModel
                {
                    TransferRequestId = transferRequest.TransferRequestId.ToString(),
                    GramsOfGold = gram,
                    Price = transPrice,
                    BazFiyat = priceWithout,
                    KarVergi = karvevergi,
                    Photo = photo,
                    RequestDateTime = transferRequest.RequestDateTime.ToShortDateString(),
                    Comments = "IBAN: " + model.IBAN,
                    Source = userModel

                };

                result.Message = "Havale/EFT ile gümüş bozdurma isteğiniz hazır.";
                result.Transfer = transferRequestModel;
                result.Success = true;

            }
            catch (Exception e)
            {
                Log.Error("Exception at SellGoldEft(): " + e.Message);
                Log.Error(e.StackTrace);
                result.Success = false;
                result.Message = "Hata oluştu: " + e.Message;
            }

            return Ok(result);
        }

        /// <summary>
        /// Silver satin alma istegi ne cevap verme
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("buy_silver_eft_response")]
        public ActionResult<BuyGoldEftResponseResultModel> BuySilverEftResponse(BuyGoldEftResponseParamModel model)
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

            var result = new BuyGoldEftResponseResultModel { Success = false };
            try
            {
                var userBankTransferRequest = Repository
                        .GetAllUserBankTransferRequests()
                        .Where(x => x.BankTransferId.ToString() == model.BankTransferId)
                        .FirstOrDefault();
                if (userBankTransferRequest == null)
                {
                    result.Message = "İstek bulunamadı! Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }

                var user = Repository.GetAllUsers().Where(x => x.UserId == userBankTransferRequest.UserId).FirstOrDefault();
                if (user == null)
                {
                    result.Message = "Kullanıcı bulunamadı! Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }
                if (user.UserId != requestee.UserId)
                {
                    return Unauthorized();
                }
                var code = userBankTransferRequest.SpecialCode;
                var transferRequest = Repository.GetAllTransferRequests().Where(x => x.TransferRequestId == userBankTransferRequest.TransferRequestId).FirstOrDefault();
                if (transferRequest.Comments != "SILVER")
                {
                    result.Message = "HATALI ISLEM";
                    return Ok(result);
                }
                var bank = Repository.GetAllBanks().Where(x => x.BankId == userBankTransferRequest.BankId).FirstOrDefault();
                var transaction = Repository.GetAllTransactions().Where(x => x.TransactionId == transferRequest.TransactionRecord).FirstOrDefault();

                if (model.Confirmed)
                {
                    var message = "Havale/Eft ile gümüş alımı detayları.";
                    var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";


                    transferRequest.RequestConfirmed = true;
                    transferRequest.ConfirmationDateTime = DateTime.Now;
                    var notification = new Notification2(user.UserId, message, true, "eft",
                        userBankTransferRequest.BankTransferId.ToString(), photo);
                    Repository.AddNotification(notification);
                    Repository.SaveChanges();
                    var name = user.FirstName + " " + user.FamilyName;
                    var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetSilverPrices());
                    EmailService.InformFintagBankEFTRequest(
                        name,
                        bank.BankName,
                        currentPrice,
                        transaction.Amount,
                        transaction.GramAmount,
                        code,
                        userBankTransferRequest.CodeStartDateTime,
                        userBankTransferRequest.BankTransferId,
                        transferRequest.TransferRequestId,
                        transaction.TransactionId,
                        true);

                    result.Success = true;
                    result.Message = "İstek hazır! Lütfen verilen IBAN'a ödemeyi açıklama kodu ile beraber gönderiniz.";
                }
                else // ANIL SÜRER
                {
                    // todo delete the request from DB
                    transaction.Cancel("Kullanici iptal etti");
                    result.Message = "Havale/EFT ile altın alımı iptal edilmiştir.";
                    Repository.SaveChanges();
                }

            }
            catch (Exception e)
            {
                Log.Error("Exception at buysilvereft(): " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
            }

            return Ok(result);
        }

        /*
        [HttpPost]
        [Route("buy_silver_eft_response_robot")]
        public ActionResult<BuyGoldEftResponseResultModel> BuySilverEftResponseRobot(BuyGoldEftResponseParamModel model)
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

            var result = new BuyGoldEftResponseResultModel { Success = false };
            try
            {
                var userBankTransferRequest = Repository
                        .GetAllUserBankTransferRequests()
                        .Where(x => x.BankTransferId.ToString() == model.BankTransferId)
                        .FirstOrDefault();
                if (userBankTransferRequest == null)
                {
                    result.Message = "İstek bulunamadı! Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }

                var user = Repository.GetAllUsers().Where(x => x.UserId == userBankTransferRequest.UserId).FirstOrDefault();
                if (user == null)
                {
                    result.Message = "Kullanıcı bulunamadı! Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }
                if (user.UserId != requestee.UserId)
                {
                    return Unauthorized();
                }
                var code = userBankTransferRequest.SpecialCode;
                var transferRequest = Repository.GetAllTransferRequests().Where(x => x.TransferRequestId == userBankTransferRequest.TransferRequestId).FirstOrDefault();
                var bank = Repository.GetAllBanks().Where(x => x.BankId == userBankTransferRequest.BankId).FirstOrDefault();
                var transaction = Repository.GetAllTransactions().Where(x => x.TransactionId == transferRequest.TransactionRecord).FirstOrDefault();

                if (model.Confirmed)
                {
                    //For robot

                    var rate = KTApiService.GetCurrentPriceFromKtApi().value.FxRates.Where(x => x.FxId == 26).FirstOrDefault();

                    var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetSilverPrices());
                    var expectedCash = new GoldtagExpected(
                        user.UserId, //kullanici
                        userBankTransferRequest.BankTransferId,  // usertbank transfer istegi
                        "1",  // paranin gelcegi suffix
                        transaction.Amount,  // gelecek TL
                        26, // fxid silver
                        transaction.GramAmount, // grami
                        rate.BuyRate, // kt buy rate i
                        0, // piaysa fiyati bunu eklicem
                        currentPrice); //bizim satis fiyati 1 gr icin
                    Repository.AddGoldtagExpected(expectedCash);


                    var message = "Havale/Eft ile gümüş alımı detayları.";
                    var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";


                    transferRequest.RequestConfirmed = true;
                    transferRequest.ConfirmationDateTime = DateTime.Now;
                    var notification = new Notification2(user.UserId, message, true, "eft",
                        userBankTransferRequest.BankTransferId.ToString(), photo);
                    Repository.AddNotification(notification);
                    Repository.SaveChanges();
                    var name = user.FirstName + " " + user.FamilyName;

                    EmailService.InformFintagBankEFTRequest(
                        name,
                        bank.BankName,
                        currentPrice,
                        transaction.Amount,
                        transaction.GramAmount,
                        code, // 
                        userBankTransferRequest.CodeStartDateTime,
                        userBankTransferRequest.BankTransferId,
                        transferRequest.TransferRequestId,
                        transaction.TransactionId);

                    result.Success = true;
                    result.Message = "İstek hazır! Lütfen verilen IBAN'a ödemeyi açıklama kodu ile beraber gönderiniz.";
                }
                else
                {
                    // todo delete the request from DB
                    transaction.Cancel("Kullanici iptal etti");
                    result.Message = "Havale/EFT ile altın alımı iptal edilmiştir.";
                    Repository.SaveChanges();
                }

            }
            catch (Exception e)
            {
                Log.Error("Exception at buysılvereftrobotresponse(): " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
            }

            return Ok(result);
        }*/

        /*
        [HttpPost]
        [Route("buy_gold_eft_response_robot")]
        public ActionResult<BuyGoldEftResponseResultModel> BuyGoldEftResponseRobot(BuyGoldEftResponseParamModel model)
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

            var result = new BuyGoldEftResponseResultModel { Success = false };
            try
            {
                var userBankTransferRequest = Repository
                        .GetAllUserBankTransferRequests()
                        .Where(x => x.BankTransferId.ToString() == model.BankTransferId)
                        .FirstOrDefault();
                if (userBankTransferRequest == null)
                {
                    result.Message = "İstek bulunamadı! Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }

                var user = Repository.GetAllUsers().Where(x => x.UserId == userBankTransferRequest.UserId).FirstOrDefault();
                if (user == null)
                {
                    result.Message = "Kullanıcı bulunamadı! Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }
                if (user.UserId != requestee.UserId)
                {
                    return Unauthorized();
                }
                var code = userBankTransferRequest.SpecialCode;
                var transferRequest = Repository.GetAllTransferRequests().Where(x => x.TransferRequestId == userBankTransferRequest.TransferRequestId).FirstOrDefault();
                var bank = Repository.GetAllBanks().Where(x => x.BankId == userBankTransferRequest.BankId).FirstOrDefault();
                var transaction = Repository.GetAllTransactions().Where(x => x.TransactionId == transferRequest.TransactionRecord).FirstOrDefault();

                if (model.Confirmed)
                {
                    // For robot

                    var rate = KTApiService.GetCurrentPriceFromKtApi().value.FxRates.Where(x => x.FxId == 24).FirstOrDefault();
                    var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetCurrentPrice());
                    var expectedCash = new GoldtagExpected(user.UserId,
                        userBankTransferRequest.BankTransferId, "1", transaction.Amount, 24, transaction.GramAmount,
                        rate.BuyRate, 0, currentPrice);
                    Repository.AddGoldtagExpected(expectedCash);


                    var message = "Havale/Eft ile altın alımı detayları.";
                    var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";


                    transferRequest.RequestConfirmed = true;
                    transferRequest.ConfirmationDateTime = DateTime.Now;
                    var notification = new Notification2(user.UserId, message, true, "eft",
                        userBankTransferRequest.BankTransferId.ToString(), photo);
                    Repository.AddNotification(notification);
                    Repository.SaveChanges();
                    var name = user.FirstName + " " + user.FamilyName;
                    
                    EmailService.InformFintagBankEFTRequest(
                        name,
                        bank.BankName,
                        currentPrice,
                        transaction.Amount,
                        transaction.GramAmount,
                        code, // 
                        userBankTransferRequest.CodeStartDateTime,
                        userBankTransferRequest.BankTransferId,
                        transferRequest.TransferRequestId,
                        transaction.TransactionId);

                    result.Success = true;
                    result.Message = "İstek hazır! Lütfen verilen IBAN'a ödemeyi açıklama kodu ile beraber gönderiniz.";
                }
                else
                {
                    // todo delete the request from DB
                    transaction.Cancel("Kullanici iptal etti");
                    result.Message = "Havale/EFT ile altın alımı iptal edilmiştir.";
                    Repository.SaveChanges();
                }

            }
            catch (Exception e)
            {
                Log.Error("Exception at buygoldeftresponserobot(): " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
            }

            return Ok(result);
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("buy_gold_eft_response")]
        public ActionResult<BuyGoldEftResponseResultModel> BuyGoldEftResponse(BuyGoldEftResponseParamModel model)
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

            var result = new BuyGoldEftResponseResultModel { Success = false };
            try
            {
                var userBankTransferRequest = Repository
                        .GetAllUserBankTransferRequests()
                        .Where(x => x.BankTransferId.ToString() == model.BankTransferId)
                        .FirstOrDefault();
                if (userBankTransferRequest == null)
                {
                    result.Message = "İstek bulunamadı! Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }

                var user = Repository.GetAllUsers().Where(x => x.UserId == userBankTransferRequest.UserId).FirstOrDefault();
                if (user == null)
                {
                    result.Message = "Kullanıcı bulunamadı! Lütfen tekrar deneyiniz.";
                    return Ok(result);
                }
                if (user.UserId != requestee.UserId)
                {
                    return Unauthorized();
                }
                var code = userBankTransferRequest.SpecialCode;
                var transferRequest = Repository.GetAllTransferRequests().Where(x => x.TransferRequestId == userBankTransferRequest.TransferRequestId).FirstOrDefault();
                var bank = Repository.GetAllBanks().Where(x => x.BankId == userBankTransferRequest.BankId).FirstOrDefault();
                var transaction = Repository.GetAllTransactions().Where(x => x.TransactionId == transferRequest.TransactionRecord).FirstOrDefault();
                
                if (model.Confirmed)
                {
                    var message = "Havale/Eft ile altın alımı detayları.";
                    var photo = "http://www.fintag.net/images/temp_profile_photos/111222333.jpg";

                    
                    transferRequest.RequestConfirmed = true;
                    transferRequest.ConfirmationDateTime = DateTime.Now;
                    var notification = new Notification2(user.UserId, message, true, "eft",
                        userBankTransferRequest.BankTransferId.ToString(), photo);
                    Repository.AddNotification(notification);
                    Repository.SaveChanges();
                    var name = user.FirstName + " " + user.FamilyName;
                    var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetCurrentPrice());
                    EmailService.InformFintagBankEFTRequest(
                        name, 
                        bank.BankName, 
                        currentPrice, 
                        transaction.Amount,
                        transaction.GramAmount,
                        code, // 
                        userBankTransferRequest.CodeStartDateTime, 
                        userBankTransferRequest.BankTransferId,
                        transferRequest.TransferRequestId,
                        transaction.TransactionId);

                    result.Success = true;
                    result.Message = "İstek hazır! Lütfen verilen IBAN'a ödemeyi açıklama kodu ile beraber gönderiniz.";
                }
                else
                {
                    // todo delete the request from DB
                    transaction.Cancel("Kullanici iptal etti");
                    result.Message = "Havale/EFT ile altın alımı iptal edilmiştir.";
                    Repository.SaveChanges();
                }
                
            }
            catch (Exception e)
            {
                Log.Error("Exception at SellGoldEft(): " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu.";
            }

            return Ok(result);
        }

        /// <summary>
        /// EFT Havale ile Altın alma işlemi
        /// </summary>
        /// <param name="model">Altın Alma EFT Modeli</param>
        /// <returns>Altın Alma Sonuç Modeli</returns>
        [HttpPost]
        [Route("buy_silver_eft")]
        public ActionResult<UserBankTransferRequestResultModel> BuySilverEft(BuyGoldEftParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;


            // Log.Debug(model.ToString());

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId != Guid.Parse(model.UserId))
            {
                return Unauthorized();
            }

            var result = new UserBankTransferRequestResultModel
            {
                Success = false
            };
            try
            {
                // Kullanıcı yı BUL
                var user = Repository.GetAllUsers().Where(x => x.UserId.ToString() == model.UserId).FirstOrDefault();
                if (user == null)
                {
                    throw new Exception("Kullanıcı Bulunamadı! Lütfen tekrar deneyiniz.");
                }
                var userLevel = Repository.GetUserLevel(user.UserId);
                if (userLevel == null)
                {
                    throw new Exception("Hatali kullanici");
                }
                if (userLevel.Value == 0)
                {
                    throw new Exception("Alım satım işlemleri için hesabınızın onaylanması gerekmektedir.");
                }
                // Bankayı bul
                var bank = Repository.GetAllBanks().Where(x => x.BankId.ToString() == model.BankId).FirstOrDefault();

                if (bank == null)
                {
                    throw new Exception("Banka kaydı bulunamadı! Lütfen tekrar deneyiniz.");
                }

                GoldType type = GlobalGoldPrice.ParseType(model.Type);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
                // Log.Debug("debug eft buy gram: " + gram.ToString());

                var silverBalance = Repository.GetSilverBalance(user.UserId);
                if (silverBalance == null)
                {
                    silverBalance = new SilverBalance(user.UserId);
                    Repository.AddSilverBalance(silverBalance);
                }

                var userBalance = silverBalance.Balance;

                FxRate currentRate = GlobalGoldPrice.GetSilverPrices();
                var potentialGrams = gram + userBalance;

                var potentialTRY = potentialGrams * currentRate.BuyRate;

                if (userLevel.Value == 1 && potentialTRY > 19999)
                {
                    throw new Exception("1. seviye kullanıcılar en fazla 19,999.00 TRY değerinde gümüş bulundurabilir.");
                }


                if (userLevel.Value == 2 && potentialTRY > 99999)
                {
                    throw new Exception("2. seviye kullanıcılar en fazla 99,999.00 TRY değerinde gümüş bulundurabilir.");
                }

                
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();

                var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(currentRate);

                var priceWithout = (gram * currentPrice);
                priceWithout = Math.Truncate(100 * priceWithout) / 100;

                var transPrice = (priceWithout * taxAndExpenses.BankaMukaveleTax) + taxAndExpenses.Kar;
                transPrice = Math.Truncate(100 * transPrice) / 100;
                var rand = new Random(); // random comment
                var randString = ":silver:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;
                // transaction yarat 
                var transaction = new Transaction(
                    "TRY_FOR_SILVER",
                    model.UserId,
                    "User",
                    model.BankId,
                    "IBAN",
                    transPrice,
                    randString,
                    false,
                    gram,
                    priceWithout);
                // Log.Debug(Newtonsoft.Json.JsonConvert.SerializeObject(transaction));
                transaction.Yekun = GetYekun(user);
                Repository.AddTransaction(transaction);
                Repository.SaveChanges(); // Bu aşamada transaction id veri tabanında oluşuyor
                // yaratılan transaction ı bul, sonucta ID database tarafından veriliyor
                transaction = Repository.GetAllTransactions().Where(x => x.Comment == randString).FirstOrDefault();
                // transfer request hazırla
                var transferRequest = new TransferRequest(
                    user.UserId,
                    gram, "User", model.UserId,
                    transaction.TransactionId,
                    randString);
                //Log.Debug(Newtonsoft.Json.JsonConvert.SerializeObject(transferRequest));
                Repository.AddTransferRequest(transferRequest);
                Repository.SaveChanges(); // transfer request ID al
                // transfer requesti bul
                transferRequest = Repository.GetAllTransferRequests().Where(x => x.Comments == randString).FirstOrDefault();
                transferRequest.Comments = "SILVER";
                // Special Code yarat
                var code = Utility.GenerateRandBankTransferCode(Repository); // Bu kod EFT havale açıklamasına girilmeli
                // Bank transfer request yarat
                var userBankTransferRequest = new UserBankTransferRequest(
                    code,
                    bank.BankId,
                    transferRequest.TransferRequestId,
                    user.UserId);

                //Log.Debug(Newtonsoft.Json.JsonConvert.SerializeObject(userBankTransferRequest));
                Repository.AddUserBankTansferRequest(userBankTransferRequest);
                Repository.SaveChanges(); // Bank transfer requesti kaydet
                userBankTransferRequest = Repository.GetAllUserBankTransferRequests().Where(x => x.SpecialCode == code).FirstOrDefault();
                // result a transfer model ve bank transfer model ekle
                result.Transfer = new TransferRequestModel
                {
                    Price = transPrice,
                    GramsOfGold = gram,
                    RequestDateTime = transferRequest.RequestDateTime.ToString(),
                    TransferRequestId = transferRequest.TransferRequestId.ToString(),
                    KarVergi = transPrice - priceWithout,
                    BazFiyat = priceWithout

                };
                result.UserBankTransferRequest = new UserBankTransferRequestModel
                {
                    SpecialCode = userBankTransferRequest.SpecialCode,
                    BankTransferId = userBankTransferRequest.BankTransferId.ToString(),
                    KarVergi = transPrice - priceWithout,
                    BazFiyat = priceWithout
                };
                // işlem isteği hazır 
                result.Message = "İşlem isteği hazır.";
                result.Success = true;
            }
            catch (Exception e)
            {
                Log.Error("Error at BuyGoldEft: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
            }

            return Ok(result);
        }

        /// <summary>
        /// EFT Havale ile Altın alma işlemi
        /// </summary>
        /// <param name="model">Altın Alma EFT Modeli</param>
        /// <returns>Altın Alma Sonuç Modeli</returns>
        [HttpPost]
        [Route("buy_gold_eft")]
        public ActionResult<UserBankTransferRequestResultModel> BuyGoldEft(BuyGoldEftParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;


           // Log.Debug(model.ToString());

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId != Guid.Parse(model.UserId))
            {
                return Unauthorized();
            }

            var result = new UserBankTransferRequestResultModel
            {
                Success = false
            };
            try
            {
                // Kullanıcı yı BUL
                var user = Repository.GetAllUsers().Where(x => x.UserId.ToString() == model.UserId).FirstOrDefault();
                if (user == null)
                {
                    throw new Exception("Kullanıcı Bulunamadı! Lütfen tekrar deneyiniz.");
                }
                var userLevel = Repository.GetUserLevel(user.UserId);
                if (userLevel == null)
                {
                    throw new Exception("Hatali kullanici");
                }
                if (userLevel.Value == 0)
                {
                    throw new Exception("Alım satım işlemleri için hesabınızın onaylanması gerekmektedir.");
                }
                // Bankayı bul
                var bank = Repository.GetAllBanks().Where(x => x.BankId.ToString() == model.BankId).FirstOrDefault();

                if (bank == null)
                {
                    throw new Exception("Banka kaydı bulunamadı! Lütfen tekrar deneyiniz.");
                }

                GoldType type = GlobalGoldPrice.ParseType(model.Type);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);

                var userBalance = user.Balance;

                var potentialGrams = gram + userBalance;

                var rate = GlobalGoldPrice.GetCurrentPrice();

                var potentialTRY = potentialGrams * rate.BuyRate;

                if (userLevel.Value == 1 && potentialTRY > 19999)
                {
                    throw new Exception("1. seviye kullanıcılar en fazla 19,999.00 TRY değerinde altın bulundurabilir.");
                }


                if (userLevel.Value == 2 && potentialTRY > 99999)
                {
                    throw new Exception("2. seviye kullanıcılar en fazla 99,999.00 TRY değerinde altın bulundurabilir.");
                }

                
                FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();

                var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(currentRate);

                var priceWithout = (gram * currentPrice);
                priceWithout = Math.Truncate(100 * priceWithout) / 100;

                var transPrice = (priceWithout * taxAndExpenses.BankaMukaveleTax) + taxAndExpenses.Kar;

                transPrice = Math.Truncate(100 * transPrice) / 100;
                var rand = new Random(); // random comment
                var randString = ":arc:" + rand.Next(0, 100000).ToString() +":" + DateTime.Now;
                // transaction yarat 
                var transaction = new Transaction(
                    "TRY",
                    model.UserId, 
                    "User", 
                    model.BankId, 
                    "IBAN",
                    transPrice, 
                    randString,
                    false,
                    gram,
                    priceWithout);
               // Log.Debug(Newtonsoft.Json.JsonConvert.SerializeObject(transaction));
                transaction.Yekun = GetYekun(user); 
                Repository.AddTransaction(transaction);
                Repository.SaveChanges(); // Bu aşamada transaction id veri tabanında oluşuyor
                // yaratılan transaction ı bul, sonucta ID database tarafından veriliyor
                transaction = Repository.GetAllTransactions().Where(x => x.Comment == randString).FirstOrDefault();
                // transfer request hazırla
                var transferRequest = new TransferRequest(
                    user.UserId, 
                    gram, "User", model.UserId, 
                    transaction.TransactionId, randString);
                //Log.Debug(Newtonsoft.Json.JsonConvert.SerializeObject(transferRequest));
                Repository.AddTransferRequest(transferRequest);
                Repository.SaveChanges(); // transfer request ID al
                // transfer requesti bul
                transferRequest = Repository.GetAllTransferRequests().Where(x => x.Comments == randString).FirstOrDefault();
                // Special Code yarat
                var code = Utility.GenerateRandBankTransferCode(Repository); // Bu kod EFT havale açıklamasına girilmeli
                // Bank transfer request yarat
                var userBankTransferRequest = new UserBankTransferRequest(
                    code, 
                    bank.BankId, 
                    transferRequest.TransferRequestId, 
                    user.UserId);
                //Log.Debug(Newtonsoft.Json.JsonConvert.SerializeObject(userBankTransferRequest));
                Repository.AddUserBankTansferRequest(userBankTransferRequest);
                Repository.SaveChanges(); // Bank transfer requesti kaydet
                userBankTransferRequest = Repository.GetAllUserBankTransferRequests().Where(x => x.SpecialCode == code).FirstOrDefault();
                // result a transfer model ve bank transfer model ekle
                result.Transfer = new TransferRequestModel { 
                    Price = transPrice, 
                    GramsOfGold = gram, 
                    RequestDateTime = transferRequest.RequestDateTime.ToString(), 
                    TransferRequestId = transferRequest.TransferRequestId.ToString(),
                    KarVergi = transPrice - priceWithout,
                    BazFiyat = priceWithout
                    
                };
                result.UserBankTransferRequest = new UserBankTransferRequestModel  {  
                    SpecialCode = userBankTransferRequest.SpecialCode, 
                    BankTransferId = userBankTransferRequest.BankTransferId.ToString(),
                    KarVergi = transPrice - priceWithout,
                    BazFiyat = priceWithout
                };
                // işlem isteği hazır 
                result.Message = "İşlem isteği hazır.";
                result.Success = true;
                
                
            }
            catch (Exception e)
            {
                Log.Error("Error at BuyGoldEft: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
            }

            return Ok(result);
        }

        /// <summary>
        /// Baska kullanıcıdan altın talebini istenen kullanıcıya çekme
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get_gold_request")]
        public ActionResult<GetGoldRequestResultModel> GetGoldRequest(string id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized("token invalid");
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null)
            {
                return Unauthorized("requestee invalid");
            }
            var result = new GetGoldRequestResultModel
            {
                Success = false
            };

            try
            {
                Log.Information("get gold request started");
                var request = Repository.GetAllTransferRequests()
                    .Where(x => x.TransferRequestId.ToString() == id)
                    .FirstOrDefault();

                var transaction = Repository.GetAllTransactions()
                    .Where(x => x.TransactionId == request.TransactionRecord)
                    .FirstOrDefault();

                var sourceUser = Repository.GetAllUsers()
                    .Where(x => x.UserId == request.SourceUserId)
                    .FirstOrDefault();

                var destinationUser = Repository.GetAllUsers()
                    .Where(x => x.UserId.ToString() == request.Destination)
                    .FirstOrDefault();

                if (requestee.UserId != sourceUser.UserId)
                {
                    return Unauthorized("requestee and source user not matching");
                }

                var source = new UserModel3
                {
                    UserId = sourceUser.UserId.ToString(),
                    Balance = sourceUser.Balance,
                    MemberId = sourceUser.MemberId,
                    Name = sourceUser.FirstName + " " + sourceUser.FamilyName
                };

                var des = new UserModel3
                {
                    UserId = destinationUser.UserId.ToString(),
                    Balance = destinationUser.Balance,
                    MemberId = destinationUser.MemberId,
                    Name = destinationUser.FirstName + " " + destinationUser.FamilyName
                };

                var photo = "http://www.fintag.net/images/temp_profile_photos/" + des.MemberId + ".jpg";
                var transferRequestModel = new TransferRequestModel
                {
                    TransferRequestId = request.TransferRequestId.ToString(),
                    Source = source,
                    DestinationUser = des,
                    GramsOfGold = request.GramsOfGold,
                    Price = transaction.TlAmount,
                    RequestDateTime = request.RequestDateTime.ToString(),
                    Comments = transaction.Comment,
                    Photo = photo
                    
                };


                result.Success = true;
                result.Transfer = transferRequestModel;
                result.Message = "Transfer isteği hazır.";
                Log.Information("get gold transfer request complete");

            }
            catch (Exception e)
            {
                Log.Error("Error at GetGoldRequest: " + e.Message);
                Log.Error(e.StackTrace);
                result.Success = false;
                result.Message = "Bir hata oluştu.";
            }

            return Ok(result);
        }

        /// <summary>
        /// Baska kullanıcıdan altın talebi
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("request_gold")]
        public ActionResult<RequestGoldResultModel> RequestGoldFromAnotherUser(RequestGoldParamModel model)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requeste = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requeste == null || requeste.UserId != Guid.Parse(model.UserId))
            {
                return Unauthorized();
            }

            var result = new RequestGoldResultModel { Success = false };

            try
            {
                var user = Repository.GetAllUsers().Where(x => x.UserId.ToString() == model.UserId).FirstOrDefault();

                if (user == null)
                {
                    throw new Exception("Kullanıcı bulunamadı! Lütfen tekrar deneyiniz.");
                }
                var userLevel = Repository.GetUserLevel(user.UserId);
                if (userLevel == null)
                {
                    throw new Exception("Hatali kullanici");
                }
                if (userLevel.Value == 0)
                {
                    throw new Exception("Alım satım işlemleri için hesabınızın onaylanması gerekmektedir.");
                }

                var userBalance = user.Balance;
             
                GoldType type = GlobalGoldPrice.ParseType(model.GoldType);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.GoldAmount);
                FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();

                var potentialGrams = gram + userBalance;

                var potentialTRY = potentialGrams * currentRate.BuyRate;

                if (userLevel.Value == 1 && potentialTRY > 19999)
                {
                    throw new Exception("1. seviye kullanıcılar en fazla 19,999.00 TRY değerinde altın bulundurabilir.");
                }


                if (userLevel.Value == 2 && potentialTRY > 99999)
                {
                    throw new Exception("2. seviye kullanıcılar en fazla 99,999.00 TRY değerinde altın bulundurabilir.");
                }

                var requestee = Repository.GetAllUsers().Where(x => x.MemberId == int.Parse(model.MemberId)).FirstOrDefault();

                if (requestee == null)
                {
                    throw new Exception("Böyle bir üye numarası bulunamadı: " + model.MemberId);
                }
                var fullName = requestee.FirstName + " " + requestee.FamilyName;

                if (model.MemberName.ToLower() != fullName.ToLower())
                {
                    throw new Exception("Kullanıcı adı/soyadı ile üye numarası uyuşmamaktadır.");
                }

                if (user.UserId == requestee.UserId)
                {
                    throw new Exception("Kendinizden Altın talep edemezsiniz.");
                }
                

                var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetCurrentPrice());

                var transPrice = gram * currentPrice;

                var rand = new Random();

                var randString = ":arc:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;



                var transaction = new Transaction("GOLD", requestee.UserId.ToString(),
                    "User", user.UserId.ToString(),
                    "User", gram, randString, false, gram, transPrice);

                transaction.Yekun = GetYekun(requestee);
                transaction.YekunDestination = GetYekun(user);

                Repository.AddTransaction(transaction);
                Repository.SaveChanges();
                transaction = Repository.GetAllTransactions().Where(x => x.Comment == randString).FirstOrDefault();

                transaction.Comment = (model.Comment == null) ? "Açıklamasız." : model.Comment;

                var request = new TransferRequest(requestee.UserId, gram, "User", user.UserId.ToString(), transaction.TransactionId,
                    randString);

                Repository.AddTransferRequest(request);
                Repository.SaveChanges();

                request = Repository.GetAllTransferRequests().Where(x => x.Comments == randString).FirstOrDefault();

                var message = user.FirstName + " " + user.FamilyName + " kullanıcısından Altın talebiniz var.";
                var photo = "http://www.fintag.net/images/temp_profile_photos/" + user.MemberId + ".jpg";
                var notification = new Notification2(requestee.UserId,
                    message, true, "request", request.TransferRequestId.ToString(), photo);

                Repository.AddNotification(notification);
                Repository.SaveChanges();
                result.Success = true;
                result.Message = "Altın talebiniz iletildi.";
            } 
            catch (Exception e)
            {
                Log.Error("Error at RequestGoldFromAnotherUser() " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
                result.Success = false;
            }

            return Ok(result);
        }

        private ActionResult<String> BuyCCWedding(BuyCCEventparams model)
        {
            var user = Repository
                    .GetAllUsers()
                    .Where(x => x.UserId.ToString() == model.UserId)
                    .FirstOrDefault();
            var wedding = Repository
                .GetAllWeddings()
                .Where(x => x.WeddingId.ToString() == model.EventId)
                .FirstOrDefault();
            if (wedding == null)
            {
                return Ok("Wedding hatasi");
            }
            GoldType type = GlobalGoldPrice.ParseType(model.Type);
            decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);

            FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();
            var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();

            var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(currentRate);
            var priceWithout = (gram * currentPrice);
            priceWithout = Math.Truncate(100 * priceWithout) / 100;

            var transPrice = (gram * currentPrice * taxAndExpenses.VposCommission * taxAndExpenses.BankaMukaveleTax)
                        + taxAndExpenses.Kar;
            transPrice = Math.Truncate(100 * transPrice) / 100;
            var rand = new Random();

            var randString = ":arc:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

            var transaction = new Transaction("TRY",
                model.UserId,
                "User",
                wedding.WeddingId.ToString(),
                "Wedding",
                transPrice,
                randString,
                false,
                gram,
                priceWithout);
            Repository.AddTransaction(transaction);
            Repository.SaveChanges();
            transaction = Repository
                .GetAllTransactions()
                .Where(x => x.Source == model.UserId && x.Comment == randString)
                .FirstOrDefault();

            


            int ccTlAmount = (int)(transPrice * 100);

            string year = model.ExpiryYear;

            if (year.Length > 2)
            {
                int startIndex = (year.Length == 4) ? 2 : 1;
                year = year.Substring(startIndex);
            }

            var kartOnayModel = new KartOnayModel
            {
                CardNumber = model.CreditCard,
                ExpiryYear = year,
                ExpiryMonth = model.ExpiryMonth,
                Cvv = model.CardCode,
                HolderName = model.HolderName,
                Amount = ccTlAmount.ToString(),
                TransactionId = transaction.TransactionId.ToString(),
                CallbackType = CallbackType.EventYolla
            };

            var result = VposService.KartOnay(kartOnayModel);


            System.IO.File.WriteAllText("\\inetpub\\wwwroot\\html\\temp\\" + transaction.TransactionHtml(), result);

            return Ok("http://www.fintag.net/html/temp/" + transaction.TransactionHtml());

        }

        private ActionResult<String> BuyCCEvent(BuyCCEventparams model)
        {
            var user = Repository
                    .GetAllUsers()
                    .Where(x => x.UserId.ToString() == model.UserId)
                    .FirstOrDefault();
            var _event = Repository
                .GetAllEvents()
                .Where(x => x.EventId.ToString() == model.EventId)
                .FirstOrDefault();
            if (_event == null)
            {
                return Ok("Event hatasi");
            }
            GoldType type = GlobalGoldPrice.ParseType(model.Type);
            decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
            FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();
            var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();

            var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(currentRate);

            var priceWithout = gram * currentPrice;
            priceWithout = Math.Truncate(100 * priceWithout) / 100; 
            var transPrice = (gram * currentPrice * taxAndExpenses.VposCommission * taxAndExpenses.BankaMukaveleTax)
                        + taxAndExpenses.Kar;
            
            transPrice = Math.Truncate(100 * transPrice) / 100;

            var rand = new Random();

            var randString = ":arc:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

            var transaction = new Transaction(
                "TRY",
                model.UserId,
                "User",
                _event.EventId.ToString(),
                "Event",
                transPrice,
                randString,
                false,
                gram,
                priceWithout);
            Repository.AddTransaction(transaction);
            Repository.SaveChanges();
            transaction = Repository.GetAllTransactions().Where(x => x.Source == model.UserId && x.Comment == randString).FirstOrDefault();


            int ccTlAmount = (int)(transPrice * 100);

            string year = model.ExpiryYear;

            if (year.Length > 2)
            {
                int startIndex = (year.Length == 4) ? 2 : 1;
                year = year.Substring(startIndex);
            }

            var kartOnayModel = new KartOnayModel
            {
                CardNumber = model.CreditCard,
                ExpiryYear = year,
                ExpiryMonth = model.ExpiryMonth,
                Cvv = model.CardCode,
                HolderName = model.HolderName,
                Amount = ccTlAmount.ToString(),
                TransactionId = transaction.TransactionId.ToString(),
                CallbackType = CallbackType.EventYolla
            };

            var result = VposService.KartOnay(kartOnayModel);


            System.IO.File.WriteAllText("\\inetpub\\wwwroot\\html\\temp\\" + transaction.TransactionHtml(), result);

            return Ok("http://www.fintag.net/html/temp/" + transaction.TransactionHtml());

        }

        [HttpPost]
        [Route("buy_with_cc_event")]
        public ActionResult<string> BuyCCforEvent(BuyCCEventparams model)
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
            /*
            if (1 == 1)
            {
                return Unauthorized("Kredi kartı sistemi geçici olarak durdurulmuştur. Lütfen daha sonra tekrar deneyiniz.");
            }*/
            try
            {

                var user = Repository
                    .GetAllUsers()
                    .Where(x => x.UserId.ToString() == model.UserId)
                    .FirstOrDefault();
                if (user == null)
                {
                    return Ok("Bad request");
                }
                var userLevel = Repository.GetUserLevel(user.UserId);
                if (userLevel == null)
                {
                    return Ok("Hatali kullanici");
                }
                if (userLevel.Value == 0)
                {
                    return Ok("Alım satım işlemleri için hesabınızın onaylanması gerekmektedir.");
                }
                var userBalance = user.Balance;

                GoldType type = GlobalGoldPrice.ParseType(model.Type);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
                FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();

                var potentialGrams = gram + userBalance;

                var potentialTRY = potentialGrams * currentRate.BuyRate;

                if (userLevel.Value == 1 && potentialTRY > 19999)
                {
                    throw new Exception("1. seviye kullanıcılar en fazla 19,999.00 TRY değerinde altın satın alabilir.");
                }

                if (userLevel.Value == 2 && potentialTRY > 99999)
                {
                    throw new Exception("2. seviye kullanıcılar en fazla 99,999.00 TRY değerinde altın satın alabilir.");
                }

                var ti = new CultureInfo("tr-TR", false).TextInfo;

                var name = user.FirstName + " " + user.FamilyName;

                if (ti.ToUpper(model.HolderName) != name)
                {
                    return Ok("Goldtag hesabınızda kayıtlı isimle kart ismi uyuşmamaktadır.");
                }

                if (model.EventType == "event")
                {
                    return BuyCCEvent(model);
                }
                else
                {
                    return BuyCCWedding(model);
                }
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }
        }

        /// <summary>
        /// Kullanıcı Altın almaö vpos ile
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("buy_with_cc_silver")]
        public ActionResult<string> BuySilverCC(BuyCCparams model)
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

            try
            {
                string year = model.ExpiryYear;

                if (year.Length > 2)
                {
                    int startIndex = (year.Length == 4) ? 2 : 1;
                    year = year.Substring(startIndex);
                }
                var user = Repository
                    .GetAllUsers()
                    .Where(x => x.UserId.ToString() == model.UserId)
                    .FirstOrDefault();
                if (user == null)
                {
                    return Ok("Kullanıcı bulunamadı");
                }

                var userLevel = Repository.GetUserLevel(user.UserId);
                if (userLevel == null)
                {
                    return Ok("Hatali kullanici");
                }
                if (userLevel.Value == 0)
                {
                    return Ok("Alım satım işlemleri için hesabınızın onaylanması gerekmektedir.");
                }

                GoldType type = GlobalGoldPrice.ParseType(model.Type);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
                FxRate currentRate = GlobalGoldPrice.GetSilverPrices();

                var silverBalance = Repository.GetSilverBalance(user.UserId);
                if (silverBalance == null)
                {
                    silverBalance = new SilverBalance(user.UserId);
                    Repository.AddSilverBalance(silverBalance);
                    Repository.SaveChanges();
                }

                var userBalance = silverBalance.Balance;

                var potentialGrams = gram + userBalance;

                var potentialTRY = potentialGrams * currentRate.BuyRate;

                if (userLevel.Value == 1 && potentialTRY > 19999)
                {
                    throw new Exception("1. seviye kullanıcılar en fazla 19,999.00 TRY değerinde gümüş bulundurabilir.");
                }


                if (userLevel.Value == 2 && potentialTRY > 99999)
                {
                    throw new Exception("2. seviye kullanıcılar en fazla 99,999.00 TRY değerinde gümüş bulundurabilir.");
                }

                var ti = new CultureInfo("tr-TR", false).TextInfo;

                var name = user.FirstName + " " + user.FamilyName;

                if (ti.ToUpper(model.HolderName) != name)
                {
                    return Ok("Goldtag hesabınızda kayıtlı isimle kart ismi uyuşmamaktadır.");
                }


                if (gram < 1)
                {
                    return Ok("Minimum 1 gram gümüş alınabilinir");
                }
                
                
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();

                var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(currentRate);
                var priceWithout = (gram * currentPrice);
                priceWithout = Math.Truncate(100 * priceWithout) / 100;

                var transPrice = (gram * currentPrice * taxAndExpenses.VposCommission * taxAndExpenses.BankaMukaveleTax)
                            + taxAndExpenses.Kar;

                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = ":arc:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var transaction = new Transaction(
                    "TRY_FOR_SILVER",
                    model.UserId,
                    "User",
                    "Fintag",
                    "VirtualPos",
                    transPrice,
                    randString,
                    false,
                    gram,
                    priceWithout);
                Repository.AddTransaction(transaction);
                Repository.SaveChanges();
                transaction = Repository.GetAllTransactions().Where(x => x.Source == model.UserId && x.Comment == randString).FirstOrDefault();


                int ccTlAmount = (int)(transPrice * 100);

                var kartOnayModel = new KartOnayModel
                {
                    CardNumber = model.CreditCard,
                    ExpiryYear = year,
                    ExpiryMonth = model.ExpiryMonth,
                    Cvv = model.CardCode,
                    HolderName = model.HolderName,
                    Amount = ccTlAmount.ToString(),
                    TransactionId = transaction.TransactionId.ToString(),
                    CallbackType = CallbackType.SilverSatinAlma
                };

                var result = VposService.KartOnay(kartOnayModel);


                System.IO.File.WriteAllText("\\inetpub\\wwwroot\\html\\temp\\" + transaction.TransactionHtml(), result);

                return Ok("http://www.fintag.net/html/temp/" + transaction.TransactionHtml());
            }
            catch (Exception e)
            {
                return Ok("Hata:" + e.Message);
            }

        }


        /// <summary>
        /// Kullanıcı Altın almaö vpos ile
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("buy_with_cc")]
        public ActionResult<string> BuyGoldCC(BuyCCparams model)
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

            // temporaryly disabled
            /*
            if (1 == 1)
            {
                return Unauthorized("Kredi kartı sistemi geçici olarak durdurulmuştur. Lütfen daha sonra tekrar deneyiniz.");
            }*/

            try
            {
                string year = model.ExpiryYear;

                if (year.Length > 2)
                {
                    int startIndex = (year.Length == 4) ? 2 : 1;
                    year = year.Substring(startIndex);
                }
                var user = Repository
                    .GetAllUsers()
                    .Where(x => x.UserId.ToString() == model.UserId)
                    .FirstOrDefault();
                if (user == null)
                {
                    return Ok("Kullanıcı bulunamadı");
                }

                var userLevel = Repository.GetUserLevel(user.UserId);
                if (userLevel == null)
                {
                    return Ok("Hatali kullanici");
                }
                if (userLevel.Value == 0)
                {
                    return Ok("Alım satım işlemleri için hesabınızın onaylanması gerekmektedir.");
                }

                var userBalance = user.Balance;
                
                GoldType type = GlobalGoldPrice.ParseType(model.Type);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
                FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();

                var potentialGrams = gram + userBalance;

                var potentialTRY = potentialGrams * currentRate.BuyRate;

                if (userLevel.Value == 1 && potentialTRY > 19999)
                {
                    throw new Exception("1. seviye kullanıcılar en fazla 19,999.00 TRY değerinde altın bulundurabilir.");
                }


                if (userLevel.Value == 2 && potentialTRY > 99999)
                {
                    throw new Exception("2. seviye kullanıcılar en fazla 99,999.00 TRY değerinde altın bulundurabilir.");
                }


                var ti = new CultureInfo("tr-TR", false).TextInfo;

                var name = user.FirstName + " " + user.FamilyName;

                if (ti.ToUpper(model.HolderName) != name)
                {
                    return Ok("Goldtag hesabınızda kayıtlı isimle kart ismi uyuşmamaktadır.");
                }

                
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();

                var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(currentRate);
                var priceWithout = (gram * currentPrice);
                priceWithout = Math.Truncate(100 * priceWithout) / 100;

                var transPrice = (gram * currentPrice * taxAndExpenses.VposCommission * taxAndExpenses.BankaMukaveleTax)
                            + taxAndExpenses.Kar;
                
                transPrice = Math.Truncate(100 * transPrice) / 100;

                var rand = new Random();

                var randString = ":arc:" + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;

                var transaction = new Transaction(
                    "TRY",
                    model.UserId,
                    "User",
                    "Fintag",
                    "VirtualPos",
                    transPrice,
                    randString,
                    false,
                    gram,
                    priceWithout);
                Repository.AddTransaction(transaction);
                Repository.SaveChanges();
                transaction = Repository.GetAllTransactions().Where(x => x.Source == model.UserId && x.Comment == randString).FirstOrDefault();


                int ccTlAmount = (int)(transPrice * 100);

                var kartOnayModel = new KartOnayModel
                {
                    CardNumber = model.CreditCard,
                    ExpiryYear = year,
                    ExpiryMonth = model.ExpiryMonth,
                    Cvv = model.CardCode,
                    HolderName = model.HolderName,
                    Amount = ccTlAmount.ToString(),
                    TransactionId = transaction.TransactionId.ToString(),
                    CallbackType = CallbackType.NormalSatinAlma
                };

                var result = VposService.KartOnay(kartOnayModel);
                

                System.IO.File.WriteAllText("\\inetpub\\wwwroot\\html\\temp\\" + transaction.TransactionHtml(), result);

                return Ok("http://www.fintag.net/html/temp/" + transaction.TransactionHtml());
            } catch (Exception e)
            {
                return Ok("Hata:" + e.Message);
            }

        }

        /// <summary>
        /// Kredi Kartı ile altın alma işlemi
        /// </summary>
        /// <param name="model">İşlem için gerekli bilgi modeli</param>
        /// <returns>İşlem için gerekli formun web adresi string olarak</returns>
        /*[HttpPost]
        [Route("buy_cc")]
        public ActionResult<string> BuyGoldWithCC(BuyGoldCCParamModel model)
        {
            // todo validate param model
           // TransactionResultModel result = new TransactionResultModel {Success = false };
            try
            {
                Log.Information("Actual buy gold started amount =" + model.Amount.ToString());
                var user = Repository
                        .GetAllUsers()
                        .Where(usr => usr.UserId.ToString() == model.UserId)
                        .FirstOrDefault();
                if (user == null)
                {
                    //result.Message = "Kullanıcı bulunamadı";
                    return Ok("Kullanıcı Bulunamadı");
                }
                decimal gramAmount;
               
                FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();
                gramAmount = model.Amount / currentRate.BuyRate;
                gramAmount = Math.Truncate(100 * gramAmount) / 100;


                var potentialAmount = gramAmount + user.Balance;
                
                if (potentialAmount * currentRate.BuyRate > 19999 && !user.Verified)
                {
                    return Ok("Hesapı onaylanmamış kullanıcılar en fazla 19,999 TRY değerinde altın alabilir.");
                } 

                var rand = new Random();

                var randString = ":arc:" + rand.Next(0, 1000).ToString();

                var readlComment = "Credit Card Purchase: Gram=" + gramAmount.ToString() + " TRY=" + model.Amount.ToString();
                readlComment += randString;
                var transaction = new Transaction(
                       "TRY"
                       , user.UserId.ToString()
                       , "User"
                       , "Fintag"
                       , "VirtualPos"
                       , model.Amount
                       , readlComment
                       , false
                       , gramAmount
                       , model.Amount);


                transaction.Yekun = GetYekun(user);

                Repository.AddTransaction(transaction);

                Repository.SaveChanges();

                var createdTransaction = Repository
                    .GetAllTransactions()
                    .Where(tr => tr.Source == user.UserId.ToString()
                            && !tr.Confirmed
                            && tr.Comment == readlComment
                            && tr.Destination == "Fintag"
                            && tr.DestinationType == "VirtualPos"
                            && tr.Amount == model.Amount
                            && tr.TransactionDateTime.Date == DateTime.Now.Date)
                    .FirstOrDefault();


                var pay3D = new Pay3DParamModel(model, user, createdTransaction);

                var sipay = SiPay.StartServices();

                var content = sipay.Pay3DSecure(pay3D).Result;

                Log.Information("SIPAY RESULT CONTENT - START");
                Log.Information(content);
                Log.Information("SIPAY RESULT CONTENT - END");

                
                System.IO.File.WriteAllText("\\inetpub\\wwwroot\\html\\temp\\" + createdTransaction.TransactionHtml(), content);
                
                return "http://www.fintag.net/html/temp/" + createdTransaction.TransactionHtml();
         
            }
            catch (Exception e)
            {
                Log.Warning("Exception at Buy gold with cc " + e.Message);
            }
            return Ok("Bir hata oluştu");
        }

        */


        private TransferRequestModel ConvertModelForUser(TransferRequest request, string userId)
        {

            var sourceUser = Repository.GetAllUsers()
                .Where(x => x.UserId == request.SourceUserId)
                .FirstOrDefault();

            if (sourceUser == null)
            {
                throw new Exception("Kullanıcı bulunamadı.");
            }

            var destinationUser = Repository.GetAllUsers()
                .Where(x => x.UserId.ToString() == request.Destination)
                .FirstOrDefault();

            if (destinationUser == null)
            {
                throw new Exception("Kullanıcı bulunamadı.");
            }

            var transaction = Repository.GetAllTransactions()
                .Where(x => x.TransactionId == request.TransactionRecord)
                .FirstOrDefault();

            if (transaction == null)
            {
                throw new Exception("Altın transfer hareketi bulunamadı");
            }

            var source = new UserModel3
            {
                UserId = sourceUser.UserId.ToString(),
                Balance = sourceUser.Balance,
                MemberId = sourceUser.MemberId,
                Name = sourceUser.FirstName + " " + sourceUser.FamilyName
            };

            var des = new UserModel3
            {
                UserId = destinationUser.UserId.ToString(),
                Balance = destinationUser.Balance,
                MemberId = destinationUser.MemberId,
                Name = destinationUser.FirstName + " " + destinationUser.FamilyName
            };

            string photo = "";

            if (userId == sourceUser.UserId.ToString())
            {
                photo = "http://www.fintag.net/images/temp_profile_photos/" + des.MemberId + ".jpg";
            }
            else
            {
                photo = "http://www.fintag.net/images/temp_profile_photos/" + sourceUser.MemberId + ".jpg";
            }

            

            var transferRequestModel = new TransferRequestModel
            {
                TransferRequestId = request.TransferRequestId.ToString(),
                Source = source,
                DestinationUser = des,
                GramsOfGold = request.GramsOfGold,
                Price = transaction.TlAmount,
                RequestDateTime = request.RequestDateTime.ToString(),
                Comments = request.Comments,
                Photo = photo

            };

            return transferRequestModel;
        }

        private TransferRequestModel ConvertModelForEvent(TransferRequest request)
        {

            var sourceUser = Repository.GetAllUsers()
                .Where(x => x.UserId == request.SourceUserId)
                .FirstOrDefault();

            if (sourceUser == null)
            {
                throw new Exception("Kullanıcı bulunamadı.");
            }

            var destEvent = Repository
                    .GetEvent(request.Destination);

            if (destEvent == null)
            {
                throw new Exception("Böyle bir etkinlik bulunamadı");
            }

            var transaction = Repository.GetAllTransactions()
                .Where(x => x.TransactionId == request.TransactionRecord)
                .FirstOrDefault();

            if (transaction == null)
            {
                throw new Exception("Altın transfer hareketi bulunamadı");
            }

            var source = new UserModel3
            {
                UserId = sourceUser.UserId.ToString(),
                Balance = sourceUser.Balance,
                MemberId = sourceUser.MemberId,
                Name = sourceUser.FirstName + " " + sourceUser.FamilyName
            };

            var eventModel = new SpecialEventModel
            {
                EventName = destEvent.EventName,
                EventText = destEvent.EventText,
                Code = destEvent.EventCode.ToString()

            };

            var des = new Models.Events.EventModel
            {
                EventType = "Event",
                EventObject = eventModel
            };

            var photo = "http://www.fintag.net/images/event_photos/" + destEvent.EventCode + ".jpg";

            var transferRequestModel = new TransferRequestModel
            {
                TransferRequestId = request.TransferRequestId.ToString(),
                Source = source,
                DestinationEvent = des,
                GramsOfGold = request.GramsOfGold,
                Price = transaction.TlAmount,
                RequestDateTime = request.RequestDateTime.ToString(),
                Comments = request.Comments,
                Photo = photo

            };
            return transferRequestModel;
        }

        private TransferRequestModel ConvertModelForWedding(TransferRequest request)
        {

            var sourceUser = Repository.GetAllUsers()
                .Where(x => x.UserId == request.SourceUserId)
                .FirstOrDefault();

            if (sourceUser == null)
            {
                throw new Exception("Kullanıcı bulunamadı.");
            }

            var destinationWedding = Repository
                    .GetWedding(request.Destination);

            if (destinationWedding == null)
            {
                throw new Exception("Böyle bir düğün bulunamadı.");
            }

            var transaction = Repository.GetAllTransactions()
                .Where(x => x.TransactionId == request.TransactionRecord)
                .FirstOrDefault();

            if (transaction == null)
            {
                throw new Exception("Altın transfer hareketi bulunamadı.");
            }

            var source = new UserModel3
            {
                UserId = sourceUser.UserId.ToString(),
                Balance = sourceUser.Balance,
                MemberId = sourceUser.MemberId,
                Name = sourceUser.FirstName + " " + sourceUser.FamilyName
            };

            var wedModel = new WeddingModel
            {
                WeddingName = destinationWedding.WeddingName,
                WeddingText = destinationWedding.WeddingText,
                Code = destinationWedding.WeddingCode.ToString()

            };

            var des = new Models.Events.EventModel
            {
                EventType = "Wedding",
                EventObject = wedModel
            };

            var photo = "http://www.fintag.net/images/wedding_photos/" + destinationWedding.WeddingCode + ".jpg";

            var transferRequestModel = new TransferRequestModel
            {
                TransferRequestId = request.TransferRequestId.ToString(),
                Source = source,
                DestinationEvent = des,
                GramsOfGold = request.GramsOfGold,
                Price = transaction.TlAmount,
                RequestDateTime = request.RequestDateTime.ToString(),
                Comments = request.Comments,
                Photo = photo

            };


            return transferRequestModel;
        }

        private TransferRequestModel ConvertToModel(TransferRequest request, string userId)
        {

            if (request.DestinationType == "User")
            {
                return ConvertModelForUser(request, userId);
            }
            else if (request.DestinationType == "Wedding")
            {
                return ConvertModelForWedding(request);
            }
            else if (request.DestinationType == "Event")
            {
                return ConvertModelForEvent(request);
            }
            else 
            {
                throw new Exception("Tanımlanamayan işlem.");
            }
        }


        [HttpGet]
        [Route("get_transfer")]
        public ActionResult<GetTransferRequestResult> GetTransfer(string id, string userId)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            string userid;

            if (!Authenticator.ValidateToken(token, out userid))
            {
                return Unauthorized();
            }
            var requestee = Repository.GetAllUsers().Where(x => x.UserId.ToString() == userid).FirstOrDefault();

            if (requestee == null || requestee.UserId != Guid.Parse(userId))
            {
                return Unauthorized();
            }

            var result = new GetTransferRequestResult { Success = false }; 
            try
            {
                var transfer = Repository.GetAllTransferRequests()
                    .Where(x => x.TransferRequestId.ToString() == id)
                    .FirstOrDefault();

                if (transfer == null)
                {
                    throw new Exception("Transfer bulunamadı");
                }

                var transferRequestModel = ConvertToModel(transfer, userId);

                result.Transfer = transferRequestModel;
                result.Success = true;
                result.Message = "Transfer detayı hazır.";
                

            }
            catch (Exception e)
            {
                Log.Error("Exception at get_transfer:" + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
            }

            return Ok(result);
        }


        /// <summary>
        /// İşlem onaylama, hehangi bir transfer bu method la onaylanır
        /// </summary>
        /// <param name="model">İşlem bilgi modeli</param>
        /// <returns>İşlem Sonucu</returns>
        [HttpPost]
        [Route("silver_transfer_request_response")]
        public ActionResult<TransferRequestResponseResultModel> SilverTransferRequestResponse(TransferRequestResponseParamModel model)
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

            var result = new TransferRequestResponseResultModel { Success = false };
            try
            {
                Log.Information("silver transfer request response started: ");



                if (model == null || model.TransferRequestId == null)
                {
                    throw new Exception("Hatalı işlem.");
                }

                var transferRequest = Repository.GetAllTransferRequests()
                    .Where(tr => tr.TransferRequestId.ToString() == model.TransferRequestId)

                    .FirstOrDefault();

                if (transferRequest == null)
                {
                    throw new Exception("Transfer isteği bulunamadı.");
                }

                var transaction = Repository.GetAllTransactions()
                    .Where(tr => tr.TransactionId == transferRequest.TransactionRecord && tr.TransactionType == "SILVER")
                    .FirstOrDefault();

                if (transaction == null)
                {
                    throw new Exception("Transfer hareketi bulunamadı.");
                }

                if (model.Confirmed)
                {
                    var sourceUser = Repository
                        .GetAllUsers()
                        .Where(usr => usr.UserId == transferRequest.SourceUserId)
                        .FirstOrDefault();

                    if (sourceUser.UserId != requestee.UserId)
                    {
                        return Unauthorized();
                    }
                    var silverBalance = Repository.GetSilverBalance(sourceUser.UserId);
                    if (silverBalance == null)
                    {
                        throw new Exception("Silver balance hatasi.");
                    }

                    if (transferRequest.GramsOfGold > silverBalance.Balance)
                    {
                        throw new Exception("Gümüş bakiyeniz yeterli değil.");
                    }


                    if (transferRequest.DestinationType == "User")
                    {
                        var user = Repository
                            .GetAllUsers()
                            .Where(z => z.UserId.ToString() == transferRequest.Destination)
                            .FirstOrDefault();
                        if (user == null)
                        {
                            throw new Exception("Kullanıcı bulunamadı.");
                        }
                        var destSilverBalance = Repository.GetSilverBalance(user.UserId);
                        if (destSilverBalance == null)
                        {
                            destSilverBalance = new SilverBalance(user.UserId);
                            Repository.AddSilverBalance(destSilverBalance);
                        }

                        destSilverBalance.Balance += transferRequest.GramsOfGold;

                        transaction.YekunDestination += transferRequest.GramsOfGold;
                        var destMessage = string.Format("{0} isimli kullanıcıdan size {1}gr silver gönderildi.",
                            sourceUser.FirstName + " " + sourceUser.FamilyName, transferRequest.GramsOfGold);

                        var destPhoto = "http://www.fintag.net/images/temp_profile_photos/" + sourceUser.MemberId + ".jpg";

                        var srcMessage = string.Format("{0} isimli kullanıcıya, {1}gr silver gönderdiniz.",
                            user.FirstName + " " + user.FamilyName, transferRequest.GramsOfGold);

                        var srcPhoto = "http://www.fintag.net/images/temp_profile_photos/" + user.MemberId + ".jpg";
                        var id = transferRequest.TransferRequestId.ToString();
                        Repository.AddNotification(new Notification2(user.UserId, destMessage, true, "transfer", id, destPhoto));
                        Repository.AddNotification(new Notification2(sourceUser.UserId, srcMessage, true, "transfer", id, srcPhoto));


                    }
                    else
                    {
                        throw new Exception("Hatalı işlem.");
                    }

                    transaction.Yekun -= transferRequest.GramsOfGold;
                    silverBalance.Balance -= transferRequest.GramsOfGold;
                    transferRequest.CompleteTransfer();
                    transaction.Confirm();
                    result.Success = true;
                    result.Message = "İşlem Başarılı";


                }
                else
                {
                    transaction.Cancelled = true;
                    transaction.Comment = "Kullanıcı tarafından iptal edildi.";
                    result.Success = true;
                    result.Message = "İşlem iptal edildi.";
                }

                Repository.SaveChanges();

                Log.Information("silver transfer request response completed");
            }
            catch (Exception err)
            {
                Log.Error("Exception at transfer request response: " + err.Message);
                Log.Error(err.StackTrace);
                result.Success = false;
                result.Message = "Bir hata oluştu: " + err.Message;
            }
            return Ok(result);
        }

        /// <summary>
        /// İşlem onaylama, hehangi bir transfer bu method la onaylanır
        /// </summary>
        /// <param name="model">İşlem bilgi modeli</param>
        /// <returns>İşlem Sonucu</returns>
        [HttpPost]
        [Route("transfer_request_response")]
        public ActionResult<TransferRequestResponseResultModel> TransferRequestResponse(TransferRequestResponseParamModel model)
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

            var result = new TransferRequestResponseResultModel { Success = false };
            try
            {
                Log.Information("transfer request response started: ");



                if (model == null || model.TransferRequestId == null)
                {
                    throw new Exception("Hatalı işlem.");
                }

                var transferRequest = Repository.GetAllTransferRequests()
                    .Where(tr => tr.TransferRequestId.ToString() == model.TransferRequestId)
                    
                    .FirstOrDefault();

                if (transferRequest == null)
                {
                    throw new Exception("Transfer isteği bulunamadı.");
                }

                var transaction = Repository.GetAllTransactions()
                    .Where(tr => tr.TransactionId == transferRequest.TransactionRecord)
                    .FirstOrDefault();

                if (transaction == null)
                {
                    throw new Exception("Transfer hareketi bulunamadı.");
                }

                if (model.Confirmed)
                {
                    var sourceUser = Repository
                        .GetAllUsers()
                        .Where(usr => usr.UserId == transferRequest.SourceUserId)
                        .FirstOrDefault();

                    if (sourceUser.UserId != requestee.UserId)
                    {
                        return Unauthorized();
                    }

                    var availableBalance = sourceUser.Balance;

                    if (sourceUser.BlockedGrams.HasValue)
                    {
                        availableBalance -= sourceUser.BlockedGrams.Value;
                    }

                    if (transferRequest.GramsOfGold > availableBalance)
                    {
                        throw new Exception("Bakiyeniz yeterli değil.");
                    }
                    
                    
                    if (transferRequest.DestinationType == "User")
                    {
                        var user = Repository
                            .GetAllUsers()
                            .Where(z => z.UserId.ToString() == transferRequest.Destination)
                            .FirstOrDefault();
                        if (user == null)
                        {
                            throw new Exception("Kullanıcı bulunamadı.");
                        }
                        
                        
                        user.ManipulateBalance(transferRequest.GramsOfGold);
                        transaction.YekunDestination += transferRequest.GramsOfGold;
                        var destMessage = string.Format("{0} isimli kullanıcıdan size {1}gr altın gönderildi.",
                            sourceUser.FirstName + " " + sourceUser.FamilyName, transferRequest.GramsOfGold);

                        var destPhoto = "http://www.fintag.net/images/temp_profile_photos/" + sourceUser.MemberId + ".jpg";

                        var srcMessage = string.Format("{0} isimli kullanıcıya, {1}gr altın gönderdiniz.",
                            user.FirstName + " " + user.FamilyName, transferRequest.GramsOfGold);

                        var srcPhoto = "http://www.fintag.net/images/temp_profile_photos/" + user.MemberId + ".jpg";
                        var id = transferRequest.TransferRequestId.ToString();
                        Repository.AddNotification(new Notification2(user.UserId, destMessage, true, "transfer", id, destPhoto));
                        Repository.AddNotification(new Notification2(sourceUser.UserId, srcMessage, true, "transfer", id, srcPhoto));


                    }
                    else if (transferRequest.DestinationType == "Wedding")
                    {
                        var wedding = Repository
                            .GetWedding(transferRequest.Destination);
                        wedding.AddGold(transferRequest.GramsOfGold);

                        var weddingOwner = Repository.GetAllUsers()
                            .Where(x => x.UserId == wedding.CreatedBy)
                            .FirstOrDefault();

                        weddingOwner.ManipulateBalance(transferRequest.GramsOfGold);

                        var destMessage = string.Format("{0} isimli düğününüze, {1} tarafindan {2}gr altın gönderildi.",
                            wedding.WeddingName, sourceUser.FirstName + " " + sourceUser.FamilyName, transferRequest.GramsOfGold);

                        var photo = "http://www.fintag.net/images/wedding_photos/" + wedding.WeddingCode + ".jpg";
                        var srcMessage = string.Format("{0} isimli düğüne, {1}gr altın gönderdiniz.",
                            wedding.WeddingName, transferRequest.GramsOfGold);
                        var id = transferRequest.TransferRequestId.ToString();
                        Repository.AddNotification(new Notification2(wedding.CreatedBy, destMessage, true, "transfer", id, photo));
                        Repository.AddNotification(new Notification2(sourceUser.UserId, srcMessage, true, "transfer", id, photo));

                    }
                    else if (transferRequest.DestinationType == "Event")
                    {
                        var evt = Repository.GetEvent(transferRequest.Destination);
                        evt.AddGold(transferRequest.GramsOfGold);
                        var evtOwner = Repository.GetAllUsers().Where(x => x.UserId == evt.CreatedBy).FirstOrDefault();
                        evtOwner.ManipulateBalance(transferRequest.GramsOfGold);
                        var photo = "http://www.fintag.net/images/event_photos/" + evt.EventCode + ".jpg";
                        var destMessage = string.Format("{0} isimli etkinliğinize, {1} tarafindan {2}gr altın gönderildi.",
                            evt.EventName, sourceUser.FirstName + " " + sourceUser.FamilyName, transferRequest.GramsOfGold);


                        var srcMessage = string.Format("{0} isimli etkinliğe, {1}gr altın gönderdiniz.",
                            evt.EventName, transferRequest.GramsOfGold);
                        var id = transferRequest.TransferRequestId.ToString();
                        Repository.AddNotification(new Notification2(evt.CreatedBy, destMessage, true, "info", id, photo));
                        Repository.AddNotification(new Notification2(sourceUser.UserId, srcMessage, true, "info", id, photo));


                    }
                    else
                    {
                        throw new Exception("Hatalı işlem.");
                    }

                    transaction.Yekun -= transferRequest.GramsOfGold;
                    sourceUser.ManipulateBalance(-transferRequest.GramsOfGold);
                    transferRequest.CompleteTransfer();
                    transaction.Confirm();
                    result.Success = true;
                    result.Message = "İşlem Başarılı";


                }
                else
                {
                    transaction.Cancelled = true;
                    transaction.Comment = "Kullanıcı tarafından iptal edildi.";
                    result.Success = true;
                    result.Message = "İşlem iptal edildi.";
                }

                Repository.SaveChanges();

                Log.Information("transfer request response completed");
            }
            catch (Exception err)
            {
                Log.Error("Exception at transfer request response: " + err.Message);
                Log.Error(err.StackTrace);
                result.Success = false;
                result.Message = "Bir hata oluştu: " + err.Message;
            }
            return Ok(result);
        } 

        /// <summary>
        /// Kullanıcıdan kullanıcıya altın transferi için kullanılır
        /// </summary>
        /// <param name="model">İşlem için gerekli bilgi modeli</param>
        /// <returns>İşlem sonucu</returns>
        [HttpPost]
        [Route("user_gold_transfer")]
        public ActionResult<TransferRequestResultModel> TransferGoldFromUserToUser(TransferFromUserToUserParamModel model)
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

            TransferRequestResultModel result = new TransferRequestResultModel
            {
                Success = false
            };
            try
            {
                if (model.Amount == 0)
                {
                    throw new Exception("0 gram gönderilemez.");
                }

                Log.Information("user gold transfer request initiated");
                var sourceUser = Repository
                    .GetAllUsers()
                    .Where(usr => usr.UserId.ToString() == model.UserId)
                    .FirstOrDefault();

                var userLevel = Repository.GetUserLevel(sourceUser.UserId);
                if (userLevel == null)
                {
                    throw new Exception("Hatali kullanici");
                }
                if (userLevel.Value == 0)
                {
                    throw new Exception("Alım satım işlemleri için hesabınızın onaylanması gerekmektedir.");
                }

                //var names = model.MemberName.Split(" ");

                var destName = model.MemberName;

                //var lastname = names[names.Length - 1];

                //var ti = new CultureInfo("tr-TR", false).TextInfo;
                
                //lastname = ti.ToUpper(lastname);
                /*
                var destinationUser = Repository
                    .GetAllUsers()
                    .Where(usr => usr.MemberId == model.MemberId && lastname == usr.FamilyName)
                    .FirstOrDefault();*/


                var destinationUser = Repository
                    .GetAllUsers()
                    .Where(usr => usr.MemberId == model.MemberId)// && Utility.AlmosEqualsName(destName.ToLower(), Utility.MakeOneName(usr.FirstName, usr.FamilyName).ToLower()))
                    .FirstOrDefault();

                if (destinationUser == null || !Utility.AlmosEqualsName(destName.ToLower(), Utility.MakeOneName(destinationUser.FirstName, destinationUser.FamilyName).ToLower()))
                {

                    throw new Exception("Bulmaya çalıştığınız kullanıcının ismi ile üye numarası eşleşmiyor.");
                }

                if (sourceUser == null || destinationUser == null)
                {
                    throw new Exception("Üye bulunamadı.");
                }

                if (sourceUser.MemberId == destinationUser.MemberId)
                {
                    throw new Exception("Kişi kendi hesabına Altın gönderemez.");
                }

                GoldType type = GlobalGoldPrice.ParseType(model.GoldType);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);

                var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetCurrentPrice());

                var transPrice = gram * currentPrice;

                var availableBalance = sourceUser.Balance;
                if (sourceUser.BlockedGrams.HasValue)
                {
                    availableBalance -= sourceUser.BlockedGrams.Value;
                }
            
                if (availableBalance < gram)
                {
                    throw new Exception("Yeterli bakiyeniz bulunmamaktadır.");
                }
                
                var destBalance = destinationUser.Balance;

                var destLevel = Repository.GetUserLevel(destinationUser.UserId);
                
                if (destLevel.Value == 0)
                {
                    throw new Exception(destName + " isimli üyemizim hesabı onaylanmamış durumdadır.");
                }

                var destPotentialGrams = gram + destBalance;

                if (destLevel.Value == 1 && (destPotentialGrams * currentPrice) > 19999)
                {
                    throw new Exception("Altın göndermeye çalıştığınız kullanıcı hesabında daha fazla altın bulunduramaz.");
                }
                if (destLevel.Value == 2 && (destPotentialGrams * currentPrice) > 99999)
                {
                    throw new Exception("Altın göndermeye çalıştığınız kullanıcı hesabında daha fazla altın bulunduramaz.");
                }

                var rand = new Random();

                var randString = sourceUser.FamilyName + " - "+ destinationUser.FamilyName + " - Tr: " + rand.Next(0, 100000).ToString() +":"+DateTime.Now;

                var finalComment = (model.Text != null) ? model.Text : "-";

                var transaction = new Transaction(
                    "GOLD"
                    , sourceUser.UserId.ToString()
                    , "User"
                    , destinationUser.UserId.ToString()
                    , "User"
                    , gram
                    , randString
                    , false
                    , gram
                    , transPrice);
            
                transaction.Yekun = GetYekun(sourceUser);
                transaction.YekunDestination = GetYekun(destinationUser);
 
                Repository.AddTransaction(transaction);
                Repository.SaveChanges();

                transaction = Repository
                    .GetAllTransactions()
                    .Where(tr => tr.TransactionDateTime.Date == DateTime.Now.Date &&
                    tr.Comment == randString)
                    .FirstOrDefault();

                var transferRequest = new TransferRequest(
                    sourceUser.UserId,
                    gram,
                    "User",
                    destinationUser.UserId.ToString(),
                    transaction.TransactionId,
                    randString);

                Repository.AddTransferRequest(transferRequest);
                Repository.SaveChanges();
   
                transferRequest = Repository
                    .GetAllTransferRequests()
                    .Where(x => x.SourceUserId == sourceUser.UserId &&
                    x.RequestCompleted == false &&
                    x.Destination == destinationUser.UserId.ToString() &&
                    x.GramsOfGold == gram &&
                    x.RequestDateTime.Date == DateTime.Now.Date &&
                    x.Comments == randString)
                    .FirstOrDefault();

                


                if (transferRequest == null)
                {
                    throw new Exception("Yaratılan transfer isteği bulunamadı lütfen tekrar deneyiniz.");
                }


                transaction.Comment = finalComment;
                transferRequest.Comments = finalComment;
                Repository.SaveChanges();
                var source = new UserModel3
                {
                    UserId = sourceUser.UserId.ToString(),
                    Balance = sourceUser.Balance,
                    MemberId = sourceUser.MemberId,
                    Name = sourceUser.FirstName + " " + sourceUser.FamilyName
                };
  
                var des = new UserModel3
                {
                    UserId = destinationUser.UserId.ToString(),
                    Balance = destinationUser.Balance,
                    MemberId = destinationUser.MemberId,
                    Name = destinationUser.FirstName + " " + destinationUser.FamilyName
                };

                var photo = "http://www.fintag.net/images/temp_profile_photos/" + des.MemberId + ".jpg";

                var transferRequestModel = new TransferRequestModel
                {
                    TransferRequestId = transferRequest.TransferRequestId.ToString(),
                    Source = source,
                    DestinationUser = des,
                    GramsOfGold = gram,
                    Price = transPrice,
                    RequestDateTime = transferRequest.RequestDateTime.ToString(),
                    Comments = finalComment,
                    Photo = photo

                };

             
                result.Success = true;
                result.Transfer = transferRequestModel;
                result.Message = "Transfer isteğiniz hazır.";
                Log.Information("user gold transfer request complete");
            }
            catch (Exception e)
            {
                Log.Error("Exception at user gold transfer: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
                result.Success = false;
            }



            return Ok(result);
        }

        /// <summary>
        /// Kullanıcıdan kullanıcıya altın transferi için kullanılır
        /// </summary>
        /// <param name="model">İşlem için gerekli bilgi modeli</param>
        /// <returns>İşlem sonucu</returns>
        [HttpPost]
        [Route("user_silver_transfer")]
        public ActionResult<TransferRequestResultModel> TransferSilverFromUserToUser(TransferFromUserToUserParamModel model)
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

            TransferRequestResultModel result = new TransferRequestResultModel
            {
                Success = false
            };
            try
            {
                if (model.Amount == 0)
                {
                    throw new Exception("0 gram gönderilemez.");
                }

                Log.Information("user silver transfer request initiated");
                var sourceUser = Repository
                    .GetAllUsers()
                    .Where(usr => usr.UserId.ToString() == model.UserId)
                    .FirstOrDefault();
                var userLevel = Repository.GetUserLevel(sourceUser.UserId);
                if (userLevel == null)
                {
                    throw new Exception("Hatali kullanici");
                }
                if (userLevel.Value == 0)
                {
                    throw new Exception("Alım satım işlemleri için hesabınızın onaylanması gerekmektedir.");
                }


                //var names = model.MemberName.Split(" ");

                //var lastname = names[names.Length - 1];

                //var ti = new CultureInfo("tr-TR", false).TextInfo;
                //lastname = ti.ToUpper(lastname);
                /*
                var destinationUser = Repository
                    .GetAllUsers()
                    .Where(usr => usr.MemberId == model.MemberId && lastname == usr.FamilyName)
                    .FirstOrDefault();*/

                var destName = model.MemberName;

                var destinationUser = Repository
                    .GetAllUsers()
                    .Where(usr => usr.MemberId == model.MemberId)
                    .FirstOrDefault();

                if (destinationUser == null || !Utility.AlmosEqualsName(destName.ToLower(), Utility.MakeOneName(destinationUser.FirstName, destinationUser.FamilyName).ToLower()))
                {
                    throw new Exception("Bulmaya çalıştığınız kullanıcının ismi ile üye numarası eşleşmiyor.");
                }


                if (sourceUser == null || destinationUser == null)
                {
                    throw new Exception("Üye bulunamadı.");
                }

                if (sourceUser.MemberId == destinationUser.MemberId)
                {
                    throw new Exception("Kişi kendi hesabına Altın gönderemez.");
                }

                GoldType type = GlobalGoldPrice.ParseType(model.GoldType);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);

                var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetSilverPrices());

                var transPrice = gram * currentPrice;

                var silverBalance = Repository.GetSilverBalance(sourceUser.UserId);

                if (silverBalance == null)
                {
                    throw new Exception("Henüz gümüş almamış durumdasınız.");
                }
                var destBalance = Repository.GetSilverBalance(destinationUser.UserId);
                if (destBalance == null)
                {
                    destBalance = new SilverBalance(destinationUser.UserId);
                    Repository.AddSilverBalance(destBalance);
                }

                var destLevel = Repository.GetUserLevel(destinationUser.UserId);

                if (destLevel.Value == 0)
                {
                    throw new Exception(destName + " isimli üyemizim hesabı onaylanmamış durumdadır.");
                }

                var destPotentialGrams = gram + destBalance.Balance;

                if (destLevel.Value == 1 && (destPotentialGrams * currentPrice) > 19999)
                {
                    throw new Exception("Gümüş göndermeye çalıştığınız kullanıcı hesabında daha fazla gümüş bulunduramaz.");
                }
                if (destLevel.Value == 2 && (destPotentialGrams * currentPrice) > 99999)
                {
                    throw new Exception("Gümüş göndermeye çalıştığınız kullanıcı hesabında daha fazla gümüş bulunduramaz.");
                }


                var availableBalance = silverBalance.Balance;
                if (silverBalance.BlockedGrams.HasValue)
                {
                    availableBalance -= silverBalance.BlockedGrams.Value;
                }

                if (availableBalance < gram)
                {
                    throw new Exception("Yeterli gümüş bakiyeniz bulunmamaktadır.");
                }

                var rand = new Random();

                var randString = sourceUser.FamilyName + " - " + destinationUser.FamilyName + " - SILVER: " + rand.Next(0, 100000).ToString() + ":" + DateTime.Now;


                var finalComment = (model.Text != null) ? model.Text : "-";

                var transaction = new Transaction(
                    "SILVER"
                    , sourceUser.UserId.ToString()
                    , "User"
                    , destinationUser.UserId.ToString()
                    , "User"
                    , gram
                    , randString
                    , false
                    , gram
                    , transPrice);

                transaction.Yekun = GetYekun(sourceUser);
                transaction.YekunDestination = GetYekun(destinationUser);

                Repository.AddTransaction(transaction);
                Repository.SaveChanges();

                transaction = Repository
                    .GetAllTransactions()
                    .Where(tr => tr.TransactionDateTime.Date == DateTime.Now.Date &&
                    tr.Comment == randString)
                    .FirstOrDefault();

                var transferRequest = new TransferRequest(
                    sourceUser.UserId,
                    gram,
                    "User",
                    destinationUser.UserId.ToString(),
                    transaction.TransactionId,
                    randString);

                Repository.AddTransferRequest(transferRequest);
                Repository.SaveChanges();

                transferRequest = Repository
                    .GetAllTransferRequests()
                    .Where(x => x.SourceUserId == sourceUser.UserId &&
                    x.RequestCompleted == false &&
                    x.Destination == destinationUser.UserId.ToString() &&
                    x.GramsOfGold == gram &&
                    x.RequestDateTime.Date == DateTime.Now.Date &&
                    x.Comments == randString)
                    .FirstOrDefault();




                if (transferRequest == null)
                {
                    throw new Exception("Yaratılan transfer isteği bulunamadı lütfen tekrar deneyiniz.");
                }


                transaction.Comment = finalComment;
                transferRequest.Comments = finalComment;
                Repository.SaveChanges();
                var source = new UserModel3
                {
                    UserId = sourceUser.UserId.ToString(),
                    Balance = silverBalance.Balance,
                    MemberId = sourceUser.MemberId,
                    Name = sourceUser.FirstName + " " + sourceUser.FamilyName
                };

                var des = new UserModel3
                {
                    UserId = destinationUser.UserId.ToString(),
                    Balance = destBalance.Balance,
                    MemberId = destinationUser.MemberId,
                    Name = destinationUser.FirstName + " " + destinationUser.FamilyName
                };

                var photo = "http://www.fintag.net/images/temp_profile_photos/" + des.MemberId + ".jpg";

                var transferRequestModel = new TransferRequestModel
                {
                    TransferRequestId = transferRequest.TransferRequestId.ToString(),
                    Source = source,
                    DestinationUser = des,
                    GramsOfGold = gram,
                    Price = transPrice,
                    RequestDateTime = transferRequest.RequestDateTime.ToString(),
                    Comments = finalComment,
                    Photo = photo

                };


                result.Success = true;
                result.Transfer = transferRequestModel;
                result.Message = "Silver transfer isteğiniz hazır.";
                Log.Information("user silver transfer request complete");
            }
            catch (Exception e)
            {
                Log.Error("Exception at user silver transfer: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
                result.Success = false;
            }



            return Ok(result);
        }

        #region wedding_gold_transfer
        /// <summary>
        /// Kullanıcıdan düğüne altın transfer işlemi
        /// </summary>
        /// <param name="model">işlem için gerekli bilgi modeli</param>
        /// <returns>İşlem sonucu</returns>
        [HttpPost]
        [Route("wedding_gold_transfer")]
        public ActionResult<TransferRequestResultModel> TransferGoldFromUserToWedding(TransferFromUserToWeddingParamModel model)
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

            TransferRequestResultModel result = new TransferRequestResultModel { Success = false };
            try
            {
                Log.Information("TransferGoldFromUserToWedding() initiated.");
                var sourceUser = Repository
                    .GetAllUsers()
                    .Where(usr => usr.UserId.ToString() == model.UserId)
                    .FirstOrDefault();

                if (sourceUser == null)
                {
                    throw new Exception("Kullanıcı bulunamadı.");
                }
                var userLevel = Repository.GetUserLevel(sourceUser.UserId);
                if (userLevel == null)
                {
                    throw new Exception("Hatali kullanici");
                }
                if (userLevel.Value == 0)
                {
                    throw new Exception("Alım satım işlemleri için hesabınızın onaylanması gerekmektedir.");
                }


                var destinationWedding = Repository
                    .GetWedding(model.WeddingId);

                if (destinationWedding == null)
                {
                    throw new Exception("Böyle bir düğün bulunamadı.");
                }

                if (sourceUser.UserId == destinationWedding.CreatedBy)
                {
                    throw new Exception("Kendi düğününüze altın gönderemezsiniz.");
                }

                GoldType type = GlobalGoldPrice.ParseType(model.GoldType);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
                decimal currentprice = (decimal)GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetCurrentPrice());
                decimal transPrice = gram * currentprice;

                var availableBalance = sourceUser.Balance;
                if (sourceUser.BlockedGrams.HasValue)
                {
                    availableBalance -= sourceUser.BlockedGrams.Value;
                }

                if (gram > availableBalance)
                {
                    throw new Exception("Yeterli bakiyeniz bulunmamaktadır.");
                }

                var rand = new Random();

                var randString = sourceUser.FamilyName + " - " + destinationWedding.WeddingName + " tr :" + rand.Next(0, 100000).ToString()+":"+DateTime.Now;

                string comment;
                
                if (model.TransferMessage == null || model.TransferMessage == "")
                {
                    comment = randString;
                }
                else
                {
                    comment = model.TransferMessage;
                }

                var transaction = new Transaction(
                   "GOLD"
                   , sourceUser.UserId.ToString()
                   , "User"
                   , destinationWedding.WeddingId.ToString()
                   , "Wedding"
                   , gram
                   , randString
                   , false
                   , gram
                   , transPrice);
                transaction.Yekun = GetYekun(sourceUser);

                Repository.AddTransaction(transaction);
                Repository.SaveChanges();

                transaction = Repository
                    .GetAllTransactions()
                    .Where(tr => tr.TransactionDateTime.Date == DateTime.Now.Date &&
                    tr.Comment == randString)
                    .FirstOrDefault();
                

                var transferRequest = new TransferRequest(
                    sourceUser.UserId,
                    gram,
                    "Wedding",
                    destinationWedding.WeddingId.ToString(),
                    transaction.TransactionId,
                    randString);

                transaction.Comment = (model.TransferMessage != null && model.TransferMessage != "") ? model.TransferMessage : "-";

                Repository.AddTransferRequest(transferRequest);
                Repository.SaveChanges();


                transferRequest = Repository
                    .GetAllTransferRequests()
                    .Where(x => x.SourceUserId == sourceUser.UserId &&
                    x.RequestCompleted == false &&
                    x.Destination == destinationWedding.WeddingId.ToString() &&
                    x.GramsOfGold == gram &&
                    x.RequestDateTime.Date == DateTime.Now.Date &&
                    x.Comments == randString)
                    .FirstOrDefault();

                if (transferRequest == null)
                {
                    throw new Exception("Yaratılan transfer isteği bulunamadı lütfen tekrar deneyiniz");
                }

                transferRequest.Comments = transaction.Comment;
                Repository.SaveChanges();
                

                var source = new UserModel3
                {
                    UserId = sourceUser.UserId.ToString(),
                    Balance = sourceUser.Balance,
                    MemberId = sourceUser.MemberId,
                    Name = sourceUser.FirstName + " " + sourceUser.FamilyName
                };

                var wedModel = new WeddingModel
                {
                    WeddingName = destinationWedding.WeddingName,
                    WeddingText = destinationWedding.WeddingText,
                    Code = destinationWedding.WeddingCode.ToString()
                    
                };

                var des = new Models.Events.EventModel 
                {
                    EventType = "Wedding",
                    EventObject = wedModel
                };

                var photo = "http://www.fintag.net/images/wedding_photos/" + destinationWedding.WeddingCode + ".jpg";

                var transferRequestModel = new TransferRequestModel
                {
                    TransferRequestId = transferRequest.TransferRequestId.ToString(),
                    Source = source,
                    DestinationEvent = des,
                    GramsOfGold = gram,
                    Price = transPrice,
                    RequestDateTime = transferRequest.RequestDateTime.ToString(),
                    Comments = transaction.Comment,
                    Photo = photo

                };

                result.Success = true;
                result.Transfer = transferRequestModel;
                result.Message = "Transfer isteğiniz hazır.";
                Log.Information("user to wedding gold transfer request complete");
            }
            catch (Exception e)
            {
                Log.Error("Exception at transfer to wedding request: " + e.Message);
                result.Message = "Bir hata oluştu: " + e.Message;
                result.Success = false;
            }

            return Ok(result);
        }

        /// <summary>
        /// Kullanıcıdan özel etkinliğe transfer işlemi
        /// </summary>
        /// <param name="model">işlem için gerekli bilgi modeli</param>
        /// <returns>işlem sonucu</returns>
        [HttpPost]
        [Route("event_gold_transfer")]
        public ActionResult<TransferRequestResultModel> TransferGoldFromUserToEvent(TransferFromUserToEventParamModel model)
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
            TransferRequestResultModel result = new TransferRequestResultModel { Success = false };
            try
            {
                Log.Information("TransferGoldFromUserToEvent() initiated.");
                var sourceUser = Repository
                    .GetAllUsers()
                    .Where(usr => usr.UserId.ToString() == model.UserId)
                    .FirstOrDefault();

                if (sourceUser == null)
                {
                    throw new Exception("Kullanıcı bulunamadı.");
                }
                var userLevel = Repository.GetUserLevel(sourceUser.UserId);
                if (userLevel == null)
                {
                    throw new Exception("Hatali kullanici");
                }
                if (userLevel.Value == 0)
                {
                    throw new Exception("Alım satım işlemleri için hesabınızın onaylanması gerekmektedir.");
                }


                var destEvent = Repository
                    .GetEvent(model.EventId);

                if (destEvent == null)
                {
                    throw new Exception("Böyle bir etkinlik bulunamadı.");
                }

                if (sourceUser.UserId == destEvent.CreatedBy)
                {
                    throw new Exception("Kendi etkinliğinize altın gönderemezsiniz.");
                }

                GoldType type = GlobalGoldPrice.ParseType(model.GoldType);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
                decimal currentprice = (decimal)GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetCurrentPrice());
                decimal transPrice = gram * currentprice;
                var availableBalance = sourceUser.Balance;
                if (sourceUser.BlockedGrams.HasValue)
                {
                    availableBalance -= sourceUser.BlockedGrams.Value;
                }

                if (gram > availableBalance)
                {
                    throw new Exception("Yeterli bakiyeniz bulunmamaktadır.");
                }

                var rand = new Random();

                var randString = sourceUser.FamilyName + " - " + destEvent.EventName + " tr :" + rand.Next(0, 100000).ToString()+":"+DateTime.Now;

                string comment;

                if (model.TransferMessage == null || model.TransferMessage == "")
                {
                    comment = randString;
                }
                else
                {
                    comment = model.TransferMessage;
                }

                var transaction = new Transaction(
                   "GOLD"
                   , sourceUser.UserId.ToString()
                   , "User"
                   , destEvent.EventId.ToString()
                   , "Event"
                   , gram
                   , randString
                   , false
                   , gram
                   , transPrice);
                transaction.Yekun = GetYekun(sourceUser);
                Repository.AddTransaction(transaction);
                Repository.SaveChanges();

                transaction = Repository
                    .GetAllTransactions()
                    .Where(tr => tr.TransactionDateTime.Date == DateTime.Now.Date &&
                    tr.Comment == randString)
                    .FirstOrDefault();

                var transferRequest = new TransferRequest(
                    sourceUser.UserId,
                    gram,
                    "Event",
                    destEvent.EventId.ToString(),
                    transaction.TransactionId,
                    randString);

                transaction.Comment = (model.TransferMessage != null && model.TransferMessage != "") ? model.TransferMessage : "-";

                Repository.AddTransferRequest(transferRequest);
                Repository.SaveChanges();


                transferRequest = Repository
                    .GetAllTransferRequests()
                    .Where(x => x.SourceUserId == sourceUser.UserId &&
                    x.RequestCompleted == false &&
                    x.Destination == destEvent.EventId.ToString() &&
                    x.GramsOfGold == gram &&
                    x.RequestDateTime.Date == DateTime.Now.Date &&
                    x.Comments == randString)
                    .FirstOrDefault();

                if (transferRequest == null)
                {
                    throw new Exception("Yaratılan transfer isteği bulunamadı lütfen tekrar deneyiniz.");
                }

                transferRequest.Comments = transaction.Comment;
                Repository.SaveChanges();

                var source = new UserModel3
                {
                    UserId = sourceUser.UserId.ToString(),
                    Balance = sourceUser.Balance,
                    MemberId = sourceUser.MemberId,
                    Name = sourceUser.FirstName + " " + sourceUser.FamilyName
                };

                var eventModel = new SpecialEventModel
                {
                    EventName = destEvent.EventName,
                    EventText = destEvent.EventText,
                    Code = destEvent.EventCode.ToString()

                };

                var des = new Models.Events.EventModel
                {
                    EventType = "Event",
                    EventObject = eventModel
                };

                var photo = "http://www.fintag.net/images/event_photos/" + destEvent.EventCode + ".jpg";

                var transferRequestModel = new TransferRequestModel
                {
                    TransferRequestId = transferRequest.TransferRequestId.ToString(),
                    Source = source,
                    DestinationEvent = des,
                    GramsOfGold = gram,
                    Price = transPrice,
                    RequestDateTime = transferRequest.RequestDateTime.ToString(),
                    Comments = transaction.Comment,
                    Photo = photo

                };

                result.Success = true;
                result.Transfer = transferRequestModel;
                result.Message = "Transfer isteğiniz hazır.";
                Log.Information("user to event gold transfer request complete");
            }
            catch (Exception e)
            {
                Log.Error("Exception at transfer to wedding request: " + e.Message);
                Log.Error(e.StackTrace);
                result.Message = "Bir hata oluştu: " + e.Message;
                result.Success = false;
            }

            return Ok(result);
        }

        #endregion



        [HttpPost]
        [Route("get_current_price_silver")]
        public ActionResult<PriceCheckResultModel> GetCurrentPriceCheckSilver(PriceCheckParamModel model)
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

            PriceCheckResultModel result = new PriceCheckResultModel { Success = false, };
            try
            {
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();

                GoldType type = GlobalGoldPrice.ParseType(model.Type);

                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
                FxRate currentRate = GlobalGoldPrice.GetSilverPrices();

                decimal price = currentRate.BuyRate * gram;
                price = Math.Truncate(100 * price) / 100;

                decimal salePrice = currentRate.SellRate * gram;
                salePrice = Math.Truncate(100 * salePrice) / 100;

                decimal alis = (currentRate.BuyRate * taxAndExpenses.BankaMukaveleTax) + taxAndExpenses.Kar;
                alis = Math.Truncate(100 * alis) / 100;

                decimal satis = (currentRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax)) - taxAndExpenses.SatisKomisyon;
                satis = Math.Truncate(100 * satis) / 100;

                var vposPrice = price * taxAndExpenses.BankaMukaveleTax * taxAndExpenses.VposCommission + taxAndExpenses.Kar;
                vposPrice = Math.Truncate(100 * vposPrice) / 100;

                var eftPrice = price * taxAndExpenses.BankaMukaveleTax + taxAndExpenses.Kar;
                eftPrice = Math.Truncate(100 * eftPrice) / 100;
                result.Success = true;
                result.Price = price; // baz fiyat = buy percet * gram * current rate
                result.SalePrice = salePrice; // baz sale price = sell percent * gram * current rate
                result.Alis = alis; // 1 gram icin baz fiyat * vergi + kar
                result.Satis = satis; // 1 gram icin sell
                result.Kar = taxAndExpenses.Kar; // genelde zaten 0
                result.Vergi = taxAndExpenses.BankaMukaveleTax; // 1.002
                result.Commission = taxAndExpenses.VposCommission; // 1.035
                result.SaleCommission = taxAndExpenses.SatisKomisyon; // 2tl
                result.VposPrice = vposPrice; // price *vergi * vposcomims + kar
                result.EftPrice = eftPrice; // price *vergi
            }
            catch (Exception e)
            {
                Log.Warning("Exception at price check silver" + e.Message);
            }
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get_current_price")]
        public ActionResult<PriceCheckResultModel> GetCurrentPriceCheck(PriceCheckParamModel model)
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

            PriceCheckResultModel result = new PriceCheckResultModel { Success = false, }; 
            try
            {
                var taxAndExpenses = GlobalGoldPrice.GetTaxAndExpenses();
                Log.Information("price check initiated");
                GoldType type = GlobalGoldPrice.ParseType(model.Type);

                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
                FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();

                decimal price = currentRate.BuyRate * gram;
                price = Math.Truncate(100 * price) / 100;

                decimal salePrice = currentRate.SellRate * gram;
                salePrice = Math.Truncate(100 * salePrice) / 100;

                decimal alis = (currentRate.BuyRate * taxAndExpenses.BankaMukaveleTax) + taxAndExpenses.Kar;
                alis = Math.Truncate(100 * alis) / 100;

                decimal satis = (currentRate.SellRate * (2 - taxAndExpenses.BankaMukaveleTax)) - taxAndExpenses.SatisKomisyon;
                satis = Math.Truncate(100 * satis) / 100;

                var vposPrice = price * taxAndExpenses.BankaMukaveleTax * taxAndExpenses.VposCommission + taxAndExpenses.Kar;
                vposPrice = Math.Truncate(100 * vposPrice) / 100;

                var eftPrice = price * taxAndExpenses.BankaMukaveleTax + taxAndExpenses.Kar;
                eftPrice = Math.Truncate(100 * eftPrice) / 100;

                var ktSilBuy = Repository.AccessSilverBuy().Amount.Value;
                var ktSilverkSell = Repository.AccessSilverSell().Amount.Value;

                var ktbuy = Repository.AccessKtBuy().Amount.Value;
                var ktsell = Repository.AccessKtSell().Amount.Value;
                var bpecenage = Repository.AccessBuyPercentage().Percentage.Value;
                var specentage = Repository.AccessSellPercentage().Percentage.Value;
                var automatic = Repository.AccessAutomatic().Amount.Value;

                result.Success = true;
                result.Price = price; // baz fiyat = buy percet * gram * current rate
                result.SalePrice = salePrice; // baz sale price = sell percent * gram * current rate
                result.Alis = alis; // 1 gram icin baz fiyat * vergi + kar
                result.Satis = satis; // 1 gram icin sell
                result.Kar = taxAndExpenses.Kar; // genelde zaten 0
                result.Vergi = taxAndExpenses.BankaMukaveleTax; // 1.002
                result.Commission = taxAndExpenses.VposCommission; // 1.035
                result.SaleCommission = taxAndExpenses.SatisKomisyon; // 2tl
                result.VposPrice = vposPrice; // price *vergi * vposcomims + kar
                result.EftPrice = eftPrice; // price *vergi
                result.KtBuy = ktbuy;
                result.KtSell = ktsell;
                result.BuyPercentage = bpecenage;
                result.SellPercentage = specentage;
                result.Automatic = automatic;
                result.SilverBuy = ktSilBuy;
                result.SilverSell = ktSilverkSell;
                result.ExpectedRun = Repository.GetRobotStatus().Amount.Value;
                result.BuyPercentageSilver = Repository.AccessBuyPercentageSilver().Percentage.Value;
                result.SellPercentageSilver = Repository.AccessSellPercentageSilver().Percentage.Value;
            }
            catch (Exception e)
            {
                Log.Warning("Exception at price check " + e.Message);
            }
            return Ok(result);
        }


        // custom gcode protocol
        // get gcode 

        /*
        [HttpGet]
        [Route("set_vendor_secret")]
        public ActionResult<SetSecretResultModel> SetVendorSecret(string apikey, string gcode, string secret)
        {
            var result = new SetSecretResultModel { Success = false };
            if (!VendorGiftService.ValidSecret(secret))
            {
                result.Message = "Secret must be min 16 characters and it must have at least 1 lower case letter, 1 upper case letter, 1 digit and 1 punctiation mark";
                return Ok(result);
            }



            return Ok(result);
        }*/

        // request_gift apikey + secret + amount + transactionid 

        // generate_gift_cards


        /*
         
         

         // login etc
         1) [Anonymous] vendor_login 
                <input username + password(sha256) 
                outputs> status (True/False) + message (status message) + VendorModel(balance + id + AuthToken) 

         2) [Anonymous] vendor_change_password
               <input vendor_id + new_password + old_password + g_code (special code provided by goldtag)
               outputs> status + message
         

         // request info
         3) [Auth] vendor_request_price 
            outputs> status + message +  alis + satis
          
         // request buy sell
         4) [Auth] vendor_buy_gold 
                <input gram_amount + reference_id 
                outputs> status + message gram_amount + ?(nullable)tlamount  + (nullable)?transfer_request_id

         5) [Auth] vendor_sell_gold 
                <input gram_amount + reference_id 
                outputs> status + message + gram_amount + (nullable)?tlamount + (nullable)?transfer_request_id
          
         // complete requests
         6) [Auth] vendor_complete_request 
                <input transfer_reqest_id + confirmed(True/False) + message (vendor message)
                outputs> status + message + (nullable)?transaction_id

         // request data
         7) [Auth] vender_request_transactions
                <input date_from + date_to(date given also included) 
                outputs> status + message + paged_list of TransactionModels (can be empty list)

         8) [Auth] vendor_transaction_details
                <input transaction_id
                outputs> status + message + (nullable)?TransactionModel



         */
    }
}
