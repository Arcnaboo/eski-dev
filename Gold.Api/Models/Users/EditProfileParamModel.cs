using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Users
{
    public class EditProfileParamModel
    {

        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("current_password")]
        public string CurrentPassword { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }
    }

    public class EditProfileParamModel2
    {

        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("firstname")]
        public string FirstName { get; set; }

        [JsonProperty("familyname")]
        public string FamilyName { get; set; }

        [JsonProperty("birthdate")]
        public string BirthDate { get; set; }
    }

    public class EnterTckParamModel
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("firstname")]
        public string FirstName { get; set; }

        [JsonProperty("familyname")]
        public string FamilyName { get; set; }

        [JsonProperty("tck")]
        public string TCK { get; set; }

        [JsonProperty("birthdate")]
        public string Birthdate { get; set; }
    }

    public class EditProfileResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("user")]
        public UserModel User { get; set; }
    }
}
