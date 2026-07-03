using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Patsanstha.BuildingBlocks.Infrastructure.Hosting;
using Patsanstha.Modules.Accounting.Infrastructure.Persistence;

namespace Patsanstha.Modules.Accounting.Infrastructure.Hosting;

public static class AccountingHostExtensions
{
    public static async Task MigrateAccountingSchemaAsync(this IHost host)
    {
        if (!StartupTaskPolicy.ShouldRun(host, "Startup:AutoMigrate"))
        {
            return;
        }

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AccountingDbContext>();

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
