namespace Patsanstha.BuildingBlocks.Application.Abstractions.Caching;

public static class CacheKeys
{
    public const string Permissions = "ref:permissions";
    public const string RolesPrefix = "ref:roles";
    public const string LoanProducts = "ref:loan-products";
    public const string BranchMasterPrefix = "ref:branches";
    public const string InterestRatesPrefix = "ref:interest-rates";

    public static string Roles(Guid tenantId) => $"{RolesPrefix}:{tenantId:N}";
    public static string Branch(Guid tenantId, Guid branchId) => $"{BranchMasterPrefix}:{tenantId:N}:{branchId:N}";
    public static string DashboardKpi(Guid tenantId, Guid? branchId) =>
        branchId.HasValue
            ? $"kpi:{tenantId:N}:{branchId.Value:N}"
            : $"kpi:{tenantId:N}:all";

    public static string Idempotency(string key) => $"idempotency:{key}";
    public static string RateLimit(string scope, string identifier) => $"ratelimit:{scope}:{identifier}";
    public static string SessionValidation(string tokenHash) => $"session:{tokenHash}";
}

public interface IReferenceDataCache
{
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default);

    void Invalidate(string key);

    void InvalidateByPrefix(string prefix);
}

public interface IDistributedCacheStore
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan absoluteExpiration,
        CancellationToken cancellationToken = default);

    Task<bool> TrySetAsync<T>(
        string key,
        T value,
        TimeSpan absoluteExpiration,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task<long> IncrementAsync(
        string key,
        long delta,
        TimeSpan absoluteExpiration,
        CancellationToken cancellationToken = default);
}

public interface ICacheInvalidationService
{
    void Register(string eventType, params string[] cacheKeyPrefixes);

    Task InvalidateForEventAsync(string eventType, CancellationToken cancellationToken = default);
}
