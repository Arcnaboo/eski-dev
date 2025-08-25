using Gold.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gold.Domain.Events.Interfaces
{
    public interface IEventsRepository
    {
        void AddLog(InternalLog log);
        void AddGoldDay(GoldDay goldDay);
        void AddWedding(Wedding wedding);
        void AddEvent(Event _event);
        void RemoveEvent(Wedding wedding);
        void RemoveEvent(Event _event);
        void RemoveEvent(GoldDay goldDay);

        void AddNotification(Notification notification);

        void SaveChanges();
        IQueryable<Wedding> GetAllWeddings();
        IQueryable<GoldDay> GetAllGoldDays();
        IQueryable<Event> GetAllEvents();
        IQueryable<Transaction2> GetWeddingTransactions(Guid weedingId);
        IQueryable<Transaction2> GetEventsTransactions(Guid eventId);
        IQueryable<User2> GetAllUsers();


    }
}
