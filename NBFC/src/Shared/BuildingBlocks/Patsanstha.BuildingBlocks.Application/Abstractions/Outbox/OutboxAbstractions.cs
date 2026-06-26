namespace Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;

public enum OutboxMessageStatus
{
    Pending = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3,
}

public sealed class OutboxMessage
{
    public Guid Id { get; init; }

    public Guid TenantId { get; init; }

    public string EventType { get; init; } = string.Empty;

    public string Payload { get; init; } = string.Empty;

    public DateTimeOffset OccurredOn { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public OutboxMessageStatus Status { get; set; }

    public int RetryCount { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public string? CorrelationId { get; init; }
}

public interface IOutboxProcessor
{
    Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default);
}

public interface IIntegrationEventHandler
{
    string EventType { get; }

    Task HandleAsync(string payload, CancellationToken cancellationToken = default);
}

public interface IIntegrationEventSerializer
{
    string Serialize<TEvent>(TEvent integrationEvent) where TEvent : class;

    TEvent? Deserialize<TEvent>(string payload) where TEvent : class;
}
