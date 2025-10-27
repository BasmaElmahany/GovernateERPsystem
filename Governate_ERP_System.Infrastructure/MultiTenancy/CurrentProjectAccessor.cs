using Governate_ERP_System.Application.Interfaces;
using Governate_ERP_System.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Infrastructure.MultiTenancy
{
    public class CurrentProjectAccessor : ICurrentProjectAccessor
    {
        public Project Current { get; private set; } = default!;
        public bool HasCurrent { get; private set; }

        public void Set(Project project)
        {
            Current = project;
            HasCurrent = true;
        }
    }
}
