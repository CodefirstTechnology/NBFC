using FluentValidation;
using Patsanstha.Modules.Deposits.Application.Abstractions;
using Patsanstha.Modules.Deposits.Domain.Enums;

namespace Patsanstha.Modules.Deposits.Application.DepositAccounts.ListDepositAccounts;

public sealed record ListDepositAccountsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    DepositProductType? ProductType = null,
    DepositAccountStatus? Status = null,
    Guid? BranchId = null,
    Guid? MemberId = null) : IQuery<PagedDepositAccountsResponse>;

public sealed class ListDepositAccountsQueryValidator : AbstractValidator<ListDepositAccountsQuery>
{
    public ListDepositAccountsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithErrorCode("Deposits.Page.Invalid");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("Deposits.PageSize.Invalid");
    }
}

public sealed class ListDepositAccountsQueryHandler(
    IDepositAccountRepository repository,
    IDepositAccountMapper mapper) : IQueryHandler<ListDepositAccountsQuery, PagedDepositAccountsResponse>
{
    public async Task<Result<PagedDepositAccountsResponse>> Handle(
        ListDepositAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.ListAsync(
            new ListDepositAccountsCriteria(
                request.Page,
                request.PageSize,
                request.Search,
                request.ProductType,
                request.Status,
                request.BranchId,
                request.MemberId),
            cancellationToken);

        var summaries = items.Select(mapper.ToSummary).ToList();

        return Result.Success(new PagedDepositAccountsResponse(
            summaries,
            request.Page,
            request.PageSize,
            totalCount));
    }
}
