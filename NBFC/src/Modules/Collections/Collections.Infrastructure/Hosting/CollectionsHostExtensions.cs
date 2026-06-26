using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Patsanstha.Modules.Collections.Infrastructure.Persistence;

namespace Patsanstha.Modules.Collections.Infrastructure.Hosting;

public static class CollectionsHostExtensions
{
    public static async Task MigrateCollectionsSchemaAsync(this IHost host)
    {
        if (!host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
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
