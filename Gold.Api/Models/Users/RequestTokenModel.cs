using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Users
{
    public class RequestTokenModel
    {
        public string UserId { get; set; }
        public string Password { get; set; }
    }
}
