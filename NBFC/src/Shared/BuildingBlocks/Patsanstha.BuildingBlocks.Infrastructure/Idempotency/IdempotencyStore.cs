using Patsanstha.BuildingBlocks.Application.Abstractions.Caching;
using Patsanstha.BuildingBlocks.Application.Abstractions.Idempotency;

namespace Patsanstha.BuildingBlocks.Infrastructure.Idempotency;

public sealed class IdempotencyStore(IDistributedCacheStore distributedCacheStore) : IIdempotencyStore
{
    public Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken = default) =>
        distributedCacheStore.GetAsync<IdempotencyRecord>(CacheKeys.Idempotency(key), cancellationToken);

    public Task StoreAsync(IdempotencyRecord record, TimeSpan ttl, CancellationToken cancellationToken = default) =>
        distributedCacheStore.SetAsync(CacheKeys.Idempotency(record.Key), record, ttl, cancellationToken);
}
