using Microsoft.EntityFrameworkCore;

namespace Patsanstha.BuildingBlocks.Infrastructure.Persistence.Audit;

public enum AuditAction
{
    Created = 0,
    Updated = 1,
    Deleted = 2,
}

public sealed class AuditLogEntry
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public AuditAction Action { get; set; }

    public string? Changes { get; set; }

    public Guid? UserId { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public string? CorrelationId { get; set; }
}

public static class AuditLogModelConfiguration
{
    public static void ConfigureAuditLog(ModelBuilder modelBuilder, string schema)
    {
        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.ToTable("audit_logs", schema);
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EntityType).HasMaxLength(256).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Action).IsRequired();
            entity.Property(x => x.Changes).HasColumnType("jsonb");
            entity.Property(x => x.CorrelationId).HasMaxLength(128);
            entity.Property(x => x.Timestamp).IsRequired();

            entity.HasIndex(x => new { x.TenantId, x.Timestamp })
                .HasDatabaseName("ix_audit_logs_tenant_timestamp");

            entity.HasIndex(x => new { x.EntityType, x.EntityId })
                .HasDatabaseName("ix_audit_logs_entity");
        });
    }
}
