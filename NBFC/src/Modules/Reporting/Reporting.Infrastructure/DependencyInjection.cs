using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.BuildingBlocks.Infrastructure;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;
using Patsanstha.Modules.Reporting.Application.Abstractions;
using Patsanstha.Modules.Reporting.Infrastructure.Persistence;

namespace Patsanstha.Modules.Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ReportingDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Reporting"));
            options.AddBuildingBlocksInterceptors(sp);
        });

        services.AddScoped<IReportSnapshotRepository, ReportSnapshotRepository>();
        services.AddScoped<IReportSnapshotMapper, ReportSnapshotMapper>();
        services.AddScoped<IDashboardReadStore, DashboardReadStore>();
        services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<ReportingDbContext>());

        return services;
    }
}
