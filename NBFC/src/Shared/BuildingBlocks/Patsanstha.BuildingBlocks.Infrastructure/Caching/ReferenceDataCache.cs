using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Patsanstha.BuildingBlocks.Application.Abstractions.Caching;

namespace Patsanstha.BuildingBlocks.Infrastructure.Caching;

public sealed class ReferenceDataCache(
    IMemoryCache memoryCache,
    IOptions<CacheOptions> options) : IReferenceDataCache
{
    private readonly ConcurrentDictionary<string, byte> _trackedKeys = new(StringComparer.Ordinal);

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default)
    {
        CacheKeyGuard.EnsureAllowed(key);

        if (memoryCache.TryGetValue(key, out T? cached) && cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);
        var ttl = absoluteExpiration ?? options.Value.ReferenceDataTtl;

        memoryCache.Set(key, value, ttl);
        _trackedKeys.TryAdd(key, 0);

        return value;
    }

    public void Invalidate(string key)
    {
        memoryCache.Remove(key);
        _trackedKeys.TryRemove(key, out _);
    }

    public void InvalidateByPrefix(string prefix)
    {
        foreach (var key in _trackedKeys.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)))
        {
            Invalidate(key);
        }
    }
}
