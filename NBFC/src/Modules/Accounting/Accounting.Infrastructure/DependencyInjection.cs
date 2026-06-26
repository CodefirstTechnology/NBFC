using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.BuildingBlocks.Infrastructure;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;
using Patsanstha.Modules.Accounting.Application.Abstractions;
using Patsanstha.Modules.Accounting.Infrastructure.Persistence;

namespace Patsanstha.Modules.Accounting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AccountingDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Accounting"));
            options.AddBuildingBlocksInterceptors(sp);
        });

        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<IEntryNumberGenerator, EntryNumberGenerator>();
        services.AddScoped<IJournalEntryMapper, JournalEntryMapper>();
        services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<AccountingDbContext>());

        return services;
    }
}
