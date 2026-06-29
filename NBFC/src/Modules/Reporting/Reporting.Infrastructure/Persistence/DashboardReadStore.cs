using System.Globalization;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patsanstha.Modules.Reporting.Application.Abstractions;

namespace Patsanstha.Modules.Reporting.Infrastructure.Persistence;

public sealed class DashboardReadStore(IConfiguration configuration) : IDashboardReadStore
{
    private const decimal RecoveryTargetPercent = 98m;
    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000010");

    public async Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync(
        Guid tenantId,
        Guid? branchId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(configuration.GetConnectionString("Reporting"));
        await connection.OpenAsync(cancellationToken);

        var branchFilter = branchId.HasValue ? @" AND ""BranchId"" = @branchId" : string.Empty;
        var now = DateTimeOffset.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var previousMonthStart = monthStart.AddMonths(-1);
        var weekStart = now.AddDays(-7);
        var sixMonthsStart = monthStart.AddMonths(-5);

        var kpis = await LoadKpisAsync(
            connection,
            tenantId,
            branchId,
            branchFilter,
            monthStart,
            previousMonthStart,
            weekStart,
            cancellationToken);

        var monthlyPerformance = await LoadMonthlyPerformanceAsync(
            connection,
            tenantId,
            branchId,
            branchFilter,
            sixMonthsStart,
            cancellationToken);

        var recentActivity = await LoadRecentActivityAsync(
            connection,
            tenantId,
            branchId,
            branchFilter,
            cancellationToken);

        var lastProcessedAt = await LoadLastCollectionAtAsync(
            connection,
            tenantId,
            branchId,
            branchFilter,
            cancellationToken);

        return new ExecutiveDashboardDto(
            now,
            ResolveBranchName(branchId),
            kpis,
            monthlyPerformance,
            recentActivity,
            new SystemStatusDto(
                true,
                lastProcessedAt.HasValue
                    ? $"Day end processed • {lastProcessedAt.Value.ToLocalTime():HH:mm tt}"
                    : "System online",
                lastProcessedAt));
    }

    private static string ResolveBranchName(Guid? branchId) =>
        branchId is null || branchId == DefaultBranchId ? "Main Office" : "Branch";

    private static async Task<DashboardKpiDto> LoadKpisAsync(
        NpgsqlConnection connection,
        Guid tenantId,
        Guid? branchId,
        string branchFilter,
        DateTime monthStart,
        DateTime previousMonthStart,
        DateTimeOffset weekStart,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*)::int FROM members.members
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false AND "Status" = 'Active'{0}) AS total_members,
                (SELECT COUNT(*)::int FROM members.members
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false AND "JoinedOn" >= @weekStart{0}) AS new_members_week,
                (SELECT COUNT(*)::int FROM members.members
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false AND "CreatedAt" >= @monthStart{0}) AS members_this_month,
                (SELECT COUNT(*)::int FROM members.members
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false
                   AND "CreatedAt" >= @previousMonthStart AND "CreatedAt" < @monthStart{0}) AS members_last_month,
                COALESCE((SELECT SUM("ApprovedAmount") FROM loans.loan_applications
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false AND "Status" = 'Disbursed'{0}), 0) AS active_loans_amount,
                (SELECT COUNT(*)::int FROM loans.loan_applications
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false AND "Status" = 'Disbursed'{0}) AS active_loans_count,
                COALESCE((SELECT SUM("ApprovedAmount") FROM loans.loan_applications
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false AND "Status" = 'Disbursed'
                   AND "DisbursedOn" >= @monthStartDate{0}), 0) AS loans_disbursed_this_month,
                COALESCE((SELECT SUM("ApprovedAmount") FROM loans.loan_applications
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false AND "Status" = 'Disbursed'
                   AND "DisbursedOn" >= @previousMonthStartDate AND "DisbursedOn" < @monthStartDate{0}), 0) AS loans_disbursed_last_month,
                COALESCE((SELECT SUM("CurrentBalance") FROM deposits.deposit_accounts
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false AND "Status" = 'Active'{0}), 0) AS total_deposits,
                COALESCE((SELECT SUM("PrincipalAmount") FROM deposits.deposit_accounts
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false
                   AND "CreatedAt" >= @monthStart{0}), 0) AS deposits_this_month,
                COALESCE((SELECT SUM("PrincipalAmount") FROM deposits.deposit_accounts
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false
                   AND "CreatedAt" >= @previousMonthStart AND "CreatedAt" < @monthStart{0}), 0) AS deposits_last_month,
                COALESCE((SELECT SUM("EmiAmount") FROM loans.loan_applications
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false AND "Status" = 'Disbursed'{0}), 0) AS expected_monthly_collection,
                COALESCE((SELECT SUM("Amount") FROM collections.collection_receipts
                 WHERE "TenantId" = @tenantId AND "IsDeleted" = false AND "Status" = 'Collected'
                   AND "CollectedOn" >= @monthStartDate{0}), 0) AS collected_this_month
            """;

        await using var command = new NpgsqlCommand(string.Format(CultureInfo.InvariantCulture, sql, branchFilter), connection);
        command.Parameters.AddWithValue("tenantId", tenantId);
        AddBranchParameter(command, branchId);
        command.Parameters.AddWithValue("weekStart", DateOnly.FromDateTime(weekStart.UtcDateTime));
        command.Parameters.AddWithValue("monthStart", monthStart);
        command.Parameters.AddWithValue("previousMonthStart", previousMonthStart);
        command.Parameters.AddWithValue("monthStartDate", DateOnly.FromDateTime(monthStart));
        command.Parameters.AddWithValue("previousMonthStartDate", DateOnly.FromDateTime(previousMonthStart));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        var expectedMonthly = reader.GetDecimal(reader.GetOrdinal("expected_monthly_collection"));
        var collectedThisMonth = reader.GetDecimal(reader.GetOrdinal("collected_this_month"));
        var recoveryRate = expectedMonthly > 0
            ? Math.Round(collectedThisMonth / expectedMonthly * 100m, 1)
            : collectedThisMonth > 0 ? 100m : 0m;

        return new DashboardKpiDto(
            reader.GetInt32(reader.GetOrdinal("total_members")),
            reader.GetInt32(reader.GetOrdinal("new_members_week")),
            CalcTrendPercent(reader.GetInt32(reader.GetOrdinal("members_this_month")), reader.GetInt32(reader.GetOrdinal("members_last_month"))),
            reader.GetDecimal(reader.GetOrdinal("active_loans_amount")),
            reader.GetInt32(reader.GetOrdinal("active_loans_count")),
            CalcTrendPercent(reader.GetDecimal(reader.GetOrdinal("loans_disbursed_this_month")), reader.GetDecimal(reader.GetOrdinal("loans_disbursed_last_month"))),
            reader.GetDecimal(reader.GetOrdinal("total_deposits")),
            CalcTrendPercent(reader.GetDecimal(reader.GetOrdinal("deposits_this_month")), reader.GetDecimal(reader.GetOrdinal("deposits_last_month"))),
            recoveryRate,
            RecoveryTargetPercent);
    }

    private static async Task<IReadOnlyList<MonthlyPerformancePointDto>> LoadMonthlyPerformanceAsync(
        NpgsqlConnection connection,
        Guid tenantId,
        Guid? branchId,
        string branchFilter,
        DateTime sixMonthsStart,
        CancellationToken cancellationToken)
    {
        var expectedMonthly = await LoadExpectedMonthlyCollectionAsync(connection, tenantId, branchId, branchFilter, cancellationToken);

        const string sql = """
            SELECT
                EXTRACT(MONTH FROM "CollectedOn")::int AS month_num,
                EXTRACT(YEAR FROM "CollectedOn")::int AS year_num,
                COALESCE(SUM("Amount"), 0) AS actual_recovery
            FROM collections.collection_receipts
            WHERE "TenantId" = @tenantId
              AND "IsDeleted" = false
              AND "Status" = 'Collected'
              AND "CollectedOn" >= @sixMonthsStart
              {0}
            GROUP BY year_num, month_num
            ORDER BY year_num, month_num
            """;

        await using var command = new NpgsqlCommand(string.Format(CultureInfo.InvariantCulture, sql, branchFilter), connection);
        command.Parameters.AddWithValue("tenantId", tenantId);
        AddBranchParameter(command, branchId);
        command.Parameters.AddWithValue("sixMonthsStart", DateOnly.FromDateTime(sixMonthsStart));

        var actualByMonth = new Dictionary<(int Year, int Month), decimal>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var year = reader.GetInt32(reader.GetOrdinal("year_num"));
                var month = reader.GetInt32(reader.GetOrdinal("month_num"));
                actualByMonth[(year, month)] = reader.GetDecimal(reader.GetOrdinal("actual_recovery"));
            }
        }

        var points = new List<MonthlyPerformancePointDto>();
        var cursor = new DateTime(sixMonthsStart.Year, sixMonthsStart.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        while (cursor <= end)
        {
            actualByMonth.TryGetValue((cursor.Year, cursor.Month), out var actual);
            points.Add(new MonthlyPerformancePointDto(
                cursor.ToString("MMM", CultureInfo.InvariantCulture),
                cursor.Month,
                cursor.Year,
                expectedMonthly,
                actual));

            cursor = cursor.AddMonths(1);
        }

        return points;
    }

    private static async Task<decimal> LoadExpectedMonthlyCollectionAsync(
        NpgsqlConnection connection,
        Guid tenantId,
        Guid? branchId,
        string branchFilter,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT COALESCE(SUM("EmiAmount"), 0)
            FROM loans.loan_applications
            WHERE "TenantId" = @tenantId
              AND "IsDeleted" = false
              AND "Status" = 'Disbursed'
              {branchFilter}
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", tenantId);
        AddBranchParameter(command, branchId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is decimal value ? value : 0m;
    }

    private static async Task<IReadOnlyList<RecentActivityItemDto>> LoadRecentActivityAsync(
        NpgsqlConnection connection,
        Guid tenantId,
        Guid? branchId,
        string branchFilter,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT * FROM (
                SELECT
                    "Id",
                    'Member Enrollment' AS activity_type,
                    "FullName" AS member_name,
                    "MemberNumber" AS reference_number,
                    NULL::numeric AS amount,
                    CASE WHEN "Status" = 'Active' THEN 'Completed' ELSE 'Pending Verification' END AS status,
                    "CreatedAt" AS occurred_at
                FROM members.members
                WHERE "TenantId" = @tenantId AND "IsDeleted" = false{0}

                UNION ALL

                SELECT
                    "Id",
                    CASE "ProductType"
                        WHEN 'Gold' THEN 'Gold Loan Disbursement'
                        WHEN 'Vehicle' THEN 'Vehicle Loan Disbursement'
                        WHEN 'Business' THEN 'Business Loan Application'
                        ELSE 'Personal Loan Application'
                    END AS activity_type,
                    "MemberName" AS member_name,
                    "LoanNumber" AS reference_number,
                    COALESCE("ApprovedAmount", "RequestedAmount") AS amount,
                    CASE
                        WHEN "Status" = 'Disbursed' THEN 'Completed'
                        WHEN "Status" IN ('Submitted', 'UnderReview') THEN 'Pending Verification'
                        WHEN "Status" = 'Rejected' THEN 'Rejected'
                        ELSE 'In Progress'
                    END AS status,
                    "CreatedAt" AS occurred_at
                FROM loans.loan_applications
                WHERE "TenantId" = @tenantId AND "IsDeleted" = false{0}

                UNION ALL

                SELECT
                    "Id",
                    CASE "ProductType"
                        WHEN 'FixedDeposit' THEN 'Term Deposit (FD)'
                        WHEN 'RecurringDeposit' THEN 'Recurring Deposit (RD)'
                        ELSE 'Savings Deposit'
                    END AS activity_type,
                    "MemberName" AS member_name,
                    "AccountNumber" AS reference_number,
                    "PrincipalAmount" AS amount,
                    CASE WHEN "Status" = 'Active' THEN 'Completed' ELSE 'Pending Verification' END AS status,
                    "CreatedAt" AS occurred_at
                FROM deposits.deposit_accounts
                WHERE "TenantId" = @tenantId AND "IsDeleted" = false{0}

                UNION ALL

                SELECT
                    "Id",
                    'EMI Collection' AS activity_type,
                    "MemberName" AS member_name,
                    "ReceiptNumber" AS reference_number,
                    "Amount" AS amount,
                    CASE WHEN "Status" = 'Collected' THEN 'Completed' ELSE 'Reversed' END AS status,
                    "CreatedAt" AS occurred_at
                FROM collections.collection_receipts
                WHERE "TenantId" = @tenantId AND "IsDeleted" = false{0}
            ) activity
            ORDER BY occurred_at DESC
            LIMIT 8
            """;

        await using var command = new NpgsqlCommand(string.Format(CultureInfo.InvariantCulture, sql, branchFilter), connection);
        command.Parameters.AddWithValue("tenantId", tenantId);
        AddBranchParameter(command, branchId);

        var items = new List<RecentActivityItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var amountOrdinal = reader.GetOrdinal("amount");
            items.Add(new RecentActivityItemDto(
                reader.GetGuid(reader.GetOrdinal("Id")),
                reader.GetString(reader.GetOrdinal("activity_type")),
                reader.GetString(reader.GetOrdinal("member_name")),
                reader.GetString(reader.GetOrdinal("reference_number")),
                await reader.IsDBNullAsync(amountOrdinal, cancellationToken) ? null : reader.GetDecimal(amountOrdinal),
                reader.GetString(reader.GetOrdinal("status")),
                reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("occurred_at"))));
        }

        return items;
    }

    private static async Task<DateTimeOffset?> LoadLastCollectionAtAsync(
        NpgsqlConnection connection,
        Guid tenantId,
        Guid? branchId,
        string branchFilter,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT MAX("CreatedAt")
            FROM collections.collection_receipts
            WHERE "TenantId" = @tenantId AND "IsDeleted" = false
            {branchFilter}
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", tenantId);
        AddBranchParameter(command, branchId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result switch
        {
            DateTimeOffset timestamp => timestamp,
            DateTime utc => new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)),
            null or DBNull => null,
            _ => null,
        };
    }

    private static void AddBranchParameter(NpgsqlCommand command, Guid? branchId)
    {
        if (branchId.HasValue)
        {
            command.Parameters.AddWithValue("branchId", branchId.Value);
        }
    }

    private static decimal? CalcTrendPercent(decimal current, decimal previous)
    {
        if (previous == 0m)
        {
            return current > 0m ? 100m : null;
        }

        return Math.Round((current - previous) / previous * 100m, 1);
    }
}
