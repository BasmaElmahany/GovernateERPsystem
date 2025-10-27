using Governate_ERP_System.Application.Interfaces;
using Governate_ERP_System.Domain.Entities;
using Governate_ERP_System.Infrastructure.MultiTenancy;
using Governate_ERP_System.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Infrastructure.Services
{
    public class TenantCatalogService : ITenantCatalogService
    {
        private readonly CatalogDbContext _ctx;
        private readonly IProjectProvisioner _provisioner;

        public TenantCatalogService(CatalogDbContext ctx, IProjectProvisioner provisioner)
        {
            _ctx = ctx; _provisioner = provisioner;
        }

        public async Task<Project> CreateProjectAsync(string code, string name)
        {
            var schema = SchemaNameHelper.FromCode(code);
            var p = new Project { Code = code, Name = name, Schema = schema };
            _ctx.Projects.Add(p);
            await _ctx.SaveChangesAsync();

            // ينشئ السكيما والجداول داخل نفس القاعدة + يقوم بالـ Seeding
            await _provisioner.ProvisionAsync(p);
            return p;
        }

        public Task<Project?> GetByCodeAsync(string code) =>
            _ctx.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code);

        public async Task<IReadOnlyList<Project>> ListAsync() =>
            await _ctx.Projects.AsNoTracking().OrderBy(x => x.Code).ToListAsync();
    }
}
