using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Domain.Entities
{
     public class LedgerEntry : BaseEntity
    {
        public DateTime Date { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; } = default!;
        public string JournalNumber { get; set; } = default!;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}
