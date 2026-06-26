using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Audit;

namespace Patsanstha.BuildingBlocks.Infrastructure.Persistence.Interceptors;

public sealed class AuditSaveChangesInterceptor(IAuditContextAccessor auditContextAccessor)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var auditContext = auditContextAccessor.Current;
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = auditContext.UserId;
                    entry.Entity.ModifiedAt = now;
                    entry.Entity.ModifiedBy = auditContext.UserId;

                    if (entry.Entity is ITenantScoped tenantEntity && tenantEntity.TenantId == Guid.Empty)
                    {
                        tenantEntity.TenantId = auditContext.TenantId;
                    }

                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedAt = now;
                    entry.Entity.ModifiedBy = auditContext.UserId;
                    break;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;

                if (entry.Entity is IAuditableEntity auditable)
                {
                    auditable.ModifiedAt = now;
                    auditable.ModifiedBy = auditContext.UserId;
                }
            }
        }

        WriteAuditLogs(context, auditContext, now);
    }

    private static void WriteAuditLogs(DbContext context, IAuditContext auditContext, DateTimeOffset now)
    {
        var auditSet = context.Set<AuditLogEntry>();

        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            if (!AuditChangeCapture.ShouldAudit(entry))
            {
                continue;
            }

            auditSet.Add(new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                TenantId = AuditChangeCapture.ResolveTenantId(entry.Entity, auditContext.TenantId),
                EntityType = entry.Entity.GetType().Name,
                EntityId = AuditChangeCapture.ResolveEntityId(entry.Entity),
                Action = AuditChangeCapture.ResolveAction(entry),
                Changes = AuditChangeCapture.CaptureChanges(entry),
                UserId = auditContext.UserId,
                Timestamp = now,
                CorrelationId = auditContext.CorrelationId,
            });
        }
    }
}
