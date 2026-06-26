namespace Patsanstha.Modules.Identity.Application.Abstractions;

public sealed record TokenPairResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

public sealed record AuthenticatedUserResponse(
    Guid UserId,
    string Email,
    string FullName,
    Guid TenantId,
    Guid? BranchId,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
