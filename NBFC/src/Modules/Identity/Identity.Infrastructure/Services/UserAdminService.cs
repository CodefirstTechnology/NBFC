using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Identity.Application.Abstractions;
using Patsanstha.Modules.Identity.Domain.Authorization;
using Patsanstha.Modules.Identity.Infrastructure.Identity;
using Patsanstha.Modules.Identity.Infrastructure.Persistence;

namespace Patsanstha.Modules.Identity.Infrastructure.Services;

public sealed class UserAdminService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IdentityDbContext dbContext,
    IAuditContextAccessor auditContextAccessor) : IUserAdminService
{
    public async Task<Result<PagedUsersResponse>> ListAsync(
        ListUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = auditContextAccessor.Current.TenantId;

        var query = dbContext.Users.AsNoTracking()
            .Where(u => u.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(search) ||
                u.FullName.ToLower().Contains(search));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.FullName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = new List<UserSummaryDto>();

        foreach (var user in users)
        {
            items.Add(await MapUserSummaryAsync(user, cancellationToken));
        }

        return Result.Success(new PagedUsersResponse(items, request.Page, request.PageSize, totalCount));
    }

    public async Task<Result<UserSummaryDto>> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = auditContextAccessor.Current.TenantId;

        if (tenantId == Guid.Empty)
        {
            return Result.Failure<UserSummaryDto>(
                Error.Validation("Users.Tenant.Required", "Tenant context is required."));
        }

        var roleValidation = await ValidateRolesAsync(request.Roles, cancellationToken);
        if (roleValidation.IsFailure)
        {
            return Result.Failure<UserSummaryDto>(roleValidation.Error);
        }

        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null && existing.TenantId == tenantId && !existing.IsDeleted)
        {
            return Result.Failure<UserSummaryDto>(
                Error.Conflict("Users.Email.Exists", "A user with this email already exists."));
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            NormalizedEmail = request.Email.ToUpperInvariant(),
            NormalizedUserName = request.Email.ToUpperInvariant(),
            FullName = request.FullName,
            TenantId = tenantId,
            BranchId = request.BranchId,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = auditContextAccessor.Current.UserId,
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var message = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result.Failure<UserSummaryDto>(
                Error.Validation("Users.Create.Failed", message));
        }

        var roleResult = await userManager.AddToRolesAsync(user, request.Roles);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            var message = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            return Result.Failure<UserSummaryDto>(
                Error.Validation("Users.Roles.AssignFailed", message));
        }

        return Result.Success(await MapUserSummaryAsync(user, cancellationToken));
    }

    public async Task<Result<UserSummaryDto>> UpdateAsync(
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var userResult = await GetTenantUserAsync(request.UserId, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<UserSummaryDto>(userResult.Error);
        }

        var user = userResult.Value;

        if (request.FullName is not null)
        {
            user.FullName = request.FullName;
        }

        if (request.BranchId.HasValue)
        {
            user.BranchId = request.BranchId;
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        user.ModifiedAt = DateTimeOffset.UtcNow;
        user.ModifiedBy = auditContextAccessor.Current.UserId;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var message = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            return Result.Failure<UserSummaryDto>(
                Error.Validation("Users.Update.Failed", message));
        }

        return Result.Success(await MapUserSummaryAsync(user, cancellationToken));
    }

    public async Task<Result<UserSummaryDto>> AssignRolesAsync(
        AssignUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var userResult = await GetTenantUserAsync(request.UserId, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<UserSummaryDto>(userResult.Error);
        }

        var roleValidation = await ValidateRolesAsync(request.Roles, cancellationToken);
        if (roleValidation.IsFailure)
        {
            return Result.Failure<UserSummaryDto>(roleValidation.Error);
        }

        var user = userResult.Value;
        var currentRoles = await userManager.GetRolesAsync(user);

        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            var message = string.Join("; ", removeResult.Errors.Select(e => e.Description));
            return Result.Failure<UserSummaryDto>(
                Error.Validation("Users.Roles.RemoveFailed", message));
        }

        var addResult = await userManager.AddToRolesAsync(user, request.Roles);
        if (!addResult.Succeeded)
        {
            var message = string.Join("; ", addResult.Errors.Select(e => e.Description));
            return Result.Failure<UserSummaryDto>(
                Error.Validation("Users.Roles.AssignFailed", message));
        }

        user.ModifiedAt = DateTimeOffset.UtcNow;
        user.ModifiedBy = auditContextAccessor.Current.UserId;
        await userManager.UpdateAsync(user);

        return Result.Success(await MapUserSummaryAsync(user, cancellationToken));
    }

    private async Task<Result<ApplicationUser>> GetTenantUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var tenantId = auditContextAccessor.Current.TenantId;

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId, cancellationToken);

        if (user is null || user.IsDeleted)
        {
            return Result.Failure<ApplicationUser>(
                Error.NotFound("Users.NotFound", "User not found."));
        }

        return Result.Success(user);
    }

    private async Task<Result> ValidateRolesAsync(
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken)
    {
        foreach (var roleName in roles)
        {
            if (!SystemRoles.All.Contains(roleName, StringComparer.OrdinalIgnoreCase))
            {
                return Result.Failure(
                    Error.Validation("Users.Role.Invalid", $"Role '{roleName}' is not a valid system role."));
            }

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                return Result.Failure(
                    Error.Validation("Users.Role.NotSeeded", $"Role '{roleName}' has not been provisioned."));
            }
        }

        return Result.Success();
    }

    private async Task<UserSummaryDto> MapUserSummaryAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new UserSummaryDto(
            user.Id,
            user.Email!,
            user.FullName,
            user.TenantId,
            user.BranchId,
            user.IsActive,
            roles.ToList());
    }
}
