namespace Patsanstha.BuildingBlocks.Infrastructure.Caching;

public static class CacheKeyGuard
{
    private static readonly string[] ForbiddenFragments =
    [
        "balance",
        "outstanding",
        "emi-due",
        "emi_due",
        "account-balance",
        "loan-outstanding",
        "due-amount",
        "due_amount",
    ];

    public static void EnsureAllowed(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key is required.", nameof(key));
        }

        var normalized = key.ToLowerInvariant();
        foreach (var fragment in ForbiddenFragments)
        {
            if (normalized.Contains(fragment, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Cache key '{key}' is forbidden. Balances, EMI due amounts, and loan outstanding figures must never be cached.");
            }
        }
    }
}
