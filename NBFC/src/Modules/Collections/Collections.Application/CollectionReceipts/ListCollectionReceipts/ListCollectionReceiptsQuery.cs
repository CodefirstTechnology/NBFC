using FluentValidation;
using Patsanstha.Modules.Collections.Application.Abstractions;
using Patsanstha.Modules.Collections.Domain.Enums;

namespace Patsanstha.Modules.Collections.Application.CollectionReceipts.ListCollectionReceipts;

public sealed record ListCollectionReceiptsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? BranchId = null,
    Guid? MemberId = null,
    string? LoanNumber = null) : IQuery<PagedCollectionReceiptsResponse>;

public sealed class ListCollectionReceiptsQueryValidator : AbstractValidator<ListCollectionReceiptsQuery>
{
    public ListCollectionReceiptsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithErrorCode("Collections.Page.Invalid");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("Collections.PageSize.Invalid");
    }
}

public sealed class ListCollectionReceiptsQueryHandler(
    ICollectionReceiptRepository repository,
    ICollectionReceiptMapper mapper) : IQueryHandler<ListCollectionReceiptsQuery, PagedCollectionReceiptsResponse>
{
    public async Task<Result<PagedCollectionReceiptsResponse>> Handle(
        ListCollectionReceiptsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.ListAsync(
            new ListCollectionReceiptsCriteria(
                request.Page,
                request.PageSize,
                request.Search,
                request.BranchId,
                request.MemberId,
                request.LoanNumber),
            cancellationToken);

        var summaries = items.Select(mapper.ToSummary).ToList();

        return Result.Success(new PagedCollectionReceiptsResponse(
            summaries,
            request.Page,
            request.PageSize,
            totalCount));
    }
}
