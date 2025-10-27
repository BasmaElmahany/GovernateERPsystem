using Governate_ERP_System.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Application.Interfaces
{// كتالوج المشاريع (في قاعدة رئيسية)
    public interface ITenantCatalogService
    {
        Task<Project> CreateProjectAsync(string code, string name);
        Task<Project?> GetByCodeAsync(string code);
        Task<IReadOnlyList<Project>> ListAsync();
    }
}
