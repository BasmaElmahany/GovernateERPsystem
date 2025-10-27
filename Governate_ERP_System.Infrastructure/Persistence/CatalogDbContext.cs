using Governate_ERP_System.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Infrastructure.Persistence
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }
        public DbSet<Project> Projects => Set<Project>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Project>(e =>
            {
                e.HasIndex(x => x.Code).IsUnique();
                e.Property(x => x.Code).HasMaxLength(32).IsRequired();
                e.Property(x => x.Name).HasMaxLength(256).IsRequired();
                e.Property(x => x.Schema).HasMaxLength(64).IsRequired(); // prj_CODE
                e.Property(x => x.RowVersion).IsRowVersion();
            });
        }
    }
}
