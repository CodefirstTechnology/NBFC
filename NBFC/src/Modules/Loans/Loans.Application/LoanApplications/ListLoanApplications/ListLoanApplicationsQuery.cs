using FluentValidation;
using Patsanstha.Modules.Loans.Application.Abstractions;
using Patsanstha.Modules.Loans.Domain.Enums;

namespace Patsanstha.Modules.Loans.Application.LoanApplications.ListLoanApplications;

public sealed record ListLoanApplicationsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    LoanProductType? ProductType = null,
    LoanApplicationStatus? Status = null,
    Guid? BranchId = null,
    Guid? MemberId = null) : IQuery<PagedLoanApplicationsResponse>;

public sealed class ListLoanApplicationsQueryValidator : AbstractValidator<ListLoanApplicationsQuery>
{
    public ListLoanApplicationsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithErrorCode("Loans.Page.Invalid");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("Loans.PageSize.Invalid");
    }
}

public sealed class ListLoanApplicationsQueryHandler(
    ILoanApplicationRepository repository,
    ILoanApplicationMapper mapper) : IQueryHandler<ListLoanApplicationsQuery, PagedLoanApplicationsResponse>
{
    public async Task<Result<PagedLoanApplicationsResponse>> Handle(
        ListLoanApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.ListAsync(
            new ListLoanApplicationsCriteria(
                request.Page,
                request.PageSize,
                request.Search,
                request.ProductType,
                request.Status,
                request.BranchId,
                request.MemberId),
            cancellationToken);

        var summaries = items.Select(mapper.ToSummary).ToList();

        return Result.Success(new PagedLoanApplicationsResponse(
            summaries,
            request.Page,
            request.PageSize,
            totalCount));
    }
}
