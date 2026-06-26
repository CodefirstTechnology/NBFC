namespace Patsanstha.BuildingBlocks.Domain.Abstractions;

public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOn { get; }
}

public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
