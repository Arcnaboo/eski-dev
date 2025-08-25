using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Users
{
    public class UserModel
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("firstname")]
        public string FirstName { get; set; }

        [JsonProperty("familyname")]
        public string FamilyName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("memberid")]
        public int MemberId { get; set; }

        [JsonProperty("user_level")]
        public int UserLevel { get; set; }

        [JsonProperty("ref_code")]
        public int RefCode { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }
        [JsonProperty("silver_balance")]
        public decimal SilverBalance { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("blocked")]
        public decimal Blocked { get; set; }

        [JsonProperty("loginid")]
        public string LoginId { get; set; }

        [JsonProperty("birthdate")]
        public string Birthdate { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("tck")]
        public string TCK { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("transactions")]
        public List<Gold.Api.Models.Transactions.TransactionModel> Transactions { get; set; }

        [JsonProperty("events")]
        public List<Gold.Api.Models.Events.EventModel> Events { get; set; }
    }
}
