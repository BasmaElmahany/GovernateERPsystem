using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Infrastructure.Persistence
{
    public class SchemaAwareModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
        {
            if (context is AccountingDbContext ac)
                return (context.GetType(), ac.Schema, designTime);
            return (context.GetType(), designTime);
        }
    }
}
