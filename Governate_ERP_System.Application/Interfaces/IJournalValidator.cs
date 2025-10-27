using Governate_ERP_System.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Application.Interfaces
{
    // التحقق من سلامة القيد (مزدوج القيد)
    public interface IJournalValidator
    {
        void EnsureBalanced(JournalEntry entry); // مجموع المدين = مجموع الدائن
        void EnsureAccountsActive(JournalEntry entry);
        void EnsurePeriodOpen(JournalEntry entry);
    }

}
