using Gold.Core.Events;
using Gold.Domain.Events.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gold.Domain.Events.Repositories
{
    public class EventsRepository : IEventsRepository
    {

        private readonly EventsDbContext Context;

        public EventsRepository()
        {
            Context = new EventsDbContext();
        }
        void IEventsRepository.AddWedding(Wedding wedding)
        {
            Context.Weddings.Add(wedding);
        }

        void IEventsRepository.AddNotification(Notification notification)
        {
            Context.Notifications.Add(notification);
        }

        IQueryable<Transaction2> IEventsRepository.GetWeddingTransactions(Guid weedingId)
        {
            var query = Context
                .Transactions
                .Where(x => x.TransactionType == "GOLD" && x.Destination == weedingId.ToString());

            return query;
        }

        IQueryable<Wedding> IEventsRepository.GetAllWeddings()
        {
            return Context.Weddings;
        }

        void IEventsRepository.SaveChanges()
        {
            Context.SaveChanges();
        }

        void IEventsRepository.AddGoldDay(GoldDay goldDay)
        {
            Context.GoldDays.Add(goldDay);
        }

        IQueryable<GoldDay> IEventsRepository.GetAllGoldDays()
        {
            return Context.GoldDays;
        }

        IQueryable<User2> IEventsRepository.GetAllUsers()
        {
            return Context.Users;
        }


        void IEventsRepository.RemoveEvent(Wedding wedding)
        {
            Context.Weddings.Remove(wedding);
        }

        void IEventsRepository.RemoveEvent(GoldDay goldDay)
        {
            
            Context.GoldDays.Remove(goldDay);
        }


        void IEventsRepository.AddEvent(Event _event)
        {
            Context.Events.Add(_event);
        }

        void IEventsRepository.RemoveEvent(Event _event)
        {
            Context.Events.Remove(_event);
        }

        IQueryable<Event> IEventsRepository.GetAllEvents()
        {
            return Context.Events;
        }

        IQueryable<Transaction2> IEventsRepository.GetEventsTransactions(Guid eventId)
        {
            var query = Context
                .Transactions
                .Where(x => x.TransactionType == "GOLD" && x.Destination == eventId.ToString());

            return query;
        }

        void IEventsRepository.AddLog(InternalLog log)
        {
            Context.Logs.Add(log);
        }
    }
}
