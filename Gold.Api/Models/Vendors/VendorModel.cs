using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace Gold.Api.Models.Vendors
{

    public class CreateVendorResultModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("vendor")]
        public VendorModel Vendor { get; set; }
    }
    public class VendorModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("gold")]
        public string Balance { get; set; }

        [JsonProperty("silver")]
        public string SBalance { get; set; }

        [JsonProperty("tl_suffix")]
        public string TLSuffix { get; set; }
        [JsonProperty("gold_suffix")]
        public string GOLDSuffix { get; set; }
        [JsonProperty("silver_suffix")]
        public string SLVSuffix { get; set; }
    }

    public class VendorTransactionRequestParamModel
    {
        [JsonProperty("date_from")]
        public string DateFrom { get; set; }

        [JsonProperty("date_to")]
        public string DateTo { get; set; }

        [JsonProperty("vendor_id")]
        public string VendorId { get; set; }

        public static bool ValidateModel(VendorTransactionRequestParamModel model)
        {
            
            
            if (model == null || model.DateFrom == null || model.DateTo == null)
            {
                return false;
            }
            
            try
            {
                var from = DateTime.ParseExact(model.DateFrom, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var to = DateTime.ParseExact(model.DateTo, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Log.Error("err vendor trans request param: " + e.Message);
                Log.Error(e.StackTrace);
                return false;
            }


            return true;
        }
    }
}
