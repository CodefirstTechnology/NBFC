using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.Contracts.Jobs;

namespace Patsanstha.BuildingBlocks.Infrastructure.BackgroundJobs;

public sealed class EmiGenerationJob(
    IIntegrationEventSerializer eventSerializer,
    IOptions<BackgroundJobOptions> options,
    ILogger<EmiGenerationJob> logger)
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var runDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var integrationEvent = new EmiGenerationRequestedIntegrationEvent
        {
            TenantId = options.Value.DefaultTenantId,
            RunDate = runDate,
        };

        var payload = eventSerializer.Serialize(integrationEvent);
        logger.LogInformation(
            "EMI generation job queued event {EventType} for tenant {TenantId} on {RunDate}. Payload length: {PayloadLength}",
            integrationEvent.EventType,
            integrationEvent.TenantId,
            runDate,
            payload.Length);

        return Task.CompletedTask;
    }
}

public sealed class InterestAccrualJob(
    IIntegrationEventSerializer eventSerializer,
    IOptions<BackgroundJobOptions> options,
    ILogger<InterestAccrualJob> logger)
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var runDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var integrationEvent = new InterestAccrualRequestedIntegrationEvent
        {
            TenantId = options.Value.DefaultTenantId,
            RunDate = runDate,
        };

        var payload = eventSerializer.Serialize(integrationEvent);
        logger.LogInformation(
            "Interest accrual job queued event {EventType} for tenant {TenantId} on {RunDate}. Payload length: {PayloadLength}",
            integrationEvent.EventType,
            integrationEvent.TenantId,
            runDate,
            payload.Length);

        return Task.CompletedTask;
    }
}

public sealed class AgeingRecalculationJob(
    IIntegrationEventSerializer eventSerializer,
    IOptions<BackgroundJobOptions> options,
    ILogger<AgeingRecalculationJob> logger)
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var runDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var integrationEvent = new AgeingRecalculationRequestedIntegrationEvent
        {
            TenantId = options.Value.DefaultTenantId,
            RunDate = runDate,
        };

        var payload = eventSerializer.Serialize(integrationEvent);
        logger.LogInformation(
            "Ageing recalculation job queued event {EventType} for tenant {TenantId} on {RunDate}. Payload length: {PayloadLength}",
            integrationEvent.EventType,
            integrationEvent.TenantId,
            runDate,
            payload.Length);

        return Task.CompletedTask;
    }
}

public sealed class NotificationDispatchJob(
    IIntegrationEventSerializer eventSerializer,
    IOptions<BackgroundJobOptions> options,
    ILogger<NotificationDispatchJob> logger)
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var integrationEvent = new NotificationDispatchRequestedIntegrationEvent
        {
            TenantId = options.Value.DefaultTenantId,
        };

        var payload = eventSerializer.Serialize(integrationEvent);
        logger.LogInformation(
            "Notification dispatch job queued event {EventType} for tenant {TenantId}. Payload length: {PayloadLength}",
            integrationEvent.EventType,
            integrationEvent.TenantId,
            payload.Length);

        return Task.CompletedTask;
    }
}

public sealed class ReportPreAggregationJob(
    IIntegrationEventSerializer eventSerializer,
    IOptions<BackgroundJobOptions> options,
    ILogger<ReportPreAggregationJob> logger)
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var runDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var integrationEvent = new ReportPreAggregationRequestedIntegrationEvent
        {
            TenantId = options.Value.DefaultTenantId,
            RunDate = runDate,
        };

        var payload = eventSerializer.Serialize(integrationEvent);
        logger.LogInformation(
            "Report pre-aggregation job queued event {EventType} for tenant {TenantId} on {RunDate}. Payload length: {PayloadLength}",
            integrationEvent.EventType,
            integrationEvent.TenantId,
            runDate,
            payload.Length);

        return Task.CompletedTask;
    }
}
