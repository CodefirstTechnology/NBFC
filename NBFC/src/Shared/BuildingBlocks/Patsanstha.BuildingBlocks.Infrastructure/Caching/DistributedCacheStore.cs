using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Patsanstha.BuildingBlocks.Application.Abstractions.Caching;

namespace Patsanstha.BuildingBlocks.Infrastructure.Caching;

public sealed class DistributedCacheStore(IDistributedCache distributedCache) : IDistributedCacheStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        CacheKeyGuard.EnsureAllowed(key);

        var payload = await distributedCache.GetStringAsync(key, cancellationToken);
        return payload is null ? default : JsonSerializer.Deserialize<T>(payload, SerializerOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan absoluteExpiration,
        CancellationToken cancellationToken = default)
    {
        CacheKeyGuard.EnsureAllowed(key);

        var payload = JsonSerializer.Serialize(value, SerializerOptions);
        await distributedCache.SetStringAsync(
            key,
            payload,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpiration },
            cancellationToken);
    }

    public async Task<bool> TrySetAsync<T>(
        string key,
        T value,
        TimeSpan absoluteExpiration,
        CancellationToken cancellationToken = default)
    {
        var existing = await distributedCache.GetStringAsync(key, cancellationToken);
        if (existing is not null)
        {
            return false;
        }

        await SetAsync(key, value, absoluteExpiration, cancellationToken);
        return true;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        distributedCache.RemoveAsync(key, cancellationToken);

    public async Task<long> IncrementAsync(
        string key,
        long delta,
        TimeSpan absoluteExpiration,
        CancellationToken cancellationToken = default)
    {
        var current = await GetAsync<long?>(key, cancellationToken) ?? 0;
        var next = current + delta;
        await SetAsync(key, next, absoluteExpiration, cancellationToken);
        return next;
    }
}
