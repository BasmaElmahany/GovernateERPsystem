using Governate_ERP_System.Application.Interfaces;
using Governate_ERP_System.Domain.Entities;
using Governate_ERP_System.Domain.Enums;
using Governate_ERP_System.Infrastructure.MultiTenancy;
using Governate_ERP_System.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Infrastructure.Services
{
    public class AccountingService : IAccountingService, IUnitOfWork
    {
        private readonly IAccountingDbContextFactory _factory;
        private readonly IJournalValidator _validator;

        public AccountingService(IAccountingDbContextFactory factory, IJournalValidator validator)
        {
            _factory = factory;
            _validator = validator;
        }

        private AccountingDbContext Ctx() => _factory.Create();

        public async Task<int> CreateAccountAsync(Account account)
        {
            using var ctx = Ctx();
            ctx.Accounts.Add(account);
            await ctx.SaveChangesAsync();
            return account.Id;
        }

        public async Task UpdateAccountAsync(Account account)
        {
            using var ctx = Ctx();
            ctx.Accounts.Update(account);
            await ctx.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Account>> GetChartOfAccountsAsync()
        {
            using var ctx = Ctx();
            return await ctx.Accounts.AsNoTracking().OrderBy(a => a.Code).ToListAsync();
        }

        public async Task<int> CreateJournalEntryAsync(JournalEntry entry)
        {
            using var ctx = Ctx();

            // ربط الحسابات من السياق للتأكد أنها موجودة/نشطة
            var accountIds = entry.Lines.Select(l => l.AccountId).Distinct().ToArray();
            var accounts = await ctx.Accounts.Where(a => accountIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id);
            foreach (var l in entry.Lines)
            {
                l.Account = accounts[l.AccountId];
            }

            _validator.EnsureBalanced(entry);
            _validator.EnsureAccountsActive(entry);

            entry.Number = await NextJournalNumberAsync(ctx);
            ctx.JournalEntries.Add(entry);
            await ctx.SaveChangesAsync();
            return entry.Id;
        }

        public async Task PostJournalEntryAsync(int entryId)
        {
            using var ctx = Ctx();
            var entry = await ctx.JournalEntries
                .Include(e => e.Lines).ThenInclude(l => l.Account)
                .FirstAsync(e => e.Id == entryId);

            if (entry.Status != EntryStatus.Draft) throw new InvalidOperationException("لا يمكن ترحيل قيد غير مسودة.");
            _validator.EnsureBalanced(entry);

            // إنشاء قيود الأستاذ (Ledger)
            foreach (var l in entry.Lines)
            {
                ctx.LedgerEntries.Add(new LedgerEntry
                {
                    Date = entry.Date,
                    AccountId = l.AccountId,
                    JournalNumber = entry.Number,
                    Debit = l.Debit,
                    Credit = l.Credit
                });
            }

            entry.Status = EntryStatus.Posted;
            await ctx.SaveChangesAsync();
        }

        public async Task ReverseJournalEntryAsync(int entryId)
        {
            using var ctx = Ctx();
            var entry = await ctx.JournalEntries.Include(e => e.Lines).FirstAsync(e => e.Id == entryId);
            if (entry.Status != EntryStatus.Posted) throw new InvalidOperationException("الرجوع مسموح فقط للقيود المرحلة.");

            var reverse = new JournalEntry
            {
                Date = DateTime.UtcNow.Date,
                Description = $"Reverse of {entry.Number}",
                Status = EntryStatus.Draft,
                Lines = entry.Lines.Select(l => new JournalEntryLine
                {
                    AccountId = l.AccountId,
                    Debit = l.Credit,
                    Credit = l.Debit,
                    LineDescription = "Reversal"
                }).ToList()
            };
            await CreateJournalEntryAsync(reverse);
        }

        public async Task<IReadOnlyList<JournalEntry>> ListJournalsAsync(DateTime? from = null, DateTime? to = null, EntryStatus? status = null)
        {
            using var ctx = Ctx();
            var q = ctx.JournalEntries.Include(e => e.Lines).AsQueryable();
            if (from.HasValue) q = q.Where(e => e.Date >= from);
            if (to.HasValue) q = q.Where(e => e.Date <= to);
            if (status.HasValue) q = q.Where(e => e.Status == status);
            return await q.AsNoTracking().OrderByDescending(e => e.Date).ThenBy(e => e.Number).ToListAsync();
        }

        public async Task<IReadOnlyList<LedgerRow>> GetLedgerAsync(int accountId, DateTime from, DateTime to)
        {
            using var ctx = Ctx();
            var items = await ctx.LedgerEntries
                .Where(l => l.AccountId == accountId && l.Date >= from && l.Date <= to)
                .OrderBy(l => l.Date).ThenBy(l => l.Id)
                .Select(l => new { l.Date, l.JournalNumber, l.Debit, l.Credit })
                .ToListAsync();

            decimal running = 0;
            var rows = new List<LedgerRow>();
            foreach (var it in items)
            {
                running += it.Debit - it.Credit; // للحسابات ذات الطبيعة المدينة؛ اضبط عند العرض حسب NormalSide
                rows.Add(new LedgerRow(it.Date, it.JournalNumber, it.Debit, it.Credit, running));
            }
            return rows;
        }

        public async Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(DateTime asOf)
        {
            using var ctx = Ctx();

            var q = from l in ctx.JournalEntryLines
                    where l.JournalEntry.Status == EntryStatus.Posted && l.JournalEntry.Date <= asOf
                    group l by new { l.AccountId, l.Account.Code, l.Account.Name, l.Account.NormalSide } into g
                    select new
                    {
                        g.Key.AccountId,
                        g.Key.Code,
                        g.Key.Name,
                        g.Key.NormalSide,
                        Debit = g.Sum(x => x.Debit),
                        Credit = g.Sum(x => x.Credit)
                    };

            var raw = await q.ToListAsync();

            return raw.Select(r =>
            {
                var balance = r.NormalSide == NormalSide.Debit
                    ? r.Debit - r.Credit
                    : r.Credit - r.Debit;
                return new TrialBalanceRow(r.Code, r.Name, r.Debit, r.Credit, balance);
            })
            .OrderBy(r => r.AccountCode)
            .ToList();
        }

        public async Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime asOf)
        {
            using var ctx = Ctx();
            var tb = await GetTrialBalanceAsync(asOf);
            var assets = tb.Where(x => x.AccountCode.StartsWith("1"));      // أبسط تصنيف؛ عدّل حسب دليل حساباتك
            var liabilities = tb.Where(x => x.AccountCode.StartsWith("2"));
            var equity = tb.Where(x => x.AccountCode.StartsWith("3"));
            return new BalanceSheetDto(assets.ToList(), liabilities.ToList(), equity.ToList());
        }

        public async Task<IncomeStatementDto> GetIncomeStatementAsync(DateTime from, DateTime to)
        {
            using var ctx = Ctx();
            var q = from l in ctx.JournalEntryLines
                    where l.JournalEntry.Status == EntryStatus.Posted
                       && l.JournalEntry.Date >= @from && l.JournalEntry.Date <= to
                    group l by new { l.Account.Code, l.Account.Name, l.Account.Type, l.Account.NormalSide } into g
                    select new
                    {
                        g.Key.Code,
                        g.Key.Name,
                        g.Key.Type,
                        g.Key.NormalSide,
                        Debit = g.Sum(x => x.Debit),
                        Credit = g.Sum(x => x.Credit)
                    };

            var raw = await q.ToListAsync();

            decimal ToSigned(NormalSide n, decimal d, decimal c) => n == NormalSide.Debit ? d - c : c - d;

            var revenues = raw.Where(r => r.Type == AccountType.Revenue)
                              .Select(r => new TrialBalanceRow(r.Code, r.Name, r.Debit, r.Credit, ToSigned(r.NormalSide, r.Debit, r.Credit)))
                              .ToList();

            var expenses = raw.Where(r => r.Type == AccountType.Expense)
                              .Select(r => new TrialBalanceRow(r.Code, r.Name, r.Debit, r.Credit, ToSigned(r.NormalSide, r.Debit, r.Credit)))
                              .ToList();

            var netIncome = revenues.Sum(x => x.Balance) - expenses.Sum(x => x.Balance);
            return new IncomeStatementDto(revenues, expenses, netIncome);
        }

        public async Task ClosePeriodAsync(int periodId)
        {
            using var ctx = Ctx();
            var p = await ctx.FiscalPeriods.Include(x => x.FiscalYear).FirstAsync(x => x.Id == periodId);
            if (p.IsClosed) return;

            // قيد إقفال مبسط: نقل صافي الدخل إلى الأرباح المحتجزة (3100)
            var isDto = await GetIncomeStatementAsync(p.Start, p.End);
            if (isDto.NetIncome != 0)
            {
                var reAccount = await ctx.Accounts.FirstAsync(a => a.Code == "3100");
                var entry = new JournalEntry
                {
                    Date = p.End,
                    Description = "Period closing",
                    Status = EntryStatus.Draft,
                    Lines = new List<JournalEntryLine>()
                };

                if (isDto.NetIncome > 0)
                {
                    // أرباح: اقفل الإيرادات (مدين) واقفل إلى الأرباح المحتجزة (دائن)
                    entry.Lines.Add(new JournalEntryLine { AccountId = reAccount.Id, Debit = 0, Credit = isDto.NetIncome });
                }
                else
                {
                    // خسائر
                    entry.Lines.Add(new JournalEntryLine { AccountId = reAccount.Id, Debit = Math.Abs(isDto.NetIncome), Credit = 0 });
                }
                await CreateJournalEntryAsync(entry);
                await PostJournalEntryAsync(entry.Id);
            }

            p.IsClosed = true;
            await ctx.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);

        private static async Task<string> NextJournalNumberAsync(AccountingDbContext ctx)
        {
            // مثال بسيط: YYYYMM-#####
            var prefix = DateTime.UtcNow.ToString("yyyyMM");
            var last = await ctx.JournalEntries
                .Where(j => j.Number.StartsWith(prefix))
                .OrderByDescending(j => j.Number)
                .Select(j => j.Number)
                .FirstOrDefaultAsync();

            int seq = 1;
            if (!string.IsNullOrEmpty(last) && int.TryParse(last.Split('-').Last(), out var lastNum)) seq = lastNum + 1;
            return $"{prefix}-{seq:D5}";
        }
    }
}
