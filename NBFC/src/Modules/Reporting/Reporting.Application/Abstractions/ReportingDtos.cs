using Patsanstha.Modules.Reporting.Domain.Enums;

namespace Patsanstha.Modules.Reporting.Application.Abstractions;

public sealed record ReportSnapshotSummaryDto(
    Guid Id,
    ReportType ReportType,
    string Title,
    ReportSnapshotStatus Status,
    DateTimeOffset GeneratedAt,
    Guid? GeneratedByUserId);

public sealed record ReportSnapshotDetailDto(
    Guid Id,
    ReportType ReportType,
    string Title,
    string ParametersJson,
    string ResultJson,
    ReportSnapshotStatus Status,
    DateTimeOffset GeneratedAt,
    Guid? GeneratedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt);

public sealed record PagedReportSnapshotsResponse(
    IReadOnlyList<ReportSnapshotSummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ListReportSnapshotsCriteria(
    int Page,
    int PageSize,
    ReportType? ReportType,
    ReportSnapshotStatus? Status);

public sealed record ReportExportFileDto(
    byte[] Content,
    string ContentType,
    string FileName);
