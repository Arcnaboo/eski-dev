using Gold.Api.Models.KuwaitTurk;
using Gold.Api.Models.Transactions;
using Gold.Domain.Transactions.Interfaces;
using Gold.Domain.Transactions.Repositories;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Gold.Api.Utilities
{

    public enum GoldType
    {
        Gram,
        Ceyrek,
        Yarim,
        Tam,
        Cumhuriyet
    }

    public class GlobalGoldPrice
    {
        private static readonly decimal CeyrekAltinGram = 1.60M; // 1.60
        private static readonly decimal YarimAltinGram = 3.20M; // 3.20
        private static readonly decimal TamAltinGram = 6.40M; // 6.40
        private static readonly decimal CumhuriyetkAltinGram = 7.2M;


        public static bool Automatic = true;

        public static TaxAndExpensesModel TaxAndExpenses { get; set; }
        
        public static decimal SilverBuy = 6.66m;
        public static decimal SilverSell = 6.62m;

        public static decimal GoldBuy = 800m;
        public static decimal GoldSell = 800m;

        public static decimal PltBuy = 900m;
        public static decimal PltSell = 800m;

        private static object mutex = new object();
        private static object mutex2 = new object();

        public static async Task<int> SetPricesAsync(decimal goldBuy, 
            decimal goldSell, 
            decimal silBuy, 
            decimal silSEll,
            decimal pltBuy,
            decimal pltSell)
        {
            return await Task.Run(() => {
                if (Automatic)
                {
                    SilverBuy = silBuy;
                    SilverSell = silSEll;
                    GoldBuy = goldBuy;
                    GoldSell = goldSell;
                    PltBuy = pltBuy;
                    PltSell = pltSell;
                }
                return 0;
            });
        }


        public static void SetPrices(decimal goldBuy, decimal goldSell, 
            decimal silBuy, decimal silSEll, decimal pltBuy,
            decimal pltSell)
        {/*
            lock(mutex)
            {
                SilverBuy = silBuy;
                SilverSell = silSEll;
                GoldBuy = goldBuy;
                GoldSell = goldSell;
            }*/
            if (Automatic)
            {
                SilverBuy = silBuy;
                SilverSell = silSEll;
                GoldBuy = goldBuy;
                GoldSell = goldSell;
                PltBuy = pltBuy;
                PltSell = pltSell;
            }
            
        }

        public static void SetPricesManually(decimal goldBuy, 
            decimal goldSell, decimal silBuy, decimal silSEll,
            decimal pltBuy,
            decimal pltSell
            )
        {

            Automatic = false;
            SilverBuy = silBuy;
            SilverSell = silSEll;
            GoldBuy = goldBuy;
            GoldSell = goldSell;
            PltBuy = pltBuy;
            PltSell = pltSell;
        }

        public static void SetAutomaticTrue()
        {
            Automatic = true;
        }

        public static async Task<int> UpdateTaxExpensesCache()
        {
            TaxAndExpenses = await GetTaxAndExpensesAsync();
            return 0;
        }

        public static TaxAndExpensesModel GetTaxAndExpensesCached()
        {



            return TaxAndExpenses;
        }

        public static FxRate GetGoldPricesCached()
        {

            return new FxRate { BuyRate = GoldBuy, SellRate = GoldSell };
        }

        public static FxRate GetSilverPricesCached()
        {

            return new FxRate { BuyRate = SilverBuy, SellRate = SilverSell };
        }

        public static FxRate GetPlatinPricesCached()
        {
            return new FxRate { BuyRate = PltBuy, SellRate = PltSell };
        }


        public static FxRate GetSilverPrices()
        {
            ITransactionsRepository repo = new TransactionsRepository();
            var buy = repo.AccessSilverBuy();
            var sell = repo.AccessSilverSell();

            return new FxRate { BuyRate = buy.Amount.Value, SellRate = sell.Amount.Value };
        }

        public static async Task<FxRate> GetSilverPricesAsync()
        {
       
      
            ITransactionsRepository repo = new TransactionsRepository();
                
            var buy = await repo.AccessSilverBuyAsync();
            var sell = await repo.AccessSilverSellAsync();

            return new FxRate { BuyRate = buy.Amount.Value, SellRate = sell.Amount.Value };
      
        }

        public static async Task<FxRate> GetPlatinPricesAsync()
        {
         
            ITransactionsRepository repo = new TransactionsRepository();
                
            var buy = await repo.AccessPlatinBuyAsync();
            var sell = await repo.AccessPlatinSellAsync();

            return new FxRate { BuyRate = buy.Amount.Value, SellRate = sell.Amount.Value };
       
        }

        public static FxRate GetPlatinPrices()
        {
            ITransactionsRepository repo = new TransactionsRepository();
            var buy = repo.AccessPlatinBuy();
            var sell = repo.AccessPlatinSell();

            return new FxRate { BuyRate = buy.Amount.Value, SellRate = sell.Amount.Value };
        }


        public static void RegisterSilverPricesByAiService(decimal silverBuy, decimal silverSell)
        {
            SilverBuy = silverBuy;
            SilverSell = silverSell;
        }

        public static GoldType ParseType(string type)
        {
            if (type == "ceyrek")
                return GoldType.Ceyrek;
            if (type == "yarim")
                return GoldType.Yarim;
            if (type == "tam")
                return GoldType.Tam;
            if (type == "cumhuriyet")
                return GoldType.Cumhuriyet;

            return GoldType.Gram;
        }


        public static decimal GetTotalGram(GoldType type, decimal amount)
        {
            switch(type)
            {
                case GoldType.Gram:
                    return amount;
                case GoldType.Ceyrek:
                    return CeyrekAltinGram * amount;
                case GoldType.Yarim:
                    return YarimAltinGram * amount;
                case GoldType.Tam:
                    return TamAltinGram * amount;
                case GoldType.Cumhuriyet:
                    return CumhuriyetkAltinGram * amount;

            }
            return amount;
        }
        //public static decimal TotalGram(decimal amountRequested, s)

        public static double GetBuyPrice(FxRate rate)
        {
            double result = (double )rate.BuyRate;

            return result;
        }

        public static TaxAndExpensesModel GetTaxAndExpenses()
        {
            ITransactionsRepository repo = new TransactionsRepository();


            var kar = repo.AccessKar();
            var vpos = repo.AccessVposCommission();
            var bankTax = repo.AccessBankTax();
            var satisKOmison = repo.AccessSatisKomisyon();

            var result = new TaxAndExpensesModel
            {
                BankaMukaveleTax = bankTax.Percentage.Value,
                Kar = kar.Amount.Value,
                VposCommission = vpos.Percentage.Value,
                SatisKomisyon = satisKOmison.Amount.Value
            };

            return result;
        }
        public static async Task<TaxAndExpensesModel> GetTaxAndExpensesAsync()
        {
            ITransactionsRepository repo = new TransactionsRepository();

            var karTask = await repo.AccessKarAsync();
            var vposTask = await repo.AccessVposCommissionAsync();
            var bankTaxTask = await repo.AccessBankTaxAsync();
            var satisKOmisonTask = await repo.AccessSatisKomisyonAsync();
            

            var result = new TaxAndExpensesModel
            {
                BankaMukaveleTax = bankTaxTask.Percentage.Value,
                Kar = karTask.Amount.Value,
                VposCommission = vposTask.Percentage.Value,
                SatisKomisyon = satisKOmisonTask.Amount.Value
            };
            return result;
            
        }


        public static decimal GetSellPercentage()
        {
            ITransactionsRepository repo = new TransactionsRepository();
            var buy = repo.AccessSellPercentage();

            return buy.Percentage.Value;
        }

        public static decimal GetBuyPercentage()
        {
            ITransactionsRepository repo = new TransactionsRepository();
            var buy = repo.AccessBuyPercentage();

            return buy.Percentage.Value;
        }

        public static FxRate GetCurrentPrice()
        {

            ITransactionsRepository repo = new TransactionsRepository();
            var buy = repo.AccessBuyPrice();
            var sell = repo.AccessSellPrice();

            return new FxRate { BuyRate = buy.Amount.Value, SellRate = sell.Amount.Value };

         
        }

        public static async Task<FxRate> GetCurrentGoldPriceAsync()
        {
            ITransactionsRepository repo = new TransactionsRepository();
            

            var buyTask = await repo.AccessBuyPriceAsync();
            var sellTask = await repo.AccessSellPriceAsync();

            return new FxRate { BuyRate = buyTask.Amount.Value, SellRate = sellTask.Amount.Value };
            
        }

        
    }
}
