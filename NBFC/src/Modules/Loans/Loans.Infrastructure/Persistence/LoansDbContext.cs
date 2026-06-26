using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence;
using Patsanstha.Modules.Loans.Application.Abstractions;
using Patsanstha.Modules.Loans.Domain.Entities;
using Patsanstha.Modules.Loans.Domain.Enums;

namespace Patsanstha.Modules.Loans.Infrastructure.Persistence;

public sealed class LoansDbContext(
    DbContextOptions<LoansDbContext> options,
    IAuditContextAccessor auditContextAccessor)
    : ModuleDbContextBase(options, auditContextAccessor)
{
    protected override string SchemaName => Schema;

    public const string Schema = "loans";

    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();

    protected override void ConfigureModule(ModelBuilder modelBuilder)
    {
        ConfigureAuditableEntity<LoanApplication>(modelBuilder);

        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.ToTable("loan_applications");

            entity.Property(a => a.MemberNumber).HasMaxLength(32).IsRequired();
            entity.Property(a => a.MemberName).HasMaxLength(200).IsRequired();
            entity.Property(a => a.LoanNumber).HasMaxLength(32).IsRequired();
            entity.Property(a => a.ProductType).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(a => a.RequestedAmount).HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(a => a.ApprovedAmount).HasColumnType("numeric(18,2)");
            entity.Property(a => a.InterestRate).HasColumnType("numeric(5,2)").IsRequired();
            entity.Property(a => a.EmiAmount).HasColumnType("numeric(18,2)");
            entity.Property(a => a.OutstandingPrincipal).HasColumnType("numeric(18,2)");
            entity.Property(a => a.Purpose).HasMaxLength(500).IsRequired();
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(a => a.RejectionReason).HasMaxLength(500);

            entity.HasIndex(a => new { a.TenantId, a.LoanNumber })
                .IsUnique()
                .HasDatabaseName("ux_loans_tenant_loan_number");

            entity.HasIndex(a => new { a.TenantId, a.BranchId, a.Status })
                .HasDatabaseName("ix_loans_tenant_branch_status");

            entity.HasIndex(a => new { a.TenantId, a.MemberId })
                .HasDatabaseName("ix_loans_tenant_member");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("ck_loans_requested_positive", "\"RequestedAmount\" > 0");
                t.HasCheckConstraint(
                    "ck_loans_status",
                    "\"Status\" IN ('Submitted','UnderReview','Approved','Rejected','Disbursed','Closed','Npa')");
            });
        });
    }
}

public sealed class LoanApplicationRepository(LoansDbContext dbContext) : ILoanApplicationRepository
{
    public Task AddAsync(LoanApplication application, CancellationToken cancellationToken = default)
    {
        dbContext.LoanApplications.Add(application);
        return Task.CompletedTask;
    }

    public Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.LoanApplications.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<LoanApplication> Items, int TotalCount)> ListAsync(
        ListLoanApplicationsCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.LoanApplications.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = criteria.Search.Trim().ToLowerInvariant();
            query = query.Where(a =>
                a.MemberName.ToLower().Contains(search) ||
                a.MemberNumber.ToLower().Contains(search) ||
                a.LoanNumber.ToLower().Contains(search));
        }

        if (criteria.ProductType.HasValue)
        {
            query = query.Where(a => a.ProductType == criteria.ProductType.Value);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(a => a.Status == criteria.Status.Value);
        }

        if (criteria.BranchId.HasValue)
        {
            query = query.Where(a => a.BranchId == criteria.BranchId.Value);
        }

        if (criteria.MemberId.HasValue)
        {
            query = query.Where(a => a.MemberId == criteria.MemberId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

public sealed class LoanNumberGenerator(
    LoansDbContext dbContext,
    IAuditContextAccessor auditContextAccessor) : ILoanNumberGenerator
{
    public async Task<string> GenerateNextAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = auditContextAccessor.Current.TenantId;
        var year = DateTime.UtcNow.Year;
        var prefix = $"LN-{year}-";

        var latestNumber = await dbContext.LoanApplications
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.LoanNumber.StartsWith(prefix))
            .OrderByDescending(a => a.LoanNumber)
            .Select(a => a.LoanNumber)
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

public sealed class LoanApplicationMapper : ILoanApplicationMapper
{
    public LoanApplicationSummaryDto ToSummary(LoanApplication application) =>
        new(
            application.Id,
            application.LoanNumber,
            application.MemberName,
            application.MemberNumber,
            application.ProductType,
            application.RequestedAmount,
            application.ApprovedAmount,
            application.EmiAmount,
            application.Status,
            application.AppliedOn);

    public LoanApplicationDetailDto ToDetail(LoanApplication application) =>
        new(
            application.Id,
            application.MemberId,
            application.MemberNumber,
            application.MemberName,
            application.BranchId,
            application.LoanNumber,
            application.ProductType,
            application.RequestedAmount,
            application.ApprovedAmount,
            application.InterestRate,
            application.TenureMonths,
            application.EmiAmount,
            application.OutstandingPrincipal,
            application.Purpose,
            application.Status,
            application.RejectionReason,
            application.AppliedOn,
            application.ApprovedOn,
            application.DisbursedOn,
            application.CreatedAt,
            application.ModifiedAt);
}
