using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;
using Patsanstha.Contracts.Abstractions;

namespace Patsanstha.BuildingBlocks.Infrastructure.Persistence.Interceptors;

public sealed class DispatchDomainEventsInterceptor(
    IIntegrationEventSerializer serializer,
    OutboxWriter outboxWriter,
    IAuditContextAccessor auditContextAccessor)
    : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        DispatchDomainEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        DispatchDomainEvents(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void DispatchDomainEvents(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var aggregates = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        if (aggregates.Count == 0)
        {
            return;
        }

        var outboxSet = context.Set<OutboxMessageEntity>();
        var correlationId = auditContextAccessor.Current.CorrelationId;
        var tenantId = auditContextAccessor.Current.TenantId;

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var integrationEvent = ToIntegrationEvent(domainEvent, tenantId);
                var payload = serializer.Serialize(integrationEvent);
                outboxSet.Add(outboxWriter.CreateMessage(integrationEvent, payload, correlationId));
            }

            aggregate.ClearDomainEvents();
        }
    }

    private static IIntegrationEvent ToIntegrationEvent(IDomainEvent domainEvent, Guid tenantId)
    {
        if (domainEvent is IIntegrationEvent integrationEvent)
        {
            return integrationEvent;
        }

        return new DomainEventEnvelope
        {
            EventId = domainEvent.EventId,
            OccurredOn = domainEvent.OccurredOn,
            TenantId = tenantId,
            EventType = domainEvent.GetType().Name,
            Payload = domainEvent,
        };
    }

    internal sealed class DomainEventEnvelope : IIntegrationEvent
    {
        public required Guid EventId { get; init; }

        public required DateTimeOffset OccurredOn { get; init; }

        public required Guid TenantId { get; init; }

        public required string EventType { get; init; }

        public required object Payload { get; init; }
    }
}
