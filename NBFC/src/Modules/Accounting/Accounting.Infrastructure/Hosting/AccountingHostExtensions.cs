using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Patsanstha.Modules.Accounting.Infrastructure.Persistence;

namespace Patsanstha.Modules.Accounting.Infrastructure.Hosting;

public static class AccountingHostExtensions
{
    public static async Task MigrateAccountingSchemaAsync(this IHost host)
    {
        if (!host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
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
