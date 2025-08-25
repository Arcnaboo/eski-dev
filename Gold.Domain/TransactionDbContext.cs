using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Gold.Core.Transactions;
using Gold.Domain.Settings;
using System.Diagnostics;
using Gold.Core.GoldPrices;
using Gold.Core.Banks;
using Gold.Core.Vendors;

namespace Gold.Domain
{
    public class TransactionDbContext : DbContext
    {



        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<User3> Users { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Gcode> Gcodes { get; set; }
        public DbSet<TransferRequest> TransferRequests { get; set; }
        public DbSet<Notification2> Notifications { get; set; }
        public DbSet<Wedding> Weddings { get; set; }
        public DbSet<GoldPrice> GoldPrices { get; set; }
        public DbSet<Event2> Events { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<UserBankTransferRequest> UserBankTransferRequests { get; set; }
        public DbSet<InternalLog> Logs { get; set; }
        public DbSet<SilverBalance> SilverBalances { get; set; }
        public DbSet<UserLevel> UserLevels { get; set; }
       /* public DbSet<GoldtagExpected> GoldtagExpecteds { get; set; }
        public DbSet<GoldtagUnexpected> GoldtagUnexpecteds { get; set; }
        public DbSet<GoldtagFinalized> GoldtagFinalizeds { get; set; }
        public DbSet<GoldtagNotPosition> GoldtagNotPositions { get; set; }
        public DbSet<GoldtagNotReceived> GoldtagNotReceiveds { get; set; }*/

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var s = ConnectionSettings.Cstring2;
            if (Debugger.IsAttached)
                s = ConnectionSettings.Cstring;

            optionsBuilder.UseSqlServer(s);
        }

        


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransferRequest>().ToTable("TransferRequests").HasKey(c => c.TransferRequestId);
            modelBuilder.Entity<Transaction>().ToTable("Transactions").HasKey(c => c.TransactionId);
            modelBuilder.Entity<User3>().ToTable("Users").HasKey(c => c.UserId);
            modelBuilder.Entity<Vendor>().ToTable("Vendors").HasKey(c => c.VendorId);
            modelBuilder.Entity<Gcode>().ToTable("Gcodes").HasKey(c => c.GcodeId);
            modelBuilder.Entity<Notification2>().ToTable("Notifications").HasKey(c => c.NotificationId);
            modelBuilder.Entity<Wedding>().ToTable("Weddings").HasKey(c => c.WeddingId);
            modelBuilder.Entity<Event2>().ToTable("Events").HasKey(c => c.EventId);
            modelBuilder.Entity<Notification2>().HasOne(x => x.User).WithMany().HasForeignKey(n => n.UserId);
            modelBuilder.Entity<Wedding>().HasOne(x => x.User).WithMany().HasForeignKey(n => n.CreatedBy);
            modelBuilder.Entity<Event2>().HasOne(x => x.User).WithMany().HasForeignKey(n => n.CreatedBy);
            modelBuilder.Entity<TransferRequest>().HasOne(x => x.SourceUser).WithMany().HasForeignKey(n => n.SourceUserId);
            modelBuilder.Entity<TransferRequest>().HasOne(x => x.Transaction).WithMany().HasForeignKey(n => n.TransactionRecord);
            
            modelBuilder.Entity<GoldPrice>().ToTable("GoldPrices").HasKey(c => c.GoldPriceId);

            modelBuilder.Entity<Bank>().ToTable("Banks").HasKey(c => c.BankId);
            modelBuilder.Entity<UserBankTransferRequest>().ToTable("UserBankTransferRequests").HasKey(c => c.BankTransferId);
            modelBuilder.Entity<UserBankTransferRequest>().HasOne(c => c.Bank).WithMany().HasForeignKey(x => x.BankId);
            modelBuilder.Entity<UserBankTransferRequest>().HasOne(c => c.User).WithMany().HasForeignKey(x => x.UserId);
            modelBuilder.Entity<UserBankTransferRequest>().HasOne(c => c.TransferRequest).WithMany().HasForeignKey(x => x.TransferRequestId);


            modelBuilder.Entity<InternalLog>().ToTable("Logs").HasKey(x => x.LogId);
            modelBuilder.Entity<InternalLog>().HasOne(z => z.User).WithMany().HasForeignKey(n => n.UserId);

            modelBuilder.Entity<SilverBalance>().ToTable("SilverBalances").HasKey(x => x.SilverId);
            modelBuilder.Entity<UserLevel>().ToTable("UserLevels").HasKey(x => x.LevelId);

            /*
            modelBuilder.Entity<GoldtagExpected>().ToTable("GoldtagExpecteds").HasKey(x => x.ExpectedGoldtagId);
            modelBuilder.Entity<GoldtagUnexpected>().ToTable("GoldtagUnexpecteds").HasKey(x => x.UnexpectedId);
            modelBuilder.Entity<GoldtagFinalized>().ToTable("GoldtagFinalizeds").HasKey(x => x.FinalizedId);
            modelBuilder.Entity<GoldtagNotPosition>().ToTable("GoldtagNotPositions").HasKey(x => x.NotPosId);
            modelBuilder.Entity<GoldtagNotReceived>().ToTable("GoldtagNotReceiveds").HasKey(x => x.NotReceivedId);*/
        }
    }
}
