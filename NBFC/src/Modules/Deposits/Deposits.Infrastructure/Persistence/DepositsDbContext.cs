using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence;
using Patsanstha.Modules.Deposits.Application.Abstractions;
using Patsanstha.Modules.Deposits.Domain.Entities;
using Patsanstha.Modules.Deposits.Domain.Enums;

namespace Patsanstha.Modules.Deposits.Infrastructure.Persistence;

public sealed class DepositsDbContext(
    DbContextOptions<DepositsDbContext> options,
    IAuditContextAccessor auditContextAccessor)
    : ModuleDbContextBase(options, auditContextAccessor)
{
    protected override string SchemaName => Schema;

    public const string Schema = "deposits";

    public DbSet<DepositAccount> DepositAccounts => Set<DepositAccount>();

    protected override void ConfigureModule(ModelBuilder modelBuilder)
    {
        ConfigureAuditableEntity<DepositAccount>(modelBuilder);

        modelBuilder.Entity<DepositAccount>(entity =>
        {
            entity.ToTable("deposit_accounts");

            entity.Property(a => a.MemberNumber).HasMaxLength(32).IsRequired();
            entity.Property(a => a.MemberName).HasMaxLength(200).IsRequired();
            entity.Property(a => a.AccountNumber).HasMaxLength(32).IsRequired();
            entity.Property(a => a.ProductType).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(a => a.PrincipalAmount).HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(a => a.CurrentBalance).HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(a => a.InterestRate).HasColumnType("numeric(5,2)").IsRequired();
            entity.Property(a => a.InterestPayoutMode).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

            entity.HasIndex(a => new { a.TenantId, a.AccountNumber })
                .IsUnique()
                .HasDatabaseName("ux_deposits_tenant_account_number");

            entity.HasIndex(a => new { a.TenantId, a.BranchId, a.Status })
                .HasDatabaseName("ix_deposits_tenant_branch_status");

            entity.HasIndex(a => new { a.TenantId, a.MemberId })
                .HasDatabaseName("ix_deposits_tenant_member");

            entity.HasIndex(a => new { a.TenantId, a.MemberName })
                .HasDatabaseName("ix_deposits_tenant_member_name");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("ck_deposits_principal_positive", "\"PrincipalAmount\" > 0");
                t.HasCheckConstraint("ck_deposits_balance_non_negative", "\"CurrentBalance\" >= 0");
                t.HasCheckConstraint(
                    "ck_deposits_status",
                    "\"Status\" IN ('Active','Matured','Closed','PrematureClosed')");
            });
        });
    }
}

public sealed class DepositAccountRepository(DepositsDbContext dbContext) : IDepositAccountRepository
{
    public Task AddAsync(DepositAccount account, CancellationToken cancellationToken = default)
    {
        dbContext.DepositAccounts.Add(account);
        return Task.CompletedTask;
    }

    public Task<DepositAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.DepositAccounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<DepositAccount> Items, int TotalCount)> ListAsync(
        ListDepositAccountsCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.DepositAccounts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = criteria.Search.Trim().ToLowerInvariant();
            query = query.Where(a =>
                a.MemberName.ToLower().Contains(search) ||
                a.MemberNumber.ToLower().Contains(search) ||
                a.AccountNumber.ToLower().Contains(search));
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

    public async Task<DepositSummaryDto> GetSummaryAsync(
        Guid? branchId = null,
        CancellationToken cancellationToken = default)
    {
        var activeQuery = dbContext.DepositAccounts.AsNoTracking()
            .Where(a => a.Status == DepositAccountStatus.Active);

        if (branchId.HasValue)
        {
            activeQuery = activeQuery.Where(a => a.BranchId == branchId.Value);
        }

        var now = DateTimeOffset.UtcNow;
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var previousMonthStart = monthStart.AddMonths(-1);
        var monthStartDate = DateOnly.FromDateTime(monthStart.UtcDateTime);
        var monthEndDate = DateOnly.FromDateTime(monthStart.AddMonths(1).AddDays(-1).UtcDateTime);

        var totalDepositsAmount = await activeQuery.SumAsync(a => a.CurrentBalance, cancellationToken);
        var totalActiveAccounts = await activeQuery.CountAsync(cancellationToken);
        var activeSavingsCount = await activeQuery.CountAsync(
            a => a.ProductType == DepositProductType.Savings,
            cancellationToken);
        var fixedDepositsBalance = await activeQuery
            .Where(a => a.ProductType == DepositProductType.FixedDeposit)
            .SumAsync(a => a.CurrentBalance, cancellationToken);
        var dueThisMonthCount = await activeQuery.CountAsync(
            a => a.ProductType == DepositProductType.FixedDeposit
                 && a.MaturityDate.HasValue
                 && a.MaturityDate.Value >= monthStartDate
                 && a.MaturityDate.Value <= monthEndDate,
            cancellationToken);

        var trendQuery = dbContext.DepositAccounts.AsNoTracking();
        if (branchId.HasValue)
        {
            trendQuery = trendQuery.Where(a => a.BranchId == branchId.Value);
        }

        var depositsThisMonth = await trendQuery
            .Where(a => a.CreatedAt >= monthStart)
            .SumAsync(a => a.PrincipalAmount, cancellationToken);
        var depositsLastMonth = await trendQuery
            .Where(a => a.CreatedAt >= previousMonthStart && a.CreatedAt < monthStart)
            .SumAsync(a => a.PrincipalAmount, cancellationToken);

        return new DepositSummaryDto(
            totalDepositsAmount,
            CalcTrendPercent(depositsThisMonth, depositsLastMonth),
            totalActiveAccounts,
            activeSavingsCount,
            fixedDepositsBalance,
            dueThisMonthCount);
    }

    private static decimal? CalcTrendPercent(decimal current, decimal previous)
    {
        if (previous == 0m)
        {
            return current > 0m ? 100m : null;
        }

        return Math.Round((current - previous) / previous * 100m, 1);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

public sealed class DepositAccountNumberGenerator(
    DepositsDbContext dbContext,
    IAuditContextAccessor auditContextAccessor) : IDepositAccountNumberGenerator
{
    public async Task<string> GenerateNextAsync(
        DepositProductType productType,
        CancellationToken cancellationToken = default)
    {
        var tenantId = auditContextAccessor.Current.TenantId;
        var year = DateTime.UtcNow.Year;
        var prefix = productType switch
        {
            DepositProductType.Savings => $"SB-{year}-",
            DepositProductType.RecurringDeposit => $"RD-{year}-",
            DepositProductType.FixedDeposit => $"FD-{year}-",
            _ => throw new ArgumentOutOfRangeException(nameof(productType), productType, null),
        };

        var latestNumber = await dbContext.DepositAccounts
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.AccountNumber.StartsWith(prefix))
            .OrderByDescending(a => a.AccountNumber)
            .Select(a => a.AccountNumber)
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

public sealed class DepositAccountMapper : IDepositAccountMapper
{
    public DepositAccountSummaryDto ToSummary(DepositAccount account) =>
        new(
            account.Id,
            account.AccountNumber,
            account.MemberName,
            account.MemberNumber,
            account.ProductType,
            account.CurrentBalance,
            account.InterestRate,
            account.MaturityDate,
            account.Status,
            account.OpenedOn);

    public DepositAccountDetailDto ToDetail(DepositAccount account) =>
        new(
            account.Id,
            account.MemberId,
            account.MemberNumber,
            account.MemberName,
            account.BranchId,
            account.AccountNumber,
            account.ProductType,
            account.PrincipalAmount,
            account.CurrentBalance,
            account.InterestRate,
            account.TenureMonths,
            account.InterestPayoutMode,
            account.AutoRenewal,
            account.OpenedOn,
            account.MaturityDate,
            account.Status,
            account.CreatedAt,
            account.ModifiedAt);
}
