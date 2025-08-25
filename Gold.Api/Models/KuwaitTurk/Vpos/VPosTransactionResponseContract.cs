using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.KuwaitTurk.Vpos
{
    public class VPosTransactionResponseContract
    {
        public VPosMessage VPosMessage { get; set; }
        public bool IsEnrolled { get; set; }
        public bool IsVirtual { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string OrderId { get; set; }
        public DateTime TransactionTime { get; set; }
        public string MerchantOrderId { get; set; }
        public string HashData { get; set; }
        public string MD { get; set; }
        public string ReferenceId { get; set; }
        public string BusinessKey { get; set; }
    }
}
