using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence;
using Patsanstha.Modules.Accounting.Application.Abstractions;
using Patsanstha.Modules.Accounting.Domain.Entities;
using Patsanstha.Modules.Accounting.Domain.Enums;

namespace Patsanstha.Modules.Accounting.Infrastructure.Persistence;

public sealed class AccountingDbContext(
    DbContextOptions<AccountingDbContext> options,
    IAuditContextAccessor auditContextAccessor)
    : ModuleDbContextBase(options, auditContextAccessor)
{
    protected override string SchemaName => Schema;

    public const string Schema = "accounting";

    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    protected override void ConfigureModule(ModelBuilder modelBuilder)
    {
        ConfigureAuditableEntity<JournalEntry>(modelBuilder);

        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("journal_entries");

            entity.Property(e => e.EntryNumber).HasMaxLength(32).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.DebitAccountCode).HasMaxLength(32).IsRequired();
            entity.Property(e => e.CreditAccountCode).HasMaxLength(32).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(e => e.ReferenceType).HasMaxLength(64);

            entity.HasIndex(e => new { e.TenantId, e.EntryNumber })
                .IsUnique()
                .HasDatabaseName("ux_accounting_tenant_entry_number");

            entity.HasIndex(e => new { e.TenantId, e.Status })
                .HasDatabaseName("ix_accounting_tenant_status");

            entity.HasIndex(e => new { e.TenantId, e.EntryDate })
                .HasDatabaseName("ix_accounting_tenant_entry_date");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("ck_accounting_amount_positive", "\"Amount\" > 0");
                t.HasCheckConstraint(
                    "ck_accounting_status",
                    "\"Status\" IN ('Draft','Posted','Reversed')");
            });
        });
    }
}

public sealed class JournalEntryRepository(AccountingDbContext dbContext) : IJournalEntryRepository
{
    public Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        dbContext.JournalEntries.Add(entry);
        return Task.CompletedTask;
    }

    public Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.JournalEntries.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<JournalEntry> Items, int TotalCount)> ListAsync(
        ListJournalEntriesCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.JournalEntries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = criteria.Search.Trim().ToLowerInvariant();
            query = query.Where(e =>
                e.EntryNumber.ToLower().Contains(search) ||
                e.Description.ToLower().Contains(search) ||
                e.DebitAccountCode.ToLower().Contains(search) ||
                e.CreditAccountCode.ToLower().Contains(search));
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(e => e.Status == criteria.Status.Value);
        }

        if (criteria.FromDate.HasValue)
        {
            query = query.Where(e => e.EntryDate >= criteria.FromDate.Value);
        }

        if (criteria.ToDate.HasValue)
        {
            query = query.Where(e => e.EntryDate <= criteria.ToDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

public sealed class EntryNumberGenerator(
    AccountingDbContext dbContext,
    IAuditContextAccessor auditContextAccessor) : IEntryNumberGenerator
{
    public async Task<string> GenerateNextAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = auditContextAccessor.Current.TenantId;
        var year = DateTime.UtcNow.Year;
        var prefix = $"JE-{year}-";

        var latestNumber = await dbContext.JournalEntries
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.EntryNumber.StartsWith(prefix))
            .OrderByDescending(e => e.EntryNumber)
            .Select(e => e.EntryNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;

        if (latestNumber is not null && latestNumber.Length > prefix.Length)
        {
            _ = int.TryParse(latestNumber[prefix.Length..], out sequence);
            sequence++;
        }

        return $"{prefix}{sequence:D5}";
    }
}

public sealed class JournalEntryMapper : IJournalEntryMapper
{
    public JournalEntrySummaryDto ToSummary(JournalEntry entry) =>
        new(
            entry.Id,
            entry.EntryNumber,
            entry.Description,
            entry.EntryDate,
            entry.DebitAccountCode,
            entry.CreditAccountCode,
            entry.Amount,
            entry.Status);

    public JournalEntryDetailDto ToDetail(JournalEntry entry) =>
        new(
            entry.Id,
            entry.EntryNumber,
            entry.Description,
            entry.EntryDate,
            entry.DebitAccountCode,
            entry.CreditAccountCode,
            entry.Amount,
            entry.Status,
            entry.ReferenceType,
            entry.ReferenceId,
            entry.CreatedAt,
            entry.ModifiedAt);
}
