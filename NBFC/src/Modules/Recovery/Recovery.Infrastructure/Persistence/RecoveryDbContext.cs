using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence;
using Patsanstha.Modules.Recovery.Application.Abstractions;
using Patsanstha.Modules.Recovery.Domain.Entities;
using Patsanstha.Modules.Recovery.Domain.Enums;

namespace Patsanstha.Modules.Recovery.Infrastructure.Persistence;

public sealed class RecoveryDbContext(
    DbContextOptions<RecoveryDbContext> options,
    IAuditContextAccessor auditContextAccessor)
    : ModuleDbContextBase(options, auditContextAccessor)
{
    protected override string SchemaName => Schema;

    public const string Schema = "recovery";

    public DbSet<RecoveryCase> RecoveryCases => Set<RecoveryCase>();

    protected override void ConfigureModule(ModelBuilder modelBuilder)
    {
        ConfigureAuditableEntity<RecoveryCase>(modelBuilder);

        modelBuilder.Entity<RecoveryCase>(entity =>
        {
            entity.ToTable("recovery_cases");

            entity.Property(c => c.LoanNumber).HasMaxLength(32).IsRequired();
            entity.Property(c => c.MemberNumber).HasMaxLength(32).IsRequired();
            entity.Property(c => c.MemberName).HasMaxLength(200).IsRequired();
            entity.Property(c => c.CaseNumber).HasMaxLength(32).IsRequired();
            entity.Property(c => c.OutstandingAmount).HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(c => c.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(c => c.Notes).HasMaxLength(4000);

            entity.HasIndex(c => new { c.TenantId, c.CaseNumber })
                .IsUnique()
                .HasDatabaseName("ux_recovery_tenant_case_number");

            entity.HasIndex(c => new { c.TenantId, c.BranchId, c.Status })
                .HasDatabaseName("ix_recovery_tenant_branch_status");

            entity.HasIndex(c => new { c.TenantId, c.MemberId })
                .HasDatabaseName("ix_recovery_tenant_member");

            entity.HasIndex(c => new { c.TenantId, c.LoanApplicationId })
                .HasDatabaseName("ix_recovery_tenant_loan_application");

            entity.HasIndex(c => new { c.TenantId, c.AssignedToUserId })
                .HasDatabaseName("ix_recovery_tenant_assigned_user");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("ck_recovery_outstanding_positive", "\"OutstandingAmount\" > 0");
                t.HasCheckConstraint("ck_recovery_days_past_due", "\"DaysPastDue\" >= 0");
                t.HasCheckConstraint(
                    "ck_recovery_status",
                    "\"Status\" IN ('Open','InProgress','Resolved','WrittenOff')");
            });
        });
    }
}

public sealed class RecoveryCaseRepository(RecoveryDbContext dbContext) : IRecoveryCaseRepository
{
    public Task AddAsync(RecoveryCase recoveryCase, CancellationToken cancellationToken = default)
    {
        dbContext.RecoveryCases.Add(recoveryCase);
        return Task.CompletedTask;
    }

    public Task<RecoveryCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.RecoveryCases.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<RecoveryCase> Items, int TotalCount)> ListAsync(
        ListRecoveryCasesCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.RecoveryCases.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = criteria.Search.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.MemberName.ToLower().Contains(search) ||
                c.MemberNumber.ToLower().Contains(search) ||
                c.LoanNumber.ToLower().Contains(search) ||
                c.CaseNumber.ToLower().Contains(search));
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(c => c.Status == criteria.Status.Value);
        }

        if (criteria.BranchId.HasValue)
        {
            query = query.Where(c => c.BranchId == criteria.BranchId.Value);
        }

        if (criteria.MemberId.HasValue)
        {
            query = query.Where(c => c.MemberId == criteria.MemberId.Value);
        }

        if (criteria.AssignedToUserId.HasValue)
        {
            query = query.Where(c => c.AssignedToUserId == criteria.AssignedToUserId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

public sealed class CaseNumberGenerator(
    RecoveryDbContext dbContext,
    IAuditContextAccessor auditContextAccessor) : ICaseNumberGenerator
{
    public async Task<string> GenerateNextAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = auditContextAccessor.Current.TenantId;
        var year = DateTime.UtcNow.Year;
        var prefix = $"RC-{year}-";

        var latestNumber = await dbContext.RecoveryCases
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.CaseNumber.StartsWith(prefix))
            .OrderByDescending(c => c.CaseNumber)
            .Select(c => c.CaseNumber)
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

public sealed class RecoveryCaseMapper : IRecoveryCaseMapper
{
    public RecoveryCaseSummaryDto ToSummary(RecoveryCase recoveryCase) =>
        new(
            recoveryCase.Id,
            recoveryCase.CaseNumber,
            recoveryCase.LoanNumber,
            recoveryCase.MemberName,
            recoveryCase.MemberNumber,
            recoveryCase.OutstandingAmount,
            recoveryCase.DaysPastDue,
            recoveryCase.Status,
            recoveryCase.OpenedOn,
            recoveryCase.AssignedToUserId);

    public RecoveryCaseDetailDto ToDetail(RecoveryCase recoveryCase) =>
        new(
            recoveryCase.Id,
            recoveryCase.LoanApplicationId,
            recoveryCase.LoanNumber,
            recoveryCase.MemberId,
            recoveryCase.MemberNumber,
            recoveryCase.MemberName,
            recoveryCase.BranchId,
            recoveryCase.CaseNumber,
            recoveryCase.OutstandingAmount,
            recoveryCase.DaysPastDue,
            recoveryCase.Status,
            recoveryCase.Notes,
            recoveryCase.AssignedToUserId,
            recoveryCase.OpenedOn,
            recoveryCase.ResolvedOn,
            recoveryCase.CreatedAt,
            recoveryCase.ModifiedAt);
}
