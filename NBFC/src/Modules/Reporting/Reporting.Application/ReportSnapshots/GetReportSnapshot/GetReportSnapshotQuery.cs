using Patsanstha.Modules.Reporting.Application.Abstractions;

namespace Patsanstha.Modules.Reporting.Application.ReportSnapshots.GetReportSnapshot;

public sealed record GetReportSnapshotQuery(Guid ReportSnapshotId) : IQuery<ReportSnapshotDetailDto>;

public sealed class GetReportSnapshotQueryHandler(
    IReportSnapshotRepository repository,
    IReportSnapshotMapper mapper) : IQueryHandler<GetReportSnapshotQuery, ReportSnapshotDetailDto>
{
    public async Task<Result<ReportSnapshotDetailDto>> Handle(
        GetReportSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = await repository.GetByIdAsync(request.ReportSnapshotId, cancellationToken);

        if (snapshot is null)
        {
            return Result.Failure<ReportSnapshotDetailDto>(
                Error.NotFound("Reporting.Snapshot.NotFound", "Report snapshot not found."));
        }

        return Result.Success(mapper.ToDetail(snapshot));
    }
}
