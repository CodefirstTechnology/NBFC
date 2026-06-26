using System.Text.Json;
using Patsanstha.Modules.Reporting.Domain.Enums;

namespace Patsanstha.Modules.Reporting.Application.ReportSnapshots.GenerateReport;

internal static class ReportPlaceholderResults
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Build(ReportType reportType) =>
        JsonSerializer.Serialize(CreatePayload(reportType), SerializerOptions);

    private static object CreatePayload(ReportType reportType) =>
        reportType switch
        {
            ReportType.BranchSummary => new
            {
                branchCount = 0,
                totalMembers = 0,
                totalDeposits = 0m,
                totalLoansOutstanding = 0m,
                branches = Array.Empty<object>(),
            },
            ReportType.LoanPortfolio => new
            {
                totalLoans = 0,
                totalOutstanding = 0m,
                totalDisbursed = 0m,
                byProductType = Array.Empty<object>(),
                byStatus = Array.Empty<object>(),
            },
            ReportType.CollectionsDaily => new
            {
                reportDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("O"),
                totalCollected = 0m,
                loanCollections = 0m,
                depositCollections = 0m,
                entries = Array.Empty<object>(),
            },
            ReportType.NpaSummary => new
            {
                totalNpaAccounts = 0,
                totalNpaAmount = 0m,
                npaRatio = 0m,
                byBucket = Array.Empty<object>(),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(reportType), reportType, null),
        };
}
