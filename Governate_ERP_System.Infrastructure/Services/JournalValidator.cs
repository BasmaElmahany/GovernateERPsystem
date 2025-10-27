using Governate_ERP_System.Application.Interfaces;
using Governate_ERP_System.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Infrastructure.Services
{
    public class JournalValidator : IJournalValidator
    {
        public void EnsureBalanced(JournalEntry entry)
        {
            var debit = entry.Lines.Sum(l => l.Debit);
            var credit = entry.Lines.Sum(l => l.Credit);
            if (debit != credit) throw new InvalidOperationException("القيد غير متوازن: مجموع المدين لا يساوي مجموع الدائن.");
        }

        public void EnsureAccountsActive(JournalEntry entry)
        {
            if (entry.Lines.Any(l => !l.Account.IsActive))
                throw new InvalidOperationException("لا يمكن استخدام حساب غير نشط.");
        }

        public void EnsurePeriodOpen(JournalEntry entry)
        {
            if (entry.Period is { IsClosed: true })
                throw new InvalidOperationException("لا يمكن الترحيل لفترة مغلَقة.");
        }
    }
}
