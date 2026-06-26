using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Identity.Application.Abstractions;

namespace Patsanstha.Modules.Identity.Application.Users.ListUsers;

public sealed record ListUsersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? IsActive = null) : IQuery<PagedUsersResponse>;

public sealed class ListUsersQueryValidator : AbstractValidator<ListUsersQuery>
{
    public ListUsersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithErrorCode("Users.Page.Invalid");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("Users.PageSize.Invalid");
    }
}

public sealed class ListUsersQueryHandler(IUserAdminService userAdminService)
    : IQueryHandler<ListUsersQuery, PagedUsersResponse>
{
    public Task<Result<PagedUsersResponse>> Handle(
        ListUsersQuery request,
        CancellationToken cancellationToken) =>
        userAdminService.ListAsync(
            new ListUsersRequest(request.Page, request.PageSize, request.Search, request.IsActive),
            cancellationToken);
}
