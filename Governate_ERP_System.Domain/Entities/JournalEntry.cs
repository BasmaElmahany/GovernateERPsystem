using Governate_ERP_System.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Domain.Entities
{
    public class JournalEntry : BaseEntity
    {
        public string Number { get; set; } = default!;
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public EntryStatus Status { get; set; } = EntryStatus.Draft;
        public int? PeriodId { get; set; }
        public FiscalPeriod? Period { get; set; }
        public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    }
}
