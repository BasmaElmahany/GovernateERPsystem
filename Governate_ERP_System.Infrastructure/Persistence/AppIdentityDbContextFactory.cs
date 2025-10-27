using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Infrastructure.Persistence
{
    public class AppIdentityDbContextFactory : IDesignTimeDbContextFactory<AppIdentityDbContext>
    {
        public AppIdentityDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppIdentityDbContext>();

            // ⚠️ ضع هنا نفس Connection String اللي تستخدمه في appsettings.json
            optionsBuilder.UseSqlServer("Server=Magic-Basma;Database=ERP_Main;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");

            return new AppIdentityDbContext(optionsBuilder.Options);
        }
    }
}
