namespace Patsanstha.Contracts.Abstractions;

public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOn { get; }

    Guid TenantId { get; }

    string EventType { get; }
}

public abstract record IntegrationEventBase : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;

    public required Guid TenantId { get; init; }

    public virtual string EventType => GetType().Name;
}
