using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.Modules.Identity.Application.Abstractions;

public interface IUserAdminService
{
    Task<Result<PagedUsersResponse>> ListAsync(
        ListUsersRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<UserSummaryDto>> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<UserSummaryDto>> UpdateAsync(
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<UserSummaryDto>> AssignRolesAsync(
        AssignUserRolesRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record ListUsersRequest(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? IsActive = null);

public sealed record PagedUsersResponse(
    IReadOnlyList<UserSummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string FullName,
    Guid? BranchId,
    IReadOnlyList<string> Roles);

public sealed record UpdateUserRequest(
    Guid UserId,
    string? FullName,
    Guid? BranchId,
    bool? IsActive);

public sealed record AssignUserRolesRequest(
    Guid UserId,
    IReadOnlyList<string> Roles);
