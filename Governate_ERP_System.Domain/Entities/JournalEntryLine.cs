using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Domain.Entities
{

    public class JournalEntryLine : BaseEntity
    {
        public int JournalEntryId { get; set; }
        public JournalEntry JournalEntry { get; set; } = default!;
        public int AccountId { get; set; }
        public Account Account { get; set; } = default!;
        public decimal Debit { get; set; }    // precision set in EF
        public decimal Credit { get; set; }
        public string? LineDescription { get; set; }
        public string? Reference { get; set; }
    }
}
