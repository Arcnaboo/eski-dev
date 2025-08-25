using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.KuwaitTurk.Vpos
{
    public class VPosMessage
    {
        public string OrderId { get; set; }
        public string OkUrl { get; set; }
        public string FailUrl { get; set; }
        public string MerchantId { get; set; }
        public string SubMerchantId { get; set; }
        public string CustomerId { get; set; }
        public string UserName { get; set; }
        public string HashPassword { get; set; }
        public string CardNumber { get; set; }
        public string BatchID { get; set; }
        public string InstallmentCount { get; set; }
        public string Amount { get; set; }
        public string CancelAmount { get; set; }
        public string MerchantOrderId { get; set; }
        public string FECAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string QeryId { get; set; }
        public string DebtId { get; set; }
        public string SurchargeAmount { get; set; }
        public string SGKDebtAmount { get; set; }
        public string TransactionSecurity { get; set; }
        public string InstallmentMaturityCommisionFlag { get; set; }
        public string DeferringCount { get; set; }
        public string PaymentId { get; set; }
        public string OrderPOSTransactionId { get; set; }
        public string TranDate { get; set; }
        public string TransactionUserId { get; set; }
    }
}
