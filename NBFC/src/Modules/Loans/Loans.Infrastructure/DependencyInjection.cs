using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.BuildingBlocks.Infrastructure;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;
using Patsanstha.Modules.Loans.Application.Abstractions;
using Patsanstha.Modules.Loans.Infrastructure.Persistence;

namespace Patsanstha.Modules.Loans.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLoansModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<LoansDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Loans"));
            options.AddBuildingBlocksInterceptors(sp);
        });

        services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
        services.AddScoped<ILoanNumberGenerator, LoanNumberGenerator>();
        services.AddScoped<ILoanApplicationMapper, LoanApplicationMapper>();
        services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<LoansDbContext>());

        return services;
    }
}
