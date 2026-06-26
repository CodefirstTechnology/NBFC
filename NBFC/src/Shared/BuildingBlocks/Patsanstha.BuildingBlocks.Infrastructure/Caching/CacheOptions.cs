namespace Patsanstha.BuildingBlocks.Infrastructure.Caching;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    public TimeSpan ReferenceDataTtl { get; set; } = TimeSpan.FromMinutes(30);

    public TimeSpan DashboardKpiTtl { get; set; } = TimeSpan.FromMinutes(2);

    public TimeSpan IdempotencyTtl { get; set; } = TimeSpan.FromHours(24);

    public TimeSpan SessionValidationTtl { get; set; } = TimeSpan.FromMinutes(5);

    public TimeSpan RateLimitWindow { get; set; } = TimeSpan.FromMinutes(1);
}
