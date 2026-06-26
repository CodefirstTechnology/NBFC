using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Patsanstha.Modules.Deposits.Infrastructure.Persistence;

namespace Patsanstha.Modules.Deposits.Infrastructure.Hosting;

public static class DepositsHostExtensions
{
    public static async Task MigrateDepositsSchemaAsync(this IHost host)
    {
        if (!host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            return;
        }

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DepositsDbContext>();

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
