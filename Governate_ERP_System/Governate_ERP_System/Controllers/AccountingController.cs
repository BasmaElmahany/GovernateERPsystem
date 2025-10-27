using Governate_ERP_System.Application.Interfaces;
using Governate_ERP_System.Domain.Entities;
using Governate_ERP_System.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Governate_ERP_System.Controllers
{
    public class AccountingController : Controller
    {
        private readonly IAccountingService _svc;

        public AccountingController(IAccountingService svc) => _svc = svc;

        // 1) عرض دليل الحسابات
        // GET: /Accounting/ChartOfAccounts?p=PRJ1
        public async Task<IActionResult> ChartOfAccounts()
        {
            var accounts = await _svc.GetChartOfAccountsAsync();
            return View(accounts);
        }

        // 2) إنشاء حساب
        // GET: /Accounting/CreateAccount
        public IActionResult CreateAccount() => View();

        // POST: /Accounting/CreateAccount
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromForm] Account model)
        {
            if (!ModelState.IsValid) return View(model);
            await _svc.CreateAccountAsync(model);
            return RedirectToAction(nameof(ChartOfAccounts), new { p = Request.Query["p"] });
        }

        // 3) إنشاء قيد يومية (مسودة)
        // GET: /Accounting/NewJournal
        public IActionResult NewJournal() => View(new JournalEntry { Date = DateTime.Today });

        // POST: /Accounting/NewJournal
        [HttpPost]
        public async Task<IActionResult> NewJournal([FromForm] JournalEntry entry)
        {
            if (entry.Lines == null || entry.Lines.Count < 2)
            {
                ModelState.AddModelError("", "يجب أن يحتوي القيد على سطرين على الأقل.");
                return View(entry);
            }
            var id = await _svc.CreateJournalEntryAsync(entry);
            return RedirectToAction(nameof(JournalList), new { p = Request.Query["p"] });
        }

        // 4) ترحيل قيد
        // POST: /Accounting/PostJournal/5
        [HttpPost]
        public async Task<IActionResult> PostJournal(int id)
        {
            await _svc.PostJournalEntryAsync(id);
            return RedirectToAction(nameof(JournalList), new { p = Request.Query["p"] });
        }

        // 5) قائمة القيود (مع فلاتر)
        // GET: /Accounting/JournalList?from=2025-01-01&to=2025-12-31&status=Posted
        public async Task<IActionResult> JournalList(DateTime? from, DateTime? to, EntryStatus? status)
        {
            var items = await _svc.ListJournalsAsync(from, to, status);
            return View(items);
        }

        // 6) دفتر الأستاذ لحساب محدد
        // GET: /Accounting/Ledger?accountId=10&from=2025-01-01&to=2025-12-31
        public async Task<IActionResult> Ledger(int accountId, DateTime from, DateTime to)
        {
            var rows = await _svc.GetLedgerAsync(accountId, from, to);
            return View(rows);
        }

        // 7) ميزان المراجعة حتى تاريخ
        // GET: /Accounting/TrialBalance?asOf=2025-12-31
        public async Task<IActionResult> TrialBalance(DateTime asOf)
        {
            var rows = await _svc.GetTrialBalanceAsync(asOf);
            return View(rows);
        }

        // 8) الميزانية (قائمة المركز المالي)
        // GET: /Accounting/BalanceSheet?asOf=2025-12-31
        public async Task<IActionResult> BalanceSheet(DateTime asOf)
        {
            var dto = await _svc.GetBalanceSheetAsync(asOf);
            return View(dto);
        }

        // 9) قائمة الدخل لفترة
        // GET: /Accounting/IncomeStatement?from=2025-01-01&to=2025-12-31
        public async Task<IActionResult> IncomeStatement(DateTime from, DateTime to)
        {
            var dto = await _svc.GetIncomeStatementAsync(from, to);
            return View(dto);
        }

        // 10) إقفال فترة مالية
        // POST: /Accounting/ClosePeriod/12
        [HttpPost]
        public async Task<IActionResult> ClosePeriod(int id)
        {
            await _svc.ClosePeriodAsync(id);
            return RedirectToAction(nameof(JournalList), new { p = Request.Query["p"] });
        }

        // 11) عكس قيد
        // POST: /Accounting/ReverseEntry/5
        [HttpPost]
        public async Task<IActionResult> ReverseEntry(int id)
        {
            await _svc.ReverseJournalEntryAsync(id);
            return RedirectToAction(nameof(JournalList), new { p = Request.Query["p"] });
        }
    }
}
