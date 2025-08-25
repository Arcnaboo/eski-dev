using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Models.Transactions;
using Gold.Api.Utilities;
using Gold.Core.Transactions;
using Gold.Domain.Transactions.Interfaces;
using Gold.Domain.Transactions.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Gold.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class FakeController : ControllerBase
    {
        private readonly ITransactionsRepository Repository;


        public FakeController()
        {
            Repository = new TransactionsRepository();
        }

        /// <summary>
        /// Usage 
        /// HttpPost Request
        /// content app json
        /// returns app json
        /// 
        /// request body reuquires type: gram, tam..
        /// request body requires amount: gr amount or altin amount
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /*[HttpPost]
        [Route("get_current_price")]
        public ActionResult<PriceCheckResultModel> GetCurrentPriceCheck(PriceCheckParamModel model)
        {
            PriceCheckResultModel result = null;
            try
            {
                Log.Information("price check initiated");
                GoldType type = GlobalGoldPrice.ParseType(model.Type);
                
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
                FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();

                decimal price = currentRate.BuyRate * gram;
                price = Math.Truncate(100 * price) / 100;
                Log.Information("Price calculated for " + gram + "gr " + price + "TL");


                decimal alis = currentRate.BuyRate * gram;
                alis = Math.Truncate(100 * alis) / 100;

                decimal satis = currentRate.SellRate * gram;
                satis = Math.Truncate(100 * satis) / 100;


                result = new PriceCheckResultModel { Success = true, Price = price, Alis = alis, Satis = satis };
            }
            catch (Exception e) {
                Log.Warning("Exception at price check " + e.Message);
            }
            return Ok(result);
        }*/

        /// <summary>
        /// depreciated maybe
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /*[HttpPost]
        [Route("buy_cc")]
        public ActionResult<TransactionResultModel> BuyGoldWithCC(BuyGoldCCParamModel model)
        {
            // todo validate param model
            TransactionResultModel result = null;
            try
            {
                Log.Information("Fake buy gold started");
                Guid userId;
                if (Guid.TryParse(model.UserId, out userId))
                {

                    var user = Repository
                        .GetAllUsers()
                        .Where(usr => usr.UserId == userId)
                        .FirstOrDefault();
                    if (user == null)
                    {
                        return BadRequest();
                    }

                    // user exist so calculate how much gold he can buy
                    
                    FxRate currentRate = GlobalGoldPrice.GetCurrentPrice();

                    var gramAmount = model.Amount / currentRate.BuyRate;
                    gramAmount = Math.Truncate(100 * gramAmount) / 100;

                    // ideally  implement actual virtual pos here

                    // assume after this line virtual pos run and money taken from credit card and came to Fintag account

                    var transaction = new Transaction(
                   "TRY"
                   , user.UserId.ToString()
                   , "User"
                   , "Fintag"
                   , "VirtualPos"
                   , model.Amount
                   , "Credit Card Purchase: Gram=" + gramAmount.ToString() + " TRY=" + model.Amount.ToString()
                   , true,
                   gramAmount,
                   model.Amount);

                    user.ManipulateBalance(gramAmount);

                    Repository.AddTransaction(transaction);

                    Repository.SaveChanges();

                    Log.Information("Fake buy gold success");
                    var source = new UserModel
                    {
                        UserId = user.UserId.ToString(),
                        Balance = user.Balance,
                        MemberId = user.MemberId,
                        Name = user.FirstName + " " + user.FamilyName
                    };

                    var transModel = new TransactionModel
                    {
                        Amount = transaction.Amount,
                        DateTime = transaction.TransactionDateTime.ToString(),
                        Source = source,
                        Destination = "Credit Card Transfer "


                    };

                    result = new TransactionResultModel 
                    { 
                        Success = true, 
                        Transaction = transModel,
                        Message = "İşlem Başarılı" };
                     // todo create proper json model
                }
            }
            catch (Exception e)
            {
                Log.Warning("Exception at Buy gold with cc " + e.Message);
            }
            return Ok(result);
        }
        
        */
        /*
        [HttpPost]
        [Route("wedding_gold_transfer")]
        public ActionResult<TransactionResultModel> TransferGoldFromUserToWedding(TransferFromUserToWeddingParamModel model)
        {
            TransactionResultModel result = new TransactionResultModel { Success = false };
            try
            {
                Log.Information("TransferGoldFromUserToWedding() initiated.");
                var sourceUser = Repository
                    .GetAllUsers()
                    .Where(usr => usr.UserId == Guid.Parse(model.UserId))
                    .FirstOrDefault();

                var destinationWedding = Repository
                    .GetWedding(model.WeddingId);


                GoldType type = GlobalGoldPrice.ParseType(model.GoldType);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);
                decimal currentprice = (decimal) GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetCurrentPrice());
                decimal transPrice = gram * currentprice;
                if (gram > sourceUser.Balance)
                {
                    result.Message = "Yeterli miltarda Altın bulunmamaktadır.";
                    return Ok(result);
                }

                sourceUser.ManipulateBalance(-gram);
                destinationWedding.AddGold(gram);

                var transaction = new Transaction(
                   "GOLD"
                   , sourceUser.UserId.ToString()
                   , "User"
                   , destinationWedding.WeddingId.ToString()
                   , "Wedding"
                   , gram
                   , "Internal Transfer"
                   , true
                   , gram
                   , transPrice);

                Repository.AddTransaction(transaction);


                var destMessage = "Your wedding received " + gram + "gr of Gold from " + sourceUser.FirstName +
                    " " + sourceUser.FamilyName;

                var srcMessage = "You sent " + gram + "gr of Gold to " + destinationWedding.WeddingName;

                Repository.AddNotification(new Notification(destinationWedding.User.UserId, destMessage, false));
                Repository.AddNotification(new Notification(sourceUser.UserId, srcMessage, false));

                Repository.SaveChanges();

                var source = new UserModel
                {
                    UserId = sourceUser.UserId.ToString(),
                    Balance = sourceUser.Balance,
                    MemberId = sourceUser.MemberId,
                    Name = sourceUser.FirstName + " " + sourceUser.FamilyName
                };

                var transModel = new TransactionModel
                {
                    Amount = transaction.Amount,
                    DateTime = transaction.TransactionDateTime.ToString(),
                    Source = source,
                    Destination = "Wedding: " + destinationWedding.WeddingName


                };

                result.Transaction = transModel;
                result.Success = true;
                result.Message = "Transfer başarılı.";
                Log.Information("user gold transfer to wedding complete");
            } 
            catch (Exception e)
            {
                Log.Error("Exception at transfer to wedding: " + e.Message);
                result.Message = "Bir hata oluştu lütfen daha sonra tekrar deneyiniz.";
            }

            return Ok(result);
        }
        */

            /*
        [HttpPost]
        [Route("claim_wedding_gold")]
        public ActionResult<TransactionResultModel> UserClaimWeddingGold(ClaimWeddingGoldParamModel model)
        {
            var result = new TransactionResultModel { Success = false };
            try
            {
                Log.Information("UserClaimGold() started");
                var user = Repository.GetAllUsers().Where(x => x.UserId == Guid.Parse(model.UserId)).FirstOrDefault();
                var wedding = Repository.GetWedding(Guid.Parse(model.WeddingId));

                if (user == null || wedding == null)
                {
                    result.Message = "Invalid request";
                    return Ok(result);
                }

                if (!wedding.ClaimGold(out string error))
                {
                    result.Message = error;
                    return Ok(result);
                }

                var currentPrice = GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetCurrentPrice());

                var transPrice = wedding.BalanceInGold * ((decimal)currentPrice);


                var transaction = new Transaction(
                    "GOLD"
                    , wedding.WeddingId.ToString()
                    , "Wedding"
                    , user.UserId.ToString()
                    , "User"
                    , wedding.BalanceInGold
                    , "Internal Transfer"
                    , true
                    , wedding.BalanceInGold
                    , transPrice);

                Repository.AddTransaction(transaction);
                var msg = "You have received " + wedding.BalanceInGold + "gr of Gold from Wedding: " + wedding.WeddingName;
                Repository.AddNotification(new Notification(user.UserId, msg, false));
                Repository.SaveChanges();
                var source = new UserModel
                {
                    UserId = user.UserId.ToString(),
                    Balance = user.Balance,
                    MemberId = user.MemberId,
                    Name = user.FirstName + " " + user.FamilyName
                };

                var transModel = new TransactionModel
                {
                    Amount = transaction.Amount,
                    DateTime = transaction.TransactionDateTime.ToString(),
                    Source = source,
                    Destination = user.FirstName + " " + user.FamilyName


                };

                result.Message = "Transaction successful";
                result.Success = true;
                result.Transaction = transModel;
            }
            catch (Exception e)
            { 
                result.Message = "Error at UserClaimWeddingGold(): " + e.Message;
                Log.Error(result.Message);
            }



            return Ok(result);
        }
        */
        /*
        [HttpPost]
        [Route("user_gold_transfer")]
        public ActionResult<TransactionResultModel> TransferGoldFromUserToUser(TransferFromUserToUserParamModel model)
        {
            TransactionResultModel result = null;
            try
            {
                Log.Information("user gold transfer initiated");
                var sourceUser = Repository
                    .GetAllUsers()
                    .Where(usr => usr.UserId == Guid.Parse(model.UserId))
                    .FirstOrDefault();

                var destinationUser = Repository
                    .GetAllUsers()
                    .Where(usr => usr.MemberId == model.MemberId)
                    .FirstOrDefault();



                GoldType type = GlobalGoldPrice.ParseType(model.GoldType);
                decimal gram = GlobalGoldPrice.GetTotalGram(type, model.Amount);

                var currentPrice = (decimal)GlobalGoldPrice.GetBuyPrice(GlobalGoldPrice.GetCurrentPrice());

                var transPrice = gram * currentPrice;

                if (sourceUser.Balance < gram)
                {
                    throw new Exception("Yeterli bakiye yok");
                }

                sourceUser.ManipulateBalance(-gram);
                destinationUser.ManipulateBalance(gram);

                var transaction = new Transaction(
                    "GOLD"
                    , sourceUser.UserId.ToString()
                    , "User"
                    , destinationUser.UserId.ToString()
                    , "User"
                    , gram
                    , "Internal Transfer"
                    , true
                    , gram
                    , transPrice);

                Repository.AddTransaction(transaction);


                var destMessage = "You have received " + gram + "gr of Gold from " + sourceUser.FirstName +
                    " " + sourceUser.FamilyName;

                var srcMessage = "You sent " + gram + "gr of Gold to " + destinationUser.FirstName +
                    " " + destinationUser.FamilyName;
                Repository.AddNotification(new Notification(destinationUser.UserId, destMessage, false));
                Repository.AddNotification(new Notification(sourceUser.UserId, srcMessage, false));

                Repository.SaveChanges();

                var source = new UserModel
                {
                    UserId = sourceUser.UserId.ToString(),
                    Balance = sourceUser.Balance,
                    MemberId = sourceUser.MemberId,
                    Name = sourceUser.FirstName + " " + sourceUser.FamilyName
                };

                var transModel = new TransactionModel
                {
                    Amount = transaction.Amount,
                    DateTime = transaction.TransactionDateTime.ToString(),
                    Source = source,
                    Destination = destinationUser.FirstName + " " + destinationUser.FamilyName


                };

                result = new TransactionResultModel { Success = true, Transaction = transModel };
                Log.Information("user gold transfer complete");
            }
            catch (Exception e)
            {
                Log.Error("Exception attransfer to user " + e.Message);
                result.Message = e.Message;
                result.Success = false;
            }



            return Ok(result);
        }

        */
    }
}