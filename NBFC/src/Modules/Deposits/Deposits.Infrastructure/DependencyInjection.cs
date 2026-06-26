using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.BuildingBlocks.Infrastructure;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;
using Patsanstha.Modules.Deposits.Application.Abstractions;
using Patsanstha.Modules.Deposits.Infrastructure.Persistence;

namespace Patsanstha.Modules.Deposits.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDepositsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DepositsDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Deposits"));
            options.AddBuildingBlocksInterceptors(sp);
        });

        services.AddScoped<IDepositAccountRepository, DepositAccountRepository>();
        services.AddScoped<IDepositAccountNumberGenerator, DepositAccountNumberGenerator>();
        services.AddScoped<IDepositAccountMapper, DepositAccountMapper>();
        services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<DepositsDbContext>());

        return services;
    }
}
