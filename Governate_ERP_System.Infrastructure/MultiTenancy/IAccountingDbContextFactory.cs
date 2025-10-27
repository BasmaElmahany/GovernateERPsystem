using Governate_ERP_System.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Governate_ERP_System.Infrastructure.Persistence;

namespace Governate_ERP_System.Infrastructure.MultiTenancy
{
    public interface IAccountingDbContextFactory
    {
        AccountingDbContext Create();
        AccountingDbContext CreateForSchema(string schema); // مفيد للـ Provisioning
    }

    public class AccountingDbContextFactory : IAccountingDbContextFactory
    {
        private readonly IConfiguration _cfg;
        private readonly ICurrentProjectAccessor _current;

        public AccountingDbContextFactory(IConfiguration cfg, ICurrentProjectAccessor current)
        {
            _cfg = cfg; _current = current;
        }

        public AccountingDbContext Create()
        {
            if (!_current.HasCurrent) throw new InvalidOperationException("No current project.");
            return CreateForSchema(_current.Current.Schema);
        }

        public AccountingDbContext CreateForSchema(string schema)
        {
            var opts = new DbContextOptionsBuilder<AccountingDbContext>()
                .UseSqlServer(_cfg.GetConnectionString("Catalog")) // نفس القاعدة
                .ReplaceService<IModelCacheKeyFactory, SchemaAwareModelCacheKeyFactory>()
                .Options;

            return new AccountingDbContext(opts, schema);
        }
    }
}
