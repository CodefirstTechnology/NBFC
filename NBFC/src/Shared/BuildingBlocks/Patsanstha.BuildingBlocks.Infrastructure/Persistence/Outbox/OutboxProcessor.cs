using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Patsanstha.BuildingBlocks.Application.Abstractions.Caching;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;

namespace Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;

public sealed class OutboxProcessorBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<OutboxProcessorBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int MaxRetries = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                await processor.ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unhandled error in outbox processor loop.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }
}

public sealed class OutboxProcessor(
    IEnumerable<IOutboxDbContext> dbContexts,
    IEnumerable<IIntegrationEventHandler> handlers,
    ICacheInvalidationService cacheInvalidationService,
    ILogger<OutboxProcessor> logger) : IOutboxProcessor
{
    private const int MaxRetries = 5;
    private readonly Dictionary<string, IIntegrationEventHandler> _handlers =
        handlers.ToDictionary(h => h.EventType, StringComparer.Ordinal);

    public async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var dbContext in dbContexts)
        {
            await ProcessContextAsync(dbContext, cancellationToken);
        }
    }

    private async Task ProcessContextAsync(IOutboxDbContext dbContext, CancellationToken cancellationToken)
    {
        var messages = await dbContext.GetPendingOutboxMessagesAsync(cancellationToken);

        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                message.Status = OutboxMessageStatus.Processing;
                await dbContext.SaveChangesAsync(cancellationToken);

                if (!_handlers.TryGetValue(message.EventType, out var handler))
                {
                    logger.LogWarning(
                        "No handler registered for outbox event type {EventType}. Marking as processed.",
                        message.EventType);
                }
                else
                {
                    await handler.HandleAsync(message.Payload, cancellationToken);
                }

                message.Status = OutboxMessageStatus.Processed;
                message.ProcessedAt = DateTimeOffset.UtcNow;
                message.LastError = null;

                await cacheInvalidationService.InvalidateForEventAsync(message.EventType, cancellationToken);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.LastError = ex.Message;
                message.Status = message.RetryCount >= MaxRetries
                    ? OutboxMessageStatus.Failed
                    : OutboxMessageStatus.Pending;

                logger.LogError(
                    ex,
                    "Failed to process outbox message {MessageId} of type {EventType}. Retry {RetryCount}.",
                    message.Id,
                    message.EventType,
                    message.RetryCount);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

public interface IOutboxDbContext
{
    Task<IReadOnlyList<OutboxMessageEntity>> GetPendingOutboxMessagesAsync(
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public static class OutboxDbContextExtensions
{
    public static async Task<IReadOnlyList<OutboxMessageEntity>> GetPendingOutboxMessagesAsync(
        this DbContext dbContext,
        int batchSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<OutboxMessageEntity>()
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }
}
