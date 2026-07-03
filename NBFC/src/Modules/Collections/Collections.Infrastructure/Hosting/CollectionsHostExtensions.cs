using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Patsanstha.BuildingBlocks.Infrastructure.Hosting;
using Patsanstha.Modules.Collections.Infrastructure.Persistence;

namespace Patsanstha.Modules.Collections.Infrastructure.Hosting;

public static class CollectionsHostExtensions
{
    public static async Task MigrateCollectionsSchemaAsync(this IHost host)
    {
        if (!StartupTaskPolicy.ShouldRun(host, "Startup:AutoMigrate"))
        {
            return;
        }

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CollectionsDbContext>();

        var pending = await dbContext.Database.GetPendingMigrationsAsync();
        if (pending.Any())
        {
            await dbContext.Database.MigrateAsync();
            return;
        }

        if (!(await dbContext.Database.GetAppliedMigrationsAsync()).Any())
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
    }
}
