using Governate_ERP_System.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Application.Interfaces
{ // يحدد المشروع الحالي (التينانت)
    public interface ICurrentProjectAccessor
    {
        Project Current { get; }
        void Set(Project project);
        bool HasCurrent { get; }
    }
}
