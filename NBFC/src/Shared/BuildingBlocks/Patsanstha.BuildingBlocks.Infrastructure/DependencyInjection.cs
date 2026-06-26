using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Application.Abstractions.Caching;
using Patsanstha.BuildingBlocks.Application.Abstractions.Idempotency;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.BuildingBlocks.Infrastructure.Audit;
using Patsanstha.BuildingBlocks.Infrastructure.Caching;
using Patsanstha.BuildingBlocks.Infrastructure.Idempotency;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Interceptors;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;
using Patsanstha.BuildingBlocks.Infrastructure.Resilience;

namespace Patsanstha.BuildingBlocks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<AuditContextAccessor>();
        services.AddSingleton<IAuditContextAccessor>(sp => sp.GetRequiredService<AuditContextAccessor>());
        services.AddSingleton<IReferenceDataCache, ReferenceDataCache>();
        services.AddSingleton<IDistributedCacheStore, DistributedCacheStore>();
        services.AddSingleton<ICacheInvalidationService, CacheInvalidationService>();
        services.AddSingleton<IIdempotencyStore, IdempotencyStore>();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddSingleton<IIntegrationEventSerializer, IntegrationEventSerializer>();
        services.AddSingleton<OutboxWriter>();
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();
        services.AddHostedService<OutboxProcessorBackgroundService>();
        services.AddHostedService<CacheInvalidationBootstrapper>();
        services.AddPatsansthaResilientHttpClients();

        return services;
    }

    public static DbContextOptionsBuilder AddBuildingBlocksInterceptors(
        this DbContextOptionsBuilder options,
        IServiceProvider serviceProvider)
    {
        return options
            .AddInterceptors(
                serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>(),
                serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>());
    }
}
