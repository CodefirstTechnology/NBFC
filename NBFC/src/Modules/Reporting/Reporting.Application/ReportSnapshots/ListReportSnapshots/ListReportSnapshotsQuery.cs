using FluentValidation;
using Patsanstha.Modules.Reporting.Application.Abstractions;
using Patsanstha.Modules.Reporting.Domain.Enums;

namespace Patsanstha.Modules.Reporting.Application.ReportSnapshots.ListReportSnapshots;

public sealed record ListReportSnapshotsQuery(
    int Page = 1,
    int PageSize = 20,
    ReportType? ReportType = null,
    ReportSnapshotStatus? Status = null) : IQuery<PagedReportSnapshotsResponse>;

public sealed class ListReportSnapshotsQueryValidator : AbstractValidator<ListReportSnapshotsQuery>
{
    public ListReportSnapshotsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithErrorCode("Reporting.Page.Invalid");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("Reporting.PageSize.Invalid");
    }
}

public sealed class ListReportSnapshotsQueryHandler(
    IReportSnapshotRepository repository,
    IReportSnapshotMapper mapper) : IQueryHandler<ListReportSnapshotsQuery, PagedReportSnapshotsResponse>
{
    public async Task<Result<PagedReportSnapshotsResponse>> Handle(
        ListReportSnapshotsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.ListAsync(
            new ListReportSnapshotsCriteria(
                request.Page,
                request.PageSize,
                request.ReportType,
                request.Status),
            cancellationToken);

        var summaries = items.Select(mapper.ToSummary).ToList();

        return Result.Success(new PagedReportSnapshotsResponse(
            summaries,
            request.Page,
            request.PageSize,
            totalCount));
    }
}
