using Gold.Domain.Users.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gold.Api.Models.Users
{
    public class CreateUserModel
    {

        public string RoleName { get; set; } // "Member"
        public string FirstName { get; set; } // "First name + mid"
        public string FamilyName { get; set; }
        public string Email { get; set; }
        public string TCK { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Birthdate { get; set; }
        public int? RefCode { get; set; }
    }

    public class CreateUserModel2
    {

        
        public string FirstName { get; set; } // "First name + mid"
        //public string FamilyName { get; set; }
        //public string Email { get; set; }

        
        public string Phone { get; set; }

        
        public string Password { get; set; }
        //public string Birthdate { get; set; }
        public string RefCode { get; set; }
    }
}
