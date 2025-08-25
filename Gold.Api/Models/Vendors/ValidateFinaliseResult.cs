using Gold.Core.Vendors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Vendors
{
    public class ValidateFinaliseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ResultCode { get; set; }
        public string vendorid { get; set; }
        public bool Confirmed { get; set; }
        public Vendor requestee { get; set; }
        public Vendor vendor { get; set; }
        public VendorData vendorData { get; set; }
        public VenTransaction vendorTransaction { get; set; }
        public VenTransaction previousTransaction { get; set; }

        public VendorPlatinBalance PlatinBalance { get; set; }


        public override string ToString()
        {
            return string.Format("ValidateFinaliseResult: {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}",
                Success, Message, ResultCode, vendorid, Confirmed, JsonConvert.SerializeObject(requestee),
                JsonConvert.SerializeObject(vendor), JsonConvert.SerializeObject(vendorData),
                JsonConvert.SerializeObject(vendorTransaction), JsonConvert.SerializeObject(previousTransaction));
        }
    }
}
