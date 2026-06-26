using System.Text.Json;
using System.Text.Json.Serialization;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.Contracts.Abstractions;

namespace Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;

public sealed class IntegrationEventSerializer : IIntegrationEventSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public string Serialize<TEvent>(TEvent integrationEvent) where TEvent : class =>
        JsonSerializer.Serialize(integrationEvent, Options);

    public TEvent? Deserialize<TEvent>(string payload) where TEvent : class =>
        JsonSerializer.Deserialize<TEvent>(payload, Options);
}

public sealed class OutboxWriter
{
    public OutboxMessageEntity CreateMessage(
        IIntegrationEvent integrationEvent,
        string payload,
        string? correlationId)
    {
        return new OutboxMessageEntity
        {
            Id = integrationEvent.EventId,
            TenantId = integrationEvent.TenantId,
            EventType = integrationEvent.EventType,
            Payload = payload,
            OccurredOn = integrationEvent.OccurredOn,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0,
            CorrelationId = correlationId,
        };
    }
}
