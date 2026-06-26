using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Reporting.Domain.Enums;

namespace Patsanstha.Modules.Reporting.Domain.Entities;

public sealed class ReportSnapshot : AggregateRoot
{
    public ReportType ReportType { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string ParametersJson { get; private set; } = string.Empty;

    public string ResultJson { get; private set; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; private set; }

    public Guid? GeneratedByUserId { get; private set; }

    public ReportSnapshotStatus Status { get; private set; }

    private ReportSnapshot()
    {
    }

    public static ReportSnapshot Generate(
        Guid tenantId,
        ReportType reportType,
        string title,
        string parametersJson,
        string resultJson,
        DateTimeOffset generatedAt,
        Guid? generatedByUserId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("TenantId is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Report title is required.");
        }

        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            throw new InvalidOperationException("Parameters JSON is required.");
        }

        if (string.IsNullOrWhiteSpace(resultJson))
        {
            throw new InvalidOperationException("Result JSON is required.");
        }

        return new ReportSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReportType = reportType,
            Title = title.Trim(),
            ParametersJson = parametersJson,
            ResultJson = resultJson,
            GeneratedAt = generatedAt,
            GeneratedByUserId = generatedByUserId,
            Status = ReportSnapshotStatus.Completed,
        };
    }

    public void MarkFailed()
    {
        if (Status == ReportSnapshotStatus.Completed)
        {
            throw new InvalidOperationException("Completed report snapshots cannot be marked failed.");
        }

        Status = ReportSnapshotStatus.Failed;
    }
}
