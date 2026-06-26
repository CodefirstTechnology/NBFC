using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;

namespace Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTimeOffset OccurredOn { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public OutboxMessageStatus Status { get; set; }

    public int RetryCount { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public string? CorrelationId { get; set; }
}

public static class OutboxModelConfiguration
{
    public static void ConfigureOutbox(ModelBuilder modelBuilder, string schema)
    {
        modelBuilder.Entity<OutboxMessageEntity>(entity =>
        {
            entity.ToTable("outbox_messages", schema);
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EventType).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Payload).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.RetryCount).IsRequired();
            entity.Property(x => x.LastError).HasMaxLength(4000);
            entity.Property(x => x.CorrelationId).HasMaxLength(128);

            entity.HasIndex(x => new { x.Status, x.CreatedAt })
                .HasDatabaseName("ix_outbox_messages_status_created_at");

            entity.HasIndex(x => new { x.TenantId, x.Status })
                .HasDatabaseName("ix_outbox_messages_tenant_status");
        });
    }
}
