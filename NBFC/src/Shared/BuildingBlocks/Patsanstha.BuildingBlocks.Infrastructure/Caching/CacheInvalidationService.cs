using Patsanstha.BuildingBlocks.Application.Abstractions.Caching;

namespace Patsanstha.BuildingBlocks.Infrastructure.Caching;

public sealed class CacheInvalidationService(IReferenceDataCache referenceDataCache) : ICacheInvalidationService
{
    private readonly Dictionary<string, string[]> _rules = new(StringComparer.Ordinal);

    public void Register(string eventType, params string[] cacheKeyPrefixes)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type is required.", nameof(eventType));
        }

        _rules[eventType] = cacheKeyPrefixes;
    }

    public Task InvalidateForEventAsync(string eventType, CancellationToken cancellationToken = default)
    {
        if (!_rules.TryGetValue(eventType, out var prefixes))
        {
            return Task.CompletedTask;
        }

        foreach (var prefix in prefixes)
        {
            if (prefix.EndsWith(':'))
            {
                referenceDataCache.InvalidateByPrefix(prefix);
            }
            else
            {
                referenceDataCache.Invalidate(prefix);
            }
        }

        return Task.CompletedTask;
    }
}
