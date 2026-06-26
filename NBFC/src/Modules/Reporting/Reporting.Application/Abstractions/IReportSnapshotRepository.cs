using Patsanstha.Modules.Reporting.Domain.Entities;
using Patsanstha.Modules.Reporting.Domain.Enums;

namespace Patsanstha.Modules.Reporting.Application.Abstractions;

public interface IReportSnapshotRepository
{
    Task AddAsync(ReportSnapshot snapshot, CancellationToken cancellationToken = default);

    Task<ReportSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ReportSnapshot> Items, int TotalCount)> ListAsync(
        ListReportSnapshotsCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IReportSnapshotMapper
{
    ReportSnapshotSummaryDto ToSummary(ReportSnapshot snapshot);

    ReportSnapshotDetailDto ToDetail(ReportSnapshot snapshot);
}
