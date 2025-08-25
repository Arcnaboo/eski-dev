using Gold.Core.Version2;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Domain
{
    public class Version2DbContext : DbContext
    {
        public DbSet<RegisteredUser> RegisteredUsers { get; set; }
        public DbSet<UserRegistration> UserRegistrations { get; set; }
        
    }
}
