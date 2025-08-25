using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gold.Domain.Users.Interfaces;
using Newtonsoft.Json;
namespace Gold.Api.Models.Admin
{
    public class TransactionAdminModel
    {
        [JsonProperty("memberid")]
        public string MemberId { get; set; }

        [JsonProperty("tck")]
        public string TCK { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("nerden")]
        public string Source { get; set; }

        [JsonProperty("nereye")]
        public string Destination { get; set; }

        [JsonProperty("altin")]
        public string Altin { get; set; }

        [JsonProperty("komisyonlu_fiyat")]
        public string Price { get; set; }

        [JsonProperty("bsmvsiz_komisyonlu")]
        public string NicePrice { get; set; }

        [JsonProperty("bsmvsiz_komisyonlu_bozdurma")]
        public string BsmvsizKomBozd { get; set; }

        [JsonProperty("baz_fiyat")]
        public string BazPrice { get; set; }

        [JsonProperty("komisyon")]
        public string Komisyon { get; set; }

        public TransactionAdminModel(Gold.Core.Transactions.Transaction transaction, IUsersRepository repository)
        {
            
            if (transaction.TransactionType == "TRY" || transaction.TransactionType == "TRY_FOR_SILVER")
            {
                InitiateTryTransaction(transaction, repository);
            }
            else
            {
                InitiateGoldTransaction(transaction, repository);
            }
        }

        private void InitiateGoldTransaction(Gold.Core.Transactions.Transaction transaction, IUsersRepository repository)
        {
            Date = transaction.TransactionDateTime.ToString();
            Altin = transaction.GramAmount.ToString();
            Price = transaction.TlAmount.ToString();

        }

        private void InitiateTryTransaction(Gold.Core.Transactions.Transaction transaction, IUsersRepository repository)
        {
            Date = transaction.TransactionDateTime.ToString();
            if (transaction.SourceType == "Fintag")
            {
                MemberId = "111222333";
                TCK = "Fintag";
                Source = "Fintag";
            }
            else
            {
                var user = repository.GetAllUsers().Where(x => x.UserId.ToString() == transaction.Source).FirstOrDefault();
                if (user != null)
                {
                    MemberId = user.MemberId.ToString();
                    TCK = user.TcKimlikNo;
                    Source = user.FirstName + " " + user.FamilyName;
                }
                else
                {
                    MemberId = "0";
                    TCK = "0";
                    Source = "Silinmis Kullanici";
                }
                
            }

            if (transaction.DestinationType == "IBAN")
            {
                Destination = "Fintag IBAN";
            }
            else if (transaction.DestinationType == "User")
            {
                var user = repository.GetAllUsers().Where(x => x.UserId.ToString() == transaction.Destination).FirstOrDefault();
                if (user == null)
                {
                    Destination = "Silinmis Kullanici";
                } 
                else
                {
                    Destination = user.FirstName + " " + user.FamilyName;
                }
                
            }
            else
            {
                Destination = "Fintag VPOS";
            }
            Altin = transaction.GramAmount.ToString();
            Price = transaction.Amount.ToString();
            NicePrice = (transaction.Amount * 0.998M).ToString();
            BazPrice = transaction.TlAmount.ToString();
            Komisyon = (transaction.Amount - transaction.TlAmount).ToString();
            BsmvsizKomBozd = (transaction.Amount * 1.002M).ToString();
        }

    }
}
