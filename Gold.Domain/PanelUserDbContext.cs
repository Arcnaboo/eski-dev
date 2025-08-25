using Gold.Core.PanelUsers;
using Gold.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Gold.Domain
{
    public class PanelUserDbContext : DbContext
    {
        public DbSet<PanelUser> PanelUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var s = ConnectionSettings.Cstring2;
            if (Debugger.IsAttached)
                s = ConnectionSettings.Cstring;

            optionsBuilder.UseSqlServer(s);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<PanelUser>().ToTable("PanelUsers").HasKey(c => c.PanelUserId);
        }
    }
}
