namespace Patsanstha.Modules.Identity.Application.Abstractions;

public interface ITokenService
{
    Task<Result<TokenPairResponse>> IssueTokensAsync(
        Guid userId,
        string email,
        string fullName,
        Guid tenantId,
        Guid? branchId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<Result<TokenPairResponse>> RotateRefreshTokenAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}

public interface IIdentityUserReader
{
    Task<Result<AuthenticatedUserResponse>> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticatedUserResponse>> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
}
