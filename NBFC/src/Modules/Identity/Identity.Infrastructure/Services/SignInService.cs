using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Identity.Application.Abstractions;
using Patsanstha.Modules.Identity.Application.Auth.Login;
using Patsanstha.Modules.Identity.Domain.Entities;
using Patsanstha.Modules.Identity.Domain.Authorization;
using Patsanstha.Modules.Identity.Infrastructure.Identity;
using Patsanstha.Modules.Identity.Infrastructure.Persistence;

namespace Patsanstha.Modules.Identity.Infrastructure.Services;

public sealed class SignInService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IdentityDbContext dbContext) : ISignInService, IIdentityUserReader
{
    public async Task<Result<AuthenticatedUserResponse>> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive || user.IsDeleted)
        {
            return Result.Failure<AuthenticatedUserResponse>(
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            var error = signInResult.IsLockedOut
                ? Error.Forbidden("Auth.LockedOut", "Account is locked due to repeated failed attempts.")
                : Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

            return Result.Failure<AuthenticatedUserResponse>(error);
        }

        return await MapUserAsync(user, cancellationToken);
    }

    public async Task<Result<TwoFactorChallengeResponse>> CreateTwoFactorChallengeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null || user.IsDeleted || !user.IsActive)
        {
            return Result.Failure<TwoFactorChallengeResponse>(
                Error.NotFound("Auth.User.NotFound", "User not found."));
        }

        var plainToken = TokenService.GenerateSecureToken();
        var challengeHash = TokenService.HashToken(plainToken);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);

        dbContext.TwoFactorLoginChallenges.Add(
            TwoFactorLoginChallenge.Create(user.Id, user.TenantId, challengeHash, expiresAt));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new TwoFactorChallengeResponse(plainToken, expiresAt));
    }

    public async Task<Result<AuthenticatedUserResponse>> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null || user.IsDeleted || !user.IsActive)
        {
            return Result.Failure<AuthenticatedUserResponse>(
                Error.NotFound("Auth.User.NotFound", "User not found."));
        }

        return await MapUserAsync(user, cancellationToken);
    }

    public async Task<Result<AuthenticatedUserResponse>> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || user.IsDeleted || !user.IsActive)
        {
            return Result.Failure<AuthenticatedUserResponse>(
                Error.NotFound("Auth.User.NotFound", "User not found."));
        }

        return await MapUserAsync(user, cancellationToken);
    }

    private async Task<Result<AuthenticatedUserResponse>> MapUserAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);

        var roleIds = await dbContext.Roles
            .Where(r => roles.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var permissions = await dbContext.RoleClaims
            .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == Permissions.ClaimType)
            .Select(rc => rc.ClaimValue!)
            .Distinct()
            .ToListAsync(cancellationToken);

        return Result.Success(new AuthenticatedUserResponse(
            user.Id,
            user.Email!,
            user.FullName,
            user.TenantId,
            user.BranchId,
            roles.ToList(),
            permissions));
    }
}
