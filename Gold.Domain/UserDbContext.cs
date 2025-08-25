using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Gold.Core.Users;
using Gold.Domain.Settings;
using System.Diagnostics;

namespace Gold.Domain
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ForgotPassword> ForgotPasswords { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<InternalLog> Logs { get; set; }
        public DbSet<KimlikInfo> KimlikInfos { get; set; }
        public DbSet<ProfileChange> ProfileChanges { get; set; }
        public DbSet<BannedIp> BannedIps { get; set; }
        public DbSet<Cekilis> Cekilis { get; set; }
        public DbSet<CekilisHak> CekilisHaks { get; set; }
        public DbSet<SilverBalance> SilverBalances { get; set; }
        public DbSet<UserLevel> UserLevels { get; set; }
        public DbSet<ReferansCode> ReferansCodes { get; set; }
        public DbSet<UserRef> UserRefs { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var s = ConnectionSettings.Cstring2;
            if (Debugger.IsAttached)
                s = ConnectionSettings.Cstring;

            optionsBuilder.UseSqlServer(s);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>().ToTable("Users").HasKey(c => c.UserId);
            modelBuilder.Entity<Notification>().ToTable("Notifications").HasKey(c => c.NotificationId);
            modelBuilder.Entity<Notification>().HasOne(x => x.User).WithMany().HasForeignKey(n => n.UserId);
            modelBuilder.Entity<ForgotPassword>().ToTable("ForgotPasswords").HasKey(x => x.ForgotId);
            modelBuilder.Entity<ForgotPassword>().HasOne(x => x.User).WithMany().HasForeignKey(n => n.UserId);
            modelBuilder.Entity<KimlikInfo>().ToTable("KimlikInfos").HasKey(x => x.KimlikInfoId);
            modelBuilder.Entity<Login>().ToTable("Logins").HasKey(x => x.LoginId);
            modelBuilder.Entity<Login>().HasOne(x => x.User).WithMany().HasForeignKey(n => n.UserId);
            modelBuilder.Entity<ProfileChange>().ToTable("ProfileChanges").HasKey(x => x.ChangeId);
            modelBuilder.Entity<ProfileChange>().HasOne(x => x.User).WithMany().HasForeignKey(n => n.UserId);
            modelBuilder.Entity<InternalLog>().ToTable("Logs").HasKey(x => x.LogId);
            modelBuilder.Entity<InternalLog>().HasOne(z => z.User).WithMany().HasForeignKey(n => n.UserId);
            modelBuilder.Entity<BannedIp>().ToTable("BannedIps").HasKey(x => x.BannedIpId);
            modelBuilder.Entity<Cekilis>().ToTable("Cekilis").HasKey(x => x.CekilisId);
            modelBuilder.Entity<CekilisHak>().ToTable("CekilisHaks").HasKey(x => x.HakId);
            modelBuilder.Entity<SilverBalance>().ToTable("SilverBalances").HasKey(x => x.SilverId);
            modelBuilder.Entity<UserLevel>().ToTable("UserLevels").HasKey(x => x.LevelId);
            modelBuilder.Entity<ReferansCode>().ToTable("ReferansCodes").HasKey(x => x.RefCodeId);
            modelBuilder.Entity<UserRef>().ToTable("UserRefs").HasKey(x => x.UserRefId);

        }
    }
}
