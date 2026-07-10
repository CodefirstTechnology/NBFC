using System.Globalization;
using Patsanstha.Modules.Reporting.Application.Abstractions;
using Patsanstha.Modules.Reporting.Application.ReportSnapshots.DownloadReportSnapshot;
using Patsanstha.Modules.Reporting.Domain.Enums;

namespace Patsanstha.Modules.Reporting.Tests;

public sealed class ReportExportFormatterTests
{
    private static readonly DateTimeOffset GeneratedAt = new(2026, 7, 7, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Format_json_includes_metadata_and_result()
    {
        var snapshot = CreateSnapshot();

        var file = ReportExportFormatter.Format(snapshot, ReportExportFormat.Json);

        Assert.Equal("application/json", file.ContentType);
        Assert.EndsWith(".json", file.FileName, StringComparison.Ordinal);
        var json = System.Text.Encoding.UTF8.GetString(file.Content);
        Assert.Contains("\"title\": \"Daily Collections\"", json, StringComparison.Ordinal);
        Assert.Contains("\"totalCollected\": 0", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_csv_writes_summary_and_entry_rows()
    {
        var snapshot = CreateSnapshot();

        var file = ReportExportFormatter.Format(snapshot, ReportExportFormat.Csv);

        Assert.Equal("text/csv", file.ContentType);
        Assert.EndsWith(".csv", file.FileName, StringComparison.Ordinal);
        var csv = System.Text.Encoding.UTF8.GetString(file.Content);
        Assert.Contains("Report Title,Daily Collections", csv, StringComparison.Ordinal);
        Assert.Contains("total Collected,0", csv, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("entries", csv, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildFileName_slugifies_title_and_uses_generated_date()
    {
        var snapshot = CreateSnapshot() with { Title = "Branch Summary / Main" };

        var fileName = ReportExportFormatter.BuildFileName(snapshot, ReportExportFormat.Csv);

        Assert.Equal($"branch-summary-main-{GeneratedAt.UtcDateTime:yyyyMMdd}.csv", fileName);
    }

    private static ReportSnapshotDetailDto CreateSnapshot() =>
        new(
            Guid.Parse("00000000-0000-0000-0000-000000000101"),
            ReportType.CollectionsDaily,
            "Daily Collections",
            "{}",
            """{"reportDate":"2026-07-07","totalCollected":0,"loanCollections":0,"depositCollections":0,"entries":[]}""",
            ReportSnapshotStatus.Completed,
            GeneratedAt,
            Guid.Parse("00000000-0000-0000-0000-000000000099"),
            GeneratedAt,
            null);
}
