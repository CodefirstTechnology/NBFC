using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application;

namespace Patsanstha.Modules.Reporting.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingApplication(this IServiceCollection services)
    {
        services.AddBuildingBlocksApplication(Assembly.GetExecutingAssembly());
        return services;
    }
}
