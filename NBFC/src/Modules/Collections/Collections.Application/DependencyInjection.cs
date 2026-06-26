using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application;

namespace Patsanstha.Modules.Collections.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCollectionsApplication(this IServiceCollection services)
    {
        services.AddBuildingBlocksApplication(Assembly.GetExecutingAssembly());
        return services;
    }
}
