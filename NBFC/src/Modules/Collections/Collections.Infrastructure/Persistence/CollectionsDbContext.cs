using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence;
using Patsanstha.Modules.Collections.Application.Abstractions;
using Patsanstha.Modules.Collections.Domain.Entities;
using Patsanstha.Modules.Collections.Domain.Enums;

namespace Patsanstha.Modules.Collections.Infrastructure.Persistence;

public sealed class CollectionsDbContext(
    DbContextOptions<CollectionsDbContext> options,
    IAuditContextAccessor auditContextAccessor)
    : ModuleDbContextBase(options, auditContextAccessor)
{
    protected override string SchemaName => Schema;

    public const string Schema = "collections";

    public DbSet<CollectionReceipt> CollectionReceipts => Set<CollectionReceipt>();

    protected override void ConfigureModule(ModelBuilder modelBuilder)
    {
        ConfigureAuditableEntity<CollectionReceipt>(modelBuilder);

        modelBuilder.Entity<CollectionReceipt>(entity =>
        {
            entity.ToTable("collection_receipts");

            entity.Property(r => r.MemberNumber).HasMaxLength(32).IsRequired();
            entity.Property(r => r.MemberName).HasMaxLength(200).IsRequired();
            entity.Property(r => r.LoanNumber).HasMaxLength(32).IsRequired();
            entity.Property(r => r.ReceiptNumber).HasMaxLength(32).IsRequired();
            entity.Property(r => r.Amount).HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(r => r.PaymentMode).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(r => r.ReferenceNumber).HasMaxLength(64);
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

            entity.HasIndex(r => new { r.TenantId, r.ReceiptNumber })
                .IsUnique()
                .HasDatabaseName("ux_collections_tenant_receipt_number");

            entity.HasIndex(r => new { r.TenantId, r.BranchId, r.Status })
                .HasDatabaseName("ix_collections_tenant_branch_status");

            entity.HasIndex(r => new { r.TenantId, r.MemberId })
                .HasDatabaseName("ix_collections_tenant_member");

            entity.HasIndex(r => new { r.TenantId, r.LoanNumber })
                .HasDatabaseName("ix_collections_tenant_loan_number");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("ck_collections_amount_positive", "\"Amount\" > 0");
                t.HasCheckConstraint(
                    "ck_collections_status",
                    "\"Status\" IN ('Collected','Reversed')");
            });
        });
    }
}

public sealed class CollectionReceiptRepository(CollectionsDbContext dbContext) : ICollectionReceiptRepository
{
    public Task AddAsync(CollectionReceipt receipt, CancellationToken cancellationToken = default)
    {
        dbContext.CollectionReceipts.Add(receipt);
        return Task.CompletedTask;
    }

    public Task<CollectionReceipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.CollectionReceipts.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<CollectionReceipt> Items, int TotalCount)> ListAsync(
        ListCollectionReceiptsCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.CollectionReceipts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = criteria.Search.Trim().ToLowerInvariant();
            query = query.Where(r =>
                r.MemberName.ToLower().Contains(search) ||
                r.MemberNumber.ToLower().Contains(search) ||
                r.LoanNumber.ToLower().Contains(search) ||
                r.ReceiptNumber.ToLower().Contains(search));
        }

        if (criteria.BranchId.HasValue)
        {
            query = query.Where(r => r.BranchId == criteria.BranchId.Value);
        }

        if (criteria.MemberId.HasValue)
        {
            query = query.Where(r => r.MemberId == criteria.MemberId.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.LoanNumber))
        {
            var loanNumber = criteria.LoanNumber.Trim().ToUpperInvariant();
            query = query.Where(r => r.LoanNumber == loanNumber);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

public sealed class ReceiptNumberGenerator(
    CollectionsDbContext dbContext,
    IAuditContextAccessor auditContextAccessor) : IReceiptNumberGenerator
{
    public async Task<string> GenerateNextAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = auditContextAccessor.Current.TenantId;
        var year = DateTime.UtcNow.Year;
        var prefix = $"CR-{year}-";

        var latestNumber = await dbContext.CollectionReceipts
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.ReceiptNumber.StartsWith(prefix))
            .OrderByDescending(r => r.ReceiptNumber)
            .Select(r => r.ReceiptNumber)
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

public sealed class CollectionReceiptMapper : ICollectionReceiptMapper
{
    public CollectionReceiptSummaryDto ToSummary(CollectionReceipt receipt) =>
        new(
            receipt.Id,
            receipt.ReceiptNumber,
            receipt.LoanNumber,
            receipt.MemberName,
            receipt.MemberNumber,
            receipt.Amount,
            receipt.PaymentMode,
            receipt.Status,
            receipt.CollectedOn);

    public CollectionReceiptDetailDto ToDetail(CollectionReceipt receipt) =>
        new(
            receipt.Id,
            receipt.MemberId,
            receipt.MemberNumber,
            receipt.MemberName,
            receipt.LoanApplicationId,
            receipt.LoanNumber,
            receipt.BranchId,
            receipt.ReceiptNumber,
            receipt.Amount,
            receipt.PaymentMode,
            receipt.ReferenceNumber,
            receipt.CollectedOn,
            receipt.Status,
            receipt.CreatedAt,
            receipt.ModifiedAt);
}
