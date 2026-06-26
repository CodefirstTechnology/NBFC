using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence;
using Patsanstha.Modules.Reporting.Application.Abstractions;
using Patsanstha.Modules.Reporting.Domain.Entities;
using Patsanstha.Modules.Reporting.Domain.Enums;

namespace Patsanstha.Modules.Reporting.Infrastructure.Persistence;

public sealed class ReportingDbContext(
    DbContextOptions<ReportingDbContext> options,
    IAuditContextAccessor auditContextAccessor)
    : ModuleDbContextBase(options, auditContextAccessor)
{
    protected override string SchemaName => Schema;

    public const string Schema = "reporting";

    public DbSet<ReportSnapshot> ReportSnapshots => Set<ReportSnapshot>();

    protected override void ConfigureModule(ModelBuilder modelBuilder)
    {
        ConfigureAuditableEntity<ReportSnapshot>(modelBuilder);

        modelBuilder.Entity<ReportSnapshot>(entity =>
        {
            entity.ToTable("report_snapshots");

            entity.Property(s => s.ReportType).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(s => s.Title).HasMaxLength(200).IsRequired();
            entity.Property(s => s.ParametersJson).HasColumnType("jsonb").IsRequired();
            entity.Property(s => s.ResultJson).HasColumnType("jsonb").IsRequired();
            entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

            entity.HasIndex(s => new { s.TenantId, s.ReportType, s.GeneratedAt })
                .HasDatabaseName("ix_reporting_tenant_type_generated_at");

            entity.HasIndex(s => new { s.TenantId, s.Status })
                .HasDatabaseName("ix_reporting_tenant_status");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "ck_reporting_report_type",
                    "\"ReportType\" IN ('BranchSummary','LoanPortfolio','CollectionsDaily','NpaSummary')");
                t.HasCheckConstraint(
                    "ck_reporting_status",
                    "\"Status\" IN ('Pending','Completed','Failed')");
            });
        });
    }
}

public sealed class ReportSnapshotRepository(ReportingDbContext dbContext) : IReportSnapshotRepository
{
    public Task AddAsync(ReportSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        dbContext.ReportSnapshots.Add(snapshot);
        return Task.CompletedTask;
    }

    public Task<ReportSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.ReportSnapshots.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<ReportSnapshot> Items, int TotalCount)> ListAsync(
        ListReportSnapshotsCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ReportSnapshots.AsNoTracking();

        if (criteria.ReportType.HasValue)
        {
            query = query.Where(s => s.ReportType == criteria.ReportType.Value);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(s => s.Status == criteria.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.GeneratedAt)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

public sealed class ReportSnapshotMapper : IReportSnapshotMapper
{
    public ReportSnapshotSummaryDto ToSummary(ReportSnapshot snapshot) =>
        new(
            snapshot.Id,
            snapshot.ReportType,
            snapshot.Title,
            snapshot.Status,
            snapshot.GeneratedAt,
            snapshot.GeneratedByUserId);

    public ReportSnapshotDetailDto ToDetail(ReportSnapshot snapshot) =>
        new(
            snapshot.Id,
            snapshot.ReportType,
            snapshot.Title,
            snapshot.ParametersJson,
            snapshot.ResultJson,
            snapshot.Status,
            snapshot.GeneratedAt,
            snapshot.GeneratedByUserId,
            snapshot.CreatedAt,
            snapshot.ModifiedAt);
}
