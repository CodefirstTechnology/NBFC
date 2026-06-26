using Patsanstha.Modules.Reporting.Domain.Entities;
using Patsanstha.Modules.Reporting.Domain.Enums;

namespace Patsanstha.Modules.Reporting.Tests;

public sealed class ReportSnapshotAggregateTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0000-000000000099");
    private static readonly DateTimeOffset GeneratedAt = new(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Generate_creates_completed_snapshot_with_all_fields()
    {
        var snapshot = CreateSnapshot();

        Assert.Equal(ReportType.LoanPortfolio, snapshot.ReportType);
        Assert.Equal("Loan Portfolio Summary", snapshot.Title);
        Assert.Equal("{\"branchId\":\"00000000-0000-0000-0000-000000000010\"}", snapshot.ParametersJson);
        Assert.Contains("totalLoans", snapshot.ResultJson);
        Assert.Equal(GeneratedAt, snapshot.GeneratedAt);
        Assert.Equal(UserId, snapshot.GeneratedByUserId);
        Assert.Equal(ReportSnapshotStatus.Completed, snapshot.Status);
    }

    [Fact]
    public void Generate_requires_tenant_id()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ReportSnapshot.Generate(
                Guid.Empty,
                ReportType.BranchSummary,
                "Branch Summary",
                "{}",
                "{\"branchCount\":0}",
                GeneratedAt,
                UserId));
    }

    [Fact]
    public void Generate_requires_title()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ReportSnapshot.Generate(
                TenantId,
                ReportType.BranchSummary,
                "   ",
                "{}",
                "{\"branchCount\":0}",
                GeneratedAt,
                UserId));
    }

    [Fact]
    public void Generate_requires_parameters_json()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ReportSnapshot.Generate(
                TenantId,
                ReportType.NpaSummary,
                "NPA Summary",
                "",
                "{\"totalNpaAccounts\":0}",
                GeneratedAt,
                UserId));
    }

    [Fact]
    public void Generate_requires_result_json()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ReportSnapshot.Generate(
                TenantId,
                ReportType.CollectionsDaily,
                "Daily Collections",
                "{}",
                "",
                GeneratedAt,
                UserId));
    }

    [Fact]
    public void MarkFailed_rejects_completed_snapshot()
    {
        var snapshot = CreateSnapshot();

        Assert.Throws<InvalidOperationException>(() => snapshot.MarkFailed());
    }

    private static ReportSnapshot CreateSnapshot() =>
        ReportSnapshot.Generate(
            TenantId,
            ReportType.LoanPortfolio,
            "Loan Portfolio Summary",
            "{\"branchId\":\"00000000-0000-0000-0000-000000000010\"}",
            "{\"totalLoans\":0,\"totalOutstanding\":0,\"totalDisbursed\":0,\"byProductType\":[],\"byStatus\":[]}",
            GeneratedAt,
            UserId);
}
