using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.Modules.Identity.Domain.Entities;

public sealed class TwoFactorLoginChallenge : AuditableEntity
{
    public Guid UserId { get; private set; }

    public string ChallengeHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? ConsumedAt { get; private set; }

    public bool IsActive => ConsumedAt is null && DateTimeOffset.UtcNow < ExpiresAt;

    public static TwoFactorLoginChallenge Create(
        Guid userId,
        Guid tenantId,
        string challengeHash,
        DateTimeOffset expiresAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            ChallengeHash = challengeHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
        };

    public void Consume() => ConsumedAt = DateTimeOffset.UtcNow;
}
