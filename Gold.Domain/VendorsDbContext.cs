using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Gold.Core.Transactions;
using Gold.Domain.Settings;
using System.Diagnostics;
using Gold.Core.Vendors;
using Gold.Core.GoldPrices;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Serilog;

namespace Gold.Domain
{
    public class VendorsDbContext : DbContext
    {
        public DbSet<VendorTransaction> VendorTransactions { get; set; }
        public DbSet<VendorTransactionNew> VendorTransactionsNew { get; set; }
        public DbSet<VenTransaction> VenTransactions { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorData> VendorDatas { get; set; }
        public DbSet<Gcode> Gcodes { get; set; }
        public DbSet<BannedIp2> BannedIps { get; set; }
        public DbSet<VendorConfirmation> VendorConfirmations { get; set; }
        public DbSet<VendorConfirmationNew> VendorConfirmationsNew { get; set; }
        public DbSet<ExpectedCash> ExpectedCashes { get; set; }
        public DbSet<VendorExpected> VendorExpecteds { get; set; }
        public DbSet<VendorCashPayment> VendorCashPayments { get; set; }
        public DbSet<VendorUnExpected> VendorUnExpecteds { get; set; }
        public DbSet<VendorFinalized> VendorFinalizeds { get; set; }
        public DbSet<VendorNotPosition> VendorNotPositions { get; set; }
        public DbSet<VendorNotPositionSell> VendorNotPositionSells { get; set; }
        public DbSet<UnexpectedCash> UnexpectedCashes { get; set; }
        public DbSet<FinalizedGold> FinalizedGolds { get; set; }
        public DbSet<NotPosGold> NotPosGolds { get; set; }
        public DbSet<NotPosGoldSell> NotPosGoldSells { get; set; }
        public DbSet<InterBankError> InterBankErrors { get; set; }
        public DbSet<GoldPrice> GoldPrices { get; set; }
        public DbSet<VendorPlatinBalance> VendorPlatinBalances { get; set; }
        public DbSet<UsedFirstLast> UsedFirstLasts { get; set; }

        /*
         This means that the configuration passed to AddDbContext will never be used. 
        If configuration is passed to AddDbContext, then 'VendorsDbContext'
        should declare a constructor that accepts a DbContextOptions<VendorsDbContext> and must pass it to the base constructor for DbContext.
         
         */

        //public VendorsDbContext() : base() { }
        public VendorsDbContext(DbContextOptions<VendorsDbContext> options) : base(options)
        {
           // base.OnModelCreating(Model.)
        }

        /*
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var s = ConnectionSettings.Cstring2;
            if (Debugger.IsAttached)
                s = ConnectionSettings.Cstring;
//            SqlServerDbContextOptionsBuilder x = new SqlServerDbContextOptionsBuilder(optionsBuilder);
            
            optionsBuilder.UseSqlServer(s);
                
            
        }*/
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Log.Debug("VENDORS_REPO: ON MODEL CREATING");
            modelBuilder.Entity<VendorTransactionNew>().ToTable("VendorTransactionsNew").HasKey(c => c.TransactionId);
            modelBuilder.Entity<VendorTransaction>().ToTable("VendorTransactions").HasKey(c => c.TransactionId);
            modelBuilder.Entity<VenTransaction>().ToTable("VenTransactions").HasKey(c => c.TransactionId);
            modelBuilder.Entity<Vendor>().ToTable("Vendors").HasKey(c => c.VendorId);
            modelBuilder.Entity<VendorData>().ToTable("VendorDatas").HasKey(c => c.VendorDataId);
            modelBuilder.Entity<Gcode>().ToTable("Gcodes").HasKey(c => c.GcodeId);
            modelBuilder.Entity<VendorConfirmation>().ToTable("VendorConfirmations").HasKey(c => c.ConfirmationId);
            modelBuilder.Entity<VendorConfirmationNew>().ToTable("VendorConfirmationsNew").HasKey(c => c.ConfirmationId);
            modelBuilder.Entity<BannedIp2>().ToTable("BannedIps").HasKey(x => x.BannedIpId);
            modelBuilder.Entity<UnexpectedCash>().ToTable("UnExpecteds").HasKey(x => x.UnExpectedId);
            modelBuilder.Entity<ExpectedCash>().ToTable("Expecteds").HasKey(c => c.ExpectedId);

            modelBuilder.Entity<VendorCashPayment>().ToTable("VendorCashPayments").HasKey(c => c.PaymentId);
            modelBuilder.Entity<VendorExpected>().ToTable("VendorExpecteds").HasKey(c => c.ExpectedId);
            modelBuilder.Entity<VendorFinalized>().ToTable("VendorFinalizeds").HasKey(c => c.FinalizedId);
            modelBuilder.Entity<VendorUnExpected>().ToTable("VendorUnExpecteds").HasKey(c => c.UnExpectedId);
            modelBuilder.Entity<VendorNotPosition>().ToTable("VendorNotPositions").HasKey(c => c.NotPosId);
            modelBuilder.Entity<VendorNotPositionSell>().ToTable("VendorNotPositionSells").HasKey(c => c.NotPosId);
            modelBuilder.Entity<FinalizedGold>().ToTable("FinalizedKTGolds").HasKey(c => c.FinalizedId);
            modelBuilder.Entity<NotPosGold>().ToTable("NotPosGolds").HasKey(c => c.NotPosId);
            modelBuilder.Entity<NotPosGoldSell>().ToTable("NotPosGoldSells").HasKey(c => c.NotPosId);
            modelBuilder.Entity<InterBankError>().ToTable("InterBankErrors").HasKey(c => c.IBanErrId);
            modelBuilder.Entity<UsedFirstLast>().ToTable("UsedFirstLasts").HasKey(c => c.FirstLastId);
            modelBuilder.Entity<GoldPrice>(x =>
            {
                x.ToTable("GoldPrices").HasKey(c => c.GoldPriceId);
                x.Property(y => y.Amount).HasColumnType("decimal(18, 2");
                x.Property(y => y.Percentage).HasColumnType("decimal(18, 3)");
            });
            modelBuilder.Entity<VendorPlatinBalance>().ToTable("VendorPlatinBalances").HasKey(c => c.VendorId);
            modelBuilder.Entity<VendorPlatinBalance>(x =>
            {
                x.ToTable("VendorPlatinBalances").HasKey(c => c.VendorId);
                x.Property(y => y.Balance).HasColumnType("decimal(18, 2)");
            });
            Log.Debug("VENDORS_REPO: ON MODEL CREATING - DONE");
        }
    }
}
