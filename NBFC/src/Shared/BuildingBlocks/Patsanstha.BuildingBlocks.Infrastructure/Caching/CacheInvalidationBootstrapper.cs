using Microsoft.Extensions.Hosting;
using Patsanstha.BuildingBlocks.Application.Abstractions.Caching;

namespace Patsanstha.BuildingBlocks.Infrastructure.Caching;

public sealed class CacheInvalidationBootstrapper(ICacheInvalidationService cacheInvalidationService) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        cacheInvalidationService.Register("identity.user.roles.changed", CacheKeys.RolesPrefix);
        cacheInvalidationService.Register("identity.roles.changed", CacheKeys.RolesPrefix, CacheKeys.Permissions);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
