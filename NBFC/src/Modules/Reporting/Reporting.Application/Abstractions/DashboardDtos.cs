namespace Patsanstha.Modules.Reporting.Application.Abstractions;

public sealed record ExecutiveDashboardDto(
    DateTimeOffset GeneratedAt,
    string BranchName,
    DashboardKpiDto Kpis,
    IReadOnlyList<MonthlyPerformancePointDto> MonthlyPerformance,
    IReadOnlyList<RecentActivityItemDto> RecentActivity,
    SystemStatusDto SystemStatus);

public sealed record DashboardKpiDto(
    int TotalMembers,
    int NewMembersThisWeek,
    decimal? MembersTrendPercent,
    decimal ActiveLoansAmount,
    int ActiveLoansCount,
    decimal? ActiveLoansTrendPercent,
    decimal TotalDepositsAmount,
    decimal? DepositsTrendPercent,
    decimal RecoveryRatePercent,
    decimal RecoveryTargetPercent);

public sealed record MonthlyPerformancePointDto(
    string MonthLabel,
    int Month,
    int Year,
    decimal TargetCollection,
    decimal ActualRecovery);

public sealed record RecentActivityItemDto(
    Guid Id,
    string ActivityType,
    string MemberName,
    string ReferenceNumber,
    decimal? Amount,
    string Status,
    DateTimeOffset OccurredAt);

public sealed record SystemStatusDto(
    bool IsOnline,
    string Message,
    DateTimeOffset? LastProcessedAt);
