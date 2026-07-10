using Patsanstha.Modules.Reporting.Application.Abstractions;

namespace Patsanstha.Modules.Reporting.Application.ReportSnapshots.DownloadReportSnapshot;

public sealed record DownloadReportSnapshotQuery(
    Guid ReportSnapshotId,
    ReportExportFormat Format = ReportExportFormat.Csv) : IQuery<ReportExportFileDto>;

public sealed class DownloadReportSnapshotQueryHandler(
    IReportSnapshotRepository repository,
    IReportSnapshotMapper mapper) : IQueryHandler<DownloadReportSnapshotQuery, ReportExportFileDto>
{
    public async Task<Result<ReportExportFileDto>> Handle(
        DownloadReportSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = await repository.GetByIdAsync(request.ReportSnapshotId, cancellationToken);
        if (snapshot is null)
        {
            return Result.Failure<ReportExportFileDto>(
                Error.NotFound("Reporting.Snapshot.NotFound", "Report snapshot not found."));
        }

        if (snapshot.Status != Domain.Enums.ReportSnapshotStatus.Completed)
        {
            return Result.Failure<ReportExportFileDto>(
                Error.Validation("Reporting.Snapshot.NotReady", "Only completed reports can be downloaded."));
        }

        var detail = mapper.ToDetail(snapshot);
        return Result.Success(ReportExportFormatter.Format(detail, request.Format));
    }
}
