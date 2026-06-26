using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Identity.Application.Abstractions;
using Patsanstha.Modules.Identity.Application.Auth.Login;
using Patsanstha.Modules.Identity.Domain.Authorization;
using Patsanstha.Modules.Identity.Domain.Entities;
using Patsanstha.Modules.Identity.Infrastructure.Identity;
using Patsanstha.Modules.Identity.Infrastructure.Persistence;

namespace Patsanstha.Modules.Identity.Infrastructure.Services;

public sealed class TwoFactorService(
    UserManager<ApplicationUser> userManager,
    IdentityDbContext dbContext,
    ITokenService tokenService,
    IIdentityUserReader userReader) : ITwoFactorService
{
    private const string AuthenticatorIssuer = "Patsanstha";

    public async Task<Result<TwoFactorSetupResponse>> BeginSetupAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted || !user.IsActive)
        {
            return Result.Failure<TwoFactorSetupResponse>(
                Error.NotFound("Auth.User.NotFound", "User not found."));
        }

        var eligibility = await EnsureEligibleForTwoFactorAsync(user);
        if (eligibility.IsFailure)
        {
            return Result.Failure<TwoFactorSetupResponse>(eligibility.Error);
        }

        await userManager.ResetAuthenticatorKeyAsync(user);
        var key = await userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(key))
        {
            return Result.Failure<TwoFactorSetupResponse>(
                Error.Failure("Auth.TwoFactor.SetupFailed", "Unable to generate authenticator key."));
        }

        var uri = BuildAuthenticatorUri(user.Email!, key);

        return Result.Success(new TwoFactorSetupResponse(key, uri));
    }

    public async Task<Result> ConfirmSetupAsync(
        Guid userId,
        string verificationCode,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted || !user.IsActive)
        {
            return Result.Failure(Error.NotFound("Auth.User.NotFound", "User not found."));
        }

        var eligibility = await EnsureEligibleForTwoFactorAsync(user);
        if (eligibility.IsFailure)
        {
            return eligibility;
        }

        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            verificationCode);

        if (!isValid)
        {
            return Result.Failure(
                Error.Validation("Auth.TwoFactor.InvalidCode", "Invalid verification code."));
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        return Result.Success();
    }

    public async Task<Result> DisableAsync(
        Guid userId,
        string verificationCode,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted || !user.IsActive)
        {
            return Result.Failure(Error.NotFound("Auth.User.NotFound", "User not found."));
        }

        if (!await userManager.GetTwoFactorEnabledAsync(user))
        {
            return Result.Success();
        }

        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            verificationCode);

        if (!isValid)
        {
            return Result.Failure(
                Error.Validation("Auth.TwoFactor.InvalidCode", "Invalid verification code."));
        }

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);
        return Result.Success();
    }

    public async Task<Result<AuthLoginResponse>> CompleteLoginAsync(
        string challengeToken,
        string verificationCode,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var challengeHash = TokenService.HashToken(challengeToken);

        var challenge = await dbContext.TwoFactorLoginChallenges
            .FirstOrDefaultAsync(c => c.ChallengeHash == challengeHash, cancellationToken);

        if (challenge is null || !challenge.IsActive)
        {
            return Result.Failure<AuthLoginResponse>(
                Error.Unauthorized("Auth.Challenge.Invalid", "Two-factor challenge is invalid or expired."));
        }

        var user = await userManager.FindByIdAsync(challenge.UserId.ToString());
        if (user is null || user.IsDeleted || !user.IsActive)
        {
            return Result.Failure<AuthLoginResponse>(
                Error.Unauthorized("Auth.User.Inactive", "User account is inactive."));
        }

        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            verificationCode);

        if (!isValid)
        {
            return Result.Failure<AuthLoginResponse>(
                Error.Unauthorized("Auth.TwoFactor.InvalidCode", "Invalid verification code."));
        }

        challenge.Consume();
        await dbContext.SaveChangesAsync(cancellationToken);

        var userResult = await userReader.GetByIdAsync(user.Id, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<AuthLoginResponse>(userResult.Error);
        }

        var profile = userResult.Value;
        var tokenResult = await tokenService.IssueTokensAsync(
            profile.UserId,
            profile.Email,
            profile.FullName,
            profile.TenantId,
            profile.BranchId,
            profile.Roles,
            profile.Permissions,
            ipAddress,
            cancellationToken);

        return tokenResult.IsSuccess
            ? Result.Success(AuthLoginResponse.Complete(tokenResult.Value))
            : Result.Failure<AuthLoginResponse>(tokenResult.Error);
    }

    public async Task<bool> IsTwoFactorEnabledAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is not null && await userManager.GetTwoFactorEnabledAsync(user);
    }

    internal static string BuildAuthenticatorUri(string email, string sharedKey) =>
        $"otpauth://totp/{Uri.EscapeDataString(AuthenticatorIssuer)}:{Uri.EscapeDataString(email)}" +
        $"?secret={sharedKey}&issuer={Uri.EscapeDataString(AuthenticatorIssuer)}&digits=6";

    private async Task<Result> EnsureEligibleForTwoFactorAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        if (!roles.Any(TwoFactorPolicy.IsEligibleRole))
        {
            return Result.Failure(
                Error.Forbidden(
                    "Auth.TwoFactor.NotEligible",
                    "Two-factor authentication is only available for Chairman, Branch Manager, and System Admin roles."));
        }

        return Result.Success();
    }
}
