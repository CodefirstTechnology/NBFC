using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application;

namespace Patsanstha.Modules.Members.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMembersApplication(this IServiceCollection services)
    {
        services.AddBuildingBlocksApplication(Assembly.GetExecutingAssembly());
        return services;
    }
}
