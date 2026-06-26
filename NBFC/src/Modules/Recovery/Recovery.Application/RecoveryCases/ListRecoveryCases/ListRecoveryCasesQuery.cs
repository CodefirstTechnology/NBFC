using FluentValidation;
using Patsanstha.Modules.Recovery.Application.Abstractions;
using Patsanstha.Modules.Recovery.Domain.Enums;

namespace Patsanstha.Modules.Recovery.Application.RecoveryCases.ListRecoveryCases;

public sealed record ListRecoveryCasesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    RecoveryCaseStatus? Status = null,
    Guid? BranchId = null,
    Guid? MemberId = null,
    Guid? AssignedToUserId = null) : IQuery<PagedRecoveryCasesResponse>;

public sealed class ListRecoveryCasesQueryValidator : AbstractValidator<ListRecoveryCasesQuery>
{
    public ListRecoveryCasesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithErrorCode("Recovery.Page.Invalid");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("Recovery.PageSize.Invalid");
    }
}

public sealed class ListRecoveryCasesQueryHandler(
    IRecoveryCaseRepository repository,
    IRecoveryCaseMapper mapper) : IQueryHandler<ListRecoveryCasesQuery, PagedRecoveryCasesResponse>
{
    public async Task<Result<PagedRecoveryCasesResponse>> Handle(
        ListRecoveryCasesQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.ListAsync(
            new ListRecoveryCasesCriteria(
                request.Page,
                request.PageSize,
                request.Search,
                request.Status,
                request.BranchId,
                request.MemberId,
                request.AssignedToUserId),
            cancellationToken);

        var summaries = items.Select(mapper.ToSummary).ToList();

        return Result.Success(new PagedRecoveryCasesResponse(
            summaries,
            request.Page,
            request.PageSize,
            totalCount));
    }
}
