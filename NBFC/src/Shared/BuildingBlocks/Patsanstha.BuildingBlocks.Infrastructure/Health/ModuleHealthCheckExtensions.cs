using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Patsanstha.BuildingBlocks.Infrastructure.Health;

public static class ModuleHealthCheckExtensions
{
    public static IHealthChecksBuilder AddModuleDbContextCheck<TDbContext>(
        this IHealthChecksBuilder builder,
        string moduleName)
        where TDbContext : DbContext =>
        builder.AddDbContextCheck<TDbContext>(
            name: moduleName,
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready", $"module:{moduleName}"]);
}
