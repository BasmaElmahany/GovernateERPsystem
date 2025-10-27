using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Domain.Entities
{
    public class FiscalYear : BaseEntity
    {
        public int Year { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsClosed { get; set; }
        public ICollection<FiscalPeriod> Periods { get; set; } = new List<FiscalPeriod>();
    }
}
