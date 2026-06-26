using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.Modules.Identity.Domain.Entities;

public sealed class RefreshToken : AuditableEntity
{
    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public string? CreatedByIp { get; private set; }

    public static RefreshToken Create(
        Guid userId,
        Guid tenantId,
        string tokenHash,
        DateTimeOffset expiresAt,
        string? createdByIp)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;

    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
