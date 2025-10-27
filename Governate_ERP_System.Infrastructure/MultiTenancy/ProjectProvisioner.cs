using Governate_ERP_System.Application.Interfaces;
using Governate_ERP_System.Domain.Entities;
using Governate_ERP_System.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Infrastructure.MultiTenancy
{
    public class ProjectProvisioner : IProjectProvisioner
    {
        private readonly CatalogDbContext _catalog;
        private readonly IAccountingDbContextFactory _factory;

        public ProjectProvisioner(CatalogDbContext catalog, IAccountingDbContextFactory factory)
        {
            _catalog = catalog; _factory = factory;
        }

        public async Task ProvisionAsync(Project project)
        {
            var schema = project.Schema;

            // 1) إنشاء السكيما + الجداول إن لم تكن موجودة
            await _catalog.Database.ExecuteSqlRawAsync($@"
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'{schema}')
    EXEC(N'CREATE SCHEMA [{schema}]');

-- Accounts
IF NOT EXISTS (
  SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
  WHERE t.name = N'Accounts' AND s.name = N'{schema}')
BEGIN
EXEC(N'
CREATE TABLE [{schema}].[Accounts](
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [Code] NVARCHAR(50) NOT NULL,
  [Name] NVARCHAR(200) NOT NULL,
  [Type] INT NOT NULL,
  [NormalSide] INT NOT NULL,
  [ParentAccountId] INT NULL,
  [Level] INT NOT NULL DEFAULT 1,
  [IsLeaf] BIT NOT NULL DEFAULT 1,
  [IsActive] BIT NOT NULL DEFAULT 1,
  [CreatedAtUtc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [UpdatedAtUtc] DATETIME2 NULL,
  [RowVersion] ROWVERSION NOT NULL
);
CREATE UNIQUE INDEX IX_{schema}_Accounts_Code ON [{schema}].[Accounts]([Code]);
ALTER TABLE [{schema}].[Accounts]
ADD CONSTRAINT FK_{schema}_Accounts_Parent
FOREIGN KEY([ParentAccountId]) REFERENCES [{schema}].[Accounts]([Id]);
');
END

-- FiscalYears
IF NOT EXISTS (
  SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
  WHERE t.name = N'FiscalYears' AND s.name = N'{schema}')
BEGIN
EXEC(N'
CREATE TABLE [{schema}].[FiscalYears](
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [Year] INT NOT NULL,
  [Start] DATE NOT NULL,
  [End] DATE NOT NULL,
  [IsClosed] BIT NOT NULL,
  [CreatedAtUtc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [UpdatedAtUtc] DATETIME2 NULL,
  [RowVersion] ROWVERSION NOT NULL
);
');
END

-- FiscalPeriods
IF NOT EXISTS (
  SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
  WHERE t.name = N'FiscalPeriods' AND s.name = N'{schema}')
BEGIN
EXEC(N'
CREATE TABLE [{schema}].[FiscalPeriods](
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [Number] INT NOT NULL,
  [Start] DATE NOT NULL,
  [End] DATE NOT NULL,
  [IsClosed] BIT NOT NULL,
  [FiscalYearId] INT NOT NULL,
  [CreatedAtUtc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [UpdatedAtUtc] DATETIME2 NULL,
  [RowVersion] ROWVERSION NOT NULL,
  CONSTRAINT FK_{schema}_Periods_Year
    FOREIGN KEY([FiscalYearId]) REFERENCES [{schema}].[FiscalYears]([Id]) ON DELETE CASCADE
);
');
END

-- JournalEntries
IF NOT EXISTS (
  SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
  WHERE t.name = N'JournalEntries' AND s.name = N'{schema}')
BEGIN
EXEC(N'
CREATE TABLE [{schema}].[JournalEntries](
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [Number] NVARCHAR(30) NOT NULL,
  [Date] DATE NOT NULL,
  [Description] NVARCHAR(400) NULL,
  [Status] INT NOT NULL,
  [PeriodId] INT NULL,
  [CreatedAtUtc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [UpdatedAtUtc] DATETIME2 NULL,
  [RowVersion] ROWVERSION NOT NULL
);
CREATE UNIQUE INDEX IX_{schema}_Journals_Number ON [{schema}].[JournalEntries]([Number]);
ALTER TABLE [{schema}].[JournalEntries]
ADD CONSTRAINT FK_{schema}_Journals_Period
FOREIGN KEY([PeriodId]) REFERENCES [{schema}].[FiscalPeriods]([Id]);
');
END

-- JournalEntryLines
IF NOT EXISTS (
  SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
  WHERE t.name = N'JournalEntryLines' AND s.name = N'{schema}')
BEGIN
EXEC(N'
CREATE TABLE [{schema}].[JournalEntryLines](
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [JournalEntryId] INT NOT NULL,
  [AccountId] INT NOT NULL,
  [Debit] DECIMAL(18,2) NOT NULL DEFAULT 0,
  [Credit] DECIMAL(18,2) NOT NULL DEFAULT 0,
  [LineDescription] NVARCHAR(200) NULL,
  [Reference] NVARCHAR(50) NULL,
  [CreatedAtUtc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [UpdatedAtUtc] DATETIME2 NULL,
  [RowVersion] ROWVERSION NOT NULL,
  CONSTRAINT CK_{schema}_JournalLine_OneSide
    CHECK (([Debit] = 0 AND [Credit] > 0) OR ([Credit] = 0 AND [Debit] > 0)),
  CONSTRAINT FK_{schema}_Lines_Journal
    FOREIGN KEY([JournalEntryId]) REFERENCES [{schema}].[JournalEntries]([Id]) ON DELETE CASCADE,
  CONSTRAINT FK_{schema}_Lines_Account
    FOREIGN KEY([AccountId]) REFERENCES [{schema}].[Accounts]([Id])
);
');
END

-- LedgerEntries
IF NOT EXISTS (
  SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
  WHERE t.name = N'LedgerEntries' AND s.name = N'{schema}')
BEGIN
EXEC(N'
CREATE TABLE [{schema}].[LedgerEntries](
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [Date] DATE NOT NULL,
  [AccountId] INT NOT NULL,
  [JournalNumber] NVARCHAR(30) NOT NULL,
  [Debit] DECIMAL(18,2) NOT NULL DEFAULT 0,
  [Credit] DECIMAL(18,2) NOT NULL DEFAULT 0,
  [CreatedAtUtc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [UpdatedAtUtc] DATETIME2 NULL,
  [RowVersion] ROWVERSION NOT NULL,
  CONSTRAINT FK_{schema}_Ledger_Account
    FOREIGN KEY([AccountId]) REFERENCES [{schema}].[Accounts]([Id])
);
CREATE INDEX IX_{schema}_Ledger_AccountDate ON [{schema}].[LedgerEntries]([AccountId],[Date]);
');
END
");

            // 2) Seed (بـ EF بعد إنشاء الجداول)
            using var ctx = _factory.CreateForSchema(schema);

            if (!await ctx.FiscalYears.AnyAsync())
            {
                var fy = new FiscalYear
                {
                    Year = DateTime.UtcNow.Year,
                    Start = new DateTime(DateTime.UtcNow.Year, 1, 1),
                    End = new DateTime(DateTime.UtcNow.Year, 12, 31),
                    IsClosed = false
                };
                for (int i = 1; i <= 12; i++)
                {
                    var start = new DateTime(fy.Year, i, 1);
                    var end = start.AddMonths(1).AddDays(-1);
                    fy.Periods.Add(new FiscalPeriod { Number = i, Start = start, End = end, IsClosed = false });
                }
                ctx.FiscalYears.Add(fy);
            }

            if (!await ctx.Accounts.AnyAsync())
            {
                ctx.Accounts.AddRange(DefaultChartOfAccounts());
            }

            await ctx.SaveChangesAsync();
        }

        private static IEnumerable<Account> DefaultChartOfAccounts()
        {
            var assets = new Account { Code = "1000", Name = "الأصول", Type = Domain.Enums.AccountType.Asset, NormalSide = Domain.Enums.NormalSide.Debit, Level = 1, IsLeaf = false };
            var cash = new Account { Code = "1100", Name = "الصندوق", Type = Domain.Enums.AccountType.Asset, NormalSide = Domain.Enums.NormalSide.Debit, ParentAccount = assets, Level = 2, IsLeaf = true };

            var liabilities = new Account { Code = "2000", Name = "الخصوم", Type = Domain.Enums.AccountType.Liability, NormalSide = Domain.Enums.NormalSide.Credit, Level = 1, IsLeaf = false };
            var ap = new Account { Code = "2100", Name = "الدائنون", Type = Domain.Enums.AccountType.Liability, NormalSide = Domain.Enums.NormalSide.Credit, ParentAccount = liabilities, Level = 2, IsLeaf = true };

            var equity = new Account { Code = "3000", Name = "حقوق الملكية", Type = Domain.Enums.AccountType.Equity, NormalSide = Domain.Enums.NormalSide.Credit, Level = 1, IsLeaf = false };
            var re = new Account { Code = "3100", Name = "الأرباح المحتجزة", Type = Domain.Enums.AccountType.Equity, NormalSide = Domain.Enums.NormalSide.Credit, ParentAccount = equity, Level = 2, IsLeaf = true };

            var revenue = new Account { Code = "4000", Name = "الإيرادات", Type = Domain.Enums.AccountType.Revenue, NormalSide = Domain.Enums.NormalSide.Credit, Level = 1, IsLeaf = false };
            var sales = new Account { Code = "4100", Name = "مبيعات", Type = Domain.Enums.AccountType.Revenue, NormalSide = Domain.Enums.NormalSide.Credit, ParentAccount = revenue, Level = 2, IsLeaf = true };

            var expense = new Account { Code = "5000", Name = "المصروفات", Type = Domain.Enums.AccountType.Expense, NormalSide = Domain.Enums.NormalSide.Debit, Level = 1, IsLeaf = false };
            var opex = new Account { Code = "5100", Name = "مصروفات تشغيل", Type = Domain.Enums.AccountType.Expense, NormalSide = Domain.Enums.NormalSide.Debit, ParentAccount = expense, Level = 2, IsLeaf = true };

            return new[] { assets, cash, liabilities, ap, equity, re, revenue, sales, expense, opex };
        }
    }
}
