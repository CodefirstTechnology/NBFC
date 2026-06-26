using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.BuildingBlocks.Infrastructure.Persistence.Audit;

internal static class AuditChangeCapture
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    internal static bool ShouldAudit(EntityEntry entry) =>
        entry.Entity is not (AuditLogEntry or Outbox.OutboxMessageEntity) &&
        entry.Entity is (IAuditableEntity or Entity) &&
        entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted;

    internal static AuditAction ResolveAction(EntityEntry entry) =>
        entry.State switch
        {
            EntityState.Added => AuditAction.Created,
            EntityState.Deleted => AuditAction.Deleted,
            EntityState.Modified when entry.Entity is ISoftDeletable { IsDeleted: true } => AuditAction.Deleted,
            EntityState.Modified => AuditAction.Updated,
            _ => AuditAction.Updated,
        };

    internal static string? CaptureChanges(EntityEntry entry)
    {
        return entry.State switch
        {
            EntityState.Added => SerializeProperties(entry, useCurrentValues: true),
            EntityState.Deleted => SerializeProperties(entry, useCurrentValues: true),
            EntityState.Modified => SerializeModifiedProperties(entry),
            _ => null,
        };
    }

    internal static string ResolveEntityId(object entity)
    {
        if (entity is Entity domainEntity)
        {
            return domainEntity.Id.ToString();
        }

        var idProperty = entity.GetType().GetProperty("Id");
        return idProperty?.GetValue(entity)?.ToString() ?? string.Empty;
    }

    internal static Guid ResolveTenantId(object entity, Guid fallbackTenantId) =>
        entity is ITenantScoped tenantScoped && tenantScoped.TenantId != Guid.Empty
            ? tenantScoped.TenantId
            : fallbackTenantId;

    private static string? SerializeModifiedProperties(EntityEntry entry)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (!property.IsModified || ShouldSkipProperty(property))
            {
                continue;
            }

            changes[property.Metadata.Name] = new
            {
                old = property.OriginalValue,
                @new = property.CurrentValue,
            };
        }

        return changes.Count == 0 ? null : JsonSerializer.Serialize(changes, JsonOptions);
    }

    private static string? SerializeProperties(EntityEntry entry, bool useCurrentValues)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (ShouldSkipProperty(property))
            {
                continue;
            }

            values[property.Metadata.Name] = useCurrentValues
                ? property.CurrentValue
                : property.OriginalValue;
        }

        return values.Count == 0 ? null : JsonSerializer.Serialize(values, JsonOptions);
    }

    private static bool ShouldSkipProperty(PropertyEntry property) =>
        property.Metadata.Name is "RowVersion" or "ConcurrencyStamp";
}
