using Patsanstha.Contracts.Abstractions;

namespace Patsanstha.Contracts.Jobs;

public sealed record EmiGenerationRequestedIntegrationEvent : IntegrationEventBase
{
    public required DateOnly RunDate { get; init; }
}

public sealed record InterestAccrualRequestedIntegrationEvent : IntegrationEventBase
{
    public required DateOnly RunDate { get; init; }
}

public sealed record AgeingRecalculationRequestedIntegrationEvent : IntegrationEventBase
{
    public required DateOnly RunDate { get; init; }
}

public sealed record NotificationDispatchRequestedIntegrationEvent : IntegrationEventBase;

public sealed record ReportPreAggregationRequestedIntegrationEvent : IntegrationEventBase
{
    public required DateOnly RunDate { get; init; }
}
