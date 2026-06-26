using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Patsanstha.Modules.Identity.Infrastructure.Services;

namespace Patsanstha.Modules.Identity.Infrastructure.Hosting;

public static class IdentityHostExtensions
{
    public static async Task SeedIdentityDataAsync(this IHost host)
    {
        if (!host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            return;
        }

        using var scope = host.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
        await seeder.SeedAsync();
    }
}
