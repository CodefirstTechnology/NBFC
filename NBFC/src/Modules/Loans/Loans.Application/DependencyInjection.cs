using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application;

namespace Patsanstha.Modules.Loans.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddLoansApplication(this IServiceCollection services)
    {
        services.AddBuildingBlocksApplication(Assembly.GetExecutingAssembly());
        return services;
    }
}
