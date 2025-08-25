using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Users
{
    public class CekilisModel
    {
        public string Name { get; set; }
        public string Prize { get; set; }
        public string SonKatilimTarihi { get; set; }
        public string PlanlananCekilisTarihi { get; set; }
        public int CekilisHakki { get; set; }
        public string SonHakAlisTarihi { get; set; }
        public bool YeniHakAlabilir { get; set; }
        public string HakId { get; set; }
        public string TimeToNextHak { get; set; }

    }
}
