using Governate_ERP_System.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Infrastructure.Persistence
{
    public class AppIdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // All Identity tables go to [auth] schema
            builder.HasDefaultSchema("auth");
            base.OnModelCreating(builder);

            // Example: make emails unique (Identity already enforces normalized)
            builder.Entity<ApplicationUser>().HasIndex(u => u.NormalizedEmail);
        }
    }
}
