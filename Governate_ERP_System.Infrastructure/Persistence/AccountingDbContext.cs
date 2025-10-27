using Governate_ERP_System.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Infrastructure.Persistence
{
    public class AccountingDbContext : DbContext
    {
        public string Schema { get; }

        public AccountingDbContext(DbContextOptions<AccountingDbContext> options, string schema)
            : base(options)
        {
            Schema = schema;
        }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
        public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
        public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
        public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
        public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // اجعل السكيما الافتراضي لسائر الجداول هو سكيما المشروع
            mb.HasDefaultSchema(Schema);

            mb.Entity<Account>(e =>
            {
                e.ToTable("Accounts");
                e.Property(p => p.Code).HasMaxLength(50).IsRequired();
                e.HasIndex(p => p.Code).IsUnique();
                e.Property(p => p.Name).HasMaxLength(200).IsRequired();
                e.Property(p => p.RowVersion).IsRowVersion();
                e.HasOne(p => p.ParentAccount).WithMany(p => p.Children).HasForeignKey(p => p.ParentAccountId);
            });

            mb.Entity<JournalEntry>(e =>
            {
                e.ToTable("JournalEntries");
                e.HasIndex(x => x.Number).IsUnique();
                e.Property(x => x.Number).HasMaxLength(30).IsRequired();
                e.Property(x => x.RowVersion).IsRowVersion();
            });

            mb.Entity<JournalEntryLine>(e =>
            {
                e.ToTable("JournalEntryLines");
                e.Property(p => p.Debit).HasPrecision(18, 2);
                e.Property(p => p.Credit).HasPrecision(18, 2);
                e.ToTable(tb => tb.HasCheckConstraint("CK_JournalLine_OneSide",
                    "(([Debit] = 0 AND [Credit] > 0) OR ([Credit] = 0 AND [Debit] > 0))"));
            });

            mb.Entity<LedgerEntry>(e =>
            {
                e.ToTable("LedgerEntries");
                e.Property(p => p.Debit).HasPrecision(18, 2);
                e.Property(p => p.Credit).HasPrecision(18, 2);
                e.HasIndex(p => new { p.AccountId, p.Date });
            });

            mb.Entity<FiscalYear>().ToTable("FiscalYears");
            mb.Entity<FiscalPeriod>().ToTable("FiscalPeriods");
        }
    }
}
