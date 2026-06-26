namespace Patsanstha.Modules.Identity.Application.Abstractions;

public sealed record UserSummaryDto(
    Guid Id,
    string Email,
    string FullName,
    Guid TenantId,
    Guid? BranchId,
    bool IsActive,
    IReadOnlyList<string> Roles);

public sealed record AuthLoginResponse
{
    public TokenPairResponse? Tokens { get; init; }

    public bool RequiresTwoFactor { get; init; }

    public string? TwoFactorChallengeToken { get; init; }

    public DateTimeOffset? TwoFactorChallengeExpiresAt { get; init; }

    public static AuthLoginResponse Complete(TokenPairResponse tokens) =>
        new() { Tokens = tokens };

    public static AuthLoginResponse TwoFactorRequired(
        string challengeToken,
        DateTimeOffset expiresAt) =>
        new()
        {
            RequiresTwoFactor = true,
            TwoFactorChallengeToken = challengeToken,
            TwoFactorChallengeExpiresAt = expiresAt,
        };
}

public sealed record TwoFactorSetupResponse(
    string SharedKey,
    string AuthenticatorUri);
