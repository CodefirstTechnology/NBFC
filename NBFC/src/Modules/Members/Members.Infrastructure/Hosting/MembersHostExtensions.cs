using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Patsanstha.Modules.Members.Infrastructure.Persistence;

namespace Patsanstha.Modules.Members.Infrastructure.Hosting;

public static class MembersHostExtensions
{
    public static async Task MigrateMembersSchemaAsync(this IHost host)
    {
        if (!host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            return;
        }

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MembersDbContext>();

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
