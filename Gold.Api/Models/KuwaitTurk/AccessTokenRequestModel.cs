using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.KuwaitTurk
{
    public class AccessTokenRequestModel
    {

        public string Grant_type { get; set; }
        public string Scope { get; set; }
        public string Client_id { get; set; }
        public string Client_secret { get; set; }
}
}
