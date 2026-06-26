using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.Modules.Identity.Application.Abstractions;

public interface ITwoFactorService
{
    Task<Result<TwoFactorSetupResponse>> BeginSetupAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> ConfirmSetupAsync(
        Guid userId,
        string verificationCode,
        CancellationToken cancellationToken = default);

    Task<Result> DisableAsync(
        Guid userId,
        string verificationCode,
        CancellationToken cancellationToken = default);

    Task<Result<AuthLoginResponse>> CompleteLoginAsync(
        string challengeToken,
        string verificationCode,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<bool> IsTwoFactorEnabledAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
