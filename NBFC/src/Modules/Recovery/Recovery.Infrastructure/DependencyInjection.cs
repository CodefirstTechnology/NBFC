using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.BuildingBlocks.Infrastructure;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;
using Patsanstha.Modules.Recovery.Application.Abstractions;
using Patsanstha.Modules.Recovery.Infrastructure.Persistence;

namespace Patsanstha.Modules.Recovery.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRecoveryModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<RecoveryDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Recovery"));
            options.AddBuildingBlocksInterceptors(sp);
        });

        services.AddScoped<IRecoveryCaseRepository, RecoveryCaseRepository>();
        services.AddScoped<ICaseNumberGenerator, CaseNumberGenerator>();
        services.AddScoped<IRecoveryCaseMapper, RecoveryCaseMapper>();
        services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<RecoveryDbContext>());

        return services;
    }
}
