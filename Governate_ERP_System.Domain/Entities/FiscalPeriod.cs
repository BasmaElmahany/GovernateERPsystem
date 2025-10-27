using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Domain.Entities
{

    public class FiscalPeriod : BaseEntity
    {
        public int Number { get; set; } // 1..12
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsClosed { get; set; }
        public int FiscalYearId { get; set; }
        public FiscalYear FiscalYear { get; set; } = default!;
    }
}
