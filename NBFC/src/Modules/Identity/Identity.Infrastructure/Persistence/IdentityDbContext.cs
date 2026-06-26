using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Audit;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;
using Patsanstha.Modules.Identity.Domain.Entities;
using Patsanstha.Modules.Identity.Infrastructure.Identity;

namespace Patsanstha.Modules.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(
    DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options),
        IOutboxDbContext
{
    public const string Schema = "identity";

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<TwoFactorLoginChallenge> TwoFactorLoginChallenges => Set<TwoFactorLoginChallenge>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(Schema);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users");
            entity.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            entity.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
            entity.HasQueryFilter(u => !u.IsDeleted);
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("roles");
            entity.Property(r => r.Description).HasMaxLength(500);
        });

        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(t => t.ReplacedByTokenHash).HasMaxLength(128);
            entity.Property(t => t.CreatedByIp).HasMaxLength(64);
            entity.Property(t => t.RowVersion).ConfigureRowVersion();
            entity.HasIndex(t => new { t.TenantId, t.UserId, t.TokenHash });
            entity.HasIndex(t => new { t.TokenHash, t.RevokedAt });
            entity.HasQueryFilter(t => !t.IsDeleted);
        });

        builder.Entity<TwoFactorLoginChallenge>(entity =>
        {
            entity.ToTable("two_factor_login_challenges");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ChallengeHash).HasMaxLength(128).IsRequired();
            entity.Property(c => c.RowVersion).ConfigureRowVersion();
            entity.HasIndex(c => new { c.ChallengeHash, c.ConsumedAt });
            entity.HasIndex(c => new { c.TenantId, c.UserId });
            entity.HasQueryFilter(c => !c.IsDeleted);
        });

        OutboxModelConfiguration.ConfigureOutbox(builder, Schema);
        AuditLogModelConfiguration.ConfigureAuditLog(builder, Schema);
    }

    async Task<IReadOnlyList<OutboxMessageEntity>> IOutboxDbContext.GetPendingOutboxMessagesAsync(
        CancellationToken cancellationToken) =>
        await OutboxDbContextExtensions.GetPendingOutboxMessagesAsync(this, cancellationToken: cancellationToken);

    Task IOutboxDbContext.SaveChangesAsync(CancellationToken cancellationToken) =>
        SaveChangesAsync(cancellationToken);
}
