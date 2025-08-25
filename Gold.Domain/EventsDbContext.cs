using Gold.Core.Events;
using Gold.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Gold.Domain
{
    public class EventsDbContext : DbContext
    {
        public DbSet<Wedding> Weddings { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<GoldDay> GoldDays { get; set; }
        public DbSet<User2> Users { get; set; }
        public DbSet<Transaction2> Transactions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<InternalLog> Logs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var s = ConnectionSettings.Cstring2;
            if (Debugger.IsAttached)
                s = ConnectionSettings.Cstring;

            optionsBuilder.UseSqlServer(s);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User2>().ToTable("Users").HasKey(c => c.UserId);
            modelBuilder.Entity<Transaction2>().ToTable("Transactions").HasKey(c => c.TransactionId);
            modelBuilder.Entity<Wedding>().ToTable("Weddings").HasKey(c => c.WeddingId);
            modelBuilder.Entity<Wedding>().HasOne(wed => wed.User).WithMany().HasForeignKey(wed => wed.CreatedBy);
            modelBuilder.Entity<Event>().ToTable("Events").HasKey(c => c.EventId);
            modelBuilder.Entity<Event>().HasOne(evt => evt.User).WithMany().HasForeignKey(evt => evt.CreatedBy);
            modelBuilder.Entity<Notification>().ToTable("Notifications").HasKey(c => c.NotificationId);
            modelBuilder.Entity<Notification>().HasOne(x => x.User).WithMany().HasForeignKey(c => c.UserId);
            modelBuilder.Entity<GoldDay>().ToTable("GoldDays").HasKey(c => c.GoldDayId);
            modelBuilder.Entity<GoldDay>().HasOne(wed => wed.User).WithMany().HasForeignKey(wed => wed.CreatedBy);
            modelBuilder.Entity<InternalLog>().ToTable("Logs").HasKey(x => x.LogId);
            modelBuilder.Entity<InternalLog>().HasOne(z => z.User).WithMany().HasForeignKey(n => n.UserId);
        }


    }
}
