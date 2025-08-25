using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Models.Users
{
    public class UserRecords : IEquatable<UserRecords>
    {
        public Guid UserId { get; set; }
        public int TransactionsCount { get; set; }
        public int NotificationsCount { get; set; }
        public int EventsWeddingsCount { get; set; }
        public decimal UserBalance { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as UserRecords);
        }

        public override string ToString()
        {
            return string.Format("UserRecords: {0} {1} {2} {3} {4}", UserId.ToString(), TransactionsCount, NotificationsCount, EventsWeddingsCount, (float)UserBalance);
        }
        public bool Equals(UserRecords other)
        {
            return other != null &&
                   UserId.Equals(other.UserId) &&
                   TransactionsCount == other.TransactionsCount &&
                   EventsWeddingsCount == other.EventsWeddingsCount &&
                   UserBalance == other.UserBalance;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, TransactionsCount, EventsWeddingsCount, UserBalance);
        }
    }
}
