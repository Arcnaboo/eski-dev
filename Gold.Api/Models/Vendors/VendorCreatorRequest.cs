using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Vendors
{
    public class VendorCreatorRequest
    {
        public DateTime CreatedDateTime { get; set; }
        public DateTime ValidUntil { get; set; }
        public string Code { get; set; }
        public bool Used { get; set; }
    }
}
