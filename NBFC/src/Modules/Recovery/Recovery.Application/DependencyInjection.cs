using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application;

namespace Patsanstha.Modules.Recovery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddRecoveryApplication(this IServiceCollection services)
    {
        services.AddBuildingBlocksApplication(Assembly.GetExecutingAssembly());
        return services;
    }
}
