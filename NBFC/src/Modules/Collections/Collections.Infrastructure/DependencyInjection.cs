using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.BuildingBlocks.Infrastructure;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;
using Patsanstha.Modules.Collections.Application.Abstractions;
using Patsanstha.Modules.Collections.Infrastructure.Persistence;

namespace Patsanstha.Modules.Collections.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCollectionsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CollectionsDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Collections"));
            options.AddBuildingBlocksInterceptors(sp);
        });

        services.AddScoped<ICollectionReceiptRepository, CollectionReceiptRepository>();
        services.AddScoped<IReceiptNumberGenerator, ReceiptNumberGenerator>();
        services.AddScoped<ICollectionReceiptMapper, CollectionReceiptMapper>();
        services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<CollectionsDbContext>());

        return services;
    }
}
