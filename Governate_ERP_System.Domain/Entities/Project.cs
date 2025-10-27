using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Domain.Entities
{
    public class Project : BaseEntity
    {
        public string Code { get; set; } = default!;   // فريد، مثل ACME أو P2025
        public string Name { get; set; } = default!;
        public string Schema { get; set; } = default!; // مثل prj_ACME
        public bool IsActive { get; set; } = true;
    }
}
