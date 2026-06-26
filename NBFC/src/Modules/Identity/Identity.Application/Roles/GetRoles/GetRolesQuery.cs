using Patsanstha.BuildingBlocks.Application.Abstractions.Caching;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Identity.Application.Abstractions;
using Patsanstha.Modules.Identity.Domain.Authorization;

namespace Patsanstha.Modules.Identity.Application.Roles.GetRoles;

public sealed record RoleDto(Guid Id, string Name, IReadOnlyList<string> Permissions);

public sealed record GetRolesQuery : IQuery<IReadOnlyList<RoleDto>>;

public sealed class GetRolesQueryHandler(IRoleReader roleReader) : IQueryHandler<GetRolesQuery, IReadOnlyList<RoleDto>>
{
    public Task<Result<IReadOnlyList<RoleDto>>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken) =>
        roleReader.GetAllAsync(cancellationToken);
}

public interface IRoleReader
{
    Task<Result<IReadOnlyList<RoleDto>>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed record GetPermissionsQuery : IQuery<IReadOnlyList<string>>;

public sealed class GetPermissionsQueryHandler(IReferenceDataCache referenceDataCache)
    : IQueryHandler<GetPermissionsQuery, IReadOnlyList<string>>
{
    public async Task<Result<IReadOnlyList<string>>> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await referenceDataCache.GetOrSetAsync(
            CacheKeys.Permissions,
            _ => Task.FromResult(Permissions.All),
            cancellationToken: cancellationToken);

        return Result.Success(permissions);
    }
}
