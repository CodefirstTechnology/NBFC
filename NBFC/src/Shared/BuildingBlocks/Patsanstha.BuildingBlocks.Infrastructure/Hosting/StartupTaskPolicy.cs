using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Patsanstha.BuildingBlocks.Infrastructure.Hosting;

public static class StartupTaskPolicy
{
    public static bool ShouldRun(IHost host, string productionFlagKey)
    {
        var environment = host.Services.GetRequiredService<IHostEnvironment>();
        if (environment.IsDevelopment())
        {
            return true;
        }

        return host.Services.GetRequiredService<IConfiguration>().GetValue<bool>(productionFlagKey);
    }
}
