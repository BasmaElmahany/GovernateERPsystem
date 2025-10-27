using Governate_ERP_System.Domain.Entities;
using Governate_ERP_System.Domain.Enums;

namespace Governate_ERP_System.Application.Interfaces
{
    // خدمات المحاسبة (Use‑Cases)
    public interface IAccountingService
    {
        // الدليل
        Task<int> CreateAccountAsync(Account account);
        Task UpdateAccountAsync(Account account);
        Task<IReadOnlyList<Account>> GetChartOfAccountsAsync();

        // القيود اليومية
        Task<int> CreateJournalEntryAsync(JournalEntry entry);
        Task PostJournalEntryAsync(int entryId);  // ترحيل
        Task ReverseJournalEntryAsync(int entryId);
        Task<IReadOnlyList<JournalEntry>> ListJournalsAsync(DateTime? from = null, DateTime? to = null, EntryStatus? status = null);

        // الأستاذ، ميزان المراجعة، القوائم
        Task<IReadOnlyList<LedgerRow>> GetLedgerAsync(int accountId, DateTime from, DateTime to);
        Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(DateTime asOf);
        Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime asOf);
        Task<IncomeStatementDto> GetIncomeStatementAsync(DateTime from, DateTime to);

        // الفترات
        Task ClosePeriodAsync(int periodId);
    }

    // نماذج إرجاع للتقارير
    public record LedgerRow(DateTime Date, string JournalNumber, decimal Debit, decimal Credit, decimal RunningBalance);
    public record TrialBalanceRow(string AccountCode, string AccountName, decimal Debit, decimal Credit, decimal Balance);
    public record BalanceSheetDto(IReadOnlyList<TrialBalanceRow> Assets, IReadOnlyList<TrialBalanceRow> Liabilities, IReadOnlyList<TrialBalanceRow> Equity);
    public record IncomeStatementDto(IReadOnlyList<TrialBalanceRow> Revenues, IReadOnlyList<TrialBalanceRow> Expenses, decimal NetIncome);

}
