using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Application.Abstractions.Caching;
using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Identity.Application.Roles.GetRoles;
using Patsanstha.Modules.Identity.Domain.Authorization;
using Patsanstha.Modules.Identity.Infrastructure.Persistence;

namespace Patsanstha.Modules.Identity.Infrastructure.Services;

public sealed class RoleReader(
    IdentityDbContext dbContext,
    IReferenceDataCache referenceDataCache) : IRoleReader
{
    private const string AllRolesCacheKey = "ref:roles:all";

    public async Task<Result<IReadOnlyList<RoleDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await referenceDataCache.GetOrSetAsync(
            AllRolesCacheKey,
            LoadRolesAsync,
            cancellationToken: cancellationToken);

        return Result.Success(result);
    }

    private async Task<IReadOnlyList<RoleDto>> LoadRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await dbContext.Roles.AsNoTracking().ToListAsync(cancellationToken);

        var roleIds = roles.Select(r => r.Id).ToList();

        var claims = await dbContext.RoleClaims
            .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == Permissions.ClaimType)
            .ToListAsync(cancellationToken);

        return roles
            .Select(role =>
            {
                var perms = claims
                    .Where(c => c.RoleId == role.Id)
                    .Select(c => c.ClaimValue!)
                    .OrderBy(p => p)
                    .ToList();

                return new RoleDto(role.Id, role.Name!, perms);
            })
            .OrderBy(r => r.Name)
            .ToList();
    }
}
