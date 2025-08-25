using Gold.Core.Vendors;
using Gold.Domain.Vendors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gold.Api.Models.Vendors
{
    public class ExpectedModel
    {
        [JsonProperty("vendor")]
        public string Vendor { get; set; }

        [JsonProperty("suffix")]
        public string ExpectedSuffix { get; set; }

        [JsonProperty("try")]
        public decimal ExpectedTRY { get; set; }

        [JsonProperty("grams")]
        public decimal Grams { get; set; }

        [JsonProperty("date")]
        public string DateTime { get; set; }

        public ExpectedModel() { }

        public ExpectedModel(ExpectedCash expectedCash, IVendorsRepository vendorsRepository)
        {
            Vendor = vendorsRepository.GetVendors().Where(x => x.VendorId == expectedCash.VendorId).FirstOrDefault().Name;
            ExpectedSuffix = expectedCash.ExpectedSuffix;
            ExpectedTRY = expectedCash.ExpectedTRY;
            Grams = expectedCash.Amount;
            DateTime = expectedCash.DateTime.ToString();
        }

        public ExpectedModel(VendorExpected expectedCash, IVendorsRepository vendorsRepository)
        {
            Vendor = vendorsRepository.GetVendors().Where(x => x.VendorId == expectedCash.VendorId).FirstOrDefault().Name;
            ExpectedSuffix = expectedCash.ExpectedSuffix;
            ExpectedTRY = expectedCash.ExpectedTRY;
            Grams = expectedCash.Amount;
            DateTime = expectedCash.DateTime.ToString();
        }
    }

    public class FinalizedModel
    {
        [JsonProperty("vendor")]
        public string Vendor { get; set; }

        [JsonProperty("try")]
        public decimal ExpectedTRY { get; set; }

        [JsonProperty("grams")]
        public decimal Grams { get; set; }

        [JsonProperty("date")]
        public string DateTime { get; set; }

        [JsonProperty("piyasa_kur")]
        public decimal PiyasaGramFiyat { get; set; }

        [JsonProperty("kt_kur")]
        public decimal KTGramFiyat { get; set; }

        [JsonProperty("satis_kur")]
        public decimal SatisGramFiyat { get; set; }

        [JsonProperty("altin_verildi")]
        public bool AltinVerildi { get; set; }

        [JsonProperty("kt_referans")]
        public string KTReferansId { get; set; }

        [JsonProperty("comments")]
        public string Comments { get; set; }

        [JsonProperty("muhasebe_tl")]
        public decimal MuhasebeTL { get; set; }

        [JsonProperty("muhasebe_gram")]
        public decimal MuhasebeGram { get; set; }

        [JsonProperty("final_kar")]
        public decimal FinalKar { get; set; }

        public FinalizedModel() { }

        public FinalizedModel(FinalizedGold finalizedGold, IVendorsRepository vendorsRepository)
        {
            Vendor = vendorsRepository.GetVendors().Where(x => x.VendorId == finalizedGold.VendorId).FirstOrDefault().Name;
            
            ExpectedTRY = finalizedGold.TLAmount;
            Grams = finalizedGold.GoldAmount;
            DateTime = finalizedGold.DateTime.ToString();
            PiyasaGramFiyat = finalizedGold.PiyasaGramFiyat;
            KTGramFiyat = finalizedGold.KTGramFiyat;
            SatisGramFiyat = finalizedGold.SatisGramFiyat;
            AltinVerildi = finalizedGold.AltinVerildi;
            Comments = finalizedGold.Comments;
            MuhasebeGram = finalizedGold.MuhasebeGram;
            MuhasebeTL = finalizedGold.MuhasebeTL;
            FinalKar = 0;
        }

        public FinalizedModel(VendorFinalized finalizedGold, IVendorsRepository vendorsRepository)
        {
            Vendor = vendorsRepository.GetVendors().Where(x => x.VendorId == finalizedGold.VendorId).FirstOrDefault().Name;
            ExpectedTRY = finalizedGold.TLAmount;
            Grams = finalizedGold.GoldAmount;
            DateTime = finalizedGold.DateTime.ToString();
            PiyasaGramFiyat = finalizedGold.PiyasaGramFiyat;
            KTGramFiyat = finalizedGold.KTGramFiyat;
            SatisGramFiyat = finalizedGold.SatisGramFiyat;
            AltinVerildi = finalizedGold.Comments == "VENDOR_BUY";
            Comments = finalizedGold.Comments;
            MuhasebeGram = finalizedGold.MuhasebeGram;
            MuhasebeTL = finalizedGold.MuhasebeTL;
            FinalKar = finalizedGold.FinalKar.HasValue ? finalizedGold.FinalKar.Value : 0;
        }
    }


    public class NotPositionSellModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("vendor")]
        public string Vendor { get; set; }
        [JsonProperty("transaction_count")]
        public int TransactionId { get; set; }
        [JsonProperty("total_try")]
        public decimal TlAmount { get; set; }
        [JsonProperty("total_grams")]
        public decimal Grams { get; set; }
        [JsonProperty("suffix")]
        public string ExpectedSuffix { get; set; }
        [JsonProperty("account")]
        public string Account { get; set; }
        [JsonProperty("kt_sell_rate")]
        public decimal SellRate { get; set; }
        [JsonProperty("date")]
        public string DateTime { get; set; }
        [JsonProperty("comments")]
        public string Comment { get; set; }
        [JsonProperty("first_last")]
        public string KTReferans { get; set; }
        [JsonProperty("average_piyasa_fiyat")]
        public decimal PiyasaGramFiyat { get; set; }
        [JsonProperty("kt_fiyat")]
        public decimal KTGramFiyat { get; set; }
        [JsonProperty("average_goldtag_fiyat")]
        public decimal SatisGramFiyat { get; set; }

        public NotPositionSellModel() { }

        public NotPositionSellModel(VendorNotPositionSell positionSell)
        {

            Vendor = positionSell.VendorId.ToString();
            Id = positionSell.NotPosId.ToString();
            TransactionId = positionSell.TransactionId;
            TlAmount = positionSell.TlAmount;
            Grams = positionSell.GramAmount;
            ExpectedSuffix = positionSell.Suffix;
            Account = positionSell.Account;
            SellRate = positionSell.SellRate;
            DateTime = positionSell.DateTime.ToString();
            KTReferans = positionSell.RefId;
            PiyasaGramFiyat = positionSell.PiyasaGramFiyat;
            SatisGramFiyat = positionSell.SatisGramFiyat;
            KTGramFiyat = positionSell.KTGramFiyat;
            Comment = positionSell.Comments;
        }

        public async Task AddName(IVendorsRepository vendorsRepository)
        {
            var id = Guid.Parse(Vendor);
            Vendor = (await vendorsRepository.GetVendorAsync(id)).Name;
        }

        public void AddNameSynch(IVendorsRepository vendorsRepository)
        {
            var id = Guid.Parse(Vendor);
            Vendor = vendorsRepository.GetVendors().Where(x => x.VendorId == id).FirstOrDefault().Name;
        }
    }

    public class NotPositionModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("vendor")]
        public string Vendor { get; set; }
        [JsonProperty("transaction_count")]
        public int TransactionId { get; set; }
        [JsonProperty("suffix_from")]
        public string SuffFrom { get; set; }
        [JsonProperty("suffix_to")]
        public string SuffTo { get; set; }
        [JsonProperty("total_grams")]
        public decimal Grams { get; set; }
        [JsonProperty("total_try")]
        public decimal BuyRate { get; set; }
        [JsonProperty("date")]
        public string DateTime { get; set; }
        [JsonProperty("comments")]
        public string Comment { get; set; }
        [JsonProperty("first_last")]
        public string KTReferans { get; set; }
        [JsonProperty("average_piyasa_fiyat")]
        public decimal PiyasaGramFiyat { get; set; }
        [JsonProperty("kt_fiyat")]
        public decimal KTGramFiyat { get; set; }
        [JsonProperty("average_goldtag_fiyat")]
        public decimal SatisGramFiyat { get; set; }

        public NotPositionModel() { }

        public NotPositionModel(VendorNotPosition notPosition)
        {
            Id = notPosition.NotPosId.ToString();
            Vendor = notPosition.VendorId.ToString();
            TransactionId = notPosition.TransactionId;
            SuffFrom = notPosition.SuffixFrom;
            SuffTo = notPosition.SuffixTo;
            Grams = notPosition.Amount;
            BuyRate = notPosition.BuyRate;
            DateTime = notPosition.DateTime.ToString();
            Comment = notPosition.Comments;
            KTReferans = notPosition.RefId;
            PiyasaGramFiyat = notPosition.PiyasaGramFiyat;
            KTGramFiyat = notPosition.KTGramFiyat;
            SatisGramFiyat = notPosition.SatisGramFiyat;
        }


        public async Task AddName(IVendorsRepository vendorsRepository)
        {
            var id = Guid.Parse(Vendor);
            Vendor = (await vendorsRepository.GetVendorAsync(id)).Name;
        }

        public void AddNameSynchrounous(IVendorsRepository vendorsRepository)
        {
            var id = Guid.Parse(Vendor);
            Vendor = vendorsRepository.GetVendors().Where(x => x.VendorId == id).FirstOrDefault().Name;
        }
        
    }

    public class UnExpectedModel
    {
        [JsonProperty("vendor")]
        public string Vendor { get; set; }

        [JsonProperty("transaction_id")]
        public int TransactionId { get; set; }

        [JsonProperty("suffix")]
        public string ExpectedSuffix { get; set; }

        [JsonProperty("kt_ref")]
        public string KTReferans { get; set; }

        [JsonProperty("grams")]
        public decimal Grams { get; set; }

        [JsonProperty("expected_try")]
        public decimal ExpectedTRY { get; set; }

        [JsonProperty("received_try")]
        public decimal? ReceivedTRY { get; set; }

        [JsonProperty("time_diff")]
        public int? DifferenceSEconds { get; set; }

        [JsonProperty("comments")]
        public string Comment { get; set; }

        [JsonProperty("date")]
        public string DateTime { get; set; }

        public UnExpectedModel() { }



        public UnExpectedModel(VendorUnExpected expectedCash, IVendorsRepository vendorsRepository)
        {
            TransactionId = expectedCash.TransactionId;
            var trans = vendorsRepository.GetVendorTransactionNewAsync(TransactionId);
            trans.Wait();

            Vendor = vendorsRepository.GetVendors().Where(x => x.VendorId == trans.Result.Destination).FirstOrDefault().Name;
            ExpectedSuffix = expectedCash.Suffix;
            ExpectedTRY = expectedCash.ExpectedTRY;
            ReceivedTRY = expectedCash.ReceivedTRY;
            Grams = expectedCash.Amount;
            DateTime = expectedCash.DateTime.ToString();
            Comment = expectedCash.Comment;
            DifferenceSEconds = expectedCash.DifferenceSEconds;
            KTReferans = expectedCash.KTReference;
        }
    }
}
