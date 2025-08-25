using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.KuwaitTurk.Vpos
{
    public enum CallbackType {
        NormalSatinAlma,
        EventYolla,
        SilverSatinAlma
    }

    public class KartOnayModel
    {
        public string CardNumber { get; set; }
        public string ExpiryYear { get; set; }
        public string ExpiryMonth { get; set; }
        public string Cvv { get; set; }
        public string HolderName { get; set; }
        public string Amount { get; set; } // amount 1 tl icin 100
        public string TransactionId { get; set; }
        public CallbackType CallbackType { get; set; }


        public override string ToString()
        {
            return CardNumber + "\n" +
                ExpiryYear + "/" + ExpiryMonth + "\n" +
                Cvv + "\n" +
                HolderName + "\n" +
                Amount + "\n" +
                TransactionId + "\n" + 
                CallbackType.ToString() + "\n";
        }
    }
}
